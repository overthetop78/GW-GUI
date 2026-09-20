namespace GWGUI.MediaEngine.Conversion.Migration.Apple;

/// <summary>Valide les noms ProDOS sans les transformer silencieusement.</summary>
public sealed class ProDosNamePolicy : IMigrationNamePolicy
{
    /// <inheritdoc />
    public bool IsValid(string name) => new GWGUI.MediaFileSystems.FileSystems.Apple.ProDos.ProDosNamePolicy().IsValid(name);
}
