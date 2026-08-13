namespace GWGUI.MediaEngine.Migration;

/// <summary>Décrit une destination de migration de fichiers prise en charge par le moteur.</summary>
public sealed record FileSystemMigrationTarget(
    string FormatId,
    string FileSystemId,
    string Extension);
