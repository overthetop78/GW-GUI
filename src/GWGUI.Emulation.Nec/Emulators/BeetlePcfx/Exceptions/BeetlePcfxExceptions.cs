using GWGUI.Emulation.Nec.Emulators.Common.Interop.Exceptions;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Exceptions;

internal static class BeetlePcfxExceptions
{
    internal static string InvalidBackupSize(string path) => CoreExceptions.Text("Emulation.Error.BeetlePcfx.InvalidBackupSize", path);
    internal static string UnexpectedBackupMemorySize() => CoreExceptions.Text("Emulation.Error.BeetlePcfx.UnexpectedBackupMemorySize");
    internal static string FullContentPathsRequired() => CoreExceptions.Text("Emulation.Error.BeetlePcfx.FullContentPathsRequired");
    internal static string PlaylistLimitExceeded() => CoreExceptions.Text("Emulation.Error.BeetlePcfx.PlaylistLimitExceeded");
}
