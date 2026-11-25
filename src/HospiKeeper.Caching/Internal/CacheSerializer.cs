using System.Text.Json;
using System.Text.Json.Serialization;

namespace HospiKeeper.Caching.Internal;

internal static class CacheSerializer
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public static byte[] Serialize<T>(T value) where T : class
        => JsonSerializer.SerializeToUtf8Bytes(value, Options);

    public static T? Deserialize<T>(byte[] data) where T : class
        => JsonSerializer.Deserialize<T>(data, Options);

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
