namespace GWGUI.MediaEngine.FileSystems.Acorn.FileCore;

/// <summary>Résout les adresses indirectes d'une carte d'allocation FileCore.</summary>
public interface IFileCoreAddressResolver
{
    /// <summary>Adresse indirecte du répertoire racine.</summary>
    int RootAddress { get; }
    /// <summary>Nom du volume.</summary>
    string VolumeName { get; }
    /// <summary>Espace libre déclaré, en octets.</summary>
    long FreeBytes { get; }
    /// <summary>Tente de résoudre l'offset physique d'un objet.</summary>
    bool TryResolveByteOffset(int indirectAddress, long objectByteOffset, out long physicalByteOffset);
}
