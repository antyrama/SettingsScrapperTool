using Microsoft.Extensions.Configuration.AppService.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.Configuration.AppService.Core;

internal class IncludeKeysGenerator
{
    private readonly IConfigurationRoot _configurationRoot;

    public IncludeKeysGenerator(IConfiguration configuration)
    {
        _configurationRoot = (IConfigurationRoot)configuration;
    }

    public ISet<string> Generate(IEnumerable<string> providers, IEnumerable<string> include, string separator)
    {
        var includeList = new List<string>(include);

        var configurationProviders = providers
            .SelectMany(pr => _configurationRoot.Providers.Where(p => p.GetType().Name.Equals(pr)));

        var dataProperty = typeof(ConfigurationProvider)
            .GetProperty("Data", BindingFlags.NonPublic | BindingFlags.Instance);

        if (dataProperty != null)
        {
            foreach (var provider in configurationProviders)
            {
                if (dataProperty.GetValue(provider) is IDictionary<string, string> dictionary)
                {
                    includeList.AddRange(
                        dictionary.Keys.Select(key => key.ReplaceSeparator(separator)));
                }
            }
        }

        return new HashSet<string>(includeList);
    }
}
