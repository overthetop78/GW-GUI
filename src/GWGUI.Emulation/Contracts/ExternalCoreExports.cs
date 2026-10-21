using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Contracts;

/// <summary>Exports résolus d’une bibliothèque de cœur externe, partagés par les adaptateurs de modules.</summary>
/// <remarks>Les délégués empruntent la bibliothèque qui les contient. Son propriétaire doit la garder chargée jusqu’à la fin de tous les appels et callbacks, puis arrêter et désinitialiser le cœur avant de la décharger.</remarks>
public sealed record ExternalCoreExports(
    ExternalCoreApi.SetEnvironment SetEnvironment,
    ExternalCoreApi.SetVideo SetVideo,
    ExternalCoreApi.SetAudioSample SetAudioSample,
    ExternalCoreApi.SetAudioBatch SetAudioBatch,
    ExternalCoreApi.SetInputPoll SetInputPoll,
    ExternalCoreApi.SetInputState SetInputState,
    ExternalCoreApi.VoidCall Initialize,
    ExternalCoreApi.VoidCall Deinitialize,
    ExternalCoreApi.GetSystemInfo GetSystemInfo,
    ExternalCoreApi.GetSystemAvInfo GetSystemAvInfo,
    ExternalCoreApi.SetControllerPortDevice SetControllerPortDevice,
    ExternalCoreApi.VoidCall Reset,
    ExternalCoreApi.VoidCall Run,
    ExternalCoreApi.LoadGame LoadGame,
    ExternalCoreApi.VoidCall UnloadGame,
    ExternalCoreApi.GetRegion GetRegion,
    ExternalCoreApi.GetMemoryData GetMemoryData,
    ExternalCoreApi.GetMemorySize GetMemorySize,
    ExternalCoreApi.GetSerializedSize GetSerializedSize,
    ExternalCoreApi.Serialize Serialize,
    ExternalCoreApi.Serialize Unserialize,
    ExternalCoreApi.LoadGameSpecial? LoadGameSpecial = null);
