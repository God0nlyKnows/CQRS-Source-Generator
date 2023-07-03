using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Threading;
using CQRS_Source_Generator.Models;

namespace CQRS_Source_Generator.QuerySourceGenetator
{
    internal static class Pipelines
    {
        private const string attributeFullName = "CQRS_Source_Generator.Attributes.GenerateQuery";

        // first stage
        /// <summary>
        /// This method is called for every syntax node in the compilation, checking if it has any attributes
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Returns <see langword="true"/> if interface has any attributes </returns>
        public static bool IsSyntaxTargetForGeneration(SyntaxNode node)
            => node is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 };

        // second stage
        /// <summary>
        /// This method is called for every syntax node in the compilation, checking if it has the <see cref="Attributes.GenerateQuery"/> attribute
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Returns <see cref="InterfaceDeclarationSyntax"/> if it has <see cref="Attributes.GenerateQuery"/> attribute</returns>
        public static InterfaceDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            // we know the node is a InterfaceDeclarationSyntax thanks to IsSyntaxTargetForGeneration
            // and we can hard-cast it bcs it's fastest way to get the symbol
            var InterfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;

            // loop through all the attributes on the method
            foreach (AttributeListSyntax attributeListSyntax in InterfaceDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    {
                        continue;
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    string fullName = attributeContainingTypeSymbol.ToDisplayString();

                    // check if it's [GenerateQuery]
                    if (fullName == attributeFullName)
                    {
                        return InterfaceDeclarationSyntax;
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
        /// <param name="interfaces"></param>
        /// <param name="context"></param>
        public static void Execute(Compilation compilation, ImmutableArray<InterfaceDeclarationSyntax> interfaces, SourceProductionContext context)
        {
            if (interfaces.IsDefaultOrEmpty)
            {
                return;
            }


            List<InterfaceInfo> interfacesInfo = GetTypesInfo(compilation, interfaces, context.CancellationToken);


            foreach (InterfaceInfo interfaceInfo in interfacesInfo)
            {
                string result = QueryTemplate.GetQueryTemplate(interfaceInfo);
                context.AddSource("EnumExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
            }
        }


        /// <summary>
        /// Converts the <see cref="InterfaceDeclarationSyntax"/> to <see cref="InterfaceInfo"/>
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="interfaces"></param>
        /// <param name="ct"></param>
        /// <returns>Returns a list of <see cref="InterfaceInfo"/></returns>
        static List<InterfaceInfo> GetTypesInfo(Compilation compilation, IEnumerable<InterfaceDeclarationSyntax> interfaces, CancellationToken ct)
        {
            var interfacesInfo = new List<InterfaceInfo>();

            // Get the semantic representation of our marker attribute 
            INamedTypeSymbol? interfaceAttribute = compilation.GetTypeByMetadataName(attributeFullName);

            if (interfaceAttribute == null)
            {
                // If this is null, the compilation couldn't find the marker attribute type
                // which suggests there's something very wrong
                return interfacesInfo;
            }

            foreach (InterfaceDeclarationSyntax InterfaceDeclarationSyntax in interfaces)
            {
                ct.ThrowIfCancellationRequested();

                // Get the semantic representation of the interface syntax
                SemanticModel semanticModel = compilation.GetSemanticModel(InterfaceDeclarationSyntax.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(InterfaceDeclarationSyntax) is not INamedTypeSymbol interfaceSymbol)
                {
                    // something went wrong, ignore
                    continue;
                }


                // Create an interfaceInfo for use in the generation phase
                interfacesInfo.Add(new InterfaceInfo { Members = interfaceSymbol.GetMembers(), Name = interfaceSymbol.ToString(), Namespace = interfaceSymbol.ContainingNamespace.ToString() });
            }

            return interfacesInfo;
        }
    }
}
