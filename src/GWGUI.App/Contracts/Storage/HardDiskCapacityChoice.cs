namespace GWGUI.App.Contracts.Storage;

internal sealed record HardDiskCapacityChoice(long? Bytes, string Text)
{
    public override string ToString() => Text;
}
