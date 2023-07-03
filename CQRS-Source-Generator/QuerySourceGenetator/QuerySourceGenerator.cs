using CQRS_Source_Generator.Helpers;
using CQRS_Source_Generator.Models;
using CQRS_Source_Generator.QuerySourceGenetator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

namespace CQRS_Source_Generator
{
    [Generator(LanguageNames.CSharp)]
    public sealed class QuerySourceGenerator : IIncrementalGenerator
    {

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            IncrementalValuesProvider<InterfaceDeclarationSyntax> interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => Pipelines.IsSyntaxTargetForGeneration(s),           // select Interfaces with attributes
                transform: static (ctx, _) => Pipelines.GetSemanticTargetForGeneration(ctx))    // select the class with the [QueryAttribute]
            .Where(static m => m is not null)!;                                                 // filter out attributed Interfaces that we don't care about

            // Combine the selected Interfaces with the `Compilation`
            IncrementalValueProvider<(Compilation, ImmutableArray<InterfaceDeclarationSyntax>)> compilationAndInterfaces
                = context.CompilationProvider.Combine(interfaceDeclarations.Collect());

            // Generate the source using the compilation and Interfaces
            context.RegisterSourceOutput(compilationAndInterfaces,
                static (spc, source) => Pipelines.Execute(source.Item1, source.Item2, spc));

        }

        

    }
}
