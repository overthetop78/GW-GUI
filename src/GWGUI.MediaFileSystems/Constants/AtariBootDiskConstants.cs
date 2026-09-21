namespace GWGUI.MediaFileSystems.Constants;

/// <summary>Constantes de la structure de démarrage des disquettes Atari 8 bits.</summary>
internal static class AtariBootDiskConstants
{
    public const int HeaderLength = 6;
    public const int SectorCountOffset = 1;
    public const int LoadAddressOffset = 2;
    public const int InitializationAddressOffset = 4;
    public const int BootSectorSize = 128;
    public const int AddressSpaceLength = ushort.MaxValue + 1;
    public const string InvalidImageMessage = "The sector image does not contain a complete Atari boot program.";
    public const string BootableAttribute = "bootable";
    public const string DirectBootProgramAttribute = "direct-boot-program";
}
