namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariJaguarCdErrors
{
    internal const string ModelRequired = "Jaguar CD media requires the Jaguar CD model.";
    internal const string CompleteDiscRequired =
        "Jaguar CD requires a CUE sheet or CDI image, not an individual track.";
    internal const string MissingCueTrack = "The Jaguar CD CUE sheet references a missing track.";
    internal const string EmptyCue = "The Jaguar CD CUE sheet does not describe any track file.";
    internal const string FileUnreadable = "The Jaguar CD image cannot be opened for reading.";
    internal const string EjectionUnsupported =
        "Virtual Jaguar cannot remain powered on after ejecting Jaguar CD media.";
}
