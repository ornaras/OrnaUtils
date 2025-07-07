using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrnaUtils.Generators
{
    [Generator]
    public class PathExtensionsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var keywordFiles = context.AdditionalTextsProvider
                .Where(file => file.Path.EndsWith("ornautils.txt"));

            var keywordContents = keywordFiles
                .Select((file, ct) => file.GetText(ct)?.ToString() ?? string.Empty);

            var enabledFeatures = keywordContents
                .Select((text, _) => text.Split('\r', '\n')
                    .Select(line => line.Trim()));

            context.RegisterSourceOutput(enabledFeatures, Generate);
        }

        public void Generate(SourceProductionContext context, IEnumerable<string> features)
        {
            var script = new StringBuilder();
            script.Append("""
                          namespace System.IO
                          {
                              public static class PathExtensions
                              {
                          
                          """);

            if(features.Contains("RequiredDirectory"))
                script.Append("""
                                      public static string AsRequiredDirectory(this string path)                    
                                      {
                                          if (!Directory.Exists(path))
                                          {
                                              var index = path.LastIndexOf(Path.DirectorySeparatorChar);
                                              AsRequiredDirectory(path.Substring(0, index));
                                              Directory.CreateDirectory(path);
                                          }
                                          return path;
                                      }

                              """);

            script.Append("""
                              }
                          }                          
                          """);

            context.AddSource($"PathExtensions.g.cs", SourceText.From(script.ToString(), Encoding.UTF8));
        }
    }
}
