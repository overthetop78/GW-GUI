namespace GWGUI.Emulation.Contracts;

public sealed record EmulationCoreCatalogEntry<TEmulator, TMachineModel>(
    TEmulator Emulator,
    string Id,
    string LibraryName,
    string DllName,
    string ArchiveName,
    Uri ArchiveUri,
    Uri SourceUri,
    string InspectedRevision,
    IReadOnlySet<TMachineModel> Models,
    string? DescriptionResourceKey = null)
{
    internal EmulationEmulatorDefinition ToDefinition() => new(
        Id, LibraryName, DescriptionResourceKey ?? $"Emulation.Emulator.{Id}.Description",
        Models.Select(model => model!.ToString()!).ToHashSet(StringComparer.Ordinal));

    public override string ToString()
    {
        var builder = new System.Text.StringBuilder();
        builder.Append("EmulatorCatalogEntry { ");
        if (PrintMembers(builder)) builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }
}
