using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace EcsRx.ComponentTypeRegistry.Analyzer;

[Generator]
public class ComponentGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        INamedTypeSymbol? typeIComponent = context.Compilation.GetTypeByMetadataName("EcsRx.Components.IComponent");
        if (typeIComponent == null)
        {
            return;
        }

        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
        {
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(syntaxTree);

            foreach (var componentType in syntaxTree.GetRoot().DescendantNodesAndSelf()
                         .OfType<TypeDeclarationSyntax>()
                         .Where(t => t.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                         .Select(m => semanticModel.GetTypeInfo(m).Type!)
                         .Where(t => t.AllInterfaces.Contains(typeIComponent)))
            {
                StringBuilder builder = new();
                builder.AppendLine("// Test");
                
                context.AddSource($"{componentType.Name}_TypeRegistry", builder.ToString());
            }
        };
    }
}