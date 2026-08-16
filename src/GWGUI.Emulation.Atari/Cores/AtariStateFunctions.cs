namespace GWGUI.Emulation.Atari.Cores;

internal static class AtariStateFunctions
{
    internal static bool IsAvailable(AtariExternalCoreExports exports)
    {
        var size = exports.GetSerializedSize();
        return size > nuint.Zero && size <= AtariConstants.MaximumStateSize;
    }
}
