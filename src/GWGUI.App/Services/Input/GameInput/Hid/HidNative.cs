using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using GWGUI.App.Constants.Input.GameInput;
using GWGUI.App.Contracts.Input.Hid;
using GWGUI.App.Enums.Input;

namespace GWGUI.App.Services.Input.GameInput;

internal static class HidNative
{
    [DllImport(HidConstants.KernelLibrary, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern SafeFileHandle CreateFileW(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport(HidConstants.HidLibrary, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static extern bool HidD_GetPreparsedData(
        SafeFileHandle hidDeviceObject,
        out IntPtr preparsedData);

    [DllImport(HidConstants.HidLibrary)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static extern bool HidD_FreePreparsedData(IntPtr preparsedData);

    [DllImport(HidConstants.HidLibrary)]
    internal static extern int HidP_GetCaps(
        IntPtr preparsedData,
        out HidCapabilities capabilities);

    [DllImport(HidConstants.HidLibrary)]
    internal static extern int HidP_GetButtonCaps(
        HidReportType reportType,
        [Out] HidButtonCapabilities[] buttonCaps,
        ref ushort buttonCapsLength,
        IntPtr preparsedData);

    [DllImport(HidConstants.HidLibrary)]
    internal static extern int HidP_GetValueCaps(
        HidReportType reportType,
        [Out] HidValueCapabilities[] valueCaps,
        ref ushort valueCapsLength,
        IntPtr preparsedData);

    [DllImport(HidConstants.HidLibrary)]
    internal static extern uint HidP_MaxUsageListLength(
        HidReportType reportType,
        ushort usagePage,
        IntPtr preparsedData);

    [DllImport(HidConstants.HidLibrary)]
    internal static extern int HidP_GetUsages(
        HidReportType reportType,
        ushort usagePage,
        ushort linkCollection,
        [Out] ushort[] usageList,
        ref uint usageLength,
        IntPtr preparsedData,
        [In] byte[] report,
        uint reportLength);

    [DllImport(HidConstants.HidLibrary)]
    internal static extern int HidP_GetUsageValue(
        HidReportType reportType,
        ushort usagePage,
        ushort linkCollection,
        ushort usage,
        out uint usageValue,
        IntPtr preparsedData,
        [In] byte[] report,
        uint reportLength);
}
