using System.Text.Json;
// ReSharper disable CheckNamespace

namespace WebAPI
{
    public static class Utility
    {
        public static bool IsEmpty(string variable)
        {
            return (variable.Length == 0);
        }

        public static bool IsNull<T>(T variable)
        {
            return (variable == null);
        }

        public static void UpdateDataFile<T>(string filePath, T data) where T : class
        {
            var option = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(data, option));
        }
    }
}