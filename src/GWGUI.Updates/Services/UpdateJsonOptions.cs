using System.Text.Json;
using System.Text.Json.Serialization;

namespace GWGUI.Updates.Services;

public static class UpdateJsonOptions
{
    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
