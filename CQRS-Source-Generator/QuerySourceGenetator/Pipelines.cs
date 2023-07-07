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

namespace CQRS_Source_Generator.QuerySourceGenetator
{
    internal static class Pipelines
    {
        private const string attributeFullName = "CQRS_Source_Generator.Attributes.GenerateQuery";

        // first stage
        /// <summary>
        /// This method is called for every syntax node in the compilation, checking if it is a method and has any attributes
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Returns <see langword="true"/> if method has any methods </returns>
        public static bool IsMethodWithAttributes(SyntaxNode node)
            => node is MethodDeclarationSyntax { AttributeLists.Count: > 0};

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
                        if (attributeContainingTypeSymbol.ToDisplayString().Equals(attributeFullName))
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
                    hintName: string.Format("{0}Query.g.cs",methodInfo.Name),
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

            // Get the semantic representation of our marker attribute 
            INamedTypeSymbol? generateQueryAttribute = compilation.GetTypeByMetadataName(attributeFullName);

            if (generateQueryAttribute == null)
            {
                // If this is null, the compilation couldn't find the marker attribute type
                // which suggests there's something very wrong
                return methodsInfo;
            }

            foreach (MethodDeclarationSyntax methodDeclarationSyntax in methods)
            {
                ct.ThrowIfCancellationRequested();

                SemanticModel semanticModel = compilation.GetSemanticModel(methodDeclarationSyntax.SyntaxTree);
                // Extract parameter information
                if (methodDeclarationSyntax.ParameterList.Parameters.Count != 1)
                {
                    throw new Exception("Query must have only 1 parameter. Use single DTO class");
                }

                var parameterSyntax = methodDeclarationSyntax.ParameterList.Parameters[0];

                // Create an methodInfo for use in the generation phase
                methodsInfo.Add(new MethodInfo { 
                    Name = methodDeclarationSyntax.Identifier.ValueText,
                    Namespace = methodDeclarationSyntax.Ancestors().OfType<NamespaceDeclarationSyntax>().First().Name.ToString(),
                    ParentInterface = methodDeclarationSyntax.Ancestors().OfType<InterfaceDeclarationSyntax>().First().Identifier.ValueText,
                    ReturnType = semanticModel.GetTypeInfo(methodDeclarationSyntax.ReturnType).Type.ToString(),
                    Parameter = (semanticModel.GetTypeInfo(parameterSyntax.Type).Type.ToString(), parameterSyntax.Identifier.ValueText)
                });
            }

            return methodsInfo;
        }
    }

}
