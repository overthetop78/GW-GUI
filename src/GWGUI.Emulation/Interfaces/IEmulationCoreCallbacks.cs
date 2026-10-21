using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Callbacks natifs communs fournis par l’hôte d’un cœur indépendant.</summary>
/// <remarks>Le fournisseur conserve ces délégués vivants jusqu’à la fin de leur enregistrement natif et de tous les appels du cœur.</remarks>
public interface IEmulationCoreCallbacks
{
    /// <summary>Reçoit les commandes d’environnement et répond aux capacités prises en charge.</summary>
    ExternalCoreApi.EnvironmentCallback Environment { get; }
    /// <summary>Reçoit une image dont les octets sont empruntés pendant le callback.</summary>
    ExternalCoreApi.VideoCallback Video { get; }
    /// <summary>Reçoit une frame audio stéréo.</summary>
    ExternalCoreApi.AudioSampleCallback AudioSample { get; }
    /// <summary>Reçoit un bloc audio emprunté et renvoie le nombre de frames consommées.</summary>
    ExternalCoreApi.AudioBatchCallback AudioBatch { get; }
    /// <summary>Fige les entrées utilisées par les lectures de la frame.</summary>
    ExternalCoreApi.InputPollCallback InputPoll { get; }
    /// <summary>Lit une entrée par port, périphérique, index et identifiant natifs.</summary>
    ExternalCoreApi.InputStateCallback InputState { get; }
}
