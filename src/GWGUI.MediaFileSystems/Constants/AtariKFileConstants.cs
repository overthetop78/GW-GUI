namespace GWGUI.MediaFileSystems.Constants;

/// <summary>Constantes de structure et de résultat des disquettes Atari K-file utilisant KBoot.</summary>
internal static class AtariKFileConstants
{
    public const int BootSectorCount = 3;
    public const int BootFlagOffset = 0;
    public const int BootSectorCountOffset = 1;
    public const int BootLoadAddressOffset = 2;
    public const int BootInitAddressOffset = 4;
    public const int JumpOpcodeOffset = 6;
    public const int JumpAddressLowOffset = 7;
    public const int JumpAddressHighOffset = 8;
    public const int PayloadLengthOffset = 9;
    public const int SectorSize = 128;
    public const int MinimumExecutableLength = 2;
    public const ushort BootLoadAddress = 0x0700;
    public const ushort ExecutableMarker = 0xffff;
    public const byte JumpOpcode = 0x4c;
    public const byte JumpAddressLow = 0x14;
    public const byte JumpAddressHigh = 0x07;
    public const string InvalidImageMessage = "The sector image is not an Atari K-file disk using KBoot.";
    public const string BootableAttribute = "bootable";
    public const string SingleProgramAttribute = "single-program";
    public const string KBootAttribute = "kboot";
}
