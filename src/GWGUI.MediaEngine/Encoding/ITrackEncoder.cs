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
    /// <exception cref="ArgumentNullException">La requête est nulle.</exception>
    /// <exception cref="ArgumentException">La requête ne respecte pas les contraintes communes ou celles du format.</exception>
    /// <exception cref="InvalidOperationException">L'encodeur ne produit aucune cellule binaire.</exception>
    EncodedTrack Encode(TrackEncodeRequest request);
}
