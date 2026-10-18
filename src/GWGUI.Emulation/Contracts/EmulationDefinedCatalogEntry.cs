namespace GWGUI.Emulation.Contracts;

internal sealed record EmulationDefinedCatalogEntry<TEmulator>(
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
