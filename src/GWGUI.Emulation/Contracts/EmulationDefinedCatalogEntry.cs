namespace GWGUI.Emulation.Contracts;

/// <summary>Associe la valeur d’émulateur propre au module à sa définition publique commune.</summary>
/// <remarks>Le catalogue reste dans le module ; cette association ne crée pas de registre central des familles connues.</remarks>
public sealed record EmulationDefinedCatalogEntry<TEmulator>(
    TEmulator Emulator,
    EmulationEmulatorDefinition Definition)
{
    public override string ToString()
    {
        var builder = new System.Text.StringBuilder();
        builder.Append("EmulatorCatalogEntry { ");
        if (PrintMembers(builder)) builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }
}
