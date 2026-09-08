using System.Text.Json.Nodes;
using GWGUI.App.Services.Emulation;

namespace GWGUI.Tests.Emulation.Modules;

public sealed class EmulationModuleManifestTests
{
    private const string Valid = """
        {"schemaVersion":1,"id":"amiga","entryAssembly":"gwgui.emulation.amiga.dll",
         "moduleVersion":"1.0.0","hostApiMinimum":"1.0","hostApiMaximum":"1.0"}
        """;

    [Fact]
    public void CurrentManifestIsAccepted()
    {
        var manifest = EmulationModuleManifestReader.Parse(Valid);
        Assert.Equal("amiga", manifest.Id);
        Assert.Equal("1.0.0", manifest.ModuleVersion);
    }

    [Theory]
    [InlineData("schemaVersion")]
    [InlineData("id")]
    [InlineData("entryAssembly")]
    [InlineData("moduleVersion")]
    [InlineData("hostApiMinimum")]
    [InlineData("hostApiMaximum")]
    public void MissingRequiredFieldIsRejected(string field)
    {
        var json = JsonNode.Parse(Valid)!.AsObject();
        json.Remove(field);
        Assert.Throws<InvalidDataException>(() => EmulationModuleManifestReader.Parse(json.ToJsonString()));
    }

    [Theory]
    [InlineData("id", "../amiga")]
    [InlineData("id", "..")]
    [InlineData("id", "C:\\amiga")]
    [InlineData("id", "CON")]
    [InlineData("id", "amiga ")]
    [InlineData("id", "")]
    [InlineData("entryAssembly", "../outside.dll")]
    [InlineData("entryAssembly", "folder\\module.dll")]
    [InlineData("entryAssembly", "module.exe")]
    [InlineData("entryAssembly", "NUL.dll")]
    [InlineData("moduleVersion", "1.0")]
    [InlineData("moduleVersion", "1.0.0-preview")]
    [InlineData("hostApiMinimum", "1.x")]
    [InlineData("hostApiMaximum", "1.0.0")]
    public void InvalidFieldIsRejected(string field, string value)
    {
        var json = JsonNode.Parse(Valid)!;
        json[field] = value;
        Assert.Throws<InvalidDataException>(() => EmulationModuleManifestReader.Parse(json.ToJsonString()));
    }

    [Theory]
    [InlineData("1.9", "1.10", "1.9", true)]
    [InlineData("1.9", "1.10", "1.10", true)]
    [InlineData("1.9", "1.10", "1.8", false)]
    [InlineData("1.9", "1.10", "1.11", false)]
    [InlineData("1.10", "1.9", "1.9", false)]
    [InlineData("1.0", "1.0", "0.9", false)]
    [InlineData("1.0", "1.0", "1.1", false)]
    public void CompatibilityUsesNumericInclusiveBounds(string minimum, string maximum, string current, bool accepted)
    {
        var json = JsonNode.Parse(Valid)!;
        json["hostApiMinimum"] = minimum;
        json["hostApiMaximum"] = maximum;
        if (accepted)
            Assert.NotNull(EmulationModuleManifestReader.Parse(json.ToJsonString(), Version.Parse(current)));
        else
            Assert.Throws<InvalidDataException>(() => EmulationModuleManifestReader.Parse(json.ToJsonString(), Version.Parse(current)));
    }

    [Fact]
    public void UnknownSchemaAndDuplicateFieldsAreRejected()
    {
        Assert.Throws<InvalidDataException>(() => EmulationModuleManifestReader.Parse(Valid.Replace("\"schemaVersion\":1", "\"schemaVersion\":2")));
        Assert.Throws<InvalidDataException>(() => EmulationModuleManifestReader.Parse(Valid.Replace("\"id\":\"amiga\"", "\"id\":\"amiga\",\"Id\":\"atari\"")));
        Assert.ThrowsAny<System.Text.Json.JsonException>(() => EmulationModuleManifestReader.Parse("{"));
        Assert.Throws<InvalidDataException>(() => EmulationModuleManifestReader.Parse("[]"));
    }
}
