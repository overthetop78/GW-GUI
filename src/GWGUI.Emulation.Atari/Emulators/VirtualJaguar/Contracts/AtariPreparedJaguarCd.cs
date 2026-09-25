using GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Constants;
using GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Contracts;
using GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Functions;

namespace GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Contracts;

internal sealed record AtariPreparedJaguarCd(
    MediaConfiguration Configuration,
    string RuntimePath,
    bool NeedsFullPath,
    IReadOnlySet<string> ActivityPaths);
