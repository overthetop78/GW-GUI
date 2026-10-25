using GWGUI.Emulation.Nec.Common.Exceptions;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Exceptions;

internal static class BeetlePcfxExceptions
{
    internal static string InvalidBackupSize(string path) => ExceptionText.Get("Emulation.Error.BeetlePcfx.InvalidBackupSize", path);
    internal static string UnexpectedBackupMemorySize() => ExceptionText.Get("Emulation.Error.BeetlePcfx.UnexpectedBackupMemorySize");
    internal static string FullContentPathsRequired() => ExceptionText.Get("Emulation.Error.BeetlePcfx.FullContentPathsRequired");
    internal static string PlaylistLimitExceeded() => ExceptionText.Get("Emulation.Error.BeetlePcfx.PlaylistLimitExceeded");
}
