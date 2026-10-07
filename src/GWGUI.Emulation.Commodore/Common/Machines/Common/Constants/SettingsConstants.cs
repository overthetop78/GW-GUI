namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Constants;

internal static class MachineSettingsConstants
{
    internal const string AudioEnabled = "configuration.audioEnabled";
    internal const string AudioOutput = "configuration.audioOutput";
    internal const string AudioLatency = "configuration.audioLatency";
    internal const string AudioStereoSeparation = "configuration.audioStereoSeparation";
    internal const string Enabled = "enabled";
    internal const string Disabled = "disabled";
    internal const string CandidateRoleSeparator = "|";
    internal const string AllFirmwareFiles = "*";
    internal const char CpuSpeedSeparator = '|';
    internal const int NoDevices = 0;
    internal const int DefaultAudioLatencyMilliseconds = 50;
    internal const int FullStereoSeparation = 100;
    internal const int TwoSettingColumns = 2;
    internal const string ProcessorField = "machine.processors";
    internal const string RamField = "machine.ram";
    internal const string ChipsetField = "machine.chipset";
    internal const string RamKibFormat = "{0} KiB";
    internal const string ProcessorSeparator = " / ";
    internal const int InformationColumnCount = 1;
    internal const string MosDisplayPrefix = "MOS ";
    internal const string WdcDisplayPrefix = "WDC ";
    internal const string ZilogDisplayName = "Z80";
    internal const string ProcessorDisplayPrefix = "Motorola ";
}
