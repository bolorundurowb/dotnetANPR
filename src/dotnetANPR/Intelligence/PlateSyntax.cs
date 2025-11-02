using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotNetANPR.Intelligence;

public class PlateSyntax
{
    [JsonPropertyName("syntaxes")]
    public List<SyntaxDefinition> Syntaxes { get; set; }
}

public class SyntaxDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("patterns")]
    public List<string> Patterns { get; set; }

    // Internal cache
    [JsonIgnore]
    private List<HashSet<char>>? _allowedCharsCache;

    public bool IsCharAllowed(char c, int position)
    {
        if (position >= Patterns.Count) return false;

        // Build cache on first use
        if (_allowedCharsCache == null)
        {
            _allowedCharsCache = new List<HashSet<char>>();
            foreach (var pattern in Patterns)
            {
                var set = new HashSet<char>();
                foreach (var part in pattern.Split(','))
                {
                    if (part == "A-Z")
                        for (var l = 'A'; l <= 'Z'; l++) set.Add(l);
                    else if (part == "0-9")
                        for (var l = '0'; l <= '9'; l++) set.Add(l);
                    else
                        foreach (var pc in part) set.Add(pc);
                }
                _allowedCharsCache.Add(set);
            }
        }

        return _allowedCharsCache[position].Contains(c);
    }
}