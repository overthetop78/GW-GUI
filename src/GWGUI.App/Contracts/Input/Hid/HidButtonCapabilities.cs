using System.Runtime.InteropServices;

namespace GWGUI.App.Contracts.Input.Hid;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal unsafe struct HidButtonCapabilities
{
    internal ushort UsagePage;
    internal byte ReportId;
    internal byte IsAlias;
    internal ushort BitField;
    internal ushort LinkCollection;
    internal ushort LinkUsage;
    internal ushort LinkUsagePage;
    internal byte IsRange;
    internal byte IsStringRange;
    internal byte IsDesignatorRange;
    internal byte IsAbsolute;
    internal ushort ReportCount;
    internal ushort Reserved2;
    internal fixed uint Reserved[9];
    internal HidCapabilitiesRange Union;
}
