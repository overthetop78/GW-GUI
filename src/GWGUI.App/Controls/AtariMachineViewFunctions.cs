using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using GWGUI.App.Rendering;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariMachineViewFunctions
{
    internal static AtariMachineConfiguration WithMountedMedia(AtariMachineConfiguration configuration,
        IEnumerable<AtariMediaConfiguration> mountedMedia)
    {
        var mounted = mountedMedia.Select(item => item with { IsInserted = true }).ToArray();
        var mountedSlots = mounted.Select(item => item.Slot).ToHashSet();
        var media = configuration.Media.Where(item => !mountedSlots.Contains(item.Slot))
            .Concat(mounted).OrderBy(item => item.Slot).ToArray();
        return new AtariMachineConfiguration(configuration.Model, configuration.Firmwares, media,
            configuration.Options, configuration.Input, configuration.Id, configuration.SchemaVersion,
            configuration.AudioEnabled, configuration.VideoRenderer, configuration.Folders);
    }

    internal static IReadOnlyList<AtariMachineMediaView> Media(AtariMachineConfiguration configuration) =>
        AtariStorageSettingsFunctions.Create(configuration).Devices
            .OrderBy(device => device.Configuration.Slot)
            .Select(device => new AtariMachineMediaView(device.Configuration,
                device.Identifier, Glyph(device.Configuration.Kind), IsRemovable(device.Configuration.Kind)))
            .ToArray();

    internal static Size Fit(double width, double height, float aspectRatio) =>
        EmulationVideoLayout.Fit(width, height,
            float.IsFinite(aspectRatio) && aspectRatio > AtariMachineViewConstants.EmptyMeasurement
                ? aspectRatio : AtariMachineViewConstants.DefaultAspectRatio);

    internal static AtariMachineStatusView Status(AtariRuntimeStatus status, VideoFrame frame,
        double measuredFramesPerSecond, bool audioMuted, bool mouseAvailable, bool controllerAvailable)
    {
        var frequency = status.FramesPerSecond > AtariMachineViewConstants.EmptyMeasurement
            ? status.FramesPerSecond : measuredFramesPerSecond;
        var aspect = frame.AspectRatio > AtariMachineViewConstants.EmptyMeasurement ? frame.AspectRatio
            : status.Geometry?.AspectRatio > AtariMachineViewConstants.EmptyMeasurement ? status.Geometry.AspectRatio
            : (float)AtariMachineViewConstants.DefaultAspectRatio;
        return new AtariMachineStatusView(string.Format(CultureInfo.CurrentCulture,
                AtariMachineViewConstants.StatusFormat, frame.Width, frame.Height, frequency,
                measuredFramesPerSecond), aspect, status.MediaActivity,
            !audioMuted && status.SampleRate > AtariMachineViewConstants.EmptyMeasurement,
            mouseAvailable, controllerAvailable);
    }

    internal static string RendererName(EmulationVideoRenderer renderer) => renderer switch
    {
        EmulationVideoRenderer.Direct3D11 => AtariMachineViewConstants.Direct3DRendererName,
        EmulationVideoRenderer.Wpf => AtariMachineViewConstants.WpfRendererName,
        _ => renderer.ToString()
    };

    internal static string SaveScreenshot(BitmapSource snapshot, string captureFolder, DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Directory.CreateDirectory(captureFolder);
        var stem = AtariMachineViewConstants.CaptureStem + AtariMachineViewConstants.FileNameSeparator
            + timestamp.ToString(AtariMachineViewConstants.CaptureTimestampFormat, CultureInfo.InvariantCulture);
        var path = UniquePath(captureFolder, stem, AtariMachineViewConstants.CaptureFileExtension);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(snapshot));
        using var stream = File.Create(path);
        encoder.Save(stream);
        return path;
    }

    internal static string QuickStatePath(string stateFolder, Guid configurationId) =>
        Path.Combine(stateFolder,
            configurationId.ToString(AtariEmulationConstants.IdentifierFormat)
            + AtariMachineViewConstants.StateFileExtension);

    internal static async Task ExecuteShortcutAsync(string action, AtariMachineShortcutActions actions)
    {
        switch (action)
        {
            case EmulationShortcutActions.Power: await actions.TogglePower(); break;
            case EmulationShortcutActions.PauseResume: await actions.TogglePause(); break;
            case EmulationShortcutActions.SoftReset: await actions.SoftReset(); break;
            case EmulationShortcutActions.HardReset: await actions.HardReset(); break;
            case EmulationShortcutActions.QuickSave: await actions.QuickSave(); break;
            case EmulationShortcutActions.QuickLoad: await actions.QuickLoad(); break;
            case EmulationShortcutActions.Screenshot: await actions.Screenshot(); break;
            case EmulationShortcutActions.ToggleFullscreen: await actions.ToggleFullscreen(); break;
            case EmulationShortcutActions.ReleaseMouse: actions.ReleaseMouse(); break;
            case EmulationShortcutActions.ToggleMute: await actions.ToggleMute(); break;
        }
    }

    private static string UniquePath(string folder, string stem, string extension)
    {
        var path = Path.Combine(folder, stem + extension);
        var suffix = AtariMachineViewConstants.FirstDuplicateSuffix;
        while (File.Exists(path)) path = Path.Combine(folder,
            stem + AtariMachineViewConstants.FileNameSeparator + suffix++ + extension);
        return path;
    }

    private static string Glyph(AtariMediaKind kind) => kind switch
    {
        AtariMediaKind.CompactDisc => AtariMachineViewConstants.CompactDiscGlyph,
        AtariMediaKind.Cartridge => AtariMachineViewConstants.CartridgeGlyph,
        AtariMediaKind.Cassette => AtariMachineViewConstants.CassetteGlyph,
        AtariMediaKind.HardDisk or AtariMediaKind.Directory => AtariMachineViewConstants.DiskGlyph,
        _ => AtariMachineViewConstants.FloppyGlyph
    };

    private static bool IsRemovable(AtariMediaKind kind) => kind is AtariMediaKind.Floppy
        or AtariMediaKind.Cassette or AtariMediaKind.Cartridge or AtariMediaKind.CompactDisc;
}
