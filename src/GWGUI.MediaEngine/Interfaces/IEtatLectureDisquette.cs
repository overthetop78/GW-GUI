namespace GWGUI.MediaEngine.Exploration.Contracts;

/// <summary>Instantané structuré envoyé pendant une lecture physique.</summary>
public interface IEtatLectureDisquette
{
    string Etape { get; }
    int NombrePistesTerminees { get; }
    int NombrePistesTotal { get; }
    int? Cylindre { get; }
    int? Face { get; }
    int Tentative { get; }
    IPiste? PisteAcquise { get; }
    IReadOnlyList<IEtatPisteLecture> EtatsPistes { get; }
    string? CodeMessage { get; }
    IReadOnlyDictionary<string, string> ParametresMessage { get; }
    string? MessageExterne { get; }
}

/// <summary>État courant d'une piste demandée pendant l'acquisition.</summary>
public interface IEtatPisteLecture
{
    int Cylindre { get; }
    int Face { get; }
    string Etat { get; }
    int Tentatives { get; }
}
