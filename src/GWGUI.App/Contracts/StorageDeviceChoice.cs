using GWGUI.Emulation;

namespace GWGUI.App.Controls;

internal sealed record StorageDeviceChoice(EmulationMediaType Type, string Text)
{
    public override string ToString() => Text;
}
