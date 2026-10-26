using CommonHost = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;

/// <summary>Relais BBC pour les images cassette : le décodage des conteneurs appartient à MediaEngine via Emulation.</summary>
internal static class BbcMicroCassetteFunctions
{
    /// <summary>Charge une image sur le thread du cœur ; le service ferme ses flux avant de rendre le signal possédé.</summary>
    internal static EmulationCassetteSignal Read(string path, IReadOnlyDictionary<string, string>? parameters = null) => CommonHost.MediaProcessing.ReadCassetteAsync(path, machineId: "acorn-bbc", parameters: parameters).GetAwaiter().GetResult();
    /// <summary>Conserve le signal complet par l'écrivain du service commun ; aucun fichier n'est ouvert dans le module.</summary>
    internal static void Write(string path, EmulationCassetteSignal signal, IReadOnlyDictionary<string, string>? parameters = null) => CommonHost.MediaProcessing.WriteCassetteAsync(path, signal, parameters: parameters).GetAwaiter().GetResult();
    internal static IReadOnlyList<string> RecordingExtensions => CommonHost.MediaProcessing.CassetteRecordingExtensions;
    internal static short[] Sound(string operation, int sampleRate, int count, long offset) => CommonHost.MediaProcessing.RenderCassetteSound(operation, sampleRate, count, offset);
}
