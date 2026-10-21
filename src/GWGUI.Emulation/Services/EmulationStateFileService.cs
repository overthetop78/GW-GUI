using System.Text.Json;

namespace GWGUI.Emulation.Services;

/// <summary>Adaptateur interne de l’API vers les algorithmes de sauvegarde communs existants.</summary>
internal sealed class EmulationStateFileService : IEmulationStateFiles
{
    public void WriteState<TModel, TCore>(string path, EmulationSavedStateHeader<TModel, TCore> header, ReadOnlySpan<byte> state)
        => EmulationStateFileFunctions.WriteState(path, header, state);
    public (EmulationSavedStateHeader<TModel, TCore> Header, byte[] State) ReadState<TModel, TCore>(string path, Func<Exception> invalid)
        => EmulationStateFileFunctions.ReadState<TModel, TCore>(path, invalid);
    public string HashDocument<T>(T value, JsonSerializerOptions? options = null) => EmulationStateFileFunctions.HashDocument(value, options);
    public string HashBytes(ReadOnlySpan<byte> bytes) => EmulationStateFileFunctions.HashBytes(bytes);
    public string HashPath(string path, string pattern, char separator, int bufferSize, Func<Exception> missing)
        => EmulationStateFileFunctions.HashPath(path, pattern, separator, bufferSize, missing);
}
