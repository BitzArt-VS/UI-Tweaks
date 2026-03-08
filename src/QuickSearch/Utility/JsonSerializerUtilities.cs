using System.Text.Json;

namespace BitzArt.QuickSearch;

internal static class JsonSerializerUtilities
{
    // TODO: Remove, debug only
    public static JsonSerializerOptions LoggerOptions { get; } = new JsonSerializerOptions
    {
        WriteIndented = false,
        IncludeFields = true
    };
}
