using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using System.Text.Json;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class LightGunInputTests
{
    [Theory]
    [InlineData("swanstation", PeripheralCategory.GunCon)]
    [InlineData("mednafen_psx", PeripheralCategory.GunCon)]
    [InlineData("mednafen_psx", PeripheralCategory.Justifier)]
    [InlineData("mednafen_psx_hw", PeripheralCategory.GunCon)]
    [InlineData("mednafen_psx_hw", PeripheralCategory.Justifier)]
    [InlineData("pcsx_rearmed", PeripheralCategory.GunCon)]
    [InlineData("pcsx_rearmed", PeripheralCategory.Justifier)]
    public void ReloadCanBeConfiguredAndPersistedIndependentlyForBothPorts(string emulator, PeripheralCategory type)
    {
        var configuration = new MachineConfiguration("PlayStation", emulator, Controllers: [type, type]);
        var settings = InputSettingsFunctions.Describe(configuration);
        Assert.Equal(2, settings.ControllerPorts.Count);
        foreach (var port in settings.ControllerPorts)
        {
            var definition = Assert.Single(port.Bindings.Definitions, item => item.Id == EmulationControllerCommandIds.Reload);
            Assert.Equal("Emulation.Controller.Action.Reload", definition.DisplayResourceKey);
            Assert.Empty(definition.DefaultBinding);
            Assert.Empty(port.Bindings.Values);
        }
        settings = settings with
        {
            ControllerPorts = settings.ControllerPorts.Select(port => port with
            {
                Bindings = port.Bindings with
                {
                    Values = new Dictionary<string, string>
                    {
                        [EmulationControllerCommandIds.Reload] = port.Number == 1 ? "Mouse:XButton1" : "Keyboard:S"
                    }
                }
            }).ToArray()
        };
        var configured = InputSettingsFunctions.Apply(configuration, settings);
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(configured))!;
        var physical = EmulationInputSnapshot.Empty with { Pointer = EmulationInputSnapshot.Empty.Pointer with { ExtendedButton1 = true } };
        var mapped = InputSnapshotFunctions.Apply(physical, restored.Input, false, 2);
        Assert.True(ControllerInputFunctions.LightGunButton(mapped.Controllers[0], LightGunInput.Reload));
        Assert.False(ControllerInputFunctions.LightGunButton(mapped.Controllers[1], LightGunInput.Reload));
        physical = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.S } };
        mapped = InputSnapshotFunctions.Apply(physical, restored.Input, false, 2);
        Assert.False(ControllerInputFunctions.LightGunButton(mapped.Controllers[0], LightGunInput.Reload));
        Assert.True(ControllerInputFunctions.LightGunButton(mapped.Controllers[1], LightGunInput.Reload));
        var released = InputSnapshotFunctions.Apply(EmulationInputSnapshot.Empty, restored.Input, false, 2);
        Assert.All(released.Controllers, controller => Assert.False(ControllerInputFunctions.LightGunButton(controller, LightGunInput.Reload)));
    }

    [Fact]
    public void UnassignedMouseAndGamepadButtonsCannotFireTheGun()
    {
        var physical = EmulationInputSnapshot.Empty with
        {
            Pointer = new EmulationPointerState(10, 20, 1, true, true, true, true, true),
            Controllers = [EmulationControllerState.Empty with { Buttons = ushort.MaxValue }]
        };
        var mapped = InputSnapshotFunctions.Apply(physical, null, false, 2);
        Assert.True(mapped.Pointer.Left && mapped.Pointer.Right && mapped.Pointer.Middle && mapped.Pointer.ExtendedButton1);
        foreach (var port in mapped.Controllers)
            foreach (var input in Enum.GetValues<LightGunInput>())
                Assert.False(ControllerInputFunctions.LightGunButton(port, input));
    }

    [Fact]
    public void ExplicitMouseAndKeyboardBindingsReachTheNativeGunCommands()
    {
        var configuration = new InputConfiguration(ControllerBindings:
        [
            new(0, PeripheralCategory.GunCon, ButtonMappings: new Dictionary<string, string>
            {
                [EmulationControllerCommandIds.B] = "Mouse:Left",
                [EmulationControllerCommandIds.A] = "Mouse:Right",
                [EmulationControllerCommandIds.X] = "Mouse:Middle",
                [EmulationControllerCommandIds.Y] = "Keyboard:A",
                [EmulationControllerCommandIds.Reload] = "Mouse:XButton1"
            })
        ]);
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.A },
            Pointer = new EmulationPointerState(0, 0, 0, true, true, true, true)
        };
        var mapped = InputSnapshotFunctions.Apply(physical, configuration, false, 2);
        foreach (var input in new[] { LightGunInput.Trigger, LightGunInput.AuxiliaryA, LightGunInput.AuxiliaryB, LightGunInput.AuxiliaryC, LightGunInput.Reload })
        {
            Assert.True(ControllerInputFunctions.LightGunButton(mapped.Controllers[0], input));
            Assert.False(ControllerInputFunctions.LightGunButton(mapped.Controllers[1], input));
        }
        var released = InputSnapshotFunctions.Apply(EmulationInputSnapshot.Empty, configuration, false, 2);
        Assert.All(Enum.GetValues<LightGunInput>(), input => Assert.False(ControllerInputFunctions.LightGunButton(released.Controllers[0], input)));
    }

    [Theory]
    [InlineData((uint)LightGunInput.Start, EmulationControllerCommandIds.Start)]
    [InlineData((uint)LightGunInput.Pause, EmulationControllerCommandIds.Start)]
    [InlineData((uint)LightGunInput.Select, EmulationControllerCommandIds.Select)]
    [InlineData((uint)LightGunInput.Up, EmulationControllerCommandIds.Up)]
    [InlineData((uint)LightGunInput.Down, EmulationControllerCommandIds.Down)]
    [InlineData((uint)LightGunInput.Left, EmulationControllerCommandIds.Left)]
    [InlineData((uint)LightGunInput.Right, EmulationControllerCommandIds.Right)]
    public void EachPortKeepsItsOwnExplicitCommand(uint nativeId, string command)
    {
        var configuration = new InputConfiguration(ControllerBindings:
        [
            new(0, PeripheralCategory.GunCon, ButtonMappings: new Dictionary<string, string> { [command] = "Keyboard:A" }),
            new(1, PeripheralCategory.GunCon, ButtonMappings: new Dictionary<string, string> { [command] = "Keyboard:S" })
        ]);
        var physical = EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A } };
        var mapped = InputSnapshotFunctions.Apply(physical, configuration, false, 2);
        Assert.True(ControllerInputFunctions.LightGunButton(mapped.Controllers[0], (LightGunInput)nativeId));
        Assert.False(ControllerInputFunctions.LightGunButton(mapped.Controllers[1], (LightGunInput)nativeId));
    }
}
