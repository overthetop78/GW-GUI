
namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Contracts;

internal sealed record StContent(
    MediaConfiguration Configuration,
    string RuntimePath,
    SessionMedia? SessionMedia,
    StStorage? Storage,
    SessionMedia? BootFloppy = null);
