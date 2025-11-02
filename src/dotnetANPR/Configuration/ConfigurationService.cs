using System;
using System.IO;
using System.Text.Json;

namespace DotNetANPR.Configuration;

public class ConfigurationService
{
    public AppSettings Settings { get; private set; }
        
    public ConfigurationService(string configPath = "config.json")
    {
        try
        {
            LoadConfiguration(configPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Couldn't load configuration file {configPath}. {ex.Message}");
            // In a real app, you might exit or throw
            throw;
        }
    }

    private void LoadConfiguration(string path)
    {
        var jsonString = File.ReadAllText(path);
            
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        Settings = JsonSerializer.Deserialize<AppSettings>(jsonString, options);
            
        if (Settings == null)
        {
            throw new InvalidDataException("Failed to deserialize config.json.");
        }
    }

    // Helper to mimic original GetPathProperty
    public string GetPathProperty(string path)
    {
        return path.Replace('/', Path.DirectorySeparatorChar);
    }
}