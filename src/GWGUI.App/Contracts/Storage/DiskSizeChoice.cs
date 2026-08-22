namespace GWGUI.App.Contracts.Storage;

internal sealed record DiskSizeChoice(long? SizeMiB, string Text)
{
    public override string ToString() => Text;
}
