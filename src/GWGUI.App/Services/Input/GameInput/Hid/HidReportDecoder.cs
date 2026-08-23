using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace GWGUI.App.Services.Input.GameInput;

internal sealed class HidReportDecoder : IDisposable
{
    private const int HidpStatusSuccess = 0x00110000;
    private readonly SafeFileHandle _handle;
    private readonly IntPtr _preparsedData;
    private readonly HidNative.HidCaps _caps;
    private readonly IReadOnlyList<Binding> _bindings;
    private bool _disposed;

    private HidReportDecoder(
        SafeFileHandle handle,
        IntPtr preparsedData,
        HidNative.HidCaps caps,
        IReadOnlyList<Binding> bindings)
    {
        _handle = handle;
        _preparsedData = preparsedData;
        _caps = caps;
        _bindings = bindings;
        Controls = bindings.Select(binding => binding.Descriptor).ToArray();
    }

    internal IReadOnlyList<GameInputControlDescriptor> Controls { get; }

    internal static bool TryCreate(string pnpPath, out HidReportDecoder? decoder)
    {
        decoder = null;
        if (string.IsNullOrWhiteSpace(pnpPath)) return false;

        var handle = HidNative.CreateFileW(
            pnpPath,
            0,
            HidNative.FileShareRead | HidNative.FileShareWrite,
            IntPtr.Zero,
            HidNative.OpenExisting,
            0,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            handle.Dispose();
            return false;
        }

        IntPtr preparsedData = IntPtr.Zero;
        try
        {
            if (!HidNative.HidD_GetPreparsedData(handle, out preparsedData) ||
                preparsedData == IntPtr.Zero ||
                HidNative.HidP_GetCaps(preparsedData, out var caps) != HidpStatusSuccess)
                return false;

            var bindings = ReadBindings(preparsedData, caps);
            if (bindings.Count == 0) return false;
            decoder = new HidReportDecoder(handle, preparsedData, caps, bindings);
            handle = null!;
            preparsedData = IntPtr.Zero;
            return true;
        }
        catch (Exception exception) when (exception is
            DllNotFoundException or EntryPointNotFoundException or
            BadImageFormatException or ExternalException or
            ArgumentException or OverflowException)
        {
            return false;
        }
        finally
        {
            if (preparsedData != IntPtr.Zero)
                HidNative.HidD_FreePreparsedData(preparsedData);
            handle?.Dispose();
        }
    }

    internal IReadOnlyList<GameInputControlValue> NeutralControls() =>
        _bindings.Select(binding => new GameInputControlValue(
            binding.Descriptor.Type,
            binding.Descriptor.Index,
            binding.Descriptor.Label,
            binding.Descriptor.Type == GameInputControlType.Axis ? .5f : 0f,
            GameInputSwitchPosition.Center)).ToArray();

    internal IReadOnlyList<GameInputControlValue> Decode(IReadOnlyList<byte> rawReport)
    {
        if (_disposed || rawReport.Count == 0) return NeutralControls();
        var report = NormalizeReport(rawReport);
        if (report.Length == 0) return NeutralControls();

        var result = new List<GameInputControlValue>(_bindings.Count);
        foreach (var binding in _bindings)
        {
            if (binding.ReportId != 0 && report[0] != binding.ReportId)
            {
                result.Add(Neutral(binding));
                continue;
            }

            switch (binding.Descriptor.Type)
            {
                case GameInputControlType.Button:
                    result.Add(ReadButton(binding, report));
                    break;
                case GameInputControlType.Switch:
                    result.Add(ReadSwitch(binding, report));
                    break;
                default:
                    result.Add(ReadAxis(binding, report));
                    break;
            }
        }
        return result;
    }

    private GameInputControlValue ReadButton(Binding binding, byte[] report)
    {
        var usages = new ushort[Math.Max(1, binding.UsageCount)];
        uint count = (uint)usages.Length;
        var status = HidNative.HidP_GetUsages(
            HidNative.HidReportType.Input,
            binding.UsagePage,
            binding.LinkCollection,
            usages,
            ref count,
            _preparsedData,
            report,
            (uint)report.Length);
        var pressed = status == HidpStatusSuccess &&
            usages.AsSpan(0, checked((int)Math.Min(count, (uint)usages.Length)))
                .Contains(binding.Usage);
        return new GameInputControlValue(
            binding.Descriptor.Type,
            binding.Descriptor.Index,
            binding.Descriptor.Label,
            pressed ? 1f : 0f);
    }

    private GameInputControlValue ReadAxis(Binding binding, byte[] report)
    {
        var status = HidNative.HidP_GetUsageValue(
            HidNative.HidReportType.Input,
            binding.UsagePage,
            binding.LinkCollection,
            binding.Usage,
            out var rawValue,
            _preparsedData,
            report,
            (uint)report.Length);
        if (status != HidpStatusSuccess) return Neutral(binding);
        var value = SignExtend(rawValue, binding.BitSize, binding.LogicalMinimum < 0);
        return new GameInputControlValue(
            binding.Descriptor.Type,
            binding.Descriptor.Index,
            binding.Descriptor.Label,
            Normalize(value, binding.LogicalMinimum, binding.LogicalMaximum));
    }

    private GameInputControlValue ReadSwitch(Binding binding, byte[] report)
    {
        var status = HidNative.HidP_GetUsageValue(
            HidNative.HidReportType.Input,
            binding.UsagePage,
            binding.LinkCollection,
            binding.Usage,
            out var rawValue,
            _preparsedData,
            report,
            (uint)report.Length);
        if (status != HidpStatusSuccess) return Neutral(binding);
        var value = SignExtend(rawValue, binding.BitSize, binding.LogicalMinimum < 0);
        var ordinal = value - binding.LogicalMinimum;
        var position = DecodeHat(value, binding.LogicalMinimum);
        return new GameInputControlValue(
            binding.Descriptor.Type,
            binding.Descriptor.Index,
            binding.Descriptor.Label,
            ordinal,
            position);
    }

    private byte[] NormalizeReport(IReadOnlyList<byte> rawReport)
    {
        var expected = _caps.InputReportByteLength;
        if (expected == 0) return [];
        if (rawReport.Count == expected) return rawReport.ToArray();

        var result = new byte[expected];
        var hasNumberedReports = _bindings.Any(binding => binding.ReportId != 0);
        var offset = !hasNumberedReports && rawReport.Count == expected - 1 ? 1 : 0;
        var count = Math.Min(rawReport.Count, result.Length - offset);
        for (var index = 0; index < count; index++) result[index + offset] = rawReport[index];
        return result;
    }

    private static GameInputControlValue Neutral(Binding binding) => new(
        binding.Descriptor.Type,
        binding.Descriptor.Index,
        binding.Descriptor.Label,
        binding.Descriptor.Type == GameInputControlType.Axis ? .5f : 0f,
        GameInputSwitchPosition.Center);

    internal static int SignExtend(uint value, ushort bitSize, bool signed)
    {
        if (!signed || bitSize == 0 || bitSize >= 32) return unchecked((int)value);
        var shift = 32 - bitSize;
        return unchecked((int)(value << shift)) >> shift;
    }

    internal static float Normalize(int value, int minimum, int maximum)
    {
        if (maximum <= minimum) return 0f;
        return (float)Math.Clamp(
            (value - (double)minimum) / (maximum - (double)minimum),
            0d,
            1d);
    }

    internal static GameInputSwitchPosition DecodeHat(int value, int logicalMinimum)
    {
        var ordinal = value - logicalMinimum;
        return ordinal is >= 0 and < 8
            ? (GameInputSwitchPosition)(ordinal + 1)
            : GameInputSwitchPosition.Center;
    }

    private static IReadOnlyList<Binding> ReadBindings(IntPtr preparsedData, HidNative.HidCaps caps)
    {
        var result = new List<Binding>();
        var axisIndex = 0;
        var buttonIndex = 0;
        var switchIndex = 0;

        if (caps.NumberInputButtonCaps > 0 && caps.NumberInputButtonCaps <= 1024)
        {
            var buttonCaps = new HidNative.HidButtonCaps[caps.NumberInputButtonCaps];
            var count = caps.NumberInputButtonCaps;
            if (HidNative.HidP_GetButtonCaps(
                    HidNative.HidReportType.Input,
                    buttonCaps,
                    ref count,
                    preparsedData) == HidpStatusSuccess)
            {
                foreach (var cap in buttonCaps.Take(count))
                {
                    var minimum = cap.IsRange != 0 ? cap.Union.UsageMinimum : cap.Union.Usage;
                    var maximum = cap.IsRange != 0 ? cap.Union.UsageMaximum : cap.Union.Usage;
                    if (maximum < minimum || maximum - minimum > 1024) continue;
                    var maximumUsageListLength = HidNative.HidP_MaxUsageListLength(
                        HidNative.HidReportType.Input,
                        cap.UsagePage,
                        preparsedData);
                    var usageCount = checked((int)Math.Clamp(
                        maximumUsageListLength,
                        1u,
                        4096u));
                    for (var usageValue = (int)minimum; usageValue <= maximum; usageValue++)
                    {
                        var usage = checked((ushort)usageValue);
                        var descriptor = new GameInputControlDescriptor(
                            GameInputControlType.Button,
                            buttonIndex++,
                            GameInputLabel.None);
                        result.Add(new Binding(
                            descriptor,
                            cap.ReportId,
                            cap.UsagePage,
                            usage,
                            cap.LinkCollection,
                            1,
                            0,
                            1,
                            usageCount));
                    }
                }
            }
        }

        if (caps.NumberInputValueCaps > 0 && caps.NumberInputValueCaps <= 1024)
        {
            var valueCaps = new HidNative.HidValueCaps[caps.NumberInputValueCaps];
            var count = caps.NumberInputValueCaps;
            if (HidNative.HidP_GetValueCaps(
                    HidNative.HidReportType.Input,
                    valueCaps,
                    ref count,
                    preparsedData) == HidpStatusSuccess)
            {
                foreach (var cap in valueCaps.Take(count))
                {
                    var minimum = cap.IsRange != 0 ? cap.Union.UsageMinimum : cap.Union.Usage;
                    var maximum = cap.IsRange != 0 ? cap.Union.UsageMaximum : cap.Union.Usage;
                    if (maximum < minimum || maximum - minimum > 1024) continue;
                    for (var usageValue = (int)minimum; usageValue <= maximum; usageValue++)
                    {
                        var usage = checked((ushort)usageValue);
                        var isHat = cap.UsagePage == 0x01 && usage == 0x39;
                        var descriptor = isHat
                            ? new GameInputControlDescriptor(
                                GameInputControlType.Switch,
                                switchIndex++,
                                GameInputLabel.None,
                                GameInputSwitchKind.EightWay,
                                HatLabels)
                            : new GameInputControlDescriptor(
                                GameInputControlType.Axis,
                                axisIndex++,
                                GameInputLabel.None);
                        result.Add(new Binding(
                            descriptor,
                            cap.ReportId,
                            cap.UsagePage,
                            usage,
                            cap.LinkCollection,
                            cap.BitSize,
                            cap.LogicalMinimum,
                            cap.LogicalMaximum,
                            1));
                    }
                }
            }
        }
        return result;
    }

    private static readonly IReadOnlyList<GameInputLabel> HatLabels =
    [
        GameInputLabel.ArrowUp,
        GameInputLabel.ArrowUpRight,
        GameInputLabel.ArrowRight,
        GameInputLabel.ArrowDownRight,
        GameInputLabel.ArrowDown,
        GameInputLabel.ArrowDownLeft,
        GameInputLabel.ArrowLeft,
        GameInputLabel.ArrowUpLeft
    ];

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        HidNative.HidD_FreePreparsedData(_preparsedData);
        _handle.Dispose();
    }

    private sealed record Binding(
        GameInputControlDescriptor Descriptor,
        byte ReportId,
        ushort UsagePage,
        ushort Usage,
        ushort LinkCollection,
        ushort BitSize,
        int LogicalMinimum,
        int LogicalMaximum,
        int UsageCount);
}

internal static class HidNative
{
    internal const uint FileShareRead = 0x00000001;
    internal const uint FileShareWrite = 0x00000002;
    internal const uint OpenExisting = 3;

    internal enum HidReportType
    {
        Input,
        Output,
        Feature
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal unsafe struct HidCaps
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

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct HidCapsUnion
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

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal unsafe struct HidButtonCaps
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
        internal HidCapsUnion Union;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal unsafe struct HidValueCaps
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
        internal HidCapsUnion Union;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern SafeFileHandle CreateFileW(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("hid.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static extern bool HidD_GetPreparsedData(
        SafeFileHandle hidDeviceObject,
        out IntPtr preparsedData);

    [DllImport("hid.dll")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static extern bool HidD_FreePreparsedData(IntPtr preparsedData);

    [DllImport("hid.dll")]
    internal static extern int HidP_GetCaps(
        IntPtr preparsedData,
        out HidCaps capabilities);

    [DllImport("hid.dll")]
    internal static extern int HidP_GetButtonCaps(
        HidReportType reportType,
        [Out] HidButtonCaps[] buttonCaps,
        ref ushort buttonCapsLength,
        IntPtr preparsedData);

    [DllImport("hid.dll")]
    internal static extern int HidP_GetValueCaps(
        HidReportType reportType,
        [Out] HidValueCaps[] valueCaps,
        ref ushort valueCapsLength,
        IntPtr preparsedData);

    [DllImport("hid.dll")]
    internal static extern uint HidP_MaxUsageListLength(
        HidReportType reportType,
        ushort usagePage,
        IntPtr preparsedData);

    [DllImport("hid.dll")]
    internal static extern int HidP_GetUsages(
        HidReportType reportType,
        ushort usagePage,
        ushort linkCollection,
        [Out] ushort[] usageList,
        ref uint usageLength,
        IntPtr preparsedData,
        [In] byte[] report,
        uint reportLength);

    [DllImport("hid.dll")]
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
