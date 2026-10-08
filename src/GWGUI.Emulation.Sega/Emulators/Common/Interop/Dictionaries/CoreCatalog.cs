using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Dictionaries;

internal static class CoreCatalog
{
    internal static IReadOnlyList<CoreDefinition> All { get; } =
    [
        Emulators.BlastEm.Constants.CoreConstants.Definition,
        Emulators.ClownMDEmu.Constants.CoreConstants.Definition,
        Emulators.Flycast.Constants.CoreConstants.Definition,
        Emulators.Gearsystem.Constants.CoreConstants.Definition,
        Emulators.GenesisPlusGX.Constants.CoreConstants.Definition,
        Emulators.GenesisPlusGXWide.Constants.CoreConstants.Definition,
        Emulators.Kronos.Constants.CoreConstants.Definition,
        Emulators.BeetleSaturn.Constants.CoreConstants.Definition,
        Emulators.PicoDrive.Constants.CoreConstants.Definition,
        Emulators.SmsPlusGX.Constants.CoreConstants.Definition,
        Emulators.Supermodel.Constants.CoreConstants.Definition,
        Emulators.VeMUlator.Constants.CoreConstants.Definition,
        Emulators.YabaSanshiro.Constants.CoreConstants.Definition,
        Emulators.Yabause.Constants.CoreConstants.Definition,
        Emulators.Ymir.Constants.CoreConstants.Definition,
        Emulators.BlueMSX.Constants.CoreConstants.Definition,
    ];
    internal static CoreDefinition Get(string id) => All.First(core => core.Id.Equals(id, StringComparison.Ordinal));
}
