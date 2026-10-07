using BeetlePceCore = GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants.CoreConstants;
using Quasi88Core = GWGUI.Emulation.Nec.Emulators.Quasi88.Constants.CoreConstants;
using NekoProjectIICore = GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants.CoreConstants;
using NP2KaiCore = GWGUI.Emulation.Nec.Emulators.NP2Kai.Constants.CoreConstants;
using GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetleSgx.Constants;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;
using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Dictionaries;

internal static class CoreCatalog
{
    internal static IReadOnlyList<CoreDefinition> All { get; } =
    [
        new(BeetlePceFastConstants.Id, BeetlePceFastConstants.LibraryName, ReleaseConstants.InstalledLibraryName),
        new(BeetleSgxConstants.Id, BeetleSgxConstants.LibraryName, BeetleSgxConstants.InstalledLibraryName),
        new(GeargrafxConstants.Id, GeargrafxConstants.LibraryName, GeargrafxConstants.InstalledLibraryName),
        new(BeetlePceCore.Id, BeetlePceCore.LibraryName, BeetlePceCore.LibraryFileName),
        new(Quasi88Core.Id, Quasi88Core.LibraryName, Quasi88Core.LibraryFileName,
            Subsystems: GWGUI.Emulation.Nec.Emulators.Quasi88.Constants.StorageConstants.Subsystems),
        new(NekoProjectIICore.Id, NekoProjectIICore.LibraryName, NekoProjectIICore.LibraryFileName,
            GWGUI.Emulation.Nec.Emulators.NekoProjectII.Functions.ContentFunctions.Prepare),
        new(NP2KaiCore.Id, NP2KaiCore.LibraryName, NP2KaiCore.LibraryFileName,
            GWGUI.Emulation.Nec.Emulators.NP2Kai.Functions.ContentFunctions.Prepare),
    ];

    internal static CoreDefinition Get(string id) => All.Single(core => core.Id == id);
}
