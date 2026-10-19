using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class ControllerBindingProjectionTests
{
    private enum Peripheral { None, Joystick }

    [Fact]
    public void ProjectionKeepsPortOrderMetadataAndIndependentOrdinalMappings()
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Fire"] = "Keyboard:A", ["fire"] = "", [""] = "Keyboard:B"
        };
        var ports = new[] { Port(2, "Joystick", values), Port(1, "None", values) };
        var visited = new List<int>();
        var result = EmulationInputMappingFunctions.CreateControllerBindings<Peripheral, EmulationControllerPort>(
            new(null, null, ports),
            (index, _) => { visited.Add(index); return true; },
            static (port, peripheral, mappings) => port with
            {
                SelectedControllerId = peripheral.ToString(),
                Bindings = port.Bindings with { Values = mappings }
            });

        Assert.Equal(new[] { 1, 0 }, visited);
        Assert.Equal(new[] { 2, 1 }, result.Select(port => port.Number));
        for (var index = 0; index < ports.Length; index++)
        {
            Assert.Equal(ports[index].PhysicalDeviceId, result[index].PhysicalDeviceId);
            Assert.Equal(ports[index].DeadZonePercent, result[index].DeadZonePercent);
            Assert.Equal(ports[index].VisualId, result[index].VisualId);
            Assert.NotSame(values, result[index].Bindings.Values);
            Assert.Equal(3, result[index].Bindings.Values.Count);
            Assert.Equal("Keyboard:A", result[index].Bindings.Values["Fire"]);
            Assert.Equal("", result[index].Bindings.Values["fire"]);
            Assert.Equal("Keyboard:B", result[index].Bindings.Values[""]);
            Assert.False(result[index].Bindings.Values.ContainsKey("FIRE"));
        }
        Assert.NotSame(result[0].Bindings.Values, result[1].Bindings.Values);
        values["Fire"] = "Keyboard:C";
        Assert.All(result, port => Assert.Equal("Keyboard:A", port.Bindings.Values["Fire"]));
    }

    [Theory]
    [InlineData("missing", 0)]
    [InlineData("joystick", 0)]
    [InlineData("2", 1)]
    public void InvalidOrUnsupportedPeripheralStopsBeforeFactory(string selected, int expectedCompatibilityCalls)
    {
        var compatibilityCalls = 0;
        var factoryCalls = 0;
        var error = Assert.Throws<ArgumentException>(() =>
            EmulationInputMappingFunctions.CreateControllerBindings<Peripheral, int>(
                new(null, null, [Port(1, selected)]),
                (_, _) => { compatibilityCalls++; return false; },
                (_, _, _) => { factoryCalls++; return 0; }));

        Assert.Null(error.ParamName);
        Assert.Equal("settings", error.Message);
        Assert.Equal(expectedCompatibilityCalls, compatibilityCalls);
        Assert.Equal(0, factoryCalls);
    }

    [Fact]
    public void LaterRefusalPreservesEarlierProjectionOrder()
    {
        var visited = new List<int>();
        var created = new List<int>();
        Assert.Throws<ArgumentException>(() =>
            EmulationInputMappingFunctions.CreateControllerBindings<Peripheral, int>(
                new(null, null, [Port(3, "Joystick"), Port(1, "Joystick"), Port(2, "Joystick")]),
                (index, _) => { visited.Add(index); return index != 0; },
                (port, _, _) => { created.Add(port.Number); return port.Number; }));
        Assert.Equal(new[] { 2, 0 }, visited);
        Assert.Equal(new[] { 3 }, created);
    }

    [Fact]
    public void EmptyPortsDoNotInvokeCallbacks()
    {
        var result = EmulationInputMappingFunctions.CreateControllerBindings<Peripheral, int>(
            new(null, null, []),
            (_, _) => throw new InvalidOperationException(),
            (_, _, _) => throw new InvalidOperationException());
        Assert.Empty(result);
    }

    private static EmulationControllerPort Port(int number, string selected,
        IReadOnlyDictionary<string, string>? values = null) =>
        new(number, [], selected, "physical-" + number,
            new([], values ?? new Dictionary<string, string>(), EmulationInputSource.Controller),
            17 + number, "visual-" + number);
}
