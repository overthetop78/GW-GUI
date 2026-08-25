
namespace GWGUI.Emulation.Atari.Cores;

internal sealed record AtariExternalCoreExports(
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
    ExternalCoreApi.Serialize Unserialize);
