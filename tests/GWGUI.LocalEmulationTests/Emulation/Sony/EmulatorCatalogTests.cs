using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Sony.Common.Machines.Common.Functions;
using GWGUI.Emulation.Sony.Common.Dictionaries;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Constants;
using GWGUI.Emulation.Sony.Common.Interfaces;
using GWGUI.Emulation.Sony.Common.Contracts;
using BeetleCore = GWGUI.Emulation.Sony.Emulators.BeetlePsx.Constants.CoreConstants;
using BeetleHardwareCore = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants.CoreConstants;
using RearmedCore = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants.CoreConstants;
using PceeCore = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.CoreConstants;
using PlayCore = GWGUI.Emulation.Sony.Emulators.Play.Constants.CoreConstants;
using RpcsCore = GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants.CoreConstants;
using EmulatorCreationContext = GWGUI.Emulation.Contracts.EmulatorCreationContext<GWGUI.Emulation.Sony.Common.Machines.Common.Contracts.MachineConfiguration>;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class EmulatorCatalogTests
{
    [Fact]
    public void Pcee2RetainsItsBiosDataWithoutChangingOtherCoresPersistence()
    {
        var persistence = Assert.IsType<GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts.CoreFirmwarePersistenceSettings>(PceeCore.Definition.FirmwarePersistence);
        Assert.Equal("firmware.system", persistence.IdentityFieldId);
        Assert.Equal("bios", persistence.DirectoryName);
        Assert.Collection(persistence.Files,
            file => { Assert.Equal("firmware.nvram", file.FieldId); Assert.Equal("pcsx2/bios/scph.nvm", file.FileName); Assert.Equal(1024, file.Size); },
            file => { Assert.Equal("firmware.mechanics", file.FieldId); Assert.Equal("pcsx2/bios/scph.mec", file.FileName); Assert.Equal(4, file.Size); });
        Assert.Null(RpcsCore.Definition.FirmwarePersistence);
        Assert.Null(PlayCore.Definition.FirmwarePersistence);
        Assert.Null(BeetleCore.Definition.FirmwarePersistence);
        Assert.Null(RearmedCore.Definition.FirmwarePersistence);
    }

    [Fact]
    public void PlayStationHardwareCatalogsKeepEveryNativeRendererAndTheirActualDefaults()
    {
        var swan = GWGUI.Emulation.Sony.Emulators.SwanStation.Constants.CoreConstants.Definition.Options.Single(option => option.Key == "swanstation_GPU_Renderer");
        Assert.Equal("Auto", swan.DefaultValue);
        Assert.Equal(new[] { "Auto", "OpenGL", "D3D11", "Vulkan", "Software" }, swan.Values.Select(value => value.Value));
        var beetle = BeetleHardwareCore.Definition.Options.Single(option => option.Key == "beetle_psx_hw_renderer");
        Assert.Equal("hardware", beetle.DefaultValue);
        Assert.Equal(new[] { "hardware", "hardware_gl", "hardware_vk", "software" }, beetle.Values.Select(value => value.Value));
    }

    [Theory]
    [InlineData(null, 1)]
    [InlineData("1", 1)]
    [InlineData("7", 7)]
    public void EveryRpcs3PortOffersOnlyItsDualShock3VisualWithoutPhysicalAssignments(string? selectedCount, int portCount)
    {
        var configuration = new MachineConfiguration(ModelConstants.PlayStation3, RpcsCore.Id,
            Options: selectedCount is null ? null : new Dictionary<string, string> { ["input.controllerCount"] = selectedCount });
        var settings = InputSettingsFunctions.Describe(configuration);
        Assert.Equal(Enumerable.Range(1, portCount), settings.ControllerPorts.Select(port => port.Number));
        foreach (var port in settings.ControllerPorts)
        {
            var controller = Assert.Single(port.ControllerChoices, choice => choice.Id == "Joystick");
            Assert.Equal(EmulationControllerVisualIds.SonyDualShock3, Assert.Single(controller.CompatibleVisualIds!));
            Assert.Equal(EmulationControllerVisualIds.SonyDualShock3, controller.DefaultVisualId);
            Assert.Empty(port.Bindings.Values);
            Assert.Null(port.PhysicalDeviceId);
        }
    }

    [Theory]
    [InlineData(RpcsCore.Id, ModelConstants.PlayStation3, false)]
    [InlineData(BeetleCore.Id, ModelConstants.PlayStation, true)]
    [InlineData(BeetleHardwareCore.Id, ModelConstants.PlayStation, true)]
    [InlineData(RearmedCore.Id, ModelConstants.PlayStation, true)]
    [InlineData(PlayCore.Id, ModelConstants.PlayStation2, true)]
    public async Task AdapterPublishesSavedStateSupportBeforeNativeInitialization(string id, string model, bool expected)
    {
        var adapter = Assert.Single(EmulatorCatalog.CreateAdapters(), item => item.EmulatorId == id);
        var context = new EmulatorCreationContext("uncreated-session-" + Guid.NewGuid().ToString("N"), "unused-core.dll", "unused-host.exe", null, null);
        var machine = adapter.Create(new MachineConfiguration(model, id, AudioEnabled: false), context);
        try
        {
            Assert.Equal(expected, machine.SavedStates.IsSupported);
            if (!expected)
            {
                await Assert.ThrowsAsync<NotSupportedException>(async () => await machine.SavedStates.SaveAsync("unused-state"));
                await Assert.ThrowsAsync<NotSupportedException>(async () => await machine.SavedStates.LoadAsync("unused-state"));
            }
        }
        finally { await machine.DisposeAsync(); }
    }

    [Theory]
    [InlineData(null, "vulkan")]
    [InlineData("vulkan", "vulkan")]
    [InlineData("opengl", "opengl")]
    [InlineData("software", "software")]
    public void Pcee2PresentsItsNativeRenderersAndDefaultWithoutOverwritingSavedChoices(string? selected, string expected)
    {
        const string key = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.OptionConstants.RendererKey;
        var configuration = new MachineConfiguration(ModelConstants.PlayStation2, PceeCore.Id,
            Options: selected is null ? null : new Dictionary<string, string> { [key] = selected });
        var adapter = Assert.IsAssignableFrom<IEmulatorSettings>(EmulatorCatalog.CreateAdapters().Single(item => item.EmulatorId == PceeCore.Id));
        Assert.Equal(expected, ((IEmulatorAdapter)adapter).GetRuntimeOptions(configuration)[key]);
        var option = Assert.Single(PceeCore.Definition.Options, item => item.Key == key);
        Assert.Equal(new[] { "vulkan", "opengl", "software" }, option.Values.Select(item => item.Value));
        Assert.Equal("vulkan", option.DefaultValue);
        Assert.True(option.RequiresRestart);
        var field = adapter.DescribeSettings(configuration).SelectMany(block => block.Fields).Single(item => item.Id == key);
        Assert.Equal(expected, field.Value);
        Assert.Equal("Vulkan", Assert.Single(option.Values, item => item.Value == "vulkan").Label);
        var hardware = Assert.Single(field.Choices!, item => item.DisplayResourceKey == "Emulation.Option.Common.Value.HardwareRendering");
        Assert.Equal(selected == "opengl" ? "opengl" : "vulkan", hardware.Id);
        Assert.Equal(2, field.Choices!.Count);
        Assert.Contains(field.Choices, item => item.Id == "software");
        Assert.Equal(selected, configuration.Options?.GetValueOrDefault(key));
    }

    [Theory]
    [InlineData(null, "vulkan")]
    [InlineData("opengl", "opengl")]
    [InlineData("null", "null")]
    [InlineData("vulkan", "vulkan")]
    public void Rpcs3OffersEveryRendererAndPreservesAnExplicitSelection(string? selected, string expected)
    {
        const string key = GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants.OptionConstants.RendererKey;
        var configuration = new MachineConfiguration(ModelConstants.PlayStation3, RpcsCore.Id,
            Options: selected is null ? null : new Dictionary<string, string> { [key] = selected });
        var adapter = Assert.IsAssignableFrom<IEmulatorSettings>(EmulatorCatalog.CreateAdapters().Single(item => item.EmulatorId == RpcsCore.Id));
        Assert.Equal(expected, ((IEmulatorAdapter)adapter).GetRuntimeOptions(configuration)[key]);
        var option = Assert.Single(RpcsCore.Definition.Options, item => item.Key == key);
        Assert.Equal(new[] { "opengl", "vulkan", "null" }, option.Values.Select(item => item.Value));
        Assert.Equal("vulkan", option.DefaultValue);
        Assert.Equal(selected, configuration.Options?.GetValueOrDefault(key));
    }

    [Theory]
    [InlineData(BeetleCore.Id, ModelConstants.PlayStation, false)]
    [InlineData(BeetleHardwareCore.Id, ModelConstants.PlayStation, false)]
    [InlineData(RearmedCore.Id, ModelConstants.PlayStation, false)]
    [InlineData(PceeCore.Id, ModelConstants.PlayStation2, true)]
    [InlineData(PlayCore.Id, ModelConstants.PlayStation2, false)]
    [InlineData(RpcsCore.Id, ModelConstants.PlayStation3, true)]
    public void NewCoreUsesItsOwnMachineAndFirmwarePolicy(string id, string model, bool required)
    {
        var adapter = Assert.Single(EmulatorCatalog.CreateAdapters(), item => item.EmulatorId == id);
        var firmware = Assert.IsAssignableFrom<IEmulatorFirmwareAdapter>(adapter);
        var formats = Assert.IsAssignableFrom<IEmulatorContentFormats>(adapter);
        var settings = Assert.IsAssignableFrom<IEmulatorSettings>(adapter);
        Assert.Equal(model, Assert.Single(adapter.Definition.MachineIds));
        var configuration = new MachineConfiguration(model, EmulatorId: id);
        Assert.Equal(required, firmware.RequiresExternalFirmware(configuration));
        Assert.Equal(required, adapter.GetFirmwareSlots(configuration).Any(slot => slot.IsRequired));
        Assert.Contains(EmulatorCatalog.GetAll(model), item => item.Id == id);
        Assert.NotEmpty(formats.SupportedContentExtensions);
        if (id == PlayCore.Id)
        {
            Assert.Empty(adapter.GetFirmwareSlots(configuration));
        }
        Assert.NotEmpty(settings.DescribeSettings(configuration));
    }
}
