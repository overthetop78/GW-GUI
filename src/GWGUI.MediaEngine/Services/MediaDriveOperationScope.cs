using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Services;

/// <summary>Termine l’activité dans le finally du consommateur ; le lecteur emprunté reste possédé par son appelant.</summary>
public sealed class MediaDriveOperationScope : IDisposable
{
    private IMediaDriveMechanics? _drive;

    /// <summary>Conserve une opération active pendant le traitement des données.</summary>
    public MediaDriveOperationScope(IMediaDriveMechanics? drive)
    {
        _drive = drive;
        drive?.BeginOperation();
    }

    /// <summary>Termine une seule fois l’opération sans détruire le lecteur.</summary>
    public void Dispose() => Interlocked.Exchange(ref _drive, null)?.EndOperation();
}
