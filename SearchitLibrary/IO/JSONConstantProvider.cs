// JsonConstantProvider.cs

using System.Text.Json;
using SearchitLibrary.Abstractions;

namespace SearchitLibrary.IO;

public class JsonConstantProvider(string filePath) : IConstantProvider
{
    public Constants Get()
    {
        if (!File.Exists(filePath))
            return new Constants();

        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Constants>(json) ?? new Constants();
        }
        catch
        {
            return new Constants();
        }
    }

    public void Save(Constants constants)
    {
        var json = JsonSerializer.Serialize(constants, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}