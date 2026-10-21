using GWGUI.Emulation;
using System.Buffers.Binary;
using System.IO;
using System.Reflection;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class StateFingerprintContractTests
{
    private static readonly string[] Families = ["Apple", "Atari", "Bandai", "Coleco", "Epoch", "Fairchild", "Mattel", "Panasonic", "Philips", "Sharp", "Sinclair", "Snk", "Thomson", "Watara", "Wenquxing"];
    private static readonly string[] ControllerFamilies = ["Apple", "Atari", "Bandai", "Coleco", "Epoch", "Fairchild", "Mattel", "Panasonic", "Philips", "Sharp", "Sinclair", "Snk", "Thomson", "Watara"];

    public static IEnumerable<object[]> NativeCases() => Families.Select(family => new object[] { family });
    public static IEnumerable<object[]> ControllerCases() => ControllerFamilies.Select(family => new object[] { family });

    [Theory]
    [MemberData(nameof(NativeCases))]
    public void NativeHeaderAndContainerKeepTheirJsonAndHashes(string family)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var prefix = "GWGUI.Emulation." + family + ".Common.Machines.Common.Enums.";
        var modelType = assembly.GetType(prefix + "MachineModel", throwOnError: true)!;
        var coreType = assembly.GetType(prefix + "Emulator", throwOnError: true)!;
        var model = Enum.GetValues(modelType).GetValue(0)!;
        var core = Enum.GetValues(coreType).GetValue(0)!;
        var payload = new byte[] { 1, 2, 255 };
        var hash = EmulationSdk.StateFiles.HashBytes(payload);
        var headerType = typeof(EmulationSavedStateHeader<,>).MakeGenericType(modelType, coreType);
        var header = headerType.GetConstructors().Single().Invoke(new object[] { 1, core, "core", "version", "core-sha", model, "config-sha", "content-sha", hash });
        var expected = new { FormatVersion = 1, Core = core, CoreName = "core", CoreVersion = "version", CoreSha256 = "core-sha", Model = model, ConfigurationSha256 = "config-sha", ContentSha256 = "content-sha", StateSha256 = hash };
        AssertSameJson(expected, header);
        typeof(StateFingerprintContractTests).GetMethod(nameof(ReadNativeHeader), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(modelType, coreType).Invoke(null, new[] { header, payload });
        var containerType = typeof(EmulationSavedStateFile<,>).MakeGenericType(modelType, coreType);
        var container = containerType.GetConstructors().Single().Invoke(new[] { header, payload });
        payload[0] = 9;
        AssertSameJson(new { Header = expected, State = payload }, container);
    }

    [Theory]
    [MemberData(nameof(ControllerCases))]
    public void ControllerFingerprintsKeepTheirOriginalFieldOrderTypesAndValues(string family)
    {
        var assembly = Assembly.Load("gwgui.emulation." + family.ToLowerInvariant());
        var peripheralType = assembly.GetType("GWGUI.Emulation." + family + ".Common.Machines.Common.Enums.PeripheralCategory", throwOnError: true)!;
        var peripheral = Enum.GetValues(peripheralType).GetValue(0)!;
        var mappings = new[] { new KeyValuePair<string, string>("Fire", "Keyboard:A") };
        var type = typeof(EmulationStateControllerFingerprint<>).MakeGenericType(peripheralType);
        var actual = type.GetConstructors().Single().Invoke(new object?[] { 2, peripheral, null, 23, mappings });
        AssertSameJson(new { Port = 2, Peripheral = peripheral, DeviceId = (string?)null, DeadZonePercent = 23, Mappings = mappings }, actual);
    }

    [Fact]
    public void TextIdentitiesUseTheSameCompleteHeaderAsEnumIdentities()
    {
        var actual = new EmulationSavedStateHeader<string, string>(1, "core", "core-name", "core-version",
            "core-sha", "model", "configuration-sha", "content-sha", "state-sha");
        AssertSameJson(new { FormatVersion = 1, Core = "core", CoreName = "core-name", CoreVersion = "core-version",
            CoreSha256 = "core-sha", Model = "model", ConfigurationSha256 = "configuration-sha",
            ContentSha256 = "content-sha", StateSha256 = "state-sha" }, actual);
        Assert.True(EmulationStateFileFunctions.IsHeaderValid(actual));
        Assert.False(EmulationStateFileFunctions.IsHeaderValid(actual with { CoreName = "" }));
        var incomplete = JsonSerializer.Deserialize<EmulationSavedStateHeader<string, string>>(
            "{\"FormatVersion\":1,\"Model\":\"model\",\"CoreSha256\":\"core-sha\",\"StateSha256\":\"state-sha\"}");
        Assert.False(EmulationStateFileFunctions.IsHeaderValid(incomplete!));
    }

    [Fact]
    public void ContentEntriesKeepTheirOriginalThreeFields()
        => AssertSameJson(new { Slot = "slot", Category = "media", Hash = "hash" }, new EmulationStateContentEntry("slot", "media", "hash"));

    [Fact]
    public void CanonicalFingerprintIgnoresKeyOrderButKeepsValuesAndArrayOrder()
    {
        var first = new Dictionary<string, object> { ["options"] = new Dictionary<string, string>
            { ["z"] = "last", ["a"] = "first" }, ["media"] = new[] { "one", "two" } };
        var reordered = new Dictionary<string, object> { ["media"] = new[] { "one", "two" },
            ["options"] = new Dictionary<string, string> { ["a"] = "first", ["z"] = "last" } };
        Assert.Equal(EmulationSdk.StateFiles.HashDocument(first), EmulationSdk.StateFiles.HashDocument(reordered));
        reordered["media"] = new[] { "two", "one" };
        Assert.NotEqual(EmulationSdk.StateFiles.HashDocument(first), EmulationSdk.StateFiles.HashDocument(reordered));
        reordered["media"] = new[] { "one", "two" };
        reordered["options"] = new Dictionary<string, string> { ["a"] = "changed", ["z"] = "last" };
        Assert.NotEqual(EmulationSdk.StateFiles.HashDocument(first), EmulationSdk.StateFiles.HashDocument(reordered));
    }

    [Fact]
    public void MissingMappingsStillProduceAnEmptySequence()
        => Assert.Empty(EmulationSdk.StateFiles.OrderInputMappings<string>(null));

    [Fact]
    public void MappingOrderIsOrdinalAndValuesAreNotConverted()
    {
        var values = new Dictionary<string, EmulationKey> { ["z"] = EmulationKey.A, ["a"] = EmulationKey.B, ["A"] = EmulationKey.C, ["é"] = EmulationKey.D };
        var sorted = EmulationSdk.StateFiles.OrderInputMappings(values).ToArray();
        Assert.Equal(new[] { "A", "a", "z", "é" }, sorted.Select(pair => pair.Key));
        Assert.Equal(new[] { EmulationKey.C, EmulationKey.B, EmulationKey.A, EmulationKey.D }, sorted.Select(pair => pair.Value));
    }

    [Fact]
    public void OrderingStaysDeferredAndTheCallerOwnsMaterialization()
    {
        var values = new Dictionary<string, string> { ["z"] = "last" };
        var ordered = EmulationSdk.StateFiles.OrderInputMappings(values);
        values.Add("A", "first");
        var snapshot = ordered.ToArray();
        values.Add("a", "later");
        Assert.Equal(new[] { "A", "z" }, snapshot.Select(pair => pair.Key));
        Assert.Equal(new[] { "A", "a", "z" }, ordered.Select(pair => pair.Key));
    }

    [Fact]
    public void ClosedPeripheralTypesKeepSeparateEqualityContracts()
    {
        var mappings = Array.Empty<KeyValuePair<string, string>>();
        var first = new EmulationStateControllerFingerprint<FirstPeripheral>(0, FirstPeripheral.None, null, 20, mappings);
        var same = new EmulationStateControllerFingerprint<FirstPeripheral>(0, FirstPeripheral.None, null, 20, mappings);
        var other = new EmulationStateControllerFingerprint<SecondPeripheral>(0, SecondPeripheral.None, null, 20, mappings);
        Assert.Equal(first, same);
        Assert.False(first.Equals((object)other));
    }

    private static void ReadNativeHeader<TModel, TCore>(object value, byte[] payload)
        where TModel : struct, Enum
        where TCore : struct, Enum
    {
        var header = (EmulationSavedStateHeader<TModel, TCore>)value;
        Assert.True(EmulationStateFileFunctions.IsHeaderValid(header));
        foreach (var invalid in new[]
        {
            header with { FormatVersion = 0 },
            header with { FormatVersion = -1 },
            header with { Core = Undefined<TCore>() },
            header with { Model = Undefined<TModel>() },
            header with { CoreName = null! },
            header with { CoreVersion = " " },
            header with { CoreSha256 = string.Empty },
            header with { ConfigurationSha256 = string.Empty },
            header with { ContentSha256 = " " },
            header with { StateSha256 = null! }
        })
            Assert.False(EmulationStateFileFunctions.IsHeaderValid(invalid));
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var encoded = JsonSerializer.SerializeToUtf8Bytes(header, options);
        byte[] magic = [1, 2, 3, 4];
        using var stream = new MemoryStream();
        stream.Write(magic);
        Span<byte> length = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(length, encoded.Length);
        stream.Write(length);
        stream.Write(encoded);
        stream.Write(payload);
        stream.Position = 0;
        var decoded = EmulationStateFileFunctions.Read<EmulationSavedStateHeader<TModel, TCore>>(
            stream, magic, 4096, options, static item => item.StateSha256, static () => new InvalidDataException(), EmulationStateFileFunctions.IsHeaderValid);
        Assert.Equal(header, decoded.Header);
        Assert.Equal(payload, decoded.State);
    }

    private static TEnum Undefined<TEnum>() where TEnum : struct, Enum =>
        Enumerable.Range(0, 1024).Select(value => (TEnum)Enum.ToObject(typeof(TEnum), value))
            .First(value => !Enum.IsDefined(value));
    private static void AssertSameJson(object expected, object actual)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var before = JsonSerializer.SerializeToUtf8Bytes(expected, expected.GetType(), options);
        var after = JsonSerializer.SerializeToUtf8Bytes(actual, actual.GetType(), options);
        Assert.Equal(before, after);
        Assert.Equal(EmulationSdk.StateFiles.HashBytes(before), EmulationSdk.StateFiles.HashBytes(after));
    }

    private enum FirstPeripheral { None }
    private enum SecondPeripheral { None }
}
