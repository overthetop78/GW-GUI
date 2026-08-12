using GWGUI.MediaEngine.Geometries.Commodore;

namespace GWGUI.MediaEngine.Containers.Commodore.D64;

/// <summary>Décrit une disposition D64 et l'emplacement facultatif de sa carte d'erreurs.</summary>
/// <param name="Name">Nom technique lisible de la disposition.</param>
/// <param name="ImageLength">Longueur totale du conteneur.</param>
/// <param name="TrackCount">Nombre de pistes.</param>
/// <param name="DataBlockCount">Nombre de blocs de données.</param>
/// <param name="HasErrorMap">Indique la présence d'une carte d'erreurs.</param>
/// <param name="ErrorMapOffset">Offset de la carte d'erreurs lorsqu'elle existe.</param>
public sealed record D64Layout(string Name, int ImageLength, int TrackCount, int DataBlockCount, bool HasErrorMap, int? ErrorMapOffset)
{
    /// <summary>Disposition standard de 35 pistes sans carte d'erreurs.</summary>
    public static D64Layout Tracks35 { get; } = Create("35 pistes", Commodore1541Geometry.StandardTrackCount, false);
    /// <summary>Disposition standard de 35 pistes avec carte d'erreurs.</summary>
    public static D64Layout Tracks35WithErrors { get; } = Create("35 pistes avec erreurs", Commodore1541Geometry.StandardTrackCount, true);
    /// <summary>Disposition étendue de 40 pistes sans carte d'erreurs.</summary>
    public static D64Layout Tracks40 { get; } = Create("40 pistes", Commodore1541Geometry.ExtendedTrackCount, false);
    /// <summary>Disposition étendue de 40 pistes avec carte d'erreurs.</summary>
    public static D64Layout Tracks40WithErrors { get; } = Create("40 pistes avec erreurs", Commodore1541Geometry.ExtendedTrackCount, true);
    /// <summary>Catalogue immuable des quatre dispositions reconnues.</summary>
    public static IReadOnlyList<D64Layout> Supported { get; } = Array.AsReadOnly(new[] { Tracks35, Tracks35WithErrors, Tracks40, Tracks40WithErrors });

    /// <summary>Recherche la disposition correspondant exactement à la longueur indiquée.</summary>
    public static D64Layout? Find(int imageLength) => Supported.SingleOrDefault(layout => layout.ImageLength == imageLength);

    /// <summary>Construit une disposition depuis la géométrie et la longueur exacte de sa carte.</summary>
    private static D64Layout Create(string name, int tracks, bool errors)
    {
        var blocks = Commodore1541Geometry.BlocksPerSide(tracks);
        var dataLength = blocks * Commodore1541Geometry.SectorSize;
        return new(name, dataLength + (errors ? blocks : 0), tracks, blocks, errors, errors ? dataLength : null);
    }
}
