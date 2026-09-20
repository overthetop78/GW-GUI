namespace GWGUI.MediaEngine.Formats;

public interface IImageFormatCatalog
{
    IReadOnlyList<DiskFormat> Formats { get; }
    IReadOnlyList<DiskFormat> GetCompatibleOutputs(string sourceExtension);
}
