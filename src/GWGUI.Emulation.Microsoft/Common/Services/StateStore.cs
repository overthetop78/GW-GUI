using System.IO;




namespace GWGUI.Emulation.Microsoft.Common.Services;

internal static class StateStore
{

    internal static void Write(string path, SavedStateHeader header, ReadOnlySpan<byte> state) =>
        EmulationSdk.StateFiles.WriteState(path, header, state);

    internal static (SavedStateHeader Header, byte[] State) Read(string path) =>
        EmulationSdk.StateFiles.ReadState<string, string>(path, static () => MachineExceptions.SavedStateInvalid());

    internal static string HashPath(string path) =>
        EmulationSdk.StateFiles.HashPath(path, StateStoreConstants.Value, '/', 64 * 1024, () => MachineExceptions.MediaNotFound());

    internal static string HashBytes(ReadOnlySpan<byte> bytes) => EmulationSdk.StateFiles.HashBytes(bytes);


}
