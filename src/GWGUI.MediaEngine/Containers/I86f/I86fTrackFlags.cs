namespace GWGUI.MediaEngine.Containers.I86f;

/// <summary>Définit les drapeaux propres à une piste 86F.</summary>
[Flags]
public enum I86fTrackFlags : ushort
{
    /// <summary>Aucun drapeau.</summary>
    None = 0,
    /// <summary>Masque sélectionnant le mode d'encodage de la piste.</summary>
    EncodingMask = 0x0018,
    /// <summary>Valeur du masque d'encodage correspondant à ISO MFM ; les autres valeurs prises en charge sont lues en ISO FM.</summary>
    MfmEncoding = 0x0008
}
