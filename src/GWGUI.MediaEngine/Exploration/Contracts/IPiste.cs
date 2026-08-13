namespace GWGUI.MediaEngine.Exploration.Contracts;

/// <summary>Piste physique ou logique d'une image.</summary>
public interface IPiste
{
    int? NumeroSource { get; }
    int Cylindre { get; }
    int Face { get; }
    IReadOnlyList<IRevolution> Revolutions { get; }
    IReadOnlyList<ISecteurSource> SecteursSource { get; }
}

/// <summary>Révolution de flux capturée ou synthétique.</summary>
public interface IRevolution
{
    int Numero { get; }
    long DebutIndex { get; }
    long DureeNanosecondes { get; }
    int Resolution { get; }
    uint? NombreFluxDeclare { get; }
    string Origine { get; }
    IReadOnlyList<uint> TransitionsFlux { get; }
}

/// <summary>Secteur directement fourni par une image sectorielle source.</summary>
public interface ISecteurSource
{
    int Numero { get; }
    int Taille { get; }
    ReadOnlyMemory<byte> Donnees { get; }
}
