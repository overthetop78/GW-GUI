using GWGUI.Emulation.Atari.Emulators.Atari800.Constants;
using GWGUI.Emulation.Atari.Emulators.Atari800.Contracts;
using GWGUI.Emulation.Atari.Emulators.Atari800.Enums;
using GWGUI.Emulation.Atari.Emulators.Atari800.Functions;

namespace GWGUI.Emulation.Atari.Emulators.Atari800.Contracts;


internal sealed record Atari800PreparedMedia(
    MediaConfiguration Configuration,
    Atari800ContentType ContentType,
    string RuntimePath,
    SessionMedia? SessionMedia);
