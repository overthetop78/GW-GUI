using GWGUI.MediaEngine.Images.Formats;
namespace GWGUI.MediaEngine.Images.Formats.Detection;

public sealed record DetectedImageFormat(
    string Extension,
    DiskFormat? Format,
    FormatConfidence Confidence,
    IReadOnlyList<DiskFormat> Candidates,
    string ExplanationKey)
{
    public bool RequiresUserChoice => Confidence == FormatConfidence.Ambiguous || Format is null;
}
