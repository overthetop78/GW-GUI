namespace GWGUI.MediaEngine.FileSystems.Fat;

/// <summary>Identifie les descripteurs FAT historiques pris en charge pour les disquettes IBM PC.</summary>
public enum FatMediaDescriptor : byte
{
    /// <summary>160 Kio, 40 pistes, une face et huit secteurs.</summary>
    Ibm160 = 0xfe,
    /// <summary>180 Kio, 40 pistes, une face et neuf secteurs.</summary>
    Ibm180 = 0xfc,
    /// <summary>320 Kio, 40 pistes, deux faces et huit secteurs.</summary>
    Ibm320 = 0xff,
    /// <summary>360 Kio, 40 pistes, deux faces et neuf secteurs.</summary>
    Ibm360 = 0xfd
}
