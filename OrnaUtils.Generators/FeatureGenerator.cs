using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Text;

namespace OrnaUtils.Generators
{
    public abstract class FeatureGenerator : IIncrementalGenerator
    {
        protected abstract string FeatureName { get; }
        protected abstract string Script { get; }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var keywordFiles = context.AdditionalTextsProvider
                .Where(file => file.Path.EndsWith("ornautils.txt"));

            var keywordContents = keywordFiles
                .Select((file, ct) => file.GetText(ct)?.ToString() ?? string.Empty);

            var featureEnabled = keywordContents
                .Select((text, _) => text.Split('\r', '\n')
                    .Select(line => line.Trim()).Contains(FeatureName));

            var filtered = featureEnabled.Where(isEnabled => isEnabled);

            context.RegisterSourceOutput(filtered, (spc, _) =>
                spc.AddSource($"{FeatureName}.g.cs", SourceText.From(Script, Encoding.UTF8)));
        }
    }
}
