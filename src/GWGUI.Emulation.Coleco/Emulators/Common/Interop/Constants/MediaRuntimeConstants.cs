namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Constants;

internal static class MediaRuntimeConstants
{
    internal const int Os7Bytes = 8192;
    internal const int EosBytes = 8192;
    internal const int SmartWriterBytes = 32768;
    internal const string Os7FileName = "coleco.rom";
    internal const string EosFileName = "eos.rom";
    internal const string SmartWriterFileName = "writer.rom";
    internal const string ColecoVisionHardware = "ColecoVision";
    internal const string AdamHardware = "ADAM";
    internal const string Disabled = "Disabled";
    internal const string SaveWorkingCopy = "Save-directory working copy";
    internal const string SuperAction = "Super Action Controller";
    internal const string Wheel = "Wheel Controller";
    internal const string Roller = "Roller Controller";
    internal static readonly string[] DriveNames = ["Disk 1", "Disk 2", "Data Pack 1", "Data Pack 2"];
}
