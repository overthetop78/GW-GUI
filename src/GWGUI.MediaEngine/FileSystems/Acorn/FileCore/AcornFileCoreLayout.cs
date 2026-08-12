using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Acorn.FileCore;

/// <summary>Définit la disposition générale des cartes d'allocation FileCore.</summary>
public static class AcornFileCoreLayout
{
    /// <summary>Premier bloc contenant le DiscRecord.</summary>
    public const int DiscRecordBlock = 0;
    /// <summary>Offset du DiscRecord dans son bloc.</summary>
    public const int DiscRecordOffset = 4;
    /// <summary>Longueur du DiscRecord en octets.</summary>
    public const int DiscRecordLength = 60;
    /// <summary>Longueur du DiscRecord en bits.</summary>
    public const int DiscRecordBitLength = DiscRecordLength * BitPrimitives.BitsPerByte;
    /// <summary>Identifiant du fragment racine.</summary>
    public const int RootFragmentId = 2;
    /// <summary>Longueur de l'en-tête précédant la carte de zone.</summary>
    public const int ZoneHeaderBitLength = 32;
    /// <summary>Offset du lien libre dans une zone.</summary>
    public const int FreeLinkBitOffset = BitPrimitives.BitsPerByte;
    /// <summary>Masque de l'offset de partage d'une adresse indirecte.</summary>
    public const int ShareOffsetMask = 0xff;
    /// <summary>Valeur retranchée à un offset de partage non nul avant son application.</summary>
    public const int ShareOffsetBias = 1;
    /// <summary>Décalage de l'identifiant de fragment.</summary>
    public const int FragmentIdShift = BitPrimitives.BitsPerByte;
    /// <summary>Masque des liens libres.</summary>
    public const uint FreeLinkMask = 0x7fff;
    /// <summary>Longueur maximale d'un identifiant de liste libre.</summary>
    public const int MaximumFreeIdBitLength = 15;
    /// <summary>Nombre d'octets de la fenêtre de lecture des bits.</summary>
    public const int BitWindowByteLength = sizeof(uint);
    /// <summary>Masque d'un offset intra-octet.</summary>
    public const int IntraByteBitMask = BitPrimitives.BitsPerByte - 1;
}
