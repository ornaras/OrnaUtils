using Microsoft.CodeAnalysis;

namespace OrnaUtils.Generators
{
    [Generator]
    public class AttributesGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx => 
                ctx.AddSource("Attributes.g.cs", $$"""
                    namespace OrnaLibs.Attributes
                    {
                        {{NotifyChangedAttribute}}
                    }
                    """));
        }

        private string NotifyChangedAttribute => """
            [AttributeUsage(AttributeTargets.Field)]
                public class NotifyChangedAttribute : Attribute
                {
                    public NotifyChangedAttribute(params string[] otherProps) { }
                }

            """;
    }
}
