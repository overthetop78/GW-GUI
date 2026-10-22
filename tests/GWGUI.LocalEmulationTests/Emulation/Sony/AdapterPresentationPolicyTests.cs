using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Sony.Common.Interfaces;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcee2.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Factories;
using PceeOptions = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.OptionConstants;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class AdapterPresentationPolicyTests
{
    [Theory]
    [InlineData("disabled", "0,1", "MC 1,MC 2")]
    [InlineData("port1", "0,1,2,3,4", "MC 1,MC 2,MC 1B,MC 1C,MC 1D")]
    [InlineData("port2", "0,1,5,6,7", "MC 1,MC 2,MC 2B,MC 2C,MC 2D")]
    [InlineData("both", "0,1,2,3,4,5,6,7", "MC 1,MC 2,MC 1B,MC 1C,MC 1D,MC 2B,MC 2C,MC 2D")]
    public void MultitapPresentationKeepsNativeSlotIdentity(string mode, string indexes, string labels)
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcee2",
            Options: new Dictionary<string, string> { [PceeOptions.MultitapKey] = mode });
        var expected = indexes.Split(',').Select(value => int.Parse(value, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        IEmulatorMediaAdapter adapter = new Pcee2MachineFactory();
        Assert.Equal(expected, adapter.GetMemoryCardIndexes(configuration));
        var cards = StorageSettingsFunctions.Describe(configuration).AvailableDevices
            .Where(device => device.MediaType == EmulationMediaType.MemoryCard).ToArray();
        Assert.Equal(expected, cards.Select(card => card.Slot.Index));
        Assert.Equal(labels.Split(','), cards.Select(card => card.DisplayLabel));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("enabled", true)]
    [InlineData("disabled", false)]
    public void MemoryStickInsertionRoundTripsThroughOriginalOption(string? value, bool inserted)
    {
        const string key = "ppsspp_memstick_inserted";
        var options = new Dictionary<string, string> { ["unrelated"] = "preserved" };
        if (value is not null) options[key] = value;
        var configuration = new MachineConfiguration(ModelConstants.Psp, "ppsspp", Options: options);
        var storage = StorageSettingsFunctions.Describe(configuration);
        Assert.Equal(inserted, storage.ConfiguredSlots.Contains(EmulationMediaSlot.MemoryCard0));
        var changed = StorageSettingsFunctions.Apply(configuration, storage with
        {
            ConfiguredSlots = inserted ? [] : [EmulationMediaSlot.MemoryCard0]
        });
        Assert.Equal(inserted ? "disabled" : "enabled", changed.Options![key]);
        Assert.Equal("preserved", changed.Options["unrelated"]);
        Assert.Equal(key, ((IEmulatorMediaAdapter)new PpssppMachineFactory()).MemoryStickInsertedOptionKey);
    }

    [Fact]
    public void UsbKeyboardUsesTheSameNativeKeysAndOrdering()
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcsx2",
            Input: new InputConfiguration(UsbBindings: [new ControllerBinding(0, ControllerType.Keyboard)]));
        var keyboard = InputSettingsFunctions.Describe(configuration).Keyboard;
        Assert.NotNull(keyboard);
        var expected = EmulationSdk.InputProcessing.CreateKeyMap().Keys
            .Where(key => key is not (EmulationKey.LeftAmiga or EmulationKey.RightAmiga or EmulationKey.Break or EmulationKey.Undo))
            .OrderBy(key => (int)key).Select(key => key.ToString()).ToArray();
        Assert.NotEmpty(expected);
        Assert.Equal(expected, keyboard.Definitions.Select(definition => definition.Id));
    }

    [Fact]
    public void GunCon2UsesExistingAdapterDefinitions()
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, "pcee2",
            Input: new InputConfiguration(ControllerBindings: [new ControllerBinding(0, ControllerType.GunCon2)]));
        var expected = new Pcee2MachineFactory().GetControllerBindingDefinitions(configuration, ControllerType.GunCon2, 0);
        var port = InputSettingsFunctions.Describe(configuration).ControllerPorts.Single(port => port.Number == 1);
        Assert.NotNull(expected);
        Assert.Same(expected, port.Bindings.Definitions);
    }

    [Theory]
    [InlineData("swanstation_ControllerPorts_MultitapMode", "Disabled", "Port1Only", "Port2Only", "BothPorts")]
    [InlineData("pcsx_rearmed_multitap", "disabled", "port 1", "port 2", "ports 1 and 2")]
    [InlineData("pcsx2_multitap", "disabled", "port1", "port2", "both")]
    public void MultitapUpdatesMergeOtherPortAndRemoveTemporaryKeys(string key, string disabled, string port1, string port2, string both)
    {
        IEmulatorSettingsAdapter adapter = new Pcsx2MachineFactory();
        foreach (var previous in new[] { disabled, port1, port2, both })
        foreach (var selected in new[] { false, true })
        {
            var options = new Dictionary<string, string>
            {
                [key] = previous, [key + ".port1"] = "temporary", [key + ".port2"] = "temporary", ["other"] = "kept"
            };
            adapter.ApplySettingsOptions(options, new Dictionary<string, string?>
            {
                [key + ".port1"] = selected ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled
            });
            var second = previous == port2 || previous == both;
            Assert.Equal(selected ? second ? both : port1 : second ? port2 : disabled, options[key]);
            Assert.False(options.ContainsKey(key + ".port1"));
            Assert.False(options.ContainsKey(key + ".port2"));
            Assert.Equal("kept", options["other"]);
        }
    }
}
