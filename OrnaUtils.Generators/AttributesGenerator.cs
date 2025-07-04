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
                    using System;

                    namespace OrnaUtils.Attributes
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
