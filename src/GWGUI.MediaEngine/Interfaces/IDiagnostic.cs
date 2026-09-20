namespace GWGUI.MediaEngine.Exploration.Contracts;

/// <summary>Information, avertissement ou erreur structurée et traduisible.</summary>
public interface IDiagnostic
{
    string Niveau { get; }
    string Code { get; }
    IReadOnlyDictionary<string, string> Parametres { get; }
    int? Cylindre { get; }
    int? Face { get; }
    int? Revolution { get; }
    int? Secteur { get; }
}
