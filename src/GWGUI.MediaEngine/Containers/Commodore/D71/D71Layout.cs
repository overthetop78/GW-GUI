using GWGUI.MediaEngine.Geometries.Commodore;

namespace GWGUI.MediaEngine.Containers.Commodore.D71;

/// <summary>Décrit une disposition D71 contenant successivement ses deux faces 1541.</summary>
/// <param name="Name">Nom technique lisible de la disposition.</param>
/// <param name="ImageLength">Longueur totale du conteneur.</param>
/// <param name="TracksPerSide">Nombre de pistes par face.</param>
/// <param name="DataBlockCount">Nombre de blocs des deux faces.</param>
/// <param name="HasErrorMap">Indique la présence d'une carte d'erreurs.</param>
/// <param name="ErrorMapOffset">Offset de la carte d'erreurs lorsqu'elle existe.</param>
public sealed record D71Layout(string Name, int ImageLength, int TracksPerSide, int DataBlockCount, bool HasErrorMap, int? ErrorMapOffset)
{
    /// <summary>Disposition de 35 pistes par face sans carte d'erreurs.</summary>
    public static D71Layout Tracks35 { get; } = Create("35 pistes par face", Commodore1541Geometry.StandardTrackCount, false);
    /// <summary>Disposition de 35 pistes par face avec carte d'erreurs.</summary>
    public static D71Layout Tracks35WithErrors { get; } = Create("35 pistes par face avec erreurs", Commodore1541Geometry.StandardTrackCount, true);
    /// <summary>Disposition de 40 pistes par face sans carte d'erreurs.</summary>
    public static D71Layout Tracks40 { get; } = Create("40 pistes par face", Commodore1541Geometry.ExtendedTrackCount, false);
    /// <summary>Disposition de 40 pistes par face avec carte d'erreurs.</summary>
    public static D71Layout Tracks40WithErrors { get; } = Create("40 pistes par face avec erreurs", Commodore1541Geometry.ExtendedTrackCount, true);
    /// <summary>Catalogue immuable des quatre dispositions reconnues.</summary>
    public static IReadOnlyList<D71Layout> Supported { get; } = Array.AsReadOnly(new[] { Tracks35, Tracks35WithErrors, Tracks40, Tracks40WithErrors });

    /// <summary>Recherche la disposition correspondant exactement à une longueur.</summary>
    public static D71Layout? Find(int imageLength) => Supported.SingleOrDefault(layout => layout.ImageLength == imageLength);

    /// <summary>Calcule une disposition depuis deux faces 1541 et sa carte facultative.</summary>
    private static D71Layout Create(string name, int tracks, bool errors)
    {
        var blocks = Commodore1541Geometry.BlocksPerSide(tracks) * 2;
        var dataLength = blocks * Commodore1541Geometry.SectorSize;
        return new(name, dataLength + (errors ? blocks : 0), tracks, blocks, errors, errors ? dataLength : null);
    }
}
