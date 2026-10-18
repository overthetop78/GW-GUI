namespace GWGUI.Emulation.Contracts;

public sealed record EmulationCatalogCoreRelease<TEmulator>(
    TEmulator Emulator,
    string Id,
    string DeclaredVersion,
    Uri DownloadUri,
    DateTimeOffset PublishedUtc,
    long? ExpectedArchiveSize)
{
    public override string ToString()
    {
        var builder = new System.Text.StringBuilder();
        builder.Append("CoreRelease { ");
        if (PrintMembers(builder)) builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }
}
