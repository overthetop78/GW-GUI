namespace GWGUI.Emulation.Coleco.Emulators.BlueMSX.Constants;
internal static class SystemFilesConstants
{
    internal const string MachinesDirectory = "Machines";
    internal const string MachineDirectory = "COL - ColecoVision";
    internal const string ConfigurationFileName = "config.ini";
    internal const string FirmwareFileName = "coleco.rom";
    internal const string Configuration = "\r\n[CMOS]\r\nEnable CMOS=0\r\nBattery Backed=0\r\n[Video]\r\nversion=TMS99x8A\r\nvram size=16kB\r\n[Subslotted Slots]\r\nslot 0=0\r\nslot 1=0\r\nslot 2=0\r\nslot 3=0\r\n[External Slots]\r\nslot A=1 0\r\nslot B=1 0\r\n[FDC]\r\nCount=2\r\n[CPU]\r\nZ80 Frequency=3579545Hz\r\n[Board]\r\ntype=ColecoVision\r\n[Slots]\r\n0 0 0 1 20 \"Machines/COL - ColecoVision/coleco.rom\" \"\"\r\n0 0 3 1 93 \"\" \"\"\r\n";
}