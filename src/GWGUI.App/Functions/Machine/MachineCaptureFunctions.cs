using GWGUI.App.Constants.Machine;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;

namespace GWGUI.App.Functions.Machine;

internal static class MachineCaptureFunctions
{
    internal static string Save(BitmapSource snapshot, string captureFolder, DateTime timestamp)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Directory.CreateDirectory(captureFolder);
        var stem = MachinePresentationConstants.CaptureStem + MachinePresentationConstants.FileNameSeparator
            + timestamp.ToString(MachinePresentationConstants.CaptureTimestampFormat, CultureInfo.InvariantCulture);
        var path = UniquePath(captureFolder, stem, MachinePresentationConstants.CaptureFileExtension);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(snapshot));
        using var stream = File.Create(path);
        encoder.Save(stream);
        return path;
    }

    private static string UniquePath(string folder, string stem, string extension)
    {
        var path = Path.Combine(folder, stem + extension);
        var suffix = MachinePresentationConstants.FirstDuplicateSuffix;
        while (File.Exists(path))
            path = Path.Combine(folder, stem + MachinePresentationConstants.FileNameSeparator + suffix++ + extension);
        return path;
    }
}
