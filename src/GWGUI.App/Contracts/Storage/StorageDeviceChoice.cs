using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Storage;

internal sealed record StorageDeviceChoice(EmulationMediaType Type, string Text)
{
    public override string ToString() => Text;
}
