using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Antyrama.Tools.Scribe.Core.Extensions;

internal static class StringExtensions
{
    public static string ReplaceSeparator(this string key, string replacement)
    {
        return key.Replace(":", replacement);
    }

    public static string BeautifyJson(this string json)
    {
        var matches = _matchQuotes.Matches(json).Reverse().ToArray();
        if (matches.Length > 0)
        {
            var builder = new StringBuilder(json);
            foreach (var match in matches)
            {
                if (!match.Success || builder.Length <= match.Index)
                {
                    continue;
                }

                var index = match.Index + match.Length;
                var @char = builder[index];
                if (match.Success && @char is ',' or ':')
                {
                    builder.Insert(index + 1, ' ');
                }
            }
            return builder.ToString();
        }

        return json;
    }

    private static readonly Regex _matchQuotes = new("((?<![\\\\])['\"])((?:.(?!(?<![\\\\])\\1))*.?)\\1", RegexOptions.Compiled);
}
