using System;
using System.IO;
using System.Text.Json;

namespace DotNetANPR.Config
{
    public class ConfigurationService
    {
        public AppSettings Settings { get; private set; }

        public ConfigurationService(string configPath)
        {
            try
            {
                var jsonString = File.ReadAllText(configPath);
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
            catch (Exception ex)
            {
                throw new FileNotFoundException($"Error loading configuration file '{configPath}'. {ex.Message}", ex);
            }
        }
    }
}