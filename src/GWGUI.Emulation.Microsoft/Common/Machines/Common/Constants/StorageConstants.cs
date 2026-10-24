using CommonCoreHostFunctions = GWGUI.Emulation.Microsoft.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Microsoft.Common.Machines.Common.Constants;

internal static class StorageSettingsFunctionsConstants
{
    internal const string CompactDiscDriveLabel = "Optical disc";

    internal static readonly IReadOnlyList<string> XboxExtensions = [CommonCoreHostFunctions.MediaProcessing.FileExtensions["Iso"]];
}
