using System.Runtime.InteropServices;

namespace GWGUI.App.Contracts.Input.Hid;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal unsafe struct HidValueCapabilities
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
    internal byte HasNull;
    internal byte Reserved;
    internal ushort BitSize;
    internal ushort ReportCount;
    internal fixed ushort Reserved2[5];
    internal uint UnitsExponent;
    internal uint Units;
    internal int LogicalMinimum;
    internal int LogicalMaximum;
    internal int PhysicalMinimum;
    internal int PhysicalMaximum;
    internal HidCapabilitiesRange Union;
}
