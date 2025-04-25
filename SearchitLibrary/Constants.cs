using System.Text.Json;

namespace SearchitLibrary;

public class Constants
{
    // Default values
    public const int DefaultWindowWidth = 800;
    public const int DefaultWindowHeight = 600;
    public const string DefaultGameTitle = "SearchitBreakher";
    public const int DefaultFontSize = 12;
    
    // Properties
    public int WindowWidth { get; set; } = DefaultWindowWidth;
    public int WindowHeight { get; set; } = DefaultWindowHeight;
    public string GameTitle { get; set; } = DefaultGameTitle;
    public int FontSize { get; set; } = DefaultFontSize;
    
    // Create a default instance
    private static readonly Lazy<Constants> _default = new(() => new Constants());
    
    // Provide access to default instance
    public static Constants Default => _default.Value;
    
    // JSON serialization and deserialization
    public void SerializeToJson(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(this, options);
        File.WriteAllText(filePath, jsonString);
    }
    
    public static Constants DeserializeFromJson(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return Default;
        }
        
        string jsonString = File.ReadAllText(filePath);
        try
        {
            var constants = JsonSerializer.Deserialize<Constants>(jsonString);
            return constants ?? Default;
        }
        catch (JsonException)
        {
            return Default;
        }
    }
}