namespace GWGUI.App.Controls;

internal sealed record DiskSizeChoice(long? SizeMiB, string Text)
{
    public override string ToString() => Text;
}
