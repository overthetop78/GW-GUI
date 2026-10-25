using System.Reflection;
using System.Text.Json;

namespace GWGUI.Tests.Emulation;

public sealed class ConfigurationDocumentContractTests
{
    private static readonly string[] Families =
        ["Acorn", "Apple", "Atari", "Bandai", "Coleco", "Epoch", "Fairchild",
         "Mattel", "Panasonic", "Philips", "Sharp", "Sinclair", "Snk", "Wenquxing"];

    public static IEnumerable<object[]> SerializationCases() =>
        Families.SelectMany(family => new[] { false, true }.Select(indented => new object[] { family, indented }));

    public static IEnumerable<object[]> ModuleCases() =>
        Families.Select(family => new object[] { family });

    [Theory]
    [MemberData(nameof(SerializationCases))]
    public void ClosedDocumentsKeepOriginalJsonOrderAndRoundTrip(string family, bool indented)
    {
        var type = DocumentType(family);
        var constructor = Assert.Single(type.GetConstructors());
        var parameters = constructor.GetParameters();
        Assert.Equal(OriginalNames(family), parameters.Select(parameter => parameter.Name));
        var arguments = parameters.Select(parameter => Value(parameter.Name!, parameter.ParameterType)).ToArray();
        var expected = new Dictionary<string, object?>();
        for (var index = 0; index < parameters.Length; index++)
            expected.Add(JsonNamingPolicy.CamelCase.ConvertName(parameters[index].Name!), arguments[index]);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = indented };
        var document = constructor.Invoke(arguments);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(document, type, options);

        Assert.Equal(JsonSerializer.SerializeToUtf8Bytes(expected, options), bytes);
        var restored = JsonSerializer.Deserialize(bytes, type, options);
        Assert.NotNull(restored);
        Assert.Equal(bytes, JsonSerializer.SerializeToUtf8Bytes(restored, type, options));
        for (var index = 0; index < parameters.Length; index++)
            Assert.Equal(arguments[index], type.GetProperty(parameters[index].Name!)!.GetValue(document));

        var initialContent = parameters.SingleOrDefault(parameter => parameter.Name == "InitialContent");
        if (initialContent is not null)
        {
            Assert.True(initialContent.IsOptional);
            Assert.Null(initialContent.DefaultValue);
        }
    }

    [Theory]
    [MemberData(nameof(ModuleCases))]
    public void MissingPropertiesKeepOriginalPrimaryConstructorDefaults(string family)
    {
        var type = DocumentType(family);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var document = JsonSerializer.Deserialize("{}", type, options);
        Assert.NotNull(document);
        var expected = new Dictionary<string, object?>();
        foreach (var parameter in Assert.Single(type.GetConstructors()).GetParameters())
        {
            var value = parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null;
            Assert.Equal(value, type.GetProperty(parameter.Name!)!.GetValue(document));
            expected.Add(JsonNamingPolicy.CamelCase.ConvertName(parameter.Name!), value);
        }
        Assert.Equal(JsonSerializer.SerializeToUtf8Bytes(expected, options),
            JsonSerializer.SerializeToUtf8Bytes(document, type, options));
    }

    private static Type DocumentType(string family)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var functions = assembly.GetType("GWGUI.Emulation." + family
            + ".Common.Machines.Common.Functions.ConfigurationStoreFunctions", throwOnError: true)!;
        return functions.GetMethod("ToDocument", BindingFlags.NonPublic | BindingFlags.Static)!.ReturnType;
    }

    private static string[] OriginalNames(string family) => (family switch
    {
        "Acorn" or "Apple" or "Atari" or "Bandai" =>
            "SchemaVersion|Id|Model|Core|Firmwares|Media|Options|Input|Folders|AudioEnabled",
        "Coleco" or "Epoch" or "Fairchild" or "Mattel" or "Philips" or "Sharp" =>
            "SchemaVersion|Id|Model|Core|Firmwares|Media|Options|Input|Folders|AudioEnabled|ConfiguredSlots",
        "Sinclair" or "Snk" =>
            "SchemaVersion|Id|Model|Core|Firmwares|Media|Options|Input|Folders|AudioEnabled|ConfiguredSlots|InitialContent",
        "Panasonic" or "Wenquxing" =>
            "SchemaVersion|Id|Model|Core|Firmware|Media|Options|Input|AudioEnabled|ConfiguredSlots",
        _ => throw new InvalidOperationException("Unexpected module: " + family)
    }).Split('|');

    private static object? Value(string name, Type type) => name switch
    {
        "SchemaVersion" => 2,
        "Id" => Guid.Parse("a847a43f-cc08-40b7-b62f-a4f71f15f799"),
        "Model" or "Core" => Enum.GetValues(type).GetValue(0),
        "Firmwares" or "Media" or "ConfiguredSlots" => Array.CreateInstance(type.GenericTypeArguments[0], 0),
        "Options" => new Dictionary<string, string> { ["option"] = "valeur" },
        "Input" or "Folders" or "Firmware" => null,
        "AudioEnabled" => true,
        "InitialContent" => "boot",
        _ => throw new InvalidOperationException("Unexpected original property: " + name)
    };
}
