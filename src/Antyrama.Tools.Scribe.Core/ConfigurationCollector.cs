using System.Collections.Generic;
using Antyrama.Tools.Scribe.Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace Antyrama.Tools.Scribe.Core;

internal class ConfigurationCollector
{
    private readonly IConfiguration _configuration;

    public ConfigurationCollector(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IReadOnlyList<KeyValuePair<string, string>> Collect(string pathSeparator)
    {
        var settings = new List<KeyValuePair<string, string>>();

        foreach (var section in _configuration.GetChildren())
        {
            Collect(section, settings, pathSeparator);
        }

        return settings;
    }

    private void Collect(IConfigurationSection section, ICollection<KeyValuePair<string, string>> settings,
        string pathSeparator)
    {
        if (section == null || string.IsNullOrEmpty(section.Key))
        {
            return;
        }

        var hasChildren = false;

        foreach (var childSection in section.GetChildren())
        {
            hasChildren = true;

            Collect(childSection, settings, pathSeparator);
        }

        if (!string.IsNullOrEmpty(section.Value) || !hasChildren)
        {
            settings.Add(new KeyValuePair<string, string>(section.Path.ReplaceSeparator(pathSeparator),
                section.Value));
        }
    }
}
