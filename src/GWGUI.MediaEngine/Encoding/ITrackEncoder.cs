namespace GWGUI.MediaEngine.Encoding;

/// <summary>Définit un encodeur transformant une piste logique en cellules binaires et en flux.</summary>
public interface ITrackEncoder
{
    /// <summary>Obtient l'identifiant technique stable de l'encodeur.</summary>
    string Id { get; }

    /// <summary>Obtient le nom de l'encodeur destiné à l'affichage.</summary>
    string DisplayName { get; }

    /// <summary>Encode une piste logique complète.</summary>
    /// <param name="request">Description de la piste et de ses secteurs.</param>
    /// <returns>Piste encodée et révolution de flux correspondante.</returns>
    EncodedTrack Encode(TrackEncodeRequest request);
}
