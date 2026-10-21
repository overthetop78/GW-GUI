namespace GWGUI.Emulation.Interfaces;

/// <summary>Variante existante pour les cœurs dont l’API d’insertion reçoit un chemin de contenu.</summary>
public interface IEmulationCorePathMediaCommands : IEmulationCoreStateCommands
{
    /// <summary>Nombre d’images disponibles dans le contenu chargé.</summary>
    int DiskCount { get; }
    /// <summary>Index de l’image actuellement sélectionnée selon l’API du cœur.</summary>
    int CurrentDiskIndex { get; }
    /// <summary>Insère le contenu du chemin fourni, sans transférer la propriété du fichier.</summary>
    void InsertMedia(string path);
    /// <summary>Éjecte le contenu du lecteur exposé par cette API.</summary>
    void EjectMedia();
}
