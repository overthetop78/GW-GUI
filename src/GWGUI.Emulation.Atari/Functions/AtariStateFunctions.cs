namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariStateFunctions
{
    internal static bool IsAvailable(AtariExternalCoreExports exports)
    {
        var size = exports.GetSerializedSize();
        return size > nuint.Zero && size <= AtariConstants.MaximumStateSize;
    }
}
