using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using Ps3Hardware = GWGUI.Emulation.Sony.Common.Machines.PlayStation3.Constants.HardwareConstants;
using Rpcs3Core = GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants.CoreConstants;
using SwanCore = GWGUI.Emulation.Sony.Emulators.SwanStation.Constants.CoreConstants;
using Pcsx2Core = GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants.Pcsx2Constants;
using PlayCore = GWGUI.Emulation.Sony.Emulators.Play.Constants.CoreConstants;
using PortCounts = GWGUI.Emulation.Sony.Common.Constants.ControllerPortConstants;
using SwanOptions = GWGUI.Emulation.Sony.Emulators.SwanStation.Constants.OptionConstants;
using RearmedCore = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants.CoreConstants;
using RearmedOptions = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants.OptionConstants;
using PceeCore = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.CoreConstants;
using PceeOptions = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.OptionConstants;
using BeetleCore = GWGUI.Emulation.Sony.Emulators.BeetlePsx.Constants.CoreConstants;
using BeetleOptions = GWGUI.Emulation.Sony.Emulators.BeetlePsx.Constants.OptionConstants;
using BeetleHwCore = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants.CoreConstants;
using BeetleHwOptions = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants.OptionConstants;
using Toggle = GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants.ToggleValueConstants;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class ControllerConfigurationTests
{
    private const int FirstPort = 0;
    private const string KeyboardA = "Keyboard:A";
    private const string PhysicalDevice = "controller-test";
    private const string Axis = "TestAxis";
    private const string PositiveAxisSource = "Controller:controller-test:TestAxisPositive";
    private const float PartialPressure = .25f;

    [Theory]
    [InlineData(ModelConstants.PlayStation2, PlayCore.Id, PortCounts.OneControllerPort)]
    [InlineData(ModelConstants.PlayStation2, Pcsx2Core.Id, PortCounts.TwoControllerPorts)]
    [InlineData(ModelConstants.PlayStation3, Rpcs3Core.Id, Ps3Hardware.ControllerPortCount)]
    public void ControllerSettingsRespectTheEmulatorPortCapacity(string model, string emulator, int count)
    {
        AssertPortCapacity(new MachineConfiguration(model, emulator), count);
    }

    [Theory]
    [InlineData(SwanCore.Id, SwanOptions.ControllerPortsMultitapModeKey,
        SwanOptions.ControllerPortsMultitapModePort1Only, PortCounts.SingleMultitapControllerCount)]
    [InlineData(SwanCore.Id, SwanOptions.ControllerPortsMultitapModeKey,
        SwanOptions.ControllerPortsMultitapModePort2Only, PortCounts.SingleMultitapControllerCount)]
    [InlineData(SwanCore.Id, SwanOptions.ControllerPortsMultitapModeKey,
        SwanOptions.ControllerPortsMultitapModeBothPorts, PortCounts.MaximumControllerPortCount)]
    [InlineData(RearmedCore.Id, RearmedOptions.MultitapKey,
        RearmedOptions.MultitapPort1, PortCounts.SingleMultitapControllerCount)]
    [InlineData(RearmedCore.Id, RearmedOptions.MultitapKey,
        RearmedOptions.MultitapPort2, PortCounts.SingleMultitapControllerCount)]
    [InlineData(RearmedCore.Id, RearmedOptions.MultitapKey,
        RearmedOptions.MultitapPorts1And2, PortCounts.MaximumControllerPortCount)]
    [InlineData(PceeCore.Id, PceeOptions.MultitapKey,
        PceeOptions.MultitapPort1, PortCounts.SingleMultitapControllerCount)]
    [InlineData(PceeCore.Id, PceeOptions.MultitapKey,
        PceeOptions.MultitapPort2, PortCounts.SingleMultitapControllerCount)]
    [InlineData(PceeCore.Id, PceeOptions.MultitapKey,
        PceeOptions.MultitapBoth, PortCounts.MaximumControllerPortCount)]
    public void MultitapMakesAllConfiguredPlayersAvailable(string emulator, string key, string value, int count)
    {
        var model = emulator == PceeCore.Id ? ModelConstants.PlayStation2 : ModelConstants.PlayStation;
        AssertPortCapacity(new MachineConfiguration(model, emulator,
            Options: new Dictionary<string, string> { [key] = value }), count);
    }

    [Theory]
    [InlineData(BeetleCore.Id, BeetleOptions.EnableMultitapPort1Key, BeetleOptions.EnableMultitapPort2Key)]
    [InlineData(BeetleHwCore.Id, BeetleHwOptions.EnableMultitapPort1Key, BeetleHwOptions.EnableMultitapPort2Key)]
    public void IndependentMultitapsExposeEveryPlayer(string emulator, string firstKey, string secondKey)
    {
        foreach (var firstEnabled in new[] { false, true })
        foreach (var secondEnabled in new[] { false, true })
        {
            var count = firstEnabled && secondEnabled ? PortCounts.MaximumControllerPortCount
                : firstEnabled || secondEnabled ? PortCounts.SingleMultitapControllerCount
                : PortCounts.TwoControllerPorts;
            AssertPortCapacity(new MachineConfiguration(ModelConstants.PlayStation, emulator,
                Options: new Dictionary<string, string>
                {
                    [firstKey] = firstEnabled ? Toggle.Enabled : Toggle.Disabled,
                    [secondKey] = secondEnabled ? Toggle.Enabled : Toggle.Disabled
                }), count);
        }
    }

    private static void AssertPortCapacity(MachineConfiguration configuration, int count)
    {
        var settings = InputSettingsFunctions.Describe(configuration);
        Assert.Equal(count, settings.ControllerPorts.Count);
        Assert.Equal(count, ControllerFunctions.PortCount(configuration));
        ControllerFunctions.ValidatePort(configuration, count - PortCounts.OneControllerPort);
        Assert.Throws<ArgumentOutOfRangeException>(() => ControllerFunctions.ValidatePort(configuration, count));
    }

    [Fact]
    public void SwanMultitapChoicesMatchTheDevicesAdvertisedForEachPort()
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation, SwanCore.Id,
            Options: new Dictionary<string, string>
            {
                [SwanOptions.ControllerPortsMultitapModeKey] = SwanOptions.ControllerPortsMultitapModeBothPorts
            });
        var settings = InputSettingsFunctions.Describe(configuration);
        foreach (var port in Enumerable.Range(PortCounts.MinimumControllerPort, settings.ControllerPorts.Count))
        {
            var choices = settings.ControllerPorts[port].ControllerChoices.Select(choice => choice.Id).ToHashSet();
            Assert.Equal(port < SwanCore.LightGunPortCount, choices.Contains(nameof(ControllerType.GunCon)));
            Assert.Equal(port < SwanCore.SteeringControllerPortCount, choices.Contains(nameof(ControllerType.NeGcon)));
            Assert.Contains(nameof(ControllerType.DualShock), choices);
            Assert.Contains(nameof(ControllerType.None), choices);
        }
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ControllerFunctions.WithControllerType(configuration, SwanCore.LightGunPortCount, ControllerType.GunCon));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ControllerFunctions.WithControllerType(configuration, SwanCore.SteeringControllerPortCount, ControllerType.NeGcon));
    }

    [Fact]
    public void Ps3ExposesSevenPortsWithCompleteButtonsAndBothSticks()
    {
        var settings = InputSettingsFunctions.Describe(new MachineConfiguration(
            ModelConstants.PlayStation3, Rpcs3Core.Id));
        Assert.Equal(Ps3Hardware.ControllerPortCount, settings.ControllerPorts.Count);
        Assert.All(settings.ControllerPorts, port =>
        {
            var ids = port.Bindings.Definitions.Select(definition => definition.Id).ToHashSet();
            Assert.Contains(EmulationControllerCommandIds.X, ids);
            Assert.Contains(EmulationControllerCommandIds.Y, ids);
            Assert.Contains(EmulationControllerCommandIds.L2, ids);
            Assert.Contains(EmulationControllerCommandIds.R2, ids);
            Assert.Contains(EmulationControllerCommandIds.L3, ids);
            Assert.Contains(EmulationControllerCommandIds.R3, ids);
            Assert.Contains(InputSettingsFunctionsConstants.LeftStickLeft, ids);
            Assert.Contains(InputSettingsFunctionsConstants.RightStickRight, ids);
            Assert.DoesNotContain(port.ControllerChoices, choice =>
                choice.CompatibleVisualIds?.Contains(EmulationControllerVisualIds.QuickShot) == true);
        });
    }

    [Fact]
    public void DisabledPortSuppressesPhysicalAndMappedInput()
    {
        var snapshot = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.A },
            Controllers = [EmulationControllerState.Empty with
                { Buttons = uint.MaxValue, LeftX = short.MaxValue, RightY = short.MinValue }]
        };
        var input = new InputConfiguration(ControllerBindings:
        [new ControllerBinding(FirstPort, ControllerType.None,
            ButtonMappings: new Dictionary<string, string> { [EmulationControllerCommandIds.B] = KeyboardA })]);
        var mapped = InputSnapshotFunctions.Apply(snapshot, input, false, Ps3Hardware.ControllerPortCount);
        Assert.Equal(Ps3Hardware.ControllerPortCount, mapped.Controllers.Count);
        Assert.Equal(EmulationControllerState.Empty, mapped.Controllers[FirstPort]);
    }

    [Fact]
    public void RightStickMappingKeepsPartialPressureAndUnmappedAxes()
    {
        var controller = EmulationControllerState.Empty with
        {
            DeviceId = PhysicalDevice, LeftX = short.MaxValue, LeftY = short.MinValue,
            RightY = short.MinValue,
            Controls = new EmulationControllerControls(new Dictionary<string, float> { [Axis] = PartialPressure })
        };
        var snapshot = EmulationInputSnapshot.Empty with { Controllers = [controller] };
        var input = new InputConfiguration(ControllerBindings:
        [new ControllerBinding(FirstPort, ControllerType.DualShock, PhysicalDevice,
            new Dictionary<string, string>
            { [InputSettingsFunctionsConstants.RightStickRight] = PositiveAxisSource })]);
        var mapped = InputSnapshotFunctions.Apply(snapshot, input, false).Controllers[FirstPort];
        Assert.Equal(controller.LeftX, mapped.LeftX);
        Assert.Equal(controller.LeftY, mapped.LeftY);
        Assert.Equal(controller.RightY, mapped.RightY);
        Assert.Equal((short)Math.Round(PartialPressure * short.MaxValue), mapped.RightX);
    }

    [Fact]
    public void ControllerChangePreservesMappingsPhysicalDeviceAndVisual()
    {
        var mappings = new Dictionary<string, string> { [EmulationControllerCommandIds.B] = KeyboardA };
        var configuration = new MachineConfiguration(ModelConstants.PlayStation, SwanCore.Id,
            Input: new InputConfiguration(ControllerBindings:
            [new ControllerBinding(FirstPort, ControllerType.Joystick, PhysicalDevice, mappings,
                EmulationControllerVisualIds.SonyPlayStationController)]));
        var updated = ControllerFunctions.WithControllerType(configuration, FirstPort, ControllerType.DualShock);
        var binding = Assert.Single(updated.Input!.ControllerBindings!);
        Assert.Equal(ControllerType.DualShock, binding.Type);
        Assert.Equal(PhysicalDevice, binding.DeviceId);
        Assert.Same(mappings, binding.ButtonMappings);
        Assert.Equal(EmulationControllerVisualIds.SonyPlayStationController, binding.VisualId);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ControllerFunctions.WithControllerType(configuration, FirstPort, ControllerType.Keyboard));
    }

    [Fact]
    public void UsbKeyboardHasRemappableKeysWithoutPretendingTheConsoleHasABuiltInKeyboard()
    {
        var settings = InputSettingsFunctions.Describe(new MachineConfiguration(
            ModelConstants.PlayStation2, Pcsx2Core.Id));
        Assert.NotNull(settings.Keyboard);
        Assert.Contains(settings.Keyboard.Definitions, definition => definition.Id == nameof(EmulationKey.LeftShift));
        Assert.DoesNotContain(settings.Keyboard.Definitions, definition => definition.Id == nameof(EmulationKey.LeftAmiga));
        Assert.All(settings.ControllerPorts, port => Assert.Contains(port.ControllerChoices,
            choice => choice.Id == ControllerType.Keyboard.ToString()));
    }

    [Fact]
    public void MouseMappingsReachPerPortControls()
    {
        var snapshot = EmulationInputSnapshot.Empty with
            { Keys = new HashSet<EmulationKey> { EmulationKey.A } };
        var input = new InputConfiguration(ControllerBindings:
        [new ControllerBinding(FirstPort, ControllerType.Mouse, ButtonMappings:
            new Dictionary<string, string> { [nameof(MouseAction.LeftButton)] = KeyboardA })]);
        var mapped = InputSnapshotFunctions.Apply(snapshot, input, false).Controllers[FirstPort];
        Assert.Equal(InputSnapshotFunctionsConstants.FullPressure, mapped.Controls[nameof(MouseAction.LeftButton)]);
    }
}
