using GWGUI.Emulation;
using ControllerFunctions = GWGUI.Emulation.Sony.Common.Functions.ControllerFunctions;
using System.IO;
using System.Reflection;
using System.Text.Json;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Enums;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;
using Core = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.CoreConstants;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class GunCon2InputTests
{
    [Theory]
    [InlineData("disabled", false)]
    [InlineData("usb1", true)]
    [InlineData("usb2", true)]
    [InlineData("both", true)]
    public void CrosshairUsesTheExactNativeColoursAndIsAvailableOnlyWithAGun(string usb, bool enabled)
    {
        var factory = new GWGUI.Emulation.Sony.Emulators.Pcee2.Factories.Pcee2MachineFactory();
        var configuration = Configuration(usb);
        var field = Assert.Single(factory.DescribeSettings(configuration).SelectMany(block => block.Fields), field => field.Id == "pcsx2_lightgun_crosshair");
        Assert.Equal(EmulationMachineTab.Controllers, field.Tab);
        Assert.Equal(enabled, field.IsEnabled);
        Assert.Equal("disabled", field.Value);
        Assert.Equal("Emulation.Controller.CrosshairColor", field.LabelResourceKey);
        Assert.Equal(new[] { "disabled", "FFFFFF", "2020FF", "20FF20", "FFC020", "20E0FF" }, field.Choices!.Select(choice => choice.Id));
        foreach (var value in field.Choices!)
        {
            var selected = configuration with { Options = new Dictionary<string, string>(configuration.Options!) { [field.Id] = value.Id } };
            Assert.Equal(value.Id, factory.GetRuntimeOptions(selected)[field.Id]);
            Assert.Equal(value.Id, Assert.Single(factory.DescribeSettings(selected).SelectMany(block => block.Fields), item => item.Id == field.Id).Value);
        }
        Assert.DoesNotContain(factory.DescribeSettings(configuration).SelectMany(block => block.Fields), field => field.Id == "pcsx2_lightgun");
    }

    [Theory]
    [InlineData("disabled", "None", "None")]
    [InlineData("usb1", "GunCon2", "None")]
    [InlineData("usb2", "None", "GunCon2")]
    [InlineData("both", "GunCon2", "GunCon2")]
    public void UsbDevicesHaveIndependentConfigurationAndNoAutomaticBindings(string native, string first, string second)
    {
        var configuration = Configuration(native);
        var input = InputSettingsFunctions.Describe(configuration);
        Assert.Equal(4, input.ControllerPorts.Count);
        Assert.Equal(new[] { "USB 1", "USB 2" }, input.ControllerPorts.Where(port => port.IsUsb).Select(port => port.DisplayName));
        Assert.Equal(new int?[] { 1, 2 }, input.ControllerPorts.Where(port => !port.IsUsb).Select(port => port.DisplayNumber));
        Assert.Equal(first, input.ControllerPorts[0].SelectedControllerId);
        Assert.Equal(second, input.ControllerPorts[1].SelectedControllerId);
        Assert.All(input.ControllerPorts.Take(2), port =>
        {
            Assert.Equal(new[] { "GunCon2", "None" }, port.ControllerChoices.Select(choice => choice.Id).Order());
            var gun = port.ControllerChoices.Single(choice => choice.Id == "GunCon2");
            Assert.Equal(EmulationControllerCategory.PointingDevice, gun.Category);
            Assert.Equal(EmulationControllerVisualIds.SonyGunCon2, Assert.Single(gun.CompatibleVisualIds!));
            Assert.Equal(EmulationControllerVisualIds.SonyGunCon2, gun.DefaultVisualId);
            Assert.Equal(EmulationControllerCommandIds.B, gun.VisualCommandIds![EmulationControllerVisualControl.PrimaryAction]);
            Assert.Equal(EmulationControllerCommandIds.X, gun.VisualCommandIds[EmulationControllerVisualControl.TertiaryAction]);
            Assert.Equal(EmulationControllerCommandIds.Y, gun.VisualCommandIds[EmulationControllerVisualControl.QuaternaryAction]);
            Assert.Equal(EmulationControllerCommandIds.Start, gun.VisualCommandIds[EmulationControllerVisualControl.Start]);
            Assert.Equal(EmulationControllerCommandIds.Select, gun.VisualCommandIds[EmulationControllerVisualControl.Option]);
            Assert.Equal(15, gun.BindingDefinitions!.Count);
            Assert.All(gun.BindingDefinitions, command => Assert.Empty(command.DefaultBinding));
            Assert.Empty(port.Bindings.Values);
        });
        Assert.All(input.ControllerPorts.Skip(2), port => Assert.DoesNotContain(port.ControllerChoices, choice => choice.Id == "GunCon2"));
        Assert.Equal(native, new GWGUI.Emulation.Sony.Emulators.Pcee2.Factories.Pcee2MachineFactory().GetRuntimeOptions(configuration)["pcsx2_lightgun"]);
    }

    [Fact]
    public void TwoUsbGunsAndEightGamepadsSurvivePersistenceAndNativeCallbackRouting()
    {
        var configuration = Configuration("both", "both");
        var settings = InputSettingsFunctions.Describe(configuration);
        Assert.Equal(10, settings.ControllerPorts.Count);
        settings = settings with
        {
            ControllerPorts = settings.ControllerPorts.Select(port => port with
            {
                Bindings = port.Bindings with { Values = new Dictionary<string, string> { [EmulationControllerCommandIds.B] = port.Number switch { 1 => "Keyboard:A", 2 => "Keyboard:B", 3 => "Keyboard:C", 10 => "Keyboard:D", _ => string.Empty } } }
            }).ToArray()
        };
        var restored = JsonSerializer.Deserialize<MachineConfiguration>(JsonSerializer.Serialize(InputSettingsFunctions.Apply(configuration, settings)))!;
        var mapped = InputSnapshotFunctions.Apply(EmulationInputSnapshot.Empty with { Keys = new HashSet<EmulationKey> { EmulationKey.A, EmulationKey.C, EmulationKey.D } }, restored.Input, false, 10);
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true)) EmulationSdk.HostProtocol.WriteInput(writer, mapped);
        stream.Position = 0;
        using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true)) mapped = EmulationSdk.HostProtocol.ReadInput(reader, Core.DisplayName);
        Assert.Equal(10, mapped.Controllers.Count);
        using var host = Host(restored);
        host.Input = mapped;
        host.InputPoll();
        Assert.Equal(1, host.InputState(0, 4, 0, (uint)LightGunInput.Trigger));
        Assert.Equal(0, host.InputState(1, 4, 0, (uint)LightGunInput.Trigger));
        Assert.Equal(1, host.InputState(0, 1, 0, 0));
        Assert.Equal(1, host.InputState(7, 1, 0, 0));
        host.Input = EmulationInputSnapshot.Empty;
        host.InputPoll();
        Assert.Equal(0, host.InputState(0, 4, 0, (uint)LightGunInput.Trigger));
        Assert.Equal(0, host.InputState(7, 1, 0, 0));
        var enlarged = restored with { Options = new Dictionary<string, string> { ["pcsx2_multitap"] = "disabled", ["pcsx2_lightgun"] = "both" } };
        Assert.Equal("Keyboard:A", InputSettingsFunctions.Describe(enlarged).ControllerPorts[0].Bindings.Values[EmulationControllerCommandIds.B]);
        Assert.Equal("Keyboard:C", InputSettingsFunctions.Describe(enlarged).ControllerPorts[2].Bindings.Values[EmulationControllerCommandIds.B]);
    }

    [Fact]
    public void EachGunCanUseItsOwnAnalogAimWithoutMovingTheGamepads()
    {
        var configuration = Configuration("both");
        using var host = Host(configuration);
        host.Input = EmulationInputSnapshot.Empty with
        {
            Controllers =
            [
                EmulationControllerState.Empty with { LeftX = -12345, LeftY = 23456, Controls = new(new Dictionary<string, float> { [EmulationControllerCommandIds.LeftStickLeft] = 0, [EmulationControllerCommandIds.LeftStickUp] = 0 }) },
                EmulationControllerState.Empty with { LeftX = 19000, LeftY = -7000, Controls = new(new Dictionary<string, float> { [EmulationControllerCommandIds.LeftStickRight] = 0, [EmulationControllerCommandIds.LeftStickDown] = 0 }) },
                EmulationControllerState.Empty with { LeftX = 4567, LeftY = 8901 }
            ]
        };
        host.InputPoll();
        Assert.Equal(-12345, host.InputState(0, 4, 0, (uint)LightGunInput.ScreenX));
        Assert.Equal(23456, host.InputState(0, 4, 0, (uint)LightGunInput.ScreenY));
        Assert.Equal(19000, host.InputState(1, 4, 0, (uint)LightGunInput.ScreenX));
        Assert.Equal(-7000, host.InputState(1, 4, 0, (uint)LightGunInput.ScreenY));
        Assert.Equal(4567, host.InputState(0, 5, 0, 0));
        Assert.Equal(8901, host.InputState(0, 5, 0, 1));
    }

    [Fact]
    public void GamepadVibrationIsReturnedToItsLogicalPortAndUsbCannotBeHotSwapped()
    {
        var configuration = Configuration("both", "both");
        var host = Host(configuration);
        using var core = new ExternalCore(Core.Definition, string.Empty);
        Set(core, "_host", host);
        Set(core, "_configuration", configuration);
        Set(core, "_controllerPorts", ControllerConfigurationFunctions.CreatePorts(configuration));
        var nativePorts = new List<uint>();
        Set(core, "_setControllerPortDevice", new ExternalCoreApi.SetControllerPortDevice((port, _) => nativePorts.Add(port)));
        core.SetControllerPortDevice(0, PeripheralCategory.GunCon2);
        core.SetControllerPortDevice(2, PeripheralCategory.Joystick);
        core.SetControllerPortDevice(9, PeripheralCategory.Joystick);
        Assert.Equal(new uint[] { 0, 7 }, nativePorts);
        Assert.True(host.Feedback.SetState(0, 0, ushort.MaxValue));
        Assert.True(host.Feedback.SetState(7, 1, ushort.MaxValue));
        Assert.Equal(new[] { 2, 9 }, core.ControllerFeedback.Select(state => state.Port));
        Set(core, "_gameLoaded", true);
        try { Assert.Throws<NotSupportedException>(() => core.SetControllerPortDevice(0, PeripheralCategory.None)); }
        finally { Set(core, "_gameLoaded", false); }
    }

    private static MachineConfiguration Configuration(string usb, string multitap = "disabled") => new("PlayStation2", "pcee2", Options: new Dictionary<string, string> { ["pcsx2_lightgun"] = usb, ["pcsx2_multitap"] = multitap });

    private static ExternalHostCallbacks Host(MachineConfiguration configuration)
    {
        var root = Path.GetTempPath();
        var host = new ExternalHostCallbacks(root, root, root, null, controllerPorts: ControllerConfigurationFunctions.CreatePorts(configuration));
        for (var port = 0; port < ControllerFunctions.PortCount(configuration); port++) host.SetControllerType(port, ControllerFunctions.Resolve(configuration, port));
        return host;
    }

    private static void Set(object owner, string name, object value) => owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(owner, value);
}
