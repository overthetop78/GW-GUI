using System.Globalization;
using System.IO;
using System.Reflection;
using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Amstrad.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;
using GWGUI.Emulation.Amstrad.Common.Services;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Services;
using GWGUI.Emulation.Amstrad.Modules;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using CpcClassicCatalog = GWGUI.Emulation.Amstrad.Common.Machines.CpcClassic.Dictionaries.ModelCatalog;
using CpcPlusCatalog = GWGUI.Emulation.Amstrad.Common.Machines.CpcPlus.Dictionaries.ModelCatalog;
using Gx4000Catalog = GWGUI.Emulation.Amstrad.Common.Machines.Gx4000.Dictionaries.ModelCatalog;

namespace GWGUI.Tests.Emulation.Amstrad;

public sealed class AmstradEmulatorAdapterTests
{
    [Fact]
    public void MachineGroupsExposeOnlyTheRequestedMachines()
    {
        Assert.Equal(["cpc-464", "cpc-664", "cpc-6128"],
            CpcClassicCatalog.All.Select(model => model.Id));
        Assert.Equal(["cpc-464-plus", "cpc-6128-plus"],
            CpcPlusCatalog.All.Select(model => model.Id));
        Assert.Equal(["gx4000"], Gx4000Catalog.All.Select(model => model.Id));
        Assert.Equal(6, MachineCatalog.All.Count);
    }

    [Fact]
    public void MachineConfigurationPersistsItsSelectedEmulator()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var original = new MachineConfiguration("cpc-6128", "caprice32");

        var restored = JsonSerializer.Deserialize<MachineConfiguration>(
            JsonSerializer.Serialize(original, options), options);

        Assert.NotNull(restored);
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal("caprice32", restored.EmulatorId);
    }

    [Fact]
    public void EngineDiscoversCaprice32ThroughTheCommonAdapter()
    {
        var adapterType = typeof(AmstradEmulationModule).Assembly.GetType(
            "GWGUI.Emulation.Amstrad.Emulators.Caprice32.Factories.Caprice32MachineFactory");
        var commonType = typeof(AmstradEmulationModule).Assembly.GetType(
            "GWGUI.Emulation.Amstrad.Common.Interfaces.IEmulatorAdapter");
        Assert.NotNull(adapterType);
        Assert.NotNull(commonType);
        Assert.Contains(commonType!, adapterType!.GetInterfaces());

        var field = typeof(Engine).GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic);
        var adapters = Assert.IsAssignableFrom<System.Collections.IDictionary>(field!.GetValue(new Engine()));
        Assert.Equal(["caprice32"], adapters.Keys.Cast<string>());
    }

    [Theory]
    [InlineData("cpc-464", 0, true, false)]
    [InlineData("cpc-664", 1, false, false)]
    [InlineData("cpc-6128", 1, false, false)]
    [InlineData("cpc-464-plus", 0, true, true)]
    [InlineData("cpc-6128-plus", 1, false, true)]
    [InlineData("gx4000", 0, false, true)]
    public void BuiltInStorageCannotBeRemoved(string machineId, int floppyCount,
        bool cassette, bool cartridge)
    {
        using var httpClient = new HttpClient();
        var module = new AmstradEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var configuration = module.CreateConfiguration(machineId);
        var initial = module.DescribeStorageSettings(configuration);

        Assert.Equal(floppyCount, initial.AvailableDevices.Count(device =>
            device.Slot.Category == EmulationMediaCategory.FloppyDrive && device.IsPermanent));
        Assert.Equal(cassette, initial.AvailableDevices.Any(device =>
            device.Slot == EmulationMediaSlot.Cassette0 && device.IsPermanent));
        Assert.Equal(cartridge, initial.AvailableDevices.Any(device =>
            device.Slot == EmulationMediaSlot.Cartridge0 && device.IsPermanent));

        var removed = module.ApplyStorageSettings(configuration,
            initial with { ConfiguredSlots = [] });
        var restored = module.DescribeStorageSettings(removed);
        Assert.All(initial.AvailableDevices.Where(device => device.IsPermanent),
            device => Assert.Contains(device.Slot, restored.ConfiguredSlots));
    }

    [Theory]
    [InlineData("cpc-464", 2, true)]
    [InlineData("cpc-664", 2, true)]
    [InlineData("cpc-6128", 2, true)]
    [InlineData("cpc-464-plus", 2, true)]
    [InlineData("cpc-6128-plus", 2, true)]
    [InlineData("gx4000", 0, false)]
    public void OptionalStorageMatchesMachineCapabilities(string machineId,
        int maximumFloppyCount, bool cassetteAvailable)
    {
        using var httpClient = new HttpClient();
        var module = new AmstradEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var settings = module.DescribeStorageSettings(module.CreateConfiguration(machineId));

        Assert.Equal(maximumFloppyCount, settings.AvailableDevices.Count(device =>
            device.Slot.Category == EmulationMediaCategory.FloppyDrive));
        Assert.Equal(cassetteAvailable, settings.AvailableDevices.Any(device =>
            device.Slot == EmulationMediaSlot.Cassette0));
        Assert.DoesNotContain(settings.AvailableDevices,
            device => device.Slot.Category == EmulationMediaCategory.HardDisk);
    }

    [Fact]
    public void CpcSettingsExposeAllGenericMachineTabs()
    {
        using var httpClient = new HttpClient();
        var module = new AmstradEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var configuration = module.CreateConfiguration("cpc-464");
        var description = module.Describe("cpc-464", configuration);
        var input = module.DescribeInputSettings(configuration);

        Assert.All(new[] { EmulationMachineTab.General, EmulationMachineTab.Cpu,
            EmulationMachineTab.Ram, EmulationMachineTab.Rom, EmulationMachineTab.Video,
            EmulationMachineTab.Audio }, tab => Assert.Contains(description.Blocks,
                block => block.Tab == tab && block.Fields.Count > 0));
        Assert.NotNull(input.Keyboard);
        Assert.NotNull(input.Mouse);
        Assert.Equal(2, input.ControllerPorts.Count);
        Assert.All(input.ControllerPorts, port => Assert.NotEmpty(port.Bindings.Definitions));
        Assert.True(Assert.IsType<MachineConfiguration>(configuration).Input!.CaptureMouse);
    }

    [Fact]
    public void Caprice32PointerConsumesCapturedRelativeMovement()
    {
        using var callbacks = new ExternalHostCallbacks(Path.GetTempPath(), Path.GetTempPath(),
            Path.GetTempPath(), null);
        callbacks.Input = EmulationInputSnapshot.Empty with
        {
            Pointer = EmulationInputSnapshot.Empty.Pointer with
            {
                DeltaX = 10,
                DeltaY = -5,
                Left = true
            }
        };

        callbacks.InputPoll();

        Assert.Equal(1280, callbacks.InputState(0,
            ExternalHostCallbacksConstants.PointerDevice, 0,
            ExternalHostCallbacksConstants.PointerX));
        Assert.Equal(-640, callbacks.InputState(0,
            ExternalHostCallbacksConstants.PointerDevice, 0,
            ExternalHostCallbacksConstants.PointerY));
        Assert.Equal(1, callbacks.InputState(0,
            ExternalHostCallbacksConstants.PointerDevice, 0,
            ExternalHostCallbacksConstants.PointerPressed));
    }

    [Theory]
    [InlineData("fr-FR", "french")]
    [InlineData("es-ES", "spanish")]
    [InlineData("en-US", "english")]
    [InlineData("de-DE", "english")]
    public void Caprice32LanguageFollowsApplicationLanguage(string cultureName, string expected)
    {
        var previousCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            var configuration = new MachineConfiguration("cpc-464", "caprice32");

            var native = Caprice32OptionFunctions.ToNative(configuration);

            Assert.Equal(expected, native.Options![Caprice32OptionConstants.Language]);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public void ConfiguredKeyboardMappingIsAppliedToRuntimeInput()
    {
        var configuration = new InputConfiguration(KeyboardMappings:
            new Dictionary<string, EmulationKey> { [nameof(EmulationKey.F1)] = EmulationKey.A });
        var physical = EmulationInputSnapshot.Empty with
        {
            Keys = new HashSet<EmulationKey> { EmulationKey.A }
        };

        var mapped = InputSnapshotFunctions.Apply(physical, configuration, false);

        Assert.Contains(EmulationKey.F1, mapped.Keys);
        Assert.DoesNotContain(EmulationKey.A, mapped.Keys);
    }
}
