using System.IO;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Commodore.Common.Services;


internal static class StateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal static void Write(string path, SavedStateHeader header, ReadOnlySpan<byte> state)
    {
        EmulationStateFileFunctions.WriteFile(path, header, state, StateStoreConstants.Magic, StateStoreConstants.Tmp,
            JsonOptions, useFileReplace: false);
    }

    internal static (SavedStateHeader Header, byte[] State) Read(string path)
    {
        using var stream = File.OpenRead(path);
        return EmulationStateFileFunctions.Read<SavedStateHeader>(stream, StateStoreConstants.Magic,
            StateStoreConstants.MaximumHeaderLength, JsonOptions, static header => header.StateSha256,
            static () => MachineExceptions.SavedStateInvalid());
    }



    internal static string HashPath(string path) =>
        EmulationStateFileFunctions.HashPath(path, StateStoreConstants.AllFilesPattern, StateStoreConstants.CanonicalDirectorySeparator, StateStoreConstants.DirectoryHashBufferSize, () => MachineExceptions.MediaNotFound());

    internal static string HashBytes(ReadOnlySpan<byte> bytes) => EmulationStateFileFunctions.HashBytes(bytes);
    internal static string HashFile(string path) => EmulationCoreDiagnosticFunctions.CalculateSha256(path);
}
