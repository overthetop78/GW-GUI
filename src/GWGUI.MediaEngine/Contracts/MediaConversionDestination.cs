namespace GWGUI.MediaEngine.Contracts;

/// <summary>Describes one writable media image destination exposed by the engine.</summary>
public sealed record MediaConversionDestination(
    string FormatId,
    string Extension,
    string WriterId,
    bool ProducesMultipleFiles);
