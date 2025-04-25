using System;
using System.IO;
using System.Text.Json;

namespace SearchitLibrary
{
    public class Constants
    {
        public float LookSpeed { get; set; } = 0.1f;
        public float MoveSpeed { get; set; } = 0.1f;

        public void SaveToFile(string filePath)
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
        }

        public static Constants LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new Constants();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var constants = JsonSerializer.Deserialize<Constants>(json);
                return constants ?? new Constants();
            }
            catch (Exception)
            {
                return new Constants();
            }
        }
    }
}