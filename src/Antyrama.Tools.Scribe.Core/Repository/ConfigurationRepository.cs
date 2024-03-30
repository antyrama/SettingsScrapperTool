using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antyrama.Tools.Scribe.Core.Extensions;
using Newtonsoft.Json;

namespace Antyrama.Tools.Scribe.Core.Repository;

internal interface IConfigurationRepository
{
    IReadOnlyDictionary<string, object>[] Load(Stream stream);
    void Save(Stream stream, IEnumerable<IReadOnlyDictionary<string, object>> settings);
}

internal abstract class ConfigurationRepository : IConfigurationRepository
{
    public abstract IReadOnlyDictionary<string, object>[] Load(Stream stream);

    public abstract void Save(Stream stream, IEnumerable<IReadOnlyDictionary<string, object>> settings);

    protected static string Serialize(IEnumerable<IReadOnlyDictionary<string, object>> settings)
    {
        var serialized = settings.Select(setting => $"  {JsonConvert.SerializeObject(setting)}");

        var separator = $",{Environment.NewLine}";
        var formatted = string.Join(separator, serialized)
            .BeautifyJson();

        return $"[{Environment.NewLine}{formatted}{Environment.NewLine}]";
    }

    protected static IReadOnlyDictionary<string, object>[] Deserialize(string settings)
    {
        try
        {
            var deserialized = JsonConvert.DeserializeObject<IReadOnlyDictionary<string, object>[]>(settings);

            return deserialized ?? Array.Empty<IReadOnlyDictionary<string, object>>();
        }
        catch (JsonException)
        {
            return Array.Empty<IReadOnlyDictionary<string, object>>();
        }
    }
}
