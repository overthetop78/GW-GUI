using System.Reflection;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using GWGUI.Emulation.Amiga.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Amiga.Common.Machines.Common.Enums;
using GWGUI.Emulation.Amiga.Modules;
using GWGUI.Emulation.Amiga.Common.Services;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Exceptions;
using GWGUI.App.Localization.Extensions;

namespace GWGUI.Tests.Emulation.Amiga;

public sealed class AmigaEmulatorAdapterTests
{
    [Fact]
    public void MachineConfigurationCrossesTheCoreHostJsonBoundary()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var original = new MachineConfiguration("A500", "kickstart.rom", Core: Emulator.External);

        var restored = JsonSerializer.Deserialize<MachineConfiguration>(
            JsonSerializer.Serialize(original, options), options);

        Assert.NotNull(restored);
        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.Model, restored.Model);
        Assert.Equal(original.Core, restored.Core);
        Assert.Equal(original.KickstartPath, restored.KickstartPath);
    }

    [Fact]
    public void PuaeUsesItsPhysicalNamespaceAndTheCommonAdapter()
    {
        var assembly = typeof(AmigaEmulationModule).Assembly;
        var adapter = assembly.GetType("GWGUI.Emulation.Amiga.Emulators.PUAE.Factories.PuaeMachineFactory");
        var common = assembly.GetType("GWGUI.Emulation.Amiga.Common.Interfaces.IEmulatorAdapter");
        Assert.NotNull(adapter);
        Assert.NotNull(common);
        Assert.Contains(common!, adapter!.GetInterfaces());
        var required = new[] { "Create", "FindInstalledCorePathAsync", "FindReleasesAsync",
            "GetInstallationAsync", "InstallAsync", "ResolveConfiguredMedia", "TryHandleHostCommand" };
        Assert.All(required, name => Assert.Contains(common!.GetMethods(), method => method.Name == name));
    }

    [Fact]
    public void EngineRegistersPuaeOnlyThroughTheCommonAdapter()
    {
        var field = typeof(Engine).GetField("_adapters", BindingFlags.Instance | BindingFlags.NonPublic);
        var adapters = Assert.IsAssignableFrom<System.Collections.IEnumerable>(field!.GetValue(new Engine()));
        var entries = adapters.Cast<object>().ToArray();
        var entry = Assert.Single(entries);
        Assert.Equal("puae", entry.GetType().GetProperty("Key")!.GetValue(entry));
    }

    [Fact]
    public async Task PuaeCoreAndFirmwareUseTheirMachineSubdirectories()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var moduleDirectory = Path.Combine(directory, "Emulation", "Machines", "amiga");
        var coreDirectory = Path.Combine(moduleDirectory, "Core", "puae");
        Directory.CreateDirectory(coreDirectory);
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(coreDirectory, "puae_libretro.dll"), [0]);
            await File.WriteAllTextAsync(Path.Combine(coreDirectory, "core.json"),
                "{\"version\":\"test-version\"}");
            using var httpClient = new HttpClient();
            var factory = new AmigaEmulationModuleFactory();
            var module = Assert.IsType<AmigaEmulationModule>(factory.Create(
                new EmulationModuleContext(directory, moduleDirectory, httpClient)));

            var installation = await module.GetEmulatorInstallationAsync("A500");

            Assert.Equal("test-version", installation.InstalledVersion);
            Assert.True(Directory.Exists(Path.Combine(moduleDirectory, "Firmware")));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task ConfigurationCanBeCreatedBeforeSelectingKickstart()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using var httpClient = new HttpClient();
            var module = new AmigaEmulationModule(directory, directory, httpClient, directory);
            var configuration = module.CreateConfiguration("A600");

            await module.SaveConfigurationAsync(configuration);

            Assert.Single(await module.LoadConfigurationsAsync());
            var error = await Assert.ThrowsAsync<EmulationMessageException>(() =>
                module.CreateRuntimeAsync(configuration, null!).AsTask());
            Assert.Equal(EmulationMessageCode.FirmwareMissing, error.MessageData.MessageCode);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void PuaeDeclaresOnlyItsHardwareConfigurationDialogs()
    {
        using var httpClient = new HttpClient();
        var module = new AmigaEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var storage = module.DescribeStorageSettings(module.CreateConfiguration("A1200"));

        Assert.All(storage.AvailableDevices, device => Assert.Equal(
            device.MediaType switch
            {
                EmulationMediaType.Floppy => EmulationStorageConfigurationKind.FloppyDrive,
                EmulationMediaType.HardDisk => EmulationStorageConfigurationKind.HardDiskDrive,
                _ => EmulationStorageConfigurationKind.None
            }, device.ConfigurationKind));
    }

    [Fact]
    public void AmigaKeyboardListsOnlyUniqueMachineKeysWithAutomaticHostBindings()
    {
        using var httpClient = new HttpClient();
        var module = new AmigaEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var keyboard = Assert.IsType<EmulationInputBindingSet>(module.DescribeInputSettings(
            module.CreateConfiguration("A500")).Keyboard);

        Assert.Equal([nameof(EmulationKey.Help), nameof(EmulationKey.LeftAmiga),
            nameof(EmulationKey.RightAmiga)], keyboard.Definitions.Select(key => key.Id));
        Assert.Equal(nameof(EmulationKey.Insert), keyboard.Definitions.Single(
            key => key.Id == nameof(EmulationKey.Help)).DefaultBinding);
        Assert.Equal(nameof(EmulationKey.PageUp), keyboard.Definitions.Single(
            key => key.Id == nameof(EmulationKey.LeftAmiga)).DefaultBinding);
        Assert.Equal(nameof(EmulationKey.PageDown), keyboard.Definitions.Single(
            key => key.Id == nameof(EmulationKey.RightAmiga)).DefaultBinding);
        Assert.All(keyboard.Definitions, definition =>
        {
            Assert.True(Enum.TryParse<EmulationKey>(definition.Id, out _));
            Assert.True(Enum.TryParse<EmulationKey>(definition.DefaultBinding, out _));
        });
        Assert.Null(module.DescribeInputSettings(module.CreateConfiguration("CD32")).Keyboard);
    }

    [Fact]
    public void AmigaConfigurationSummaryDoesNotDisplayAudioState()
    {
        using var httpClient = new HttpClient();
        var module = new AmigaEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration("A600"));

        Assert.Equal(module.SummarizeConfiguration(configuration with { AudioEnabled = true }).Details,
            module.SummarizeConfiguration(configuration with { AudioEnabled = false }).Details);
    }

    [Fact]
    public void EveryAmigaMachineSettingDeclaresLocalizedHelp()
    {
        using var httpClient = new HttpClient();
        var module = new AmigaEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
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
