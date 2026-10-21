using GWGUI.Emulation.Example.Common.Functions;
using System.IO;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Runtime.Versioning;
using GWGUI.Emulation;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Services;
using GWGUI.Emulation.Example.Common.Constants;
using GWGUI.Emulation.Example.Common.Exceptions;
using GWGUI.Emulation.Example.Common.Interfaces;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Functions;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Partie du cœur déporté consacrée au lancement de l’hôte et au décodage de ses données.</summary>
/// <remarks>Les lecteurs sont empruntés à la requête courante. Les segments vidéo ouverts sont possédés par ProcessCore
/// et remplacés par le lecteur commun lorsque leur génération change.</remarks>
[SupportedOSPlatform(CoreHostConstants.Windows)]
internal sealed partial class ProcessCore : IEmulatorCore
{
    /// <summary>Lance le processus enfant dans le répertoire de son exécutable avec les arguments de session.</summary>
    /// <param name="pipeName">Nom du tube auquel l’hôte doit se connecter.</param>
    /// <param name="videoMapName">Préfixe du segment vidéo à créer dans l’hôte.</param>
    /// <returns>Processus démarré, possédé par cette instance jusqu’à sa fermeture dans Dispose.</returns>
    /// <exception cref="InvalidOperationException">Le lancement ne fournit pas de processus utilisable.</exception>
    private Process StartHostProcess(string pipeName, string videoMapName)
    {
        var startInfo = EmulationSdk.HostProtocol.CreateHostStartInfo(_hostExecutablePath,
            Path.GetDirectoryName(_hostExecutablePath)!, _hostCommand, pipeName, videoMapName);
        return EmulationSdk.HostProtocol.StartHostProcess(startInfo, static () => new InvalidOperationException(CoreHostErrors.ProcessStartFailed));
    }

    /// <summary>Décode la réponse d’initialisation dans l’ordre exact écrit par CoreHost.</summary>
    /// <param name="reader">Lecteur emprunté de la réponse réussie.</param>
    /// <remarks>La réponse fournit empreinte, cadence vidéo, fréquence audio, latence minimale, support des états,
    /// statut audio, options, diagnostics, identité du cœur, extensions, LED, demande de fermeture et retours de
    /// contrôleurs. Un JSON null pour les collections devient une liste vide. Les identités et compteurs restent
    /// des valeurs locales : ce décodage n’appelle pas les exports de la bibliothèque distante.</remarks>
    private void ReadInitialization(BinaryReader reader)
    {
        CoreSha256 = reader.ReadString();
        FramesPerSecond = reader.ReadDouble();
        SampleRate = reader.ReadInt32();
        MinimumAudioLatencyMilliseconds = reader.ReadInt32();
        SupportsSaveStates = reader.ReadBoolean();
        ReadRuntimeStatus(reader);
        Options = JsonSerializer.Deserialize<IReadOnlyList<CoreOption>>(reader.ReadString(), CoreHostFunctions.JsonOptions) ?? [];
        Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(reader.ReadString(), CoreHostFunctions.JsonOptions) ?? [];
        CoreName = reader.ReadString();
        CoreVersion = reader.ReadString();
        SupportedContentExtensions = reader.ReadString().Split(CoreHostConstants.ExtensionListSeparator, StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
        LedStates = CoreHostFunctions.ReadLedStates(reader);
        ShutdownRequested = reader.ReadBoolean();
        ControllerFeedback = JsonSerializer.Deserialize<IReadOnlyList<ControllerFeedbackState>>(reader.ReadString(), CoreHostFunctions.JsonOptions) ?? [];
    }

    /// <summary>Décode les sorties produites par une image et actualise les dernières valeurs reçues.</summary>
    /// <param name="reader">Lecteur emprunté de la réponse d’image.</param>
    /// <remarks>Sans nouvelle image, la précédente est conservée. Tous les blocs audio reçus sont ajoutés à la file.
    /// Les diagnostics sont remplacés seulement si la réponse annonce leur présence ; les LED, compteurs,
    /// capacités d’état, latence, demande de fermeture et retours de contrôleurs sont actualisés à chaque réponse.</remarks>
    private void ReadFrame(BinaryReader reader)
    {
        LatestVideoFrame = CoreHostFunctions.ReadResizableSharedFrame(reader, ref _videoMemory, ref _videoMap, ref _activeVideoMapName) ?? LatestVideoFrame;
        foreach (var chunk in CoreHostFunctions.ReadAudio(reader))
        {
            LatestAudioChunk = chunk;
            _audio.Enqueue(chunk);
        }
        FramesPerSecond = reader.ReadDouble();
        SampleRate = reader.ReadInt32();
        MinimumAudioLatencyMilliseconds = reader.ReadInt32();
        ReadRuntimeStatus(reader);
        SupportsSaveStates = reader.ReadBoolean();
        if (reader.ReadBoolean())
            Diagnostics = JsonSerializer.Deserialize<IReadOnlyList<string>>(reader.ReadString(), CoreHostFunctions.JsonOptions) ?? [];
        LedStates = CoreHostFunctions.ReadLedStates(reader);
        ShutdownRequested = reader.ReadBoolean();
        ControllerFeedback = JsonSerializer.Deserialize<IReadOnlyList<ControllerFeedbackState>>(reader.ReadString(), CoreHostFunctions.JsonOptions) ?? [];
    }

    /// <summary>Décode la région vidéo et les mesures du tampon audio.</summary>
    /// <param name="reader">Lecteur emprunté positionné avant le bloc de statut.</param>
    /// <remarks>BufferedAudioFrames compte des frames audio ; les dépassements et manques sont des compteurs cumulés du cœur.</remarks>
    private void ReadRuntimeStatus(BinaryReader reader)
    => EmulationSdk.HostProtocol.ReadRuntimeStatus(reader,
        value => Region = RuntimeFunctions.ReadRegion(value),
        value => BufferedAudioFrames = value,
        value => AudioOverrunCount = value,
        value => AudioUnderrunCount = value);
}
