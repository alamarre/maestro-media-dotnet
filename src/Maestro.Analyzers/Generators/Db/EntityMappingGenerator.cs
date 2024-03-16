using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Maestro.Analyzers.Generators.Db;

[Generator]
public class EntityMappingGenerator : IIncrementalGenerator
{
    const string entityNamespace = "Maestro.Entities";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<RecordDeclarationSyntax?> syntaxProvider =
            context.SyntaxProvider.CreateSyntaxProvider(
                static (syntaxNode, ct) => syntaxNode is RecordDeclarationSyntax,
                static (context, ct) => context.Node as RecordDeclarationSyntax
            );

        IncrementalValueProvider<(Compilation, ImmutableArray<RecordDeclarationSyntax?>)> records
            = context.CompilationProvider.Combine(syntaxProvider.Collect());

        context.RegisterSourceOutput(records, (context, input) =>
        {
            var compilation = input.Item1;
            var items = input.Item2;
            var matches = items.Where(x => x?.SyntaxTree is not null)
                .Select(x => compilation.GetSemanticModel(x!.SyntaxTree)?.GetDeclaredSymbol(x))
                .Where(x => x is not null && x is ITypeSymbol typeSymbol
                                          && typeSymbol.ContainingNamespace?.ToString() == entityNamespace
                                          && typeSymbol.Name.ToString() != "TenantTable")
                .Cast<ISymbol>()
                .ToImmutableArray();

            var tenantMatches = matches.Where(x =>
                    x is not null && x is ITypeSymbol typeSymbol &&
                    typeSymbol.BaseType?.ToString() == "Maestro.Entities.TenantTable")
                .ToImmutableArray();


            if (matches.Any())
            {
                var dbSets = string.Join("\n    ",
                    matches.Select(x => @$"public required DbSet<{x!.Name}> {x.Name} {{ get; set; }}"));
                context.AddSource("MediaDbContext.g.cs", SourceText.From($@"
// <auto-generated />
using Maestro.Auth;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Entities;

public partial class MediaDbContext {{
    {dbSets}

    partial void MapQueryFilters(ModelBuilder modelBuilder) {{
        {string.Join("\n        ", tenantMatches.Select(x => @$"AddQueryFilter<{x!.Name}>(modelBuilder);"))}
    }}
}}
                ", Encoding.UTF8));
            }
        });
    }
}
