using System.Runtime.InteropServices;

namespace GWGUI.App.Contracts.Input.Hid;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal struct HidCapabilitiesRange
{
    internal ushort UsageMinimum;
    internal ushort UsageMaximum;
    internal ushort StringMinimum;
    internal ushort StringMaximum;
    internal ushort DesignatorMinimum;
    internal ushort DesignatorMaximum;
    internal ushort DataIndexMinimum;
    internal ushort DataIndexMaximum;

    internal ushort Usage => UsageMinimum;
}
