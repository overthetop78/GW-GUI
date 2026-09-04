using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using GWGUI.Emulation.Amiga.Contracts;
using GWGUI.Emulation.Amiga.Modules;
using GWGUI.Emulation.Amiga.Services;
using GWGUI.Emulation.Atari.Contracts;
using GWGUI.Emulation.Atari.Enums;
using GWGUI.Emulation.Atari.Functions;
using GWGUI.Emulation.Atari.Services;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests;

public sealed class EmulationVideoConfigurationTests
{
    [Fact]
    public void AmigaPartialSettingsPreserveValuesOwnedByOtherTabs()
    {
        using var temporary = new TemporaryDirectory();
        using var httpClient = new HttpClient();
        var module = new AmigaEmulationModule(temporary.Path, temporary.Path, httpClient,
            temporary.Path);
        var expected = AmigaMachineConfiguration.A500("kick.rom") with
        {
            ExtendedRomPath = "extended.rom",
            RomKeyPath = "rom.key",
            AudioEnabled = false,
            Audio = new AmigaAudioConfiguration(OutputDeviceId: "device"),
            Input = new AmigaInputConfiguration(ParallelJoystickAdapterEnabled: true)
        };

        var actual = Assert.IsType<AmigaMachineConfiguration>(module.ApplySettings(expected,
            new Dictionary<string, string?>
            {
                ["gfx_scanlines"] = "25"
            }));

        Assert.Equal(expected.KickstartPath, actual.KickstartPath);
        Assert.Equal(expected.ExtendedRomPath, actual.ExtendedRomPath);
        Assert.Equal(expected.RomKeyPath, actual.RomKeyPath);
        Assert.Equal(expected.AudioEnabled, actual.AudioEnabled);
        Assert.Equal(expected.Audio.OutputDeviceId, actual.Audio!.OutputDeviceId);
        Assert.Equal(expected.Input.ParallelJoystickAdapterEnabled,
            actual.Input!.ParallelJoystickAdapterEnabled);
    }

    [Fact]
    public void Normalize_ReturnsNeutralDefaultsAndClampsInvalidValues()
    {
        var neutral = EmulationVideoProcessingConfigurationFunctions.Normalize(null);

        Assert.Equal(EmulationVideoDisplayTechnology.Normal, neutral.DisplayTechnology);
        Assert.Equal(EmulationVideoSampling.Nearest, neutral.Sampling);
        Assert.Equal(new EmulationImageAdjustments(), neutral.Adjustments);
        Assert.Equal(new EmulationImageRestorationConfiguration(), neutral.Restoration);

        var normalized = EmulationVideoProcessingConfigurationFunctions.Normalize(new()
        {
            DisplayTechnology = (EmulationVideoDisplayTechnology)999,
            Sampling = (EmulationVideoSampling)999,
            Adjustments = new(-20, 20, 5, -11, 11),
            Restoration = new(Dedithering: 101, Denoising: -1, Debanding: 102,
                DetailRecovery: -2, Deinterlacing: (EmulationDeinterlacingMode)999),
            Temporal = new(GeneralPersistence: 101, MotionBlur: -1, Flicker: 102,
                Interlacing: -2, InterlacingVisibility: 102),
            SignalSimulation = new((EmulationSignalConnection)999, 101,
                (EmulationSignalStandard)999, 103),
            Stylistic = new(Grain: 101, Vhs: -1, ChromaticAberration: 102, Bloom: -2,
                Sepia: true),
            Crt = new(BeamIntensity: 101, ScanlineIntensity: -1),
            FixedPixel = new(ResponseTimeMilliseconds: 1001),
            Plasma = new(Diffusion: 101, BlackDepth: -1,
                PhosphorIntensity: 102, GammaResponse: -2,
                AutomaticBrightnessLimiter: 103),
            Vector = new(LineIntensity: -1),
            Vfd = new((EmulationVfdColor)999, PhosphorIntensity: 101,
                EmissionThreshold: -1, GlassDarkening: 102,
                Structure: (EmulationVfdStructure)999, CellSize: 103, CellGap: -2,
                HaloIntensity: -1, HaloRadius: 104, PersistenceMilliseconds: 1001),
            LedMatrix = new((EmulationLedMatrixColor)999, CellSize: 101, CellGap: -1,
                Diffusion: 102, Brightness: -2, Shape: (EmulationLedMatrixShape)999,
                HaloRadius: 103, BlackDepth: -3),
            DotMatrix = new((EmulationDotMatrixPalette)999, (EmulationDotMatrixShape)999,
                DotSize: 101, Contrast: -1, ResponseTimeMilliseconds: 1001),
            SegmentDisplay = new((EmulationSegmentDisplayLayout)999,
                (EmulationSegmentDisplayColor)999, Thickness: 101, Contrast: -1,
                Glow: 102, ResponseTimeMilliseconds: 1001),
            EPaper = new((EmulationEPaperColorMode)999, Contrast: 101, Dithering: -1,
                RefreshTimeMilliseconds: 1001, Ghosting: 102),
            Projection = new(OpticalBlur: 101, Diffusion: -1,
                ScreenTexture: 102, Convergence: -2)
        });

        Assert.Equal(EmulationVideoDisplayTechnology.Normal, normalized.DisplayTechnology);
        Assert.Equal(EmulationVideoSampling.Nearest, normalized.Sampling);
        Assert.Equal(new EmulationImageAdjustments(-10, 10, 5, -10, 10), normalized.Adjustments);
        Assert.Equal(100, normalized.Restoration.Dedithering);
        Assert.Equal(0, normalized.Restoration.Denoising);
        Assert.Equal(100, normalized.Restoration.Debanding);
        Assert.Equal(0, normalized.Restoration.DetailRecovery);
        Assert.Equal(EmulationDeinterlacingMode.Off, normalized.Restoration.Deinterlacing);
        Assert.Equal(100, normalized.Temporal.GeneralPersistence);
        Assert.Equal(0, normalized.Temporal.MotionBlur);
        Assert.Equal(100, normalized.Temporal.Flicker);
        Assert.Equal(0, normalized.Temporal.Interlacing);
        Assert.Equal(100, normalized.Temporal.InterlacingVisibility);
        Assert.Equal(EmulationSignalConnection.None, normalized.SignalSimulation.Connection);
        Assert.Equal(100, normalized.SignalSimulation.ConnectionIntensity);
        Assert.Equal(EmulationSignalStandard.Automatic, normalized.SignalSimulation.Standard);
        Assert.Equal(100, normalized.SignalSimulation.StandardIntensity);
        Assert.Equal(100, normalized.Stylistic.Grain);
        Assert.Equal(0, normalized.Stylistic.Vhs);
        Assert.Equal(100, normalized.Stylistic.ChromaticAberration);
        Assert.Equal(0, normalized.Stylistic.Bloom);
        Assert.True(normalized.Stylistic.Sepia);
        Assert.Equal(100, normalized.Crt.BeamIntensity);
        Assert.Equal(0, normalized.Crt.ScanlineIntensity);
        Assert.Equal(1000, normalized.FixedPixel.ResponseTimeMilliseconds);
        Assert.Equal(100, normalized.Plasma.Diffusion);
        Assert.Equal(0, normalized.Plasma.BlackDepth);
        Assert.Equal(100, normalized.Plasma.PhosphorIntensity);
        Assert.Equal(0, normalized.Plasma.GammaResponse);
        Assert.Equal(100, normalized.Plasma.AutomaticBrightnessLimiter);
        Assert.Equal(0, normalized.Vector.LineIntensity);
        Assert.Equal(EmulationVfdColor.Blue, normalized.Vfd.Color);
        Assert.Equal(100, normalized.Vfd.PhosphorIntensity);
        Assert.Equal(0, normalized.Vfd.EmissionThreshold);
        Assert.Equal(100, normalized.Vfd.GlassDarkening);
        Assert.Equal(EmulationVfdStructure.Graphic, normalized.Vfd.Structure);
        Assert.Equal(100, normalized.Vfd.CellSize);
        Assert.Equal(0, normalized.Vfd.CellGap);
        Assert.Equal(0, normalized.Vfd.HaloIntensity);
        Assert.Equal(100, normalized.Vfd.HaloRadius);
        Assert.Equal(1000, normalized.Vfd.PersistenceMilliseconds);
        Assert.Equal(EmulationLedMatrixColor.Rgb, normalized.LedMatrix.Color);
        Assert.Equal(100, normalized.LedMatrix.CellSize);
        Assert.Equal(0, normalized.LedMatrix.CellGap);
        Assert.Equal(100, normalized.LedMatrix.Diffusion);
        Assert.Equal(0, normalized.LedMatrix.Brightness);
        Assert.Equal(EmulationLedMatrixShape.Round, normalized.LedMatrix.Shape);
        Assert.Equal(100, normalized.LedMatrix.HaloRadius);
        Assert.Equal(0, normalized.LedMatrix.BlackDepth);
        Assert.Equal(EmulationDotMatrixPalette.Green, normalized.DotMatrix.Palette);
        Assert.Equal(EmulationDotMatrixShape.Round, normalized.DotMatrix.Shape);
        Assert.Equal(100, normalized.DotMatrix.DotSize);
        Assert.Equal(0, normalized.DotMatrix.Contrast);
        Assert.Equal(1000, normalized.DotMatrix.ResponseTimeMilliseconds);
        Assert.Equal(EmulationSegmentDisplayLayout.Seven, normalized.SegmentDisplay.Layout);
        Assert.Equal(EmulationSegmentDisplayColor.Red, normalized.SegmentDisplay.Color);
        Assert.Equal(100, normalized.SegmentDisplay.Thickness);
        Assert.Equal(0, normalized.SegmentDisplay.Contrast);
        Assert.Equal(100, normalized.SegmentDisplay.Glow);
        Assert.Equal(1000, normalized.SegmentDisplay.ResponseTimeMilliseconds);
        Assert.Equal(EmulationEPaperColorMode.Monochrome, normalized.EPaper.ColorMode);
        Assert.Equal(100, normalized.EPaper.Contrast);
        Assert.Equal(0, normalized.EPaper.Dithering);
        Assert.Equal(1000, normalized.EPaper.RefreshTimeMilliseconds);
        Assert.Equal(100, normalized.EPaper.Ghosting);
        Assert.Equal(100, normalized.Projection.OpticalBlur);
        Assert.Equal(0, normalized.Projection.Diffusion);
        Assert.Equal(100, normalized.Projection.ScreenTexture);
        Assert.Equal(0, normalized.Projection.Convergence);
        Assert.Equal(Math.Pow(2d, -0.5d), EmulationImageAdjustmentFunctions.GammaExponent(5), 12);
    }

    [Fact]
    public void FixedPixelLegacyArgbMigratesToTheNearestNamedPalette()
    {
        var normalized = EmulationVideoProcessingConfigurationFunctions.Normalize(new()
        {
            DisplayTechnology = EmulationVideoDisplayTechnology.FixedPixel,
            FixedPixel = new(Subpixels: EmulationSubpixelLayout.Monochrome,
                MonochromeColorArgb: 0xFF0060FF)
        });

        Assert.Equal(EmulationMonochromePalette.Blue,
            normalized.FixedPixel.MonochromePalette);
        Assert.Null(normalized.FixedPixel.MonochromeColorArgb);
    }

    [Fact]
    public async Task AmigaStore_RoundTripsVideoProcessing()
    {
        using var temporary = new TemporaryDirectory();
        var store = new AmigaConfigurationStore(temporary.Path);
        var expected = AmigaMachineConfiguration.A500(System.IO.Path.Combine(temporary.Path, "kick.rom"))
            with { VideoProcessing = SampleVideo() };

        await store.SaveAsync(expected);
        var actual = Assert.Single(await store.LoadAllAsync());

        Assert.Equal(expected.VideoProcessing, actual.VideoProcessing);
    }

    [Fact]
    public async Task AmigaStore_SerializesConcurrentReadsAndWrites()
    {
        using var temporary = new TemporaryDirectory();
        var writer = new AmigaConfigurationStore(temporary.Path);
        var reader = new AmigaConfigurationStore(temporary.Path);
        var configuration = AmigaMachineConfiguration.A500(
            System.IO.Path.Combine(temporary.Path, "kick.rom"));
        await writer.SaveAsync(configuration);

        var writing = Task.Run(async () =>
        {
            for (var index = 0; index < 20; index++)
                await writer.SaveAsync(configuration);
        });
        var reading = Task.Run(async () =>
        {
            for (var index = 0; index < 20; index++)
                Assert.Single(await reader.LoadAllAsync());
        });

        await Task.WhenAll(writing, reading);
        Assert.Single(await reader.LoadAllAsync());
    }
    [Fact]
    public async Task AmigaStore_LoadsLegacyDocumentWithoutVideoProcessingAsNeutral()
    {
        using var temporary = new TemporaryDirectory();
        var store = new AmigaConfigurationStore(temporary.Path);
        await store.SaveAsync(AmigaMachineConfiguration.A500(
            System.IO.Path.Combine(temporary.Path, "kick.rom")) with { VideoProcessing = SampleVideo() });
        await RemoveVideoProcessingAsync(SingleMachineDocument(temporary.Path));

        var actual = Assert.Single(await store.LoadAllAsync());

        Assert.Null(actual.VideoProcessing);
        Assert.Equal(new EmulationVideoProcessingConfiguration(),
            ((GWGUI.Emulation.Interfaces.IEmulationConfiguration)actual).VideoProcessing);
    }

    [Fact]
    public async Task AmigaStore_RemovesOnlyAnIncompatibleOptionalPropertyAndLoadsConfiguration()
    {
        using var temporary = new TemporaryDirectory();
        var store = new AmigaConfigurationStore(temporary.Path);
        await store.SaveAsync(AmigaMachineConfiguration.A500(
            System.IO.Path.Combine(temporary.Path, "kick.rom")) with
        {
            Model = "A600",
            VideoProcessing = SampleVideo()
        });
        var path = SingleMachineDocument(temporary.Path);
        var json = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
        json["videoProcessing"]!["stylistic"]!["sepia"] = 0;
        await File.WriteAllTextAsync(path, json.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        var actual = Assert.Single(await store.LoadAllAsync());
        var repaired = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();

        Assert.Equal("A600", actual.Model);
        Assert.False(actual.VideoProcessing!.Stylistic.Sepia);
        Assert.Null(repaired["videoProcessing"]!["stylistic"]!["sepia"]);
        Assert.NotNull(repaired["videoProcessing"]!["stylistic"]!["grain"]);
    }

    [Fact]
    public async Task AtariStore_RoundTripsVideoProcessing()
    {
        using var temporary = new TemporaryDirectory();
        var store = new AtariConfigurationStore(temporary.Path);
        var expected = new AtariMachineConfiguration(AtariMachineModel.St,
            videoProcessing: SampleVideo());

        await store.SaveAsync(expected);
        var actual = Assert.Single(await store.LoadAllAsync());

        Assert.Equal(expected.VideoProcessing, actual.VideoProcessing);
    }

    [Fact]
    public async Task AtariStore_SerializesConcurrentReadsAndWrites()
    {
        using var temporary = new TemporaryDirectory();
        var writer = new AtariConfigurationStore(temporary.Path);
        var reader = new AtariConfigurationStore(temporary.Path);
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St);
        await writer.SaveAsync(configuration);

        var writing = Task.Run(async () =>
        {
            for (var index = 0; index < 20; index++)
                await writer.SaveAsync(configuration);
        });
        var reading = Task.Run(async () =>
        {
            for (var index = 0; index < 20; index++)
                Assert.Single(await reader.LoadAllAsync());
        });

        await Task.WhenAll(writing, reading);
        Assert.Single(await reader.LoadAllAsync());
    }
    [Fact]
    public async Task AtariStore_LoadsLegacyDocumentWithoutVideoProcessingAsNeutral()
    {
        using var temporary = new TemporaryDirectory();
        var store = new AtariConfigurationStore(temporary.Path);
        await store.SaveAsync(new AtariMachineConfiguration(AtariMachineModel.St,
            videoProcessing: SampleVideo()));
        await RemoveVideoProcessingAsync(SingleMachineDocument(temporary.Path));

        var actual = Assert.Single(await store.LoadAllAsync());

        Assert.Equal(new EmulationVideoProcessingConfiguration(), actual.VideoProcessing);
    }

    [Fact]
    public void AtariInputReconstruction_PreservesVideoProcessing()
    {
        var expected = SampleVideo();
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St,
            videoProcessing: expected);
        var input = AtariInputSettingsFunctions.Describe(configuration);

        var rebuilt = AtariInputSettingsFunctions.Apply(configuration, input);

        Assert.Equal(expected, rebuilt.VideoProcessing);
    }

    private static EmulationVideoProcessingConfiguration SampleVideo() => new()
    {
        DisplayTechnology = EmulationVideoDisplayTechnology.Crt,
        Sampling = EmulationVideoSampling.Bicubic,
        Adjustments = new(2, -3, 4, 5, 6),
        Restoration = new(Dedithering: 37, Denoising: 46, Debanding: 55, DetailRecovery: 64,
            Deinterlacing: EmulationDeinterlacingMode.BobOddLines),
        Temporal = new(GeneralPersistence: 38, MotionBlur: 47, Flicker: 56, Interlacing: 65,
            BlackFrameInsertion: true),
        SignalSimulation = new(EmulationSignalConnection.Rf, 92,
            EmulationSignalStandard.Pal, 63),
        Stylistic = new(Grain: 41, Vhs: 32, ChromaticAberration: 23, Bloom: 14,
            Sepia: true),
        Crt = new(EmulationCrtColorMode.Amber, BeamWidth: 42, BeamIntensity: 77,
            HaloIntensity: 31, Mask: EmulationCrtMask.SlotMask, MaskIntensity: 64,
            ScanlinesEnabled: true, ScanlineIntensity: 53),
        FixedPixel = new(EmulationFixedPixelTechnology.Oled, EmulationSubpixelLayout.Bgr,
            GridIntensity: 21, ResponseTimeMilliseconds: 17, BlackDepth: 83),
        Plasma = new(CellStructure: 18, Diffusion: 27, TemporalDithering: 36,
            PersistenceIntensity: 45, BlackDepth: 54, PhosphorIntensity: 63,
            GammaResponse: 72, AutomaticBrightnessLimiter: 81),
        Vector = new(LineThreshold: 12, LineIntensity: 91, BeamWidth: 58,
            BeamFocus: 67, PhosphorColor: EmulationCrtColorMode.Green,
            HaloIntensity: 48, HaloRadius: 29, PersistenceIntensity: 34),
        Vfd = new(EmulationVfdColor.Green, PhosphorIntensity: 72,
            EmissionThreshold: 23, GlassDarkening: 64,
            Structure: EmulationVfdStructure.DotMatrix, CellSize: 55, CellGap: 17,
            HaloIntensity: 31, HaloRadius: 42, PersistenceMilliseconds: 126),
        LedMatrix = new(EmulationLedMatrixColor.Amber, CellSize: 44, CellGap: 33,
            Diffusion: 22, Brightness: 88, Shape: EmulationLedMatrixShape.Square,
            HaloRadius: 37, BlackDepth: 92),
        DotMatrix = new(EmulationDotMatrixPalette.Blue, EmulationDotMatrixShape.Square,
            DotSize: 66, Contrast: 77, ResponseTimeMilliseconds: 145),
        SegmentDisplay = new(EmulationSegmentDisplayLayout.Fourteen,
            EmulationSegmentDisplayColor.Green, Thickness: 61, Contrast: 82,
            Glow: 29, ResponseTimeMilliseconds: 48),
        EPaper = new(EmulationEPaperColorMode.Color4096, Contrast: 83, Dithering: 41,
            RefreshTimeMilliseconds: 640, Ghosting: 27),
        Projection = new(OpticalBlur: 31, Diffusion: 24,
            ScreenTexture: 17, Convergence: 12)
    };

    private static string SingleMachineDocument(string directory) =>
        Assert.Single(Directory.GetFiles(directory, "machine.json", SearchOption.AllDirectories));

    private static async Task RemoveVideoProcessingAsync(string path)
    {
        var document = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
        Assert.True(document.Remove("videoProcessing"));
        await File.WriteAllTextAsync(path, document.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                $"GWGUI-video-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
        }
    }
}
