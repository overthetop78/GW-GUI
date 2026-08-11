namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Définit l'encodage et le débit d'une piste ImageDisk.</summary>
public enum ImdMode : byte
{
    /// <summary>FM à 500 kbps.</summary>
    Fm500Kbps = 0,
    /// <summary>FM à 300 kbps.</summary>
    Fm300Kbps = 1,
    /// <summary>FM à 250 kbps.</summary>
    Fm250Kbps = 2,
    /// <summary>MFM à 500 kbps.</summary>
    Mfm500Kbps = 3,
    /// <summary>MFM à 300 kbps.</summary>
    Mfm300Kbps = 4,
    /// <summary>MFM à 250 kbps.</summary>
    Mfm250Kbps = 5
}
