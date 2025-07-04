using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Linq;

namespace OrnaUtils.Generators
{
    [Generator]
    public class NotifyChangedGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var fieldsWithAttribute = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (s, _) => s is FieldDeclarationSyntax fds && fds.AttributeLists.Count > 0,
                    transform: (ctx, _) => GetFieldWithAttribute(ctx))
                .Where(m => m is not null);

            var compilationAndFields = fieldsWithAttribute.Collect();

            context.RegisterSourceOutput(compilationAndFields, (spc, source) => Execute(source, spc));
        }

        private static (IFieldSymbol fieldSymbol, string propertyName, string[] props)? GetFieldWithAttribute(GeneratorSyntaxContext context)
        {
            var field = (FieldDeclarationSyntax)context.Node;

            foreach (var variable in field.Declaration.Variables)
            {
                var symbol = context.SemanticModel.GetDeclaredSymbol(variable);
                if (symbol is IFieldSymbol fieldSymbol &&
                    fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass?.ToDisplayString() == "OrnaUtils.Attributes.NotifyChangedAttribute"))
                {
                    var attr = fieldSymbol.GetAttributes().First(ad => ad.AttributeClass?.ToDisplayString() == "OrnaUtils.Attributes.NotifyChangedAttribute");
                    string propertyName = ToPascalCase(variable.Identifier.Text);
                    return (fieldSymbol, propertyName, attr.ConstructorArguments[0].Values.Select(v => v.Value).Cast<string>().ToArray());
                }
            }

            return null;
        }

        private static void Execute(ImmutableArray<(IFieldSymbol fieldSymbol, string propertyName, string[] props)?> fields, SourceProductionContext context)
        {
            foreach (var group in fields.Where(f => f is not null)
                .GroupBy(f => f.Value.fieldSymbol.ContainingType, SymbolEqualityComparer.Default))
            {
                var classSymbol = group.Key;
                var source = GenerateClass([..group.Select(q => q.Value)]);
                context.AddSource($"{classSymbol.Name}_NotifyChanged.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }

        private static string GenerateClass(List<(IFieldSymbol fieldSymbol, string propertyName, string[] props)> fields)
        {
            var classSymbol = fields.First().fieldSymbol.ContainingType;
            var ns = classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;

            var sb = new StringBuilder($$"""
                using System.ComponentModel;
                
                namespace {{ns}}
                {
                    public partial class {{className}} : INotifyPropertyChanged
                    {
                        public event PropertyChangedEventHandler? PropertyChanged;
                        public void OnPropertyChanged(string propertyName) =>
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                """);

            foreach (var (field, propertyName, props) in fields)
            {
                var fieldType = field.Type.ToDisplayString();
                var fieldName = field.Name;
                sb.AppendLine($$"""
                    public {{fieldType}} {{propertyName}}
                            {
                                get => {{fieldName}};
                                set
                                {
                                    if (!Equals({{fieldName}}, value))
                                    {
                                        {{fieldName}} = value;
                                        OnPropertyChanged(nameof({{propertyName}}));
                    """);
                foreach (var propName in props)
                    sb.AppendLine($"                    OnPropertyChanged(nameof({propName}));");
                sb.AppendLine(@"                }
            }
        }");
            }

            sb.AppendLine("    }\n}");
            return sb.ToString();
        }

        private static string ToPascalCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            name = name.TrimStart('_');
            return char.ToUpper(name[0]) + name.Substring(1);
        }
    }
}
