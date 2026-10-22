using System.IO;
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
    public IOrderedEnumerable<KeyValuePair<string, TValue>> OrderInputMappings<TValue>(IReadOnlyDictionary<string, TValue>? mappings) => EmulationStateFileFunctions.OrderInputMappings(mappings);
    public bool IsCompatible<TModel, TCore, TConfiguration, TOption>(EmulationSavedStateHeader<TModel, TCore> header,
        TModel model, TCore emulator, IEmulationCoreRuntime<TConfiguration, TOption> core, Func<string> configurationHash, Func<string> contentHash)
        => EmulationStateFileFunctions.IsCompatible(header, model, emulator, core, configurationHash, contentHash);
    public string HashBytes(ReadOnlySpan<byte> bytes) => EmulationStateFileFunctions.HashBytes(bytes);
    public string HashStream(Stream stream) => EmulationStateFileFunctions.HashStream(stream);
    public string HashPath(string path, string pattern, char separator, int bufferSize, Func<Exception> missing)
        => EmulationStateFileFunctions.HashPath(path, pattern, separator, bufferSize, missing);
}
