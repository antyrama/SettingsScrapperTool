using Microsoft.Extensions.Configuration.AppService.Core.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Extensions.Configuration.AppService.Core;

public class AppServiceConfigurationGenerator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ToolInternalOptions _options;

    public AppServiceConfigurationGenerator(IServiceProvider serviceProvider,
        ToolInternalOptions options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    public void Generate()
    {
        var configuration = (IConfiguration)_serviceProvider.GetService(typeof(IConfiguration));

        var includeKeys = new IncludeKeysGenerator(configuration)
            .Generate(_options.Providers, _options.IncludeKeys, _options.PathSeparator);

        var allSettings = new ConfigurationCollector(configuration)
            .Collect(_options.PathSeparator);

        var desiredSettings = allSettings
            .IntersectBy(includeKeys, setting => setting.Key)
            .ExceptBy(_options.ExcludeKeys, setting => setting.Key)
            .ToDictionary(s => s.Key, s => s.Value);

        var repository = _options.WrapInYaml
            ? (ConfigurationRepository)new YamlConfigurationRepository(_options.YamlVariableName)
            : new JsonConfigurationRepository();

        foreach (var filename in GetConfigurationFiles(_options))
        {
            var currentSettings = Load(repository, filename);

            var newSettings = MatchSettings(desiredSettings, currentSettings);

            Save(repository, filename, newSettings);
        }
    }

    private IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> Load(IConfigurationRepository repository, string filename)
    {
        try
        {
            using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            return repository.Load(stream).ToDictionary(s => s["name"].ToString(), s => s);
        }
        catch (FileNotFoundException)
        {
            return new Dictionary<string, IReadOnlyDictionary<string, object>>();
        }
    }

    private void Save(IConfigurationRepository repository, string filename, IEnumerable<IReadOnlyDictionary<string, object>> settings)
    {
        using var stream = new FileStream(filename, FileMode.Create, FileAccess.Write);

        repository.Save(stream, settings);
    }

    private static IEnumerable<IReadOnlyDictionary<string, object>> MatchSettings(IReadOnlyDictionary<string, string> desiredSettings,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> currentSettings)
    {
        var newSettings = new List<IReadOnlyDictionary<string, object>>(desiredSettings.Count);

        foreach (var setting in desiredSettings)
        {
            newSettings.Add(currentSettings.ContainsKey(setting.Key)
                ? currentSettings[setting.Key]
                : CreateNewSetting(setting));
        }

        return newSettings;
    }

    private static Dictionary<string, object> CreateNewSetting(KeyValuePair<string, string> setting)
    {
        return new Dictionary<string, object>
            {
                { "name", setting.Key },
                { "value", setting.Value },
                { "slotSetting", false },
            };
    }

    private IEnumerable<string> GetConfigurationFiles(ToolInternalOptions options)
    {
        var filepath = Path.Combine(Directory.GetCurrentDirectory(), options.FilePathTemplate);
        Console.WriteLine("Resolving configuration file name for: " + _options.FilePathTemplate);

        if (!options.Environments.Any())
        {
            Console.WriteLine("File path is: " + filepath);

            return new[] { filepath };
        }

        if (options.FilePathTemplate.Contains("{0}"))
        {
            return options.Environments.Select(environment =>
            {
                var filename = string.Format(_options.FilePathTemplate, environment);

                Console.WriteLine("File path is: " + filename);

                return filename;
            });
        }

        throw new InvalidOperationException("File path template must contain '{0}' as environment placeholder when environments specified.");

    }
}
