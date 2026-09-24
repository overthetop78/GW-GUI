using System.Runtime.InteropServices;

namespace GWGUI.App.Contracts.Input.Hid;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal unsafe struct HidCapabilities
{
    internal ushort Usage;
    internal ushort UsagePage;
    internal ushort InputReportByteLength;
    internal ushort OutputReportByteLength;
    internal ushort FeatureReportByteLength;
    internal fixed ushort Reserved[17];
    internal ushort NumberLinkCollectionNodes;
    internal ushort NumberInputButtonCaps;
    internal ushort NumberInputValueCaps;
    internal ushort NumberInputDataIndices;
    internal ushort NumberOutputButtonCaps;
    internal ushort NumberOutputValueCaps;
    internal ushort NumberOutputDataIndices;
    internal ushort NumberFeatureButtonCaps;
    internal ushort NumberFeatureValueCaps;
    internal ushort NumberFeatureDataIndices;
}
