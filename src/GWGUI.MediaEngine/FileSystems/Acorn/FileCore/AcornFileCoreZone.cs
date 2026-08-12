namespace GWGUI.MediaEngine.FileSystems.Acorn.FileCore;

/// <summary>Contient une copie validée d'une zone de carte FileCore.</summary>
public sealed class AcornFileCoreZone
{
    private readonly byte[] _data;

    /// <summary>Crée une zone et valide ses limites de bits.</summary>
    public AcornFileCoreZone(IEnumerable<byte> data, int startMapBit, int startBit, int endBit)
    {
        _data = data.ToArray();
        var capacity = checked(_data.Length * 8);
        if (startMapBit < 0 || startBit < 0 || startBit >= endBit || endBit > capacity) throw AcornFileCoreExceptions.InvalidZone(startBit, endBit, capacity);
        StartMapBit = startMapBit;
        StartBit = startBit;
        EndBit = endBit;
    }

    /// <summary>Données en lecture seule de la zone.</summary>
    public ReadOnlySpan<byte> Data => _data;
    /// <summary>Position de la zone dans la carte globale.</summary>
    public int StartMapBit { get; }
    /// <summary>Premier bit utile.</summary>
    public int StartBit { get; }
    /// <summary>Dernier bit utile exclusif.</summary>
    public int EndBit { get; }
}
