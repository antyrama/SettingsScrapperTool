using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Antyrama.Tools.Scribe.Core.Repository;

internal class HandlingListRepository
{
    private readonly ToolInternalOptions _options;

    public HandlingListRepository(ToolInternalOptions options)
    {
        _options = options;
    }

    public void PopulateState(IDictionary<string, bool> handlingList)
    {
        var filepath = Path.Combine(Directory.GetCurrentDirectory(), _options.DesiredSettingsFilename);

        if (!File.Exists(filepath))
        {
            return;
        }

        var lines = File.ReadAllLines(filepath);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith(IndicatorChar))
            {
                var key = trimmedLine[1..].Trim();
                handlingList[key] = false;
            }
            else
            {
                var key = trimmedLine[1..].Trim();
                handlingList[key] = true;
            }
        }
    }

    public void SaveState(IDictionary<string, bool> handlingList)
    {
        var filepath = Path.Combine(Directory.GetCurrentDirectory(), _options.DesiredSettingsFilename);

        File.WriteAllLines(filepath, handlingList.Select(kv => kv.Value ? kv.Key : IndicatorChar + kv.Key));
    }

    private const char IndicatorChar = '#';
}
