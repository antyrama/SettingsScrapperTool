using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antyrama.Tools.Scribe.Core.Extensions;
using System.Text.Json;

namespace Antyrama.Tools.Scribe.Core.Repository;

internal interface IConfigurationRepository
{
    IReadOnlyDictionary<string, object>[] Load(Stream stream);
    void Save(Stream stream, IEnumerable<IReadOnlyDictionary<string, object>> settings);
}

internal abstract class ConfigurationRepository : IConfigurationRepository
{
    protected readonly string Eol;

    protected ConfigurationRepository(ToolInternalOptions options)
    {
        Eol = ResolveEndOfLine(options);
    }

    public abstract IReadOnlyDictionary<string, object>[] Load(Stream stream);

    public abstract void Save(Stream stream, IEnumerable<IReadOnlyDictionary<string, object>> settings);

    protected string Serialize(IEnumerable<IReadOnlyDictionary<string, object>> settings)
    {
        var serialized = settings.Select(setting => $"  {JsonSerializer.Serialize(setting)}");

        var separator = $",{Eol}";
        var formatted = string.Join(separator, serialized)
            .BeautifyJson();

        return $"[{Eol}{formatted}{Eol}]";
    }

    protected static IReadOnlyDictionary<string, object>[] Deserialize(string settings)
    {
        try
        {
            var deserialized = JsonSerializer.Deserialize<IReadOnlyDictionary<string, object>[]>(settings);

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
