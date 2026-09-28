using MediaImageFormatIds = global::GWGUI.MediaFileSystems.Constants.MediaImageFormatIds;

namespace GWGUI.MediaEngine.Constants;

/// <summary>Regroupe les identifiants publics des formats d'images reconnus par le moteur.</summary>
public static partial class DiskImageFormatIds
{
    /// <summary>Identifiant utilisé lorsqu'aucun format n'a pu être déterminé.</summary>
    public const string Unknown = "unknown";
    /// <summary>Identifiant générique des conteneurs ImageDisk.</summary>
    public const string Imd = "imd";
    /// <summary>Identifiant générique des conteneurs Teledisk.</summary>
    public const string Td0 = "td0";
    /// <summary>Identifiant neutre des conteneurs CPCEMU DSK.</summary>
    public const string CpcEmuDsk = MediaImageFormatIds.CpcEmuDsk;
    /// <summary>Identifiant du conteneur de pistes HxC Floppy Emulator.</summary>
    public const string RawHfe = "raw.hfe";
    /// <summary>Identifiant du conteneur de flux SuperCard Pro.</summary>
    public const string RawScp = "raw.scp";
    /// <summary>Identifiant des captures de flux multipistes HxC Stream.</summary>
    public const string RawHxcStream = "raw.hxcstream";
    /// <summary>Identifiant des captures de flux multipistes KryoFlux.</summary>
    public const string RawKryoFlux = "raw.kryoflux";
    /// <summary>Identifiant des images de disque Xbox XDVDFS/XISO.</summary>
    public const string MicrosoftXboxXdvdfs = OpticalImageFormatIds.XboxXdvdfs;
    /// <summary>Identifiant des cartouches Amstrad Plus/GX4000.</summary>
    public const string AmstradCpr = MediaImageFormatIds.AmstradCpr;
    /// <summary>Identifiant des ROM brutes Amstrad CPC.</summary>
    public const string AmstradRom = MediaImageFormatIds.AmstradRom;
    /// <summary>Identifiant des images de cartouche Nintendo Entertainment System.</summary>
    public const string NintendoNes = MediaImageFormatIds.NintendoNes;
    /// <summary>Identifiant des images de disquette Famicom Disk System.</summary>
    public const string NintendoFamicomDisk = MediaImageFormatIds.NintendoFamicomDisk;
    /// <summary>Identifiant des images de cartouche Super Nintendo Entertainment System.</summary>
    public const string NintendoSnes = MediaImageFormatIds.NintendoSnes;
    /// <summary>Identifiant des images de cartouche Nintendo 64.</summary>
    public const string NintendoN64 = MediaImageFormatIds.NintendoN64;
    /// <summary>Identifiant des images de cartouche Nintendo Game Boy.</summary>
    public const string NintendoGameBoy = MediaImageFormatIds.NintendoGameBoy;
    /// <summary>Identifiant des images de cartouche Nintendo Game Boy Color.</summary>
    public const string NintendoGameBoyColor = MediaImageFormatIds.NintendoGameBoyColor;
    /// <summary>Identifiant des images de cartouche Nintendo Game Boy Advance.</summary>
    public const string NintendoGameBoyAdvance = MediaImageFormatIds.NintendoGameBoyAdvance;
    /// <summary>Identifiant des images de cartouche Nintendo DS.</summary>
    public const string NintendoNds = MediaImageFormatIds.NintendoNds;
    /// <summary>Identifiant des jeux Nintendo Game &amp; Watch.</summary>
    public const string NintendoGameWatch = MediaImageFormatIds.NintendoGameWatch;
    /// <summary>Identifiant des images de cartouche Nintendo 3DS.</summary>
    public const string Nintendo3Ds = MediaImageFormatIds.Nintendo3Ds;
    /// <summary>Identifiant des images de disque Nintendo Wii U WUD/WUX.</summary>
    public const string NintendoWiiU = MediaImageFormatIds.NintendoWiiU;
    /// <summary>Identifiant des images de cartouche Nintendo Virtual Boy.</summary>
    public const string NintendoVirtualBoy = MediaImageFormatIds.NintendoVirtualBoy;
    /// <summary>Identifiant des images de cartouche Sega SG-1000.</summary>
    public const string SegaSg1000 = MediaImageFormatIds.SegaSg1000;
    /// <summary>Identifiant des images de cartouche Sega Master System.</summary>
    public const string SegaMasterSystem = MediaImageFormatIds.SegaMasterSystem;
    /// <summary>Identifiant des images de cartouche Sega Mega Drive/Genesis.</summary>
    public const string SegaMegaDrive = MediaImageFormatIds.SegaMegaDrive;
    /// <summary>Identifiant des images de cartouche Sega Game Gear.</summary>
    public const string SegaGameGear = MediaImageFormatIds.SegaGameGear;
    /// <summary>Identifiant des extensions Sega 32X.</summary>
    public const string SegaThirtyTwoX = MediaImageFormatIds.SegaThirtyTwoX;
    /// <summary>Identifiant des images de cartouche NEC PC Engine/TurboGrafx.</summary>
    public const string NecPcEngine = MediaImageFormatIds.NecPcEngine;
    /// <summary>Identifiant des images de cartouche NEC SuperGrafx.</summary>
    public const string NecSuperGrafx = MediaImageFormatIds.NecSuperGrafx;
    /// <summary>Identifiant des images de cartouche Atari 2600.</summary>
    public const string Atari2600 = MediaImageFormatIds.Atari2600;
    /// <summary>Identifiant des images de cartouche Atari 5200.</summary>
    public const string Atari5200 = MediaImageFormatIds.Atari5200;
    /// <summary>Identifiant des images de cartouche Atari 7800.</summary>
    public const string Atari7800 = MediaImageFormatIds.Atari7800;
    /// <summary>Identifiant des images de cartouche Bandai WonderSwan.</summary>
    public const string BandaiWonderSwan = MediaImageFormatIds.BandaiWonderSwan;
    /// <summary>Identifiant des conteneurs ApriDisk ACT Apricot PC/Xi de 315 Kio.</summary>
    public const string ApricotPcXi315 = MediaImageFormatIds.ApricotPcXi315;
}
