namespace GWGUI.Emulation.Amiga.Common.Constants;

internal static class MachineConstants
{
    internal const string TheAmigaStateDoesNotMatchTheRunningMachine = "The Amiga state does not match the running machine.";
    internal const string TheAmigaStateFirmwareMediaOrOptionsDoNotMatchTheRunningMachine = "The Amiga state firmware, media or options do not match the running machine.";
    internal const string TheAmigaStateMediaListDoesNotMatchTheRunningMachine = "The Amiga state media list does not match the running machine.";
    internal const string TheAmigaMachineMustBeRunningBeforeChangingAFloppy = "The Amiga machine must be running before changing a floppy.";
    internal const string AmigaDiagnostics = "AmigaDiagnostics";
    internal const string TheAmigaMachineStopped = "The Amiga machine stopped.";
}

internal static class MachineConfigurationConstants
{
    internal const string Amiga = "amiga";
    internal const string A500 = "A500";
    internal const string OptionModel = "puae_model";
    internal const string OptionVideoStandard = "puae_video_standard";
    internal const string PAL = "PAL";
    internal const string OptionFloppyMultidrive = "puae_floppy_multidrive";
    internal const string Disabled = "disabled";
    internal const string OptionFloppyWriteProtection = "puae_floppy_write_protection";
}
