namespace GWGUI.Emulation.Commodore.Emulators.UAE.Common.Interop.Constants;

internal static class UaeConfigurationConstants
{
    internal const string ConfigurationFile = "gwgui.uae";
    internal const string ConfigurationVersion = "config_version=5.0.0";
    internal const string ChipMemory = "chipmem_size";
    internal const string SlowMemory = "bogomem_size";
    internal const string FastMemory = "fastmem_size";
    internal const string Z3Memory = "z3mem_size";
    internal const string Cpu = "cpu_model";
    internal const string ChipsetConfiguration = "chipset";
    internal const string KickstartConfiguration = "kickstart_rom_file";
    internal const string ExtendedRomConfiguration = "kickstart_ext_rom_file";
    internal const string NtScConfiguration = "ntsc";
    internal const string Enabled = "true";
    internal const string Disabled = "false";
    internal const string FloppyPrefix = "floppy";
    internal const string CdImage = "cdimage0";
    internal const string CdImageSuffix = ",image";
    internal const int MaximumFloppyDrives = 4;
    internal const int ChipMemoryUnitKib = 512;
    internal const int SlowMemoryUnitKib = 256;
    internal const string HardfilePrefix = "hardfile2";
    internal const string FileSystemPrefix = "filesystem2";
    internal const string ReadOnly = "ro";
    internal const string ReadWrite = "rw";
    internal const string HardDrivePrefix = "DH";
    internal const string HardfileGeometry = ",0,0,0,512,0";
    internal static readonly char[] InvalidConfigurationCharacters = ['\r', '\n'];
    internal const string AssignmentSeparator = "=";
    internal const string DirectoryMountFormat = "{0},{1}:{1}:{2},0";
    internal const string HardfileMountFormat = "{0},{1}:{2}{3}";
}
