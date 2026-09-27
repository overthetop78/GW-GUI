using System.Net.Http;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Atari.Common.Machines.Common.Enums;
using GWGUI.Emulation.Atari.Emulators.Libretro.Dictionaries;
using GWGUI.Emulation.Atari.Modules;
using GWGUI.Emulation.Atari.Common.Services;
using GWGUI.Emulation.Atari.Common.Machines.AtariST.Dictionaries;
using GWGUI.Emulation.Atari.Common.Machines.AtariST.Enums;
using GWGUI.App.Localization.Extensions;

namespace GWGUI.Tests.Emulation.Atari;

public sealed class AtariEmulatorAdapterTests
{
    [Fact]
    public void MachineConfigurationCrossesTheCoreHostJsonBoundary()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var original = new MachineConfiguration(MachineModel.Atari800Xl);

        var restored = JsonSerializer.Deserialize<MachineConfiguration>(
            JsonSerializer.Serialize(original, options), options);

        Assert.NotNull(restored);
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.Model, restored.Model);
        Assert.Equal(original.Core, restored.Core);
    }

    [Fact]
    public void ConcreteAdaptersUseTheirPhysicalNamespaces()
    {
        var names = typeof(AtariEmulationModule).Assembly.GetTypes().Select(type => type.FullName).ToHashSet();
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.Hatari.Factories.HatariMachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.Atari800.Factories.Atari800MachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.Stella.Factories.StellaMachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.ProSystem.Factories.ProSystemMachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.BeetleLynx.Factories.BeetleLynxMachineFactory", names);
        Assert.Contains("GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Factories.VirtualJaguarMachineFactory", names);
    }

    [Fact]
    public void EngineRegistrationsMatchTheCatalog()
    {
        var field = typeof(Engine).GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic);
        var adapters = Assert.IsAssignableFrom<System.Collections.IEnumerable>(field!.GetValue(new Engine()));
        var ids = adapters.Cast<object>().Select(item => item.GetType().GetProperty("Key")!.GetValue(item) as string)
            .Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(CoreCatalog.All.Select(item => item.Id).Order(StringComparer.Ordinal), ids);
        foreach (var entry in CoreCatalog.All)
        {
            var adapter = adapters.Cast<object>().Single(item =>
                string.Equals(item.GetType().GetProperty("Key")!.GetValue(item) as string,
                    entry.Id, StringComparison.Ordinal));
            var value = adapter.GetType().GetProperty("Value")!.GetValue(adapter)!;
            var definition = Assert.IsType<EmulationEmulatorDefinition>(
                value.GetType().GetProperty("Definition")!.GetValue(value));
            Assert.Equal(CoreCatalog.GetDefinition(entry).MachineIds.Order(StringComparer.Ordinal),
                definition.MachineIds.Order(StringComparer.Ordinal));
        }
    }

    [Theory]
    [InlineData("fr-FR", StRegion.France)]
    [InlineData("es-ES", StRegion.Spain)]
    [InlineData("de-DE", StRegion.Germany)]
    [InlineData("en-GB", StRegion.UnitedKingdom)]
    [InlineData("pt-PT", StRegion.UnitedStates)]
    public void AtariStRegionDefaultsToTheApplicationLanguage(string cultureName,
        StRegion expected)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            Assert.Equal(expected, StModelCatalog.Get(MachineModel.St).DefaultRegion);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void AtariEmulatorsDeclareOnlyTheirHardwareConfigurationDialogs()
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());

        foreach (var machine in module.Machines)
        {
            var storage = module.DescribeStorageSettings(module.CreateConfiguration(machine.Id));
            Assert.All(storage.AvailableDevices, device => Assert.Equal(
                device.MediaType switch
                {
                    EmulationMediaType.Floppy => EmulationStorageConfigurationKind.FloppyDrive,
                    EmulationMediaType.HardDisk => EmulationStorageConfigurationKind.HardDiskDrive,
                    _ => EmulationStorageConfigurationKind.None
                }, device.ConfigurationKind));
        }
    }

    [Fact]
    public void AtariComputerKeyboardsListOnlyUniqueMachineKeysWithAutomaticHostBindings()
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var tos = Assert.IsType<EmulationInputBindingSet>(module.DescribeInputSettings(
            module.CreateConfiguration(nameof(MachineModel.St))).Keyboard);
        var eightBit = Assert.IsType<EmulationInputBindingSet>(module.DescribeInputSettings(
            module.CreateConfiguration(nameof(MachineModel.Atari800Xl))).Keyboard);

        Assert.DoesNotContain(tos.Definitions, key => key.Id == nameof(EmulationKey.A));
        Assert.DoesNotContain(tos.Definitions, key => key.Id == nameof(EmulationKey.D1));
        Assert.Contains(tos.Definitions, key => key.Id == nameof(EmulationKey.Help));
        Assert.Contains(tos.Definitions, key => key.Id == nameof(EmulationKey.Undo));
        Assert.Contains(tos.Definitions, key => key.Id == nameof(EmulationKey.Break));
        Assert.DoesNotContain(eightBit.Definitions, key => key.Id == nameof(EmulationKey.A));
        Assert.DoesNotContain(eightBit.Definitions, key => key.Id == nameof(EmulationKey.D1));
        Assert.Contains(eightBit.Definitions, key => key.Id == nameof(EmulationKey.AtariOption));
        Assert.Contains(eightBit.Definitions, key => key.Id == nameof(EmulationKey.AtariSelect));
        Assert.Contains(eightBit.Definitions, key => key.Id == nameof(EmulationKey.AtariStart));
        Assert.DoesNotContain(eightBit.Definitions, key => key.Id == nameof(EmulationKey.Numpad0));
        Assert.All(tos.Definitions.Concat(eightBit.Definitions), definition =>
        {
            Assert.True(Enum.TryParse<EmulationKey>(definition.Id, out _));
            Assert.True(Enum.TryParse<EmulationKey>(definition.DefaultBinding, out _));
        });
    }

    [Fact]
    public void AtariConfigurationSummaryDoesNotDisplayAudioState()
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var configuration = Assert.IsType<MachineConfiguration>(
            module.CreateConfiguration(nameof(MachineModel.Atari800Xl)));

        Assert.Equal(module.SummarizeConfiguration(configuration with { AudioEnabled = true }).Details,
            module.SummarizeConfiguration(configuration with { AudioEnabled = false }).Details);
    }

    [Fact]
    public void EveryAtariMachineSettingDeclaresLocalizedHelp()
    {
        using var httpClient = new HttpClient();
        var module = new AtariEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());

        var missing = new HashSet<string>(StringComparer.Ordinal);
        foreach (var machine in module.Machines)
        foreach (var field in module.Describe(machine.Id,
                     module.CreateConfiguration(machine.Id)).Blocks.SelectMany(block => block.Fields))
        {
            if (string.IsNullOrWhiteSpace(field.ExplanationResourceKey))
                missing.Add($"{field.Id}:short-key");
            else if (LocExtension.GetForModule(module, field.ExplanationResourceKey)
                     is var shortHelp && (string.IsNullOrWhiteSpace(shortHelp)
                         || shortHelp == $"[{field.ExplanationResourceKey}]"))
                missing.Add($"{field.Id}:short-text");
            if (string.IsNullOrWhiteSpace(field.DetailedExplanationResourceKey))
                missing.Add($"{field.Id}:detailed-key");
            else if (LocExtension.GetForModule(module, field.DetailedExplanationResourceKey)
                     is var detailedHelp && (string.IsNullOrWhiteSpace(detailedHelp)
                         || detailedHelp == $"[{field.DetailedExplanationResourceKey}]"))
                missing.Add($"{field.Id}:detailed-text");
        }
        Assert.True(missing.Count == 0, string.Join(Environment.NewLine, missing));
    }
}
