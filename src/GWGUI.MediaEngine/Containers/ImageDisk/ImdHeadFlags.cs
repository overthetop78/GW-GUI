namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Définit la face et les cartes optionnelles d'une piste ImageDisk.</summary>
[Flags]
public enum ImdHeadFlags : byte
{
    /// <summary>Masque du numéro de face.</summary>
    HeadMask = 0x01,
    /// <summary>Une carte de faces suit la carte de numéros.</summary>
    HasHeadMap = 0x40,
    /// <summary>Une carte de cylindres suit la carte de numéros.</summary>
    HasCylinderMap = 0x80
}
