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
using GWGUI.Emulation.Example.Common.Constants;
using GWGUI.Emulation.Example.Common.Enums;
using GWGUI.Emulation.Example.Common.Exceptions;
using GWGUI.Emulation.Example.Common.Functions;
using GWGUI.Emulation.Example.Common.Interfaces;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Partie du cœur déporté assurant les requêtes, les erreurs de transport et la destruction de la session.</summary>
/// <remarks>Une requête et sa réponse forment une opération sérialisée. Une rupture rend la connexion inutilisable
/// et entraîne la fermeture du processus possédé avant propagation de l’erreur.</remarks>
[SupportedOSPlatform(CoreHostConstants.Windows)]
internal sealed partial class ProcessCore : IEmulatorCore
{
    /// <summary>Envoie une commande et décode sa réponse sous le verrou des requêtes.</summary>
    /// <param name="command">Code de commande partagé avec CoreHost.</param>
    /// <param name="write">Rédacteur facultatif des arguments ; le BinaryWriter fourni reste emprunté.</param>
    /// <param name="read">Lecteur facultatif du résultat après validation de l’en-tête de succès.</param>
    /// <param name="allowUninitialized">Autorise les commandes d’initialisation et de destruction avant le drapeau d’initialisation réussie.</param>
    /// <exception cref="ObjectDisposedException">L’instance est détruite.</exception>
    /// <exception cref="InvalidOperationException">La connexion est indisponible ou le cœur n’a pas encore été initialisé.</exception>
    /// <remarks>Une réponse d’échec reconstruit l’erreur distante. Une réponse de statut inconnu est rejetée.
    /// Les lecteurs et rédacteurs fournis aux callbacks ne doivent pas être fermés par ceux-ci.</remarks>
    private void Request(HostCommand command, Action<BinaryWriter>? write = null, Action<BinaryReader>? read = null, bool allowUninitialized = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_connectionFailed) throw new InvalidOperationException(CoreHostErrors.ProcessUnavailable);
        if (_cancellationToken.IsCancellationRequested)
            FailConnection(new OperationCanceledException(_cancellationToken));
        if (!allowUninitialized && !_initialized)
            throw new InvalidOperationException(CoreHostErrors.ProcessNotInitialized);
        EmulationSdk.HostProtocol.ExecuteRequest(_requestGate,
            () => _writer ?? throw new InvalidOperationException(CoreHostErrors.ProcessUnavailable), () => ReadResponse(command),
            writer =>
            {
                CoreHostFunctions.WriteRequestHeader(writer, command);
                write?.Invoke(writer);
            }, reader =>
            {
                var status = CoreHostFunctions.ReadResponseHeader(reader);
                if (status == HostResponseStatus.Failure) ThrowRemoteError(CoreHostFunctions.ReadError(reader));
                if (status != HostResponseStatus.Success) throw new InvalidDataException(CoreHostErrors.CommunicationFailed);
                read?.Invoke(reader);
            }, FailConnection);
    }

    /// <summary>Attend synchroniquement les octets d’une réponse avec le délai et l’annulation de l’instance.</summary>
    /// <returns>Tableau de la réponse complète fourni au décodeur de protocole.</returns>
    /// <remarks>Le lecteur commun valide la longueur avant allocation et lecture du contenu.</remarks>
    private byte[] ReadResponse(HostCommand command) => EmulationSdk.HostProtocol.ReadResponseAsync(_pipe!, RequestTimeoutFor(command, _responseTimeout),
        _cancellationToken, length => string.Format(CultureInfo.InvariantCulture, CoreHostErrors.InvalidResponseLengthFormat, length))
        .GetAwaiter().GetResult();

    /// <summary>Reconstruit et lève une erreur reçue du processus enfant.</summary>
    /// <param name="error">Description contenant message, catégorie, code, contexte et indicateur de localisation.</param>
    /// <exception cref="EmulationException">L’erreur possède une catégorie et un code internes.</exception>
    /// <exception cref="CoreMediaRequiredException">Le cœur attend un contenu ; Machine restitue alors les lecteurs
    /// disponibles pour permettre à l’utilisateur de choisir son média.</exception>
    /// <exception cref="InvalidOperationException">L’erreur distante ne fournit pas cette identité structurée.</exception>
    private static void ThrowRemoteError(HostError error)
    {
        if (error.Type == typeof(CoreMediaRequiredException).FullName && error.Category == ErrorCategory.Content && error.Code == ErrorCode.ContentRequired)
            throw new CoreMediaRequiredException(error.Message);
        if (error.Category is { } category && error.Code is { } code)
            throw new EmulationException(category, code, error.Message, error.Context, isLocalized: error.IsLocalized);
        throw new InvalidOperationException(error.Message);
    }

    /// <summary>Marque la connexion en échec, termine l’hôte et lève une erreur de communication contextualisée.</summary>
    /// <param name="error">Cause originale conservée comme exception interne.</param>
    /// <remarks>Distingue l’annulation demandée d’un dépassement de délai. Le code de sortie de l’hôte est ajouté
    /// à l’erreur de communication s’il était déjà connu avant la terminaison. Une annulation demandée
    /// reste une OperationCanceledException, après fermeture et attente de l’hôte possédé.</remarks>
    /// <exception cref="OperationCanceledException">Le propriétaire a demandé l’annulation de la communication.</exception>
    /// <exception cref="InvalidOperationException">La connexion ne peut plus être utilisée.</exception>
    private void FailConnection(Exception error)
    {
        var cancelled = _cancellationToken.IsCancellationRequested;
        var timedOut = error is OperationCanceledException && !cancelled;
        var exitSuffix = _process is { HasExited: true } ? string.Format(CultureInfo.InvariantCulture, CoreHostErrors.ProcessExitSuffixFormat, _process.ExitCode) : string.Empty;
        _connectionFailed = true;
        TerminateHostProcess();
        if (cancelled) throw new OperationCanceledException(CoreHostErrors.RequestCancelled, error, _cancellationToken);
        var message = timedOut ? CoreHostErrors.ResponseTimeout : CoreHostErrors.CommunicationFailed + exitSuffix;
        throw new InvalidOperationException(message, error);
    }

    /// <summary>Ferme le tube puis attend la fin forcée du processus, même si la fermeture du transport échoue.</summary>
    private void TerminateHostProcess()
    {
        try { EmulationSdk.HostProtocol.DisposeTransport(_pipe); }
        finally { WaitForHostExit(graceful: false); }
    }

    /// <summary>Attend la sortie effective du processus possédé avec le traitement commun de durée de vie.</summary>
    /// <param name="graceful">Autorise une attente de fermeture normale avant la terminaison imposée par le traitement commun.</param>
    /// <remarks>Le délai de fermeture normale est exprimé en millisecondes dans les constantes du protocole.</remarks>
    private void WaitForHostExit(bool graceful)
    {
        if (_process is { } process)
            EmulationSdk.HostProtocol.WaitForHostExit(process, graceful, CoreHostConstants.GracefulExitTimeoutMilliseconds);
    }

    /// <summary>Ferme la session distante puis libère processus, mémoire vidéo, verrou et transports possédés.</summary>
    /// <remarks>Une commande Dispose est tentée si le tube reste connecté ; son échec n’empêche pas le nettoyage.
    /// Les finally ferment rédacteur et tube, attendent la sortie de l’hôte, détachent les références et vident l’audio.
    /// Les dernières images et les effets de contrôleurs sont retirés pour que les consommateurs ne conservent
    /// pas une sortie active après libération des transports et de la mémoire partagée.
    /// Les déclarations using libèrent les handles du processus et des segments ainsi que le verrou à la sortie du bloc.</remarks>
    public void Dispose()
    {
        if (_disposed) return;
        try
        {
            if (!_connectionFailed && _pipe?.IsConnected == true)
            {
                try
                {
                    Request(HostCommand.Dispose, allowUninitialized: true);
                }
                catch (Exception)
                {
                }
            }
        }
        finally
        {
            _disposed = true;
            using var process = _process;
            using var videoMemory = _videoMemory;
            using var videoMap = _videoMap;
            using var requestGate = _requestGate;
            try
            {
                try
                {
                    try { EmulationSdk.HostProtocol.DisposeTransport(_writer); }
                    finally { EmulationSdk.HostProtocol.DisposeTransport(_pipe); }
                }
                finally { WaitForHostExit(graceful: true); }
            }
            finally
            {
                _writer = null;
                _pipe = null;
                _videoMap = null;
                _videoMemory = null;
                _process = null;
                _initialized = false;
                LatestVideoFrame = null;
                LatestAudioChunk = null;
                OutputAudioStatus = null;
                ControllerFeedback = [];
                SupportsSaveStates = false;
                while (_audio.TryDequeue(out _))
                {
                }
            }
        }
    }
}
