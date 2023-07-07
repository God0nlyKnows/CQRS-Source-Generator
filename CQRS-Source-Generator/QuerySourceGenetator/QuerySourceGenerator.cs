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

            IncrementalValuesProvider<MethodDeclarationSyntax> methodDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => Pipelines.IsMethodWithAttributes(s),           // select Methods with attributes
                    transform: static (ctx, _) => Pipelines.GetSemanticTargetsForGeneration(ctx))    // select all methods with the [QueryAttribute]
                .Where(static m => m is not null)!;

            // Combine the selected Methods with the `Compilation`
            IncrementalValueProvider<(Compilation, ImmutableArray<MethodDeclarationSyntax>)> compilationAndMethods
                = context.CompilationProvider.Combine(methodDeclarations.Collect());

            // Generate the source using the compilation and Methods
            context.RegisterSourceOutput(compilationAndMethods,
                static (spc, source) => Pipelines.Execute(source.Item1, source.Item2, spc));

        }

        

    }
}
