namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;

internal static class EmulationMediaConversionFunctions
{
    internal static EmulationMedia ToCommon(MediaConfiguration media) => new(media.Path, media.Slot, media.Category, media.IsReadOnly, media.IsInserted);
    internal static MediaConfiguration ToModule(EmulationMedia media, IReadOnlyList<MediaConfiguration> mountedMedia) => throw new EmulationMessageException(new EmulationMessage(EmulationMessageCategory.Media, EmulationMessageCode.MediaUnsupported, EmulationMessageSeverity.Error, EmulationMessageTarget.Dialog));
}