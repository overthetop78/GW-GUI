using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces;
using C = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;

namespace GWGUI.MediaEngine.Services;

/// <summary>Crée les mécaniques de lecteurs d’images et relaie leur activité au propriétaire de la sortie audio.</summary>
public sealed class MediaDriveService
{
    /// <summary>Signale un lecteur ayant des échantillons à produire, sans ouvrir de sortie audio.</summary>
    public event Action<IMediaDriveMechanics>? AudioAvailable;

    /// <summary>Crée le lecteur de la catégorie reconnue, ou aucun pour un média sans mécanisme disque.</summary>
    public IMediaDriveMechanics? Create(MediaKind kind)
    {
        var device = kind switch { MediaKind.Floppy => C.Floppy, MediaKind.HardDisk => C.HardDisk, MediaKind.Optical => C.Optical, _ => null };
        if (device is null) return null;
        var drive = new MediaDriveMechanics(device);
        drive.AudioAvailable += source => AudioAvailable?.Invoke(source);
        return drive;
    }

    /// <summary>Associe un lecteur au document reconnu sans recopier le contenu des secteurs.</summary>
    internal MediaImageDocument Attach(MediaImageDocument document)
    {
        if (document.Source.Drive is not null) return document;
        var drive = Create(document.MediaKind);
        if (drive is null) return document;
        drive.Insert();
        return new(document.Source with { Drive = drive }, document.FormatId, document.MediaKind,
            document.Representation, document.FileSystemVolumes, document.Diagnostics, document.Metadata);
    }
}
