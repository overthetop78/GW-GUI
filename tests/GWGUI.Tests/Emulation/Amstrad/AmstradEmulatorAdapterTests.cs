using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Net.Http;
using System.Text.Json;
using GWGUI.Emulation.Amstrad.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Amstrad.Common.Contracts;
using GWGUI.Emulation.Amstrad.Common.Machines.Common.Constants;
using GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;
using GWGUI.Emulation.Amstrad.Common.Machines.Common.Functions;
using GWGUI.Emulation.Amstrad.Common.Services;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Constants;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Functions;
using GWGUI.Emulation.Amstrad.Emulators.Caprice32.Interfaces;
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
        Assert.Equal(1, original.SchemaVersion);
        Assert.Equal(1, restored.SchemaVersion);
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

    [Fact]
    public async Task Caprice32CoreAndFirmwareUseTheirMachineSubdirectories()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var moduleDirectory = Path.Combine(directory, "Emulation", "Machines", "amstrad");
        var coreDirectory = Path.Combine(moduleDirectory, "Core", "caprice32");
        Directory.CreateDirectory(coreDirectory);
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(coreDirectory, "cap32_libretro.dll"), [0]);
            await File.WriteAllTextAsync(Path.Combine(coreDirectory, "core.json"),
                "{\"version\":\"test-version\"}");
            using var httpClient = new HttpClient();
            var factory = new AmstradEmulationModuleFactory();
            var module = Assert.IsType<AmstradEmulationModule>(factory.Create(
                new EmulationModuleContext(directory, moduleDirectory, httpClient)));

            var installation = await module.GetEmulatorInstallationAsync("cpc-6128");

            Assert.Equal("test-version", installation.InstalledVersion);
            Assert.True(Directory.Exists(Path.Combine(moduleDirectory, "Firmware")));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Caprice32UsesTheGenericReleaseSelectionContracts()
    {
        using var httpClient = new HttpClient(new ReleaseHandler());
        var module = new AmstradEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var configuration = module.CreateConfiguration("cpc-6128");

        var releases = await module.FindEmulatorReleasesAsync(configuration);

        var release = Assert.Single(releases);
        Assert.Equal("caprice32", Assert.Single(
            await module.GetEmulatorInstallationsAsync(configuration)).EmulatorId);
        Assert.StartsWith("official-", release.Id, StringComparison.Ordinal);
        Assert.Equal(release.Id, release.Version);
        Assert.True(release.IsRequired);
        Assert.Same(release, Assert.Single(releases, candidate => candidate.IsRequired));
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
        Assert.All(settings.AvailableDevices, device =>
            Assert.Equal(EmulationStorageConfigurationKind.None, device.ConfigurationKind));
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
        Assert.All(input.ControllerPorts, port => Assert.DoesNotContain(port.ControllerChoices,
            choice => choice.Id.Equals("Keyboard", StringComparison.OrdinalIgnoreCase)));
        Assert.True(Assert.IsType<MachineConfiguration>(configuration).Input!.CaptureMouse);
    }

    [Fact]
    public void Caprice32SettingsExposeOnlySupportedRomVideoAndAudioOptions()
    {
        using var httpClient = new HttpClient();
        var module = new AmstradEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var description = module.Describe("cpc-6128", module.CreateConfiguration("cpc-6128"));
        var fields = description.Blocks.SelectMany(block => block.Fields).ToDictionary(field => field.Id);

        var firmware = fields[SettingsConstants.FirmwareIntegrated];
        Assert.Equal(EmulationSettingsEditor.Information, firmware.Editor);
        Assert.Equal(SettingsDescriptionFunctionsConstants.Caprice32, firmware.Value);
        Assert.DoesNotContain(description.Blocks.SelectMany(block => block.Fields), field =>
            field.Tab == EmulationMachineTab.Rom && field.Editor != EmulationSettingsEditor.Information);

        var resolution = fields[SettingsConstants.VideoResolution];
        Assert.Equal([SettingsDescriptionFunctionsConstants.Resolution384,
            SettingsDescriptionFunctionsConstants.Resolution400],
            resolution.Choices!.Select(choice => choice.Id));
        Assert.Contains(SettingsConstants.VideoCrop, fields.Keys);
        Assert.Contains(SettingsConstants.FloppySound, fields.Keys);
        Assert.DoesNotContain(fields.Keys, key => key.Contains("volume", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AmstradConfigurationSummaryDoesNotDisplayAudioState()
    {
        using var httpClient = new HttpClient();
        var module = new AmstradEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var configuration = Assert.IsType<MachineConfiguration>(module.CreateConfiguration("cpc-6128"));

        Assert.Equal(module.SummarizeConfiguration(configuration with { AudioEnabled = true }).Details,
            module.SummarizeConfiguration(configuration with { AudioEnabled = false }).Details);
    }

    [Fact]
    public void AmstradSettingHelpIsDeclaredAndLocalized()
    {
        using var httpClient = new HttpClient();
        var module = new AmstradEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var description = module.Describe("cpc-6128", module.CreateConfiguration("cpc-6128"));
        var fields = description.Blocks.SelectMany(block => block.Fields).ToArray();
        foreach (var field in fields)
        {
            Assert.False(string.IsNullOrWhiteSpace(field.ExplanationResourceKey), field.Id);
            Assert.False(string.IsNullOrWhiteSpace(field.DetailedExplanationResourceKey), field.Id);
            if (!field.ExplanationResourceKey!.StartsWith("Emulation.Amstrad.",
                    StringComparison.Ordinal)) continue;
            Assert.True(module.TryGetString(field.ExplanationResourceKey!,
                CultureInfo.GetCultureInfo("fr-FR"), out var shortHelp));
            Assert.False(string.IsNullOrWhiteSpace(shortHelp));
            Assert.True(module.TryGetString(field.DetailedExplanationResourceKey!,
                CultureInfo.GetCultureInfo("fr-FR"), out var detailedHelp));
            Assert.False(string.IsNullOrWhiteSpace(detailedHelp));
        }
        Assert.NotEmpty(fields);
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

        Assert.Equal(ExternalHostCallbacksConstants.PointerCoordinateCenter
            + 10 * ExternalHostCallbacksConstants.PointerCoordinateScale, callbacks.InputState(0,
            ExternalHostCallbacksConstants.PointerDevice, 0,
            ExternalHostCallbacksConstants.PointerX));
        Assert.Equal(ExternalHostCallbacksConstants.PointerCoordinateCenter
            - 5 * ExternalHostCallbacksConstants.PointerCoordinateScale, callbacks.InputState(0,
            ExternalHostCallbacksConstants.PointerDevice, 0,
            ExternalHostCallbacksConstants.PointerY));
        Assert.Equal(1, callbacks.InputState(0,
            ExternalHostCallbacksConstants.PointerDevice, 0,
            ExternalHostCallbacksConstants.PointerPressed));

        callbacks.Input = EmulationInputSnapshot.Empty with
        {
            Pointer = EmulationInputSnapshot.Empty.Pointer with { DeltaX = -4, DeltaY = 3 }
        };
        callbacks.InputPoll();
        Assert.Equal(ExternalHostCallbacksConstants.PointerCoordinateCenter
            + 6 * ExternalHostCallbacksConstants.PointerCoordinateScale, callbacks.InputState(0,
            ExternalHostCallbacksConstants.PointerDevice, 0,
            ExternalHostCallbacksConstants.PointerX));
        Assert.Equal(ExternalHostCallbacksConstants.PointerCoordinateCenter
            - 2 * ExternalHostCallbacksConstants.PointerCoordinateScale, callbacks.InputState(0,
            ExternalHostCallbacksConstants.PointerDevice, 0,
            ExternalHostCallbacksConstants.PointerY));

        callbacks.Input = EmulationInputSnapshot.Empty;
        callbacks.InputPoll();
        Assert.Equal(ExternalHostCallbacksConstants.PointerCoordinateCenter
            + 6 * ExternalHostCallbacksConstants.PointerCoordinateScale, callbacks.InputState(0,
            ExternalHostCallbacksConstants.PointerDevice, 0,
            ExternalHostCallbacksConstants.PointerX));
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

    [Fact]
    public void CpcKeyboardListsOnlySpecialMachineKeysWithAutomaticHostBindings()
    {
        using var httpClient = new HttpClient();
        var module = new AmstradEmulationModule(Path.GetTempPath(), Path.GetTempPath(),
            httpClient, Path.GetTempPath());
        var keyboard = Assert.IsType<EmulationInputBindingSet>(module.DescribeInputSettings(
            module.CreateConfiguration("cpc-6128")).Keyboard);

        Assert.Contains(keyboard.Definitions, key => key.Id == nameof(EmulationKey.Backspace)
            && key.InvariantDisplayValue == "DEL");
        Assert.Contains(keyboard.Definitions, key => key.Id == nameof(EmulationKey.Delete)
            && key.InvariantDisplayValue == "CLR");
        Assert.Contains(keyboard.Definitions, key => key.Id == nameof(EmulationKey.RightAlt)
            && key.InvariantDisplayValue == "COPY");
        Assert.Contains(keyboard.Definitions, key => key.Id == nameof(EmulationKey.Numpad0)
            && key.InvariantDisplayValue == "F0");
        Assert.Contains(keyboard.Definitions, key => key.Id == nameof(EmulationKey.Numpad9)
            && key.InvariantDisplayValue == "F9");
        Assert.DoesNotContain(keyboard.Definitions, key => key.Id == nameof(EmulationKey.F1));
        Assert.DoesNotContain(keyboard.Definitions, key => key.Id == nameof(EmulationKey.A));
        Assert.DoesNotContain(keyboard.Definitions, key => key.Id == nameof(EmulationKey.D1));
        Assert.DoesNotContain(keyboard.Definitions, key => key.Id == nameof(EmulationKey.Return));
        Assert.DoesNotContain(keyboard.Definitions, key => key.Id == nameof(EmulationKey.LeftAlt));
        Assert.All(keyboard.Definitions, definition =>
        {
            Assert.True(Enum.TryParse<EmulationKey>(definition.Id, out var machineKey));
            Assert.Contains(machineKey, ExternalHostCallbacks.SupportedKeyboardKeys);
            Assert.True(Enum.TryParse<EmulationKey>(definition.DefaultBinding, out _));
        });
        var plusKeyboard = Assert.IsType<EmulationInputBindingSet>(module.DescribeInputSettings(
            module.CreateConfiguration("cpc-6128-plus")).Keyboard);
        Assert.Equal(keyboard.Definitions.Select(key => key.Id),
            plusKeyboard.Definitions.Select(key => key.Id));
        Assert.Null(module.DescribeInputSettings(module.CreateConfiguration("gx4000")).Keyboard);
    }

    [Fact]
    public void Caprice32DoesNotReceiveTheReservedFullscreenChord()
    {
        using var callbacks = new ExternalHostCallbacks(Path.GetTempPath(), Path.GetTempPath(),
            Path.GetTempPath(), null);
        var alt = new HashSet<EmulationKey> { EmulationKey.LeftAlt };
        var chord = new HashSet<EmulationKey> { EmulationKey.LeftAlt, EmulationKey.Return };

        Assert.Contains(EmulationKey.LeftAlt, callbacks.FilterReservedKeyboardChord(alt));
        Assert.Empty(callbacks.FilterReservedKeyboardChord(chord));
        Assert.Empty(callbacks.FilterReservedKeyboardChord(alt));
        Assert.Empty(callbacks.FilterReservedKeyboardChord(new HashSet<EmulationKey>()));
        Assert.Contains(EmulationKey.LeftAlt, callbacks.FilterReservedKeyboardChord(alt));
    }

    [Fact]
    public void CapturedMouseNeverInjectsJoystickButtons()
    {
        using var callbacks = new ExternalHostCallbacks(Path.GetTempPath(), Path.GetTempPath(),
            Path.GetTempPath(), null);
        callbacks.Input = EmulationInputSnapshot.Empty with
        {
            Pointer = EmulationInputSnapshot.Empty.Pointer with
            {
                DeltaX = -3,
                DeltaY = 2,
                Left = true,
                Right = true
            }
        };
        callbacks.InputPoll();
        var buttons = unchecked((ushort)callbacks.InputState(0,
            ExternalHostCallbacksConstants.JoypadDevice, 0,
            ExternalHostCallbacksConstants.JoypadMask));

        Assert.Equal(0, buttons);
    }

    [Fact]
    public async Task ColdResetUsesTheCoreColdRestartCommand()
    {
        var core = new RecordingCore();
        var configuration = new MachineConfiguration("cpc-6128", "caprice32");
        var sessionDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var machine = new Machine(configuration.Id, configuration, core, [], sessionDirectory);

        try
        {
            await machine.StartAsync();
            await machine.HardResetAsync();

            Assert.Equal(1, core.HardResetCount);
            Assert.Equal(0, core.SoftResetCount);
        }
        finally
        {
            await machine.DisposeAsync();
        }
        Assert.True(core.Disposed);
    }

    private sealed class RecordingCore : IEmulatorCore
    {
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<CoreOption> Options => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public string CoreName => "test";
        public string CoreVersion => "1";
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public string CoreSha256 => "test";
        public double FramesPerSecond => 50;
        public int SampleRate => 44_100;
        public int DiskCount => 0;
        public int CurrentDiskIndex => -1;
        public int HardResetCount { get; private set; }
        public int SoftResetCount { get; private set; }
        public bool Disposed { get; private set; }

        public void Initialize(MachineConfiguration configuration, string sessionDirectory,
            string? saveDirectory = null) { }
        public void RunFrame() => Thread.Sleep(1);
        public void HardReset() => HardResetCount++;
        public void SoftReset() => SoftResetCount++;
        public void Stop() { }
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void InsertMedia(string path) { }
        public void EjectMedia() { }
        public void SelectDisk(int index) { }
        public byte[] SaveState() => [];
        public void LoadState(ReadOnlySpan<byte> state) { }
        public void SetOption(string key, string value) { }
        public bool TryDequeueAudio(out AudioChunk? chunk) { chunk = null; return false; }
        public void Dispose() => Disposed = true;
    }

    private sealed class ReleaseHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([]),
                RequestMessage = request
            };
            response.Content.Headers.LastModified = new DateTimeOffset(2026, 9, 26, 12, 0, 0,
                TimeSpan.Zero);
            return Task.FromResult(response);
        }
    }
}
