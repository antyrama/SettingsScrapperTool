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
    private readonly string _eol;

    protected ConfigurationRepository(ToolInternalOptions options)
    {
        _eol = ResolveEndOfLine(options);
    }

    public abstract IReadOnlyDictionary<string, object>[] Load(Stream stream);

    public abstract void Save(Stream stream, IEnumerable<IReadOnlyDictionary<string, object>> settings);

    protected string Serialize(IEnumerable<IReadOnlyDictionary<string, object>> settings)
    {
        var serialized = settings.Select(setting => $"  {JsonConvert.SerializeObject(setting)}");

        var separator = $",{_eol}";
        var formatted = string.Join(separator, serialized)
            .BeautifyJson();

        return $"[{_eol}{formatted}{_eol}]";
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

    private static string ResolveEndOfLine(ToolInternalOptions options)
    {
        return options.Eol switch
        {
            EndOfLine.Cr => Cr,
            EndOfLine.CrLf => CrLf,
            EndOfLine.Lf => Lf,
            null => Environment.NewLine,
            _ => throw new ArgumentOutOfRangeException(nameof(options))
        };
    }

    private const string Cr = "\r";
    private const string CrLf = "\r\n";
    private const string Lf = "\n";
}
