using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;
using GWGUI.MediaEngine.Containers.ImageDisk;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

[Trait("Category", "LocalAssets")]
[Collection(AtariNativeCoreTestConstants.CollectionName)]
public sealed class AtariExternalCoreProbeTests
{
    public static TheoryData<string, AtariEmulator> OfficialCoreFiles => new()
    {
        { "hatari.dll", AtariEmulator.Hatari },
        { "atari800.dll", AtariEmulator.Atari800 },
        { "stella.dll", AtariEmulator.Stella },
        { "prosystem.dll", AtariEmulator.ProSystem },
        { "beetle-lynx.dll", AtariEmulator.BeetleLynx },
        { "virtual-jaguar.dll", AtariEmulator.VirtualJaguar }
    };

    [Theory]
    [MemberData(nameof(OfficialCoreFiles))]
    public void OfficialCore_ExportsExpectedAbiAndIdentity(string fileName, AtariEmulator kind)
    {
        var info = AtariExternalCoreProbe.Inspect(Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", fileName), kind);

        Assert.Equal(kind, info.Emulator);
        Assert.Equal(AtariCoreFunctions.ExpectedLibraryName(kind), info.LibraryName, ignoreCase: true);
        Assert.False(string.IsNullOrWhiteSpace(info.LibraryVersion));
        Assert.NotEmpty(info.Extensions);
    }

    [Theory]
    [MemberData(nameof(OfficialCoreFiles))]
    public void OfficialCore_AdapterCanBeCreatedAndDisposedTwice(string fileName, AtariEmulator kind)
    {
        var core = new AtariExternalCore(Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", fileName), kind);

        core.Dispose();
        core.Dispose();

        Assert.Equal(kind, core.Emulator);
        Assert.Equal(AtariConstants.Sha256HexLength, core.CoreSha256.Length);
    }

    [Fact]
    public void RelativePath_IsRejectedWithStructuredCoreError()
    {
        var error = Assert.Throws<AtariEmulationException>(() =>
            AtariExternalCoreProbe.Inspect("hatari.dll", AtariEmulator.Hatari));

        Assert.Equal(AtariErrorCategory.Core, error.Category);
        Assert.Equal(AtariErrorCode.CoreRejected, error.Code);
    }

    [Fact]
    public void MissingFile_IsRejectedWithStructuredCoreError()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.dll");
        var error = Assert.Throws<AtariEmulationException>(() =>
            AtariExternalCoreProbe.Inspect(path, AtariEmulator.Hatari));

        Assert.Equal(AtariErrorCode.CoreNotFound, error.Code);
    }

    [Fact]
    public void RealCoreWithUnexpectedIdentity_IsRejectedWithExpectedAndActualContext()
    {
        var path = Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", "atari800.dll");

        var error = Assert.Throws<AtariEmulationException>(() =>
            AtariExternalCoreProbe.Inspect(path, AtariEmulator.Hatari));

        Assert.Equal(AtariErrorCode.CoreRejected, error.Code);
        Assert.Equal(AtariCoreIdentityConstants.Hatari, error.Context[AtariConstants.ExpectedContextKey]);
        Assert.Equal(AtariCoreIdentityConstants.Atari800, error.Context[AtariConstants.ActualContextKey]);
    }

    [Fact]
    public void LibraryMissingRequiredExports_IsRejectedAndReleased()
    {
        var path = Path.Combine(Environment.SystemDirectory, "version.dll");
        var error = Assert.Throws<AtariEmulationException>(() =>
            AtariExternalCoreProbe.Inspect(path, AtariEmulator.Hatari));

        Assert.Equal(AtariErrorCode.CoreRejected, error.Code);
        Assert.Equal(AtariErrorMessages.CoreExportMissing, error.Message);
    }

    [Fact]
    public void SystemInfoAndGameInfo_UseExpectedX64Offsets()
    {
        Assert.Equal(nint.Zero, Marshal.OffsetOf<ExternalCoreApi.SystemInfo>(nameof(ExternalCoreApi.SystemInfo.LibraryName)));
        Assert.Equal((nint)IntPtr.Size, Marshal.OffsetOf<ExternalCoreApi.SystemInfo>(nameof(ExternalCoreApi.SystemInfo.LibraryVersion)));
        Assert.Equal((nint)(IntPtr.Size * 2), Marshal.OffsetOf<ExternalCoreApi.SystemInfo>(nameof(ExternalCoreApi.SystemInfo.ValidExtensions)));
        Assert.Equal(nint.Zero, Marshal.OffsetOf<ExternalCoreApi.GameInfo>(nameof(ExternalCoreApi.GameInfo.Path)));
        Assert.Equal((nint)(IntPtr.Size * 3), Marshal.OffsetOf<ExternalCoreApi.GameInfo>(nameof(ExternalCoreApi.GameInfo.Metadata)));
    }

    [Fact]
    public void AllAdapterStructures_UseExpectedX64Layout()
    {
        Assert.Equal(ExternalCoreApiConstants.X64PointerSize, IntPtr.Size);
        AssertLayout<ExternalCoreApi.SystemInfo>(ExternalCoreApiConstants.SystemInfoSizeX64,
            (nameof(ExternalCoreApi.SystemInfo.LibraryName), ExternalCoreApiConstants.SystemInfoLibraryNameOffsetX64),
            (nameof(ExternalCoreApi.SystemInfo.LibraryVersion), ExternalCoreApiConstants.SystemInfoLibraryVersionOffsetX64),
            (nameof(ExternalCoreApi.SystemInfo.ValidExtensions), ExternalCoreApiConstants.SystemInfoExtensionsOffsetX64),
            (nameof(ExternalCoreApi.SystemInfo.NeedFullPath), ExternalCoreApiConstants.SystemInfoNeedFullPathOffsetX64),
            (nameof(ExternalCoreApi.SystemInfo.BlockExtract), ExternalCoreApiConstants.SystemInfoBlockExtractOffsetX64));
        AssertLayout<ExternalCoreApi.GameInfo>(ExternalCoreApiConstants.GameInfoSizeX64,
            (nameof(ExternalCoreApi.GameInfo.Path), ExternalCoreApiConstants.GameInfoPathOffsetX64),
            (nameof(ExternalCoreApi.GameInfo.Data), ExternalCoreApiConstants.GameInfoDataOffsetX64),
            (nameof(ExternalCoreApi.GameInfo.Size), ExternalCoreApiConstants.GameInfoSizeOffsetX64),
            (nameof(ExternalCoreApi.GameInfo.Metadata), ExternalCoreApiConstants.GameInfoMetadataOffsetX64));
        AssertLayout<ExternalCoreApi.Geometry>(ExternalCoreApiConstants.GeometrySizeX64,
            (nameof(ExternalCoreApi.Geometry.BaseWidth), ExternalCoreApiConstants.GeometryBaseWidthOffsetX64),
            (nameof(ExternalCoreApi.Geometry.BaseHeight), ExternalCoreApiConstants.GeometryBaseHeightOffsetX64),
            (nameof(ExternalCoreApi.Geometry.MaximumWidth), ExternalCoreApiConstants.GeometryMaximumWidthOffsetX64),
            (nameof(ExternalCoreApi.Geometry.MaximumHeight), ExternalCoreApiConstants.GeometryMaximumHeightOffsetX64),
            (nameof(ExternalCoreApi.Geometry.AspectRatio), ExternalCoreApiConstants.GeometryAspectRatioOffsetX64));
        AssertLayout<ExternalCoreApi.Timing>(ExternalCoreApiConstants.TimingSizeX64,
            (nameof(ExternalCoreApi.Timing.FramesPerSecond), ExternalCoreApiConstants.TimingFramesPerSecondOffsetX64),
            (nameof(ExternalCoreApi.Timing.SampleRate), ExternalCoreApiConstants.TimingSampleRateOffsetX64));
        AssertLayout<ExternalCoreApi.SystemAvInfo>(ExternalCoreApiConstants.SystemAvInfoSizeX64,
            (nameof(ExternalCoreApi.SystemAvInfo.Geometry), ExternalCoreApiConstants.SystemAvInfoGeometryOffsetX64),
            (nameof(ExternalCoreApi.SystemAvInfo.Timing), ExternalCoreApiConstants.SystemAvInfoTimingOffsetX64));
        AssertLayout<ExternalCoreApi.Variable>(ExternalCoreApiConstants.VariableSizeX64,
            (nameof(ExternalCoreApi.Variable.Key), ExternalCoreApiConstants.VariableKeyOffsetX64),
            (nameof(ExternalCoreApi.Variable.Value), ExternalCoreApiConstants.VariableValueOffsetX64));
        AssertLayout<ExternalCoreApi.Message>(ExternalCoreApiConstants.MessageSizeX64,
            (nameof(ExternalCoreApi.Message.Text), ExternalCoreApiConstants.MessageTextOffsetX64),
            (nameof(ExternalCoreApi.Message.Frames), ExternalCoreApiConstants.MessageFramesOffsetX64));
        AssertLayout<ExternalCoreApi.MessageExtended>(ExternalCoreApiConstants.MessageExtendedSizeX64,
            (nameof(ExternalCoreApi.MessageExtended.Text), ExternalCoreApiConstants.MessageExtendedTextOffsetX64),
            (nameof(ExternalCoreApi.MessageExtended.DurationMilliseconds), ExternalCoreApiConstants.MessageExtendedDurationOffsetX64),
            (nameof(ExternalCoreApi.MessageExtended.Priority), ExternalCoreApiConstants.MessageExtendedPriorityOffsetX64),
            (nameof(ExternalCoreApi.MessageExtended.Level), ExternalCoreApiConstants.MessageExtendedLevelOffsetX64),
            (nameof(ExternalCoreApi.MessageExtended.Target), ExternalCoreApiConstants.MessageExtendedTargetOffsetX64),
            (nameof(ExternalCoreApi.MessageExtended.Type), ExternalCoreApiConstants.MessageExtendedTypeOffsetX64),
            (nameof(ExternalCoreApi.MessageExtended.Progress), ExternalCoreApiConstants.MessageExtendedProgressOffsetX64));
        Assert.Equal(ExternalCoreApiConstants.FunctionPointerInterfaceSizeX64, Marshal.SizeOf<ExternalCoreApi.LogInterface>());
        Assert.Equal(ExternalCoreApiConstants.FunctionPointerInterfaceSizeX64, Marshal.SizeOf<ExternalCoreApi.LedInterface>());
    }

    private static void AssertLayout<T>(int expectedSize, params (string Field, int Offset)[] fields)
    {
        Assert.Equal(expectedSize, Marshal.SizeOf<T>());
        Assert.All(fields, field => Assert.Equal((nint)field.Offset, Marshal.OffsetOf<T>(field.Field)));
    }

    [Fact]
    public void Atari800_InitializesRunsStopsAndDisposesTwice()
    {
        var corePath = Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", "atari800.dll");
        var sessionDirectory = Path.Combine(Path.GetTempPath(), $"gwgui-atari-{Guid.NewGuid():N}");
        Directory.CreateDirectory(sessionDirectory);
        try
        {
            var core = new AtariExternalCore(corePath, AtariEmulator.Atari800);
            core.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari800), sessionDirectory);

            core.RunFrame();
            var state = core.SaveState();
            core.LoadState(state);
            core.HardReset();
            core.Stop();
            core.Stop();
            core.Dispose();
            core.Dispose();

            Assert.Equal(AtariEmulator.Atari800, core.Emulator);
            Assert.False(string.IsNullOrWhiteSpace(core.CoreSha256));
        }
        finally
        {
            Directory.Delete(sessionDirectory, recursive: true);
        }
    }

    [Fact]
    [Trait("Category", "LocalAssets")]
    public void Atari400AcceptsEveryConfiguredNativeOptionExposedByTheApplication()
    {
        var corePath = Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", "atari800.dll");
        var sessionDirectory = Path.Combine(Path.GetTempPath(), $"gwgui-atari400-options-{Guid.NewGuid():N}");
        Directory.CreateDirectory(sessionDirectory);
        try
        {
            var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari400, options:
                new Dictionary<string, string>
                {
                    [AtariConfigurationOptionConstants.VideoStandard] = AtariClassicRegion.Pal.ToString(),
                    [AtariEightBitSettingsConstants.ArtifactingModeOptionKey] = "GTIA",
                    [AtariEightBitSettingsConstants.ColorHueOptionKey] = "0.50",
                    [AtariEightBitSettingsConstants.ColorSaturationOptionKey] = "-0.50",
                    [AtariEightBitSettingsConstants.ColorContrastOptionKey] = "1.00",
                    [AtariEightBitSettingsConstants.ColorBrightnessOptionKey] = "-1.00",
                    [AtariEightBitSettingsConstants.ColorGammaOptionKey] = "2.35",
                    [AtariEightBitSettingsConstants.ColorDelayOptionKey] = "23.00",
                    [AtariEightBitSettingsConstants.ExternalPaletteOptionKey] = "real",
                    [AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey] = "Joy 2B+",
                    [AtariEightBitSettingsConstants.DigitalSensitivityOptionKey] = "55",
                    [AtariEightBitSettingsConstants.AnalogSensitivityOptionKey] = "80",
                    [AtariEightBitSettingsConstants.Os400800OptionKey] = "Rev. B NTSC",
                    [AtariEightBitSettingsConstants.BasicEnabledOptionKey] = "disabled",
                    [AtariEightBitSettingsConstants.BasicVersionOptionKey] = "Altirra BASIC",
                    [AtariEightBitSettingsConstants.MosaicMemoryOptionKey] = "80 KB",
                    [AtariEightBitSettingsConstants.AxlonMemoryOptionKey] = "disabled",
                    [AtariEightBitSettingsConstants.ShowSpeedOptionKey] = "enabled",
                    [AtariEightBitSettingsConstants.ShowActivityOptionKey] = "enabled",
                    [AtariEightBitSettingsConstants.ShowSectorOptionKey] = "enabled",
                    [AtariEightBitSettingsConstants.RealTimeClockOptionKey] = "enabled",
                    [AtariEightBitSettingsConstants.PrinterDeviceOptionKey] = "enabled",
                    [AtariEightBitSettingsConstants.SerialDeviceOptionKey] = "enabled",
                    [AtariEightBitSettingsConstants.SioAccelerationOptionKey] = "enabled",
                    [AtariEightBitSettingsConstants.CassetteBootOptionKey] = "disabled",
                    [AtariEightBitSettingsConstants.PokeyStereoOptionKey] = "disabled"
                });
            using var core = new AtariExternalCore(corePath, AtariEmulator.Atari800);

            core.Initialize(configuration, sessionDirectory);
            core.RunFrame();

            Assert.NotEmpty(core.Options);
        }
        finally
        {
            Directory.Delete(sessionDirectory, recursive: true);
        }
    }

    [Fact]
    public void FailedContentInitialization_CleansInitializedStagesAndAllowsRetry()
    {
        var corePath = Path.Combine(FindRepositoryRoot(), "tmp", "atari-cores", "atari800.dll");
        var sessionDirectory = Path.Combine(Path.GetTempPath(), $"gwgui-atari-retry-{Guid.NewGuid():N}");
        var unsupportedContent = Path.Combine(sessionDirectory, "unsupported.bad");
        Directory.CreateDirectory(sessionDirectory);
        File.WriteAllBytes(unsupportedContent, [AtariConstants.NativeBooleanTrue]);
        try
        {
            using var core = new AtariExternalCore(corePath, AtariEmulator.Atari800);
            var invalidConfiguration = new AtariMachineConfiguration(AtariMachineModel.Atari800,
                media: [new AtariMediaConfiguration(unsupportedContent, AtariMediaCategory.Cartridge,
                    GWGUI.Emulation.Contracts.EmulationMediaSlot.Cartridge0)]);

            var error = Assert.Throws<AtariEmulationException>(() =>
                core.Initialize(invalidConfiguration, sessionDirectory));
            core.Initialize(new AtariMachineConfiguration(AtariMachineModel.Atari800), sessionDirectory);
            core.Stop();

            Assert.Equal(AtariErrorCode.ContentUnsupported, error.Code);
        }
        finally
        {
            Directory.Delete(sessionDirectory, recursive: true);
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "GWGUI.sln"))) current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
