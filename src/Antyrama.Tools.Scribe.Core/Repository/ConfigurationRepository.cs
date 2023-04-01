using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

    protected string Serialize(IEnumerable<IReadOnlyDictionary<string, object>> settings)
    {
        var serialized = settings.Select(setting =>
            BeautifyJson($"  {JsonConvert.SerializeObject(setting)}"));

        var separator = $",{Environment.NewLine}";
        var formatted = string.Join(separator, serialized);

        return $"[{Environment.NewLine}{formatted}{Environment.NewLine}]";
    }

    protected IReadOnlyDictionary<string, object>[] Deserialize(string settings)
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

    private static string BeautifyJson(string s)
    {
        var builder = new StringBuilder(s, s.Length * 2);

        builder.Replace("\":", "\": ");
        builder.Replace("\",", "\", ");

        return builder.ToString();
    }
}