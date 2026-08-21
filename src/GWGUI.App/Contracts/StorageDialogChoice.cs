namespace GWGUI.App.Controls;

internal sealed record StorageDialogChoice(string Value, string Text)
{
    public override string ToString() => Text;
}
