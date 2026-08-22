namespace GWGUI.App.Contracts.Storage;

internal sealed record StorageDialogChoice(string Value, string Text)
{
    public override string ToString() => Text;
}
