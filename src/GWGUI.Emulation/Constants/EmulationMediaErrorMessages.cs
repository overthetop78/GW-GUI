namespace GWGUI.Emulation.Constants;

internal static class EmulationMediaErrorMessages
{
    internal const string EmptyDocument = "The emulation media document is empty.";
    internal const string IncompatibleSlotFormat = "Media type '{0}' is not compatible with slot '{1}'.";
    internal const string DuplicateSlotFormat = "Media slot '{0}' is occupied more than once.";
    internal const string ReadOnlyRequiredFormat = "Media type '{0}' must be read-only.";
    internal const string EjectedConfigurationUnsupportedFormat = "Media type '{0}' cannot remain configured while ejected.";
    internal const string EjectionUnsupportedFormat = "Media type '{0}' cannot be ejected.";
}
