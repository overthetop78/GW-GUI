namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class SaveMemoryConstants
{
    // RETRO_MEMORY_SAVE_RAM from the Libretro ABI.
    internal const uint SaveRamId = 0;
    internal const string FileName = "save-ram.bin";
    internal const string TemporaryFileSuffix = ".tmp";
    internal const int MaximumManagedBufferSize = int.MaxValue;
}
