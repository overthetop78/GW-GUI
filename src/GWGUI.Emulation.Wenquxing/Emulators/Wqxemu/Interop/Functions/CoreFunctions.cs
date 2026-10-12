using GWGUI.Emulation.Interop;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Contracts;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Functions;

internal static class CoreFunctions
{
    internal static ExternalCoreExports ResolveExports(ExternalCoreLibrary library) => new(
        library.Resolve<ExternalCoreApi.SetEnvironment>(ExternalCoreExportNames.SetEnvironment),
        library.Resolve<ExternalCoreApi.SetVideo>(ExternalCoreExportNames.SetVideoRefresh),
        library.Resolve<ExternalCoreApi.SetAudioSample>(ExternalCoreExportNames.SetAudioSample),
        library.Resolve<ExternalCoreApi.SetAudioBatch>(ExternalCoreExportNames.SetAudioSampleBatch),
        library.Resolve<ExternalCoreApi.SetInputPoll>(ExternalCoreExportNames.SetInputPoll),
        library.Resolve<ExternalCoreApi.SetInputState>(ExternalCoreExportNames.SetInputState),
        library.Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Initialize),
        library.Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Deinitialize),
        library.Resolve<ExternalCoreApi.GetSystemInfo>(ExternalCoreExportNames.GetSystemInfo),
        library.Resolve<ExternalCoreApi.GetSystemAvInfo>(ExternalCoreExportNames.GetSystemAvInfo),
        library.Resolve<ExternalCoreApi.SetControllerPortDevice>(ExternalCoreExportNames.SetControllerPortDevice),
        library.Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Reset),
        library.Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.Run),
        library.Resolve<ExternalCoreApi.LoadGame>(ExternalCoreExportNames.LoadGame),
        library.Resolve<ExternalCoreApi.LoadGameSpecial>(ExternalCoreExportNames.LoadGameSpecial),
        library.Resolve<ExternalCoreApi.VoidCall>(ExternalCoreExportNames.UnloadGame),
        library.Resolve<ExternalCoreApi.GetRegion>(ExternalCoreExportNames.GetRegion),
        library.Resolve<ExternalCoreApi.GetMemoryData>(ExternalCoreExportNames.GetMemoryData),
        library.Resolve<ExternalCoreApi.GetMemorySize>(ExternalCoreExportNames.GetMemorySize),
        library.Resolve<ExternalCoreApi.GetSerializedSize>(ExternalCoreExportNames.GetSerializedSize),
        library.Resolve<ExternalCoreApi.Serialize>(ExternalCoreExportNames.Serialize),
        library.Resolve<ExternalCoreApi.Serialize>(ExternalCoreExportNames.Unserialize)
    );
}
