using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DotNetANPR.Intelligence;

public class SyntaxDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("pattern")]
    public List<string> Patterns { get; set; }

    // Internal cache
    [JsonIgnore]
    private List<HashSet<char>>? _allowedCharsCache;

    public bool IsCharAllowed(char c, int position)
    {
        if (position >= Patterns.Count) 
            return false;

        // Build cache on first use
        if (_allowedCharsCache != null) 
            return _allowedCharsCache[position].Contains(c);
        
        _allowedCharsCache = [];
            
        // check if pattern supports position
        if (position >= Patterns.Count)
            return false;
            
        foreach (var pattern in Patterns) 
            _allowedCharsCache.Add([..pattern]);

        return _allowedCharsCache[position].Contains(c);
    }
}