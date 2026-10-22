using EmulatorCatalogFunctions = GWGUI.Emulation.Commodore.Common.Functions.EmulatorCatalogFunctions;
using GWGUI.Emulation;
using System.IO;





namespace GWGUI.Emulation.Commodore.Common.Services;


internal static class StateStore
{

    internal static void Write(string path, SavedStateHeader header, ReadOnlySpan<byte> state) =>
        EmulationSdk.StateFiles.WriteState(path, header, state);

    internal static (SavedStateHeader Header, byte[] State) Read(string path) =>
        EmulationSdk.StateFiles.ReadState<string, string>(path, static () => MachineExceptions.SavedStateInvalid());



    internal static string HashPath(string path) =>
        EmulationSdk.StateFiles.HashPath(path, StateStoreConstants.AllFilesPattern, StateStoreConstants.CanonicalDirectorySeparator, StateStoreConstants.DirectoryHashBufferSize, () => MachineExceptions.MediaNotFound());

    internal static string HashBytes(ReadOnlySpan<byte> bytes) => EmulationSdk.StateFiles.HashBytes(bytes);
    internal static string HashFile(string path) => EmulatorCatalogFunctions.Diagnostics.CalculateSha256(path);
}
