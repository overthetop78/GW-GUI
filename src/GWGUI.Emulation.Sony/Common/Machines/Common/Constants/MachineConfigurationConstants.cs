namespace GWGUI.Emulation.Sony.Common.Machines.Common.Constants;


internal static class MachineConfigurationConstants
{
    internal const string ModuleId = "sony";
    internal const string ResourcePrefix = "Emulation.Sony.Machine.";

    internal static string ResourceKey(string modelId) => ResourcePrefix + (modelId switch
    {
        "Psp" => "PSP",
        "PsVita" => "PSVita",
        _ => modelId
    });
}
