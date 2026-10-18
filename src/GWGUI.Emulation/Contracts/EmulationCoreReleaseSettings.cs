namespace GWGUI.Emulation.Contracts;

internal sealed record EmulationCoreReleaseSettings(
    Uri OfficialArchiveUri,
    string ArchiveLibraryName,
    string InstalledLibraryName,
    string DisplayName)
{
    public override string ToString()
    {
        var builder = new System.Text.StringBuilder();
        builder.Append("CoreReleaseSettings { ");
        if (PrintMembers(builder)) builder.Append(' ');
        builder.Append('}');
        return builder.ToString();
    }
}
