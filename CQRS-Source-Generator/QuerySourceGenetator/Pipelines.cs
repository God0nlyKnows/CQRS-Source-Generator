using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Threading;
using CQRS_Source_Generator.Models;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace CQRS_Source_Generator.QuerySourceGenetator
{
    internal static class Pipelines
    {
        private const string attributeFullName = "CQRS_Source_Generator.Attributes.GenerateQuery";
        private const string attributeName = "GenerateQuery";

        // first stage
        /// <summary>
        /// This method is called for every syntax node in the compilation, checking if it is a method and has any attributes
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Returns <see langword="true"/> if method has any methods </returns>
        public static bool IsMethodWithAttributes(SyntaxNode node)
            => node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };

        // second stage
        /// <summary>
        /// This method is called for every syntax node in the compilation, checking if it has the <see cref="Attributes.GenerateQuery"/> attribute
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Returns <see cref="MethodDeclarationSyntax"/> if it has <see cref="Attributes.GenerateQuery"/> attribute</returns>
        public static MethodDeclarationSyntax? GetSemanticTargetsForGeneration(GeneratorSyntaxContext context)
        {
            // we know the node is a interfaceDeclarationSyntax thanks to IsSyntaxTargetForGeneration
            // and we can hard-cast it bcs it's fastest way to get the symbol
            var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;




            // loop through all the attributes on the method
            foreach (AttributeListSyntax attributeListSyntax in methodDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    {
                        continue;
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;

                    // check if it's [GenerateQuery]
                    if (attributeContainingTypeSymbol.ToDisplayString().StartsWith(attributeFullName))
                    {
                        return methodDeclarationSyntax;
                    }
                }
            }


            // we didn't find the attribute we were looking for
            return null;
        }

        // third stage
        /// <summary>
        /// Executes the generation of the source code
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="methods"></param>
        /// <param name="context"></param>
        public static void Execute(Compilation compilation, ImmutableArray<MethodDeclarationSyntax> methods, SourceProductionContext context)
        {
            if (methods.IsDefaultOrEmpty)
            {
                return;
            }


            List<MethodInfo> methodsInfo = GetTypesInfo(compilation, methods, context.CancellationToken);


            foreach (MethodInfo methodInfo in methodsInfo)
            {
                context.AddSource(
                    hintName: string.Format("{0}Query.g.cs", methodInfo.Name),
                    sourceText: SourceText.From(QueryTemplate.GetQueryTemplate(methodInfo), Encoding.UTF8));
            }
        }


        /// <summary>
        /// Converts the <see cref="MethodDeclarationSyntax"/> to <see cref="MethodInfo"/>
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="methods"></param>
        /// <param name="ct"></param>
        /// <returns>Returns a list of <see cref="MethodInfo"/></returns>
        static List<MethodInfo> GetTypesInfo(Compilation compilation, IEnumerable<MethodDeclarationSyntax> methods, CancellationToken ct)
        {
            var methodsInfo = new List<MethodInfo>();


            foreach (MethodDeclarationSyntax methodDeclarationSyntax in methods)
            {
                ct.ThrowIfCancellationRequested();


                // Get method symbol of our method 
                SemanticModel semanticModel = compilation.GetSemanticModel(methodDeclarationSyntax.SyntaxTree);
                IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax);

                // Get our marker attribute data
                var annotation = methodSymbol.GetAttributes().FirstOrDefault();

                if (!annotation.AttributeClass.OriginalDefinition.Name.Equals(attributeName))
                {
                    // This isn't the [GenerateQuery] attribute
                    continue;
                }

                // Extract the attribute parameters

                // typeArgument represents the type argument T in the [GenerateQuery<T>] attribute
                var typeArgument = annotation.AttributeClass.TypeArguments.FirstOrDefault();

                if (typeArgument == null)
                {
                    throw new Exception("Query must have a return type. Use single DTO class");
                }

                string typeT = typeArgument.ToDisplayString();

                //var typeTType = semanticModel.Compilation.GetTypeByMetadataName(typeT);


                // Extract parameter information

                // Check if method has any parameters
                ImmutableArray<string> orderedRequestParameters;
                bool hasParameters = methodDeclarationSyntax.ParameterList.Parameters.Count != 0;
                if (hasParameters)
                {
                    // Check if provided type has matching properties
                    ImmutableArray<IPropertySymbol> props = typeArgument.GetMembers().OfType<IPropertySymbol>().Where(x => !x.Name.Equals("EqualityContract")).ToImmutableArray();
                    if (methodDeclarationSyntax.ParameterList.Parameters.Count != props.Count())
                    {
                        throw new Exception($"Provided {typeT} class doesn't match to required parameters in method");
                    }

                    orderedRequestParameters = GetOrderedProperties(props, methodDeclarationSyntax);


                }



                // Create an methodInfo for use in the generation phase
                methodsInfo.Add(new MethodInfo
                {
                    Name = methodDeclarationSyntax.Identifier.ValueText,
                    Namespace = methodDeclarationSyntax.Ancestors().OfType<NamespaceDeclarationSyntax>().First().Name.ToString(),
                    ParentInterface = methodDeclarationSyntax.Ancestors().OfType<InterfaceDeclarationSyntax>().First().Identifier.ValueText,
                    ReturnType = semanticModel.GetTypeInfo(methodDeclarationSyntax.ReturnType).Type.ToString(),
                    RequestType = typeT,
                    HasParameters = hasParameters,
                    OrderedRequestParameters = orderedRequestParameters
                });
            }

            return methodsInfo;
        }


        static ImmutableArray<string> GetOrderedProperties(ImmutableArray<IPropertySymbol> properties, MethodDeclarationSyntax methodDeclarationSyntax)
        {
            List<string> orderedProperties = new List<string>();
            // Iterate over method parameters
            foreach (var parameter in methodDeclarationSyntax.ParameterList.Parameters)
            {
                // Find the corresponding property based on the parameter name
                var property = properties.FirstOrDefault(p => p.Name.ToLower().Equals(parameter.Identifier.ValueText.ToLower()));

                if (property == null)
                {
                    // Handle the case when a property corresponding to the parameter is not found
                    throw new Exception($"Property matching parameter '{parameter.Identifier.ValueText}' not found.");
                }

                // Add the property to the ordered list
                orderedProperties.Add(property.Name);
            }

            // Convert the list to an array if needed
            return orderedProperties.ToImmutableArray();
        }
    }

}
