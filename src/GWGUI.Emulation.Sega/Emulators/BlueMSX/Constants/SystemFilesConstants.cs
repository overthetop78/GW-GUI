namespace GWGUI.Emulation.Sega.Emulators.BlueMSX.Constants;

internal static class SystemFilesConstants
{
    internal const string MachinesDirectory = "Machines";
    internal const string ConfigurationFileName = "config.ini";
    internal const string Sg1000Directory = "SEGA - SG-1000";
    internal const string Sg1000Configuration = "\n[CMOS]\nEnable CMOS=0\nBattery Backed=0\n[Video]\nversion=TMS99x8A\nvram size=16kB\n[Subslotted Slots]\nslot 0=0\nslot 1=0\nslot 2=0\nslot 3=0\n[External Slots]\nslot A=1 0\nslot B=1 0\n[FDC]\nCount=2\n[CPU]\nZ80 Frequency=3579545Hz\n[Board]\ntype=SG-1000\n[Slots]\n0 0 6 2 111 \"\" \"\"\n";
    internal const string Sc3000Directory = "SEGA - SC-3000";
    internal const string Sc3000Configuration = "[CMOS]\nEnable CMOS=0\nBattery Backed=0\n[FDC]\nCount=2\n[CPU]\nZ80 Frequency=3579545Hz\n[Board]\ntype=SC-3000\n[Video]\nversion=TMS99x8A\nvram size=16kB\n[Subslotted Slots]\nslot 0=0\nslot 1=0\nslot 2=0\nslot 3=0\n[External Slots]\nslot A=1 0\nslot B=1 0\n[Slots]\n0 0 6 2 111 \"\" \"\"\n";
    internal const string Sf7000Directory = "SEGA - SF-7000";
    internal const string Sf7000Configuration = "[CMOS]\nEnable CMOS=0\nBattery Backed=0\n[FDC]\nCount=1\n[CPU]\nZ80 Frequency=3579545Hz\n[Board]\ntype=SF-7000\n[Video]\nversion=TMS99x8A\nvram size=16kB\n[Subslotted Slots]\nslot 0=0\nslot 1=0\nslot 2=0\nslot 3=0\n[External Slots]\nslot A=1 0\nslot B=1 0\n[Slots]\n0 0 0 8 23 \"\" \"\"\n1 0 0 2 107 \"Machines/SEGA - SF-7000/sf7000.rom\" \"\"\n";
}
