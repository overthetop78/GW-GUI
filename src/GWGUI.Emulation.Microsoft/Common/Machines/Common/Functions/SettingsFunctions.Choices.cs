using CommonCoreHostFunctions = GWGUI.Emulation.Microsoft.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Functions;

internal static partial class SettingsDescriptionFunctions
{
    private static EmulationSettingsChoice Invariant(string id, string text, long? numericValue = null) => CommonCoreHostFunctions.HardwareSettings.InvariantChoice(id, text, numericValue);
}
