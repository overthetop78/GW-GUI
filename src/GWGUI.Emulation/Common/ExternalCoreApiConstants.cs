namespace GWGUI.Emulation.Common;

internal static class ExternalCoreApiConstants
{
    internal const uint GetCanDuplicateFrames = 3;
    internal const uint SetMessage = 6;
    internal const uint GetSystemDirectory = 9;
    internal const uint SetPixelFormat = 10;
    internal const uint SetInputDescriptors = 11;
    internal const uint SetKeyboardCallback = 12;
    internal const uint SetDiskControl = 13;
    internal const uint GetVariable = 15;
    internal const uint SetVariables = 16;
    internal const uint GetVariableUpdate = 17;
    internal const uint SetSupportNoGame = 18;
    internal const uint GetLogInterface = 27;
    internal const uint GetContentDirectory = 30;
    internal const uint GetSaveDirectory = 31;
    internal const uint SetSystemAvInfo = 32;
    internal const uint SetControllerInfo = 35;
    internal const uint ExperimentalCommandFlag = 0x10000;
    internal const uint SetMemoryMaps = 36 | ExperimentalCommandFlag;
    internal const uint SetGeometry = 37;
    internal const uint SetSupportAchievements = 42 | ExperimentalCommandFlag;
    internal const uint GetVfsInterface = 45 | ExperimentalCommandFlag;
    internal const uint GetLedInterface = 46 | ExperimentalCommandFlag;
    internal const uint GetInputBitmasks = 51 | ExperimentalCommandFlag;
    internal const uint GetCoreOptionsVersion = 52;
    internal const uint SetCoreOptionsDisplay = 55;
    internal const uint GetDiskControlVersion = 57;
    internal const uint SetDiskControlExtended = 58;
    internal const uint GetMessageInterfaceVersion = 59;
    internal const uint SetMessageExtended = 60;
    internal const uint SetFastForwardingOverride = 64;
    internal const uint SetCoreOptionsV2 = 67;
    internal const uint SetCoreOptionsV2International = 68;
    internal const uint SetCoreOptionsUpdateDisplayCallback = 69;
}
