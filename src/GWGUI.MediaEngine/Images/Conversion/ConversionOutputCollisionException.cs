namespace GWGUI.MediaEngine.Images.Conversion;

public sealed class ConversionOutputCollisionException(string outputPath)
    : InvalidOperationException($"Several conversions would create '{outputPath}'.")
{
    public string OutputPath { get; } = outputPath;
}
