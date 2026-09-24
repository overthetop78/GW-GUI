using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using GWGUI.App.Constants.Input.GameInput;
using GWGUI.App.Contracts.Input.Hid;
using GWGUI.App.Enums.Input;

namespace GWGUI.App.Services.Input.GameInput;

internal sealed class HidReportDecoder : IDisposable
{
    private readonly SafeFileHandle? _handle;
    private readonly IntPtr _preparsedData;
    private readonly HidCapabilities _caps;
    private readonly IReadOnlyList<Binding> _bindings;
    private readonly Func<Binding, byte[], (bool Success, uint Value)> _readValue;
    private readonly Func<Binding, byte[], (bool Success, ushort[] Usages)> _readUsages;
    private bool _disposed;

    private HidReportDecoder(
        SafeFileHandle? handle,
        IntPtr preparsedData,
        HidCapabilities caps,
        IReadOnlyList<Binding> bindings)
    {
        _handle = handle;
        _preparsedData = preparsedData;
        _caps = caps;
        _bindings = bindings;
        _readValue = ReadNativeValue;
        _readUsages = ReadNativeUsages;
        Controls = bindings.Select(binding => binding.Descriptor).ToArray();
    }

    internal HidReportDecoder(ushort reportLength, IReadOnlyList<Binding> bindings,
        Func<Binding, byte[], (bool Success, uint Value)> readValue,
        Func<Binding, byte[], (bool Success, ushort[] Usages)> readUsages)
        : this(null, IntPtr.Zero, new HidCapabilities { InputReportByteLength = reportLength }, bindings)
    {
        _readValue = readValue;
        _readUsages = readUsages;
    }

    internal IReadOnlyList<GameInputControlDescriptor> Controls { get; }

    internal static bool TryCreate(string pnpPath, out HidReportDecoder? decoder)
    {
        decoder = null;
        if (string.IsNullOrWhiteSpace(pnpPath)) return false;

        var handle = HidNative.CreateFileW(
            pnpPath,
            HidConstants.NoDesiredAccess,
            HidConstants.FileShareRead | HidConstants.FileShareWrite,
            IntPtr.Zero,
            HidConstants.OpenExisting,
            HidConstants.NoFileAttributes,
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
                HidNative.HidP_GetCaps(preparsedData, out var caps) != HidConstants.StatusSuccess)
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
            binding.Descriptor.Type == GameInputControlType.Axis
                ? HidConstants.NeutralAxisValue
                : HidConstants.InactiveControlValue,
            GameInputSwitchPosition.Center)).ToArray();

    internal IReadOnlyList<GameInputControlValue> Decode(IReadOnlyList<byte> rawReport)
    {
        if (_disposed || rawReport.Count == 0) return NeutralControls();
        var report = NormalizeReport(rawReport);
        if (report.Length == 0) return NeutralControls();

        var result = new List<GameInputControlValue>(_bindings.Count);
        foreach (var binding in _bindings)
        {
            if (binding.ReportId != HidConstants.UnnumberedReportId && report[0] != binding.ReportId)
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
        var (success, usages) = _readUsages(binding, report);
        return new GameInputControlValue(binding.Descriptor.Type, binding.Descriptor.Index,
            binding.Descriptor.Label,
            success && usages.Contains(binding.Usage)
                ? HidConstants.ActiveControlValue
                : HidConstants.InactiveControlValue);
    }

    private (bool Success, ushort[] Usages) ReadNativeUsages(Binding binding, byte[] report)
    {
        var usages = new ushort[Math.Max(1, binding.UsageCount)];
        uint count = (uint)usages.Length;
        var status = HidNative.HidP_GetUsages(
            HidReportType.Input,
            binding.UsagePage,
            binding.LinkCollection,
            usages,
            ref count,
            _preparsedData,
            report,
            (uint)report.Length);
        return (status == HidConstants.StatusSuccess,
            usages[..checked((int)Math.Min(count, (uint)usages.Length))]);
    }

    private GameInputControlValue ReadAxis(Binding binding, byte[] report)
    {
        var (success, rawValue) = _readValue(binding, report);
        if (!success) return Neutral(binding);
        var value = SignExtend(rawValue, binding.BitSize, binding.LogicalMinimum < 0);
        return new GameInputControlValue(binding.Descriptor.Type, binding.Descriptor.Index,
            binding.Descriptor.Label, Normalize(value, binding.LogicalMinimum, binding.LogicalMaximum));
    }

    private (bool Success, uint Value) ReadNativeValue(Binding binding, byte[] report)
    {
        var status = HidNative.HidP_GetUsageValue(
            HidReportType.Input,
            binding.UsagePage,
            binding.LinkCollection,
            binding.Usage,
            out var rawValue,
            _preparsedData,
            report,
            (uint)report.Length);
        return (status == HidConstants.StatusSuccess, rawValue);
    }

    private GameInputControlValue ReadSwitch(Binding binding, byte[] report)
    {
        var (success, rawValue) = _readValue(binding, report);
        if (!success) return Neutral(binding);
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
        var hasNumberedReports = _bindings.Any(binding =>
            binding.ReportId != HidConstants.UnnumberedReportId);
        var offset = !hasNumberedReports && rawReport.Count == expected - 1 ? 1 : 0;
        var count = Math.Min(rawReport.Count, result.Length - offset);
        for (var index = 0; index < count; index++) result[index + offset] = rawReport[index];
        return result;
    }

    private static GameInputControlValue Neutral(Binding binding) => new(
        binding.Descriptor.Type,
        binding.Descriptor.Index,
        binding.Descriptor.Label,
        binding.Descriptor.Type == GameInputControlType.Axis
            ? HidConstants.NeutralAxisValue
            : HidConstants.InactiveControlValue,
        GameInputSwitchPosition.Center);

    internal static int SignExtend(uint value, ushort bitSize, bool signed)
    {
        if (!signed || bitSize == 0 || bitSize >= HidConstants.MaximumSignedValueBitSize)
            return unchecked((int)value);
        var shift = HidConstants.MaximumSignedValueBitSize - bitSize;
        return unchecked((int)(value << shift)) >> shift;
    }

    internal static float Normalize(int value, int minimum, int maximum)
    {
        if (maximum <= minimum) return HidConstants.InactiveControlValue;
        return (float)Math.Clamp(
            (value - (double)minimum) / (maximum - (double)minimum),
            HidConstants.MinimumNormalizedValue,
            HidConstants.MaximumNormalizedValue);
    }

    internal static GameInputSwitchPosition DecodeHat(int value, int logicalMinimum)
    {
        var ordinal = value - logicalMinimum;
        return ordinal is >= 0 and < HidConstants.HatSwitchPositionCount
            ? (GameInputSwitchPosition)(ordinal + 1)
            : GameInputSwitchPosition.Center;
    }

    private static IReadOnlyList<Binding> ReadBindings(IntPtr preparsedData, HidCapabilities caps)
    {
        var result = new List<Binding>();
        var axisIndex = 0;
        var buttonIndex = 0;
        var switchIndex = 0;

        if (caps.NumberInputButtonCaps > 0
            && caps.NumberInputButtonCaps <= HidConstants.MaximumCapabilityCount)
        {
            var buttonCaps = new HidButtonCapabilities[caps.NumberInputButtonCaps];
            var count = caps.NumberInputButtonCaps;
            if (HidNative.HidP_GetButtonCaps(
                    HidReportType.Input,
                    buttonCaps,
                    ref count,
                    preparsedData) == HidConstants.StatusSuccess)
            {
                foreach (var cap in buttonCaps.Take(count))
                {
                    var minimum = cap.IsRange != 0 ? cap.Union.UsageMinimum : cap.Union.Usage;
                    var maximum = cap.IsRange != 0 ? cap.Union.UsageMaximum : cap.Union.Usage;
                    if (maximum < minimum
                        || maximum - minimum > HidConstants.MaximumUsageRange) continue;
                    var maximumUsageListLength = HidNative.HidP_MaxUsageListLength(
                        HidReportType.Input,
                        cap.UsagePage,
                        preparsedData);
                    var usageCount = checked((int)Math.Clamp(
                        maximumUsageListLength,
                        1u,
                        HidConstants.MaximumUsageListLength));
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

        if (caps.NumberInputValueCaps > 0
            && caps.NumberInputValueCaps <= HidConstants.MaximumCapabilityCount)
        {
            var valueCaps = new HidValueCapabilities[caps.NumberInputValueCaps];
            var count = caps.NumberInputValueCaps;
            if (HidNative.HidP_GetValueCaps(
                    HidReportType.Input,
                    valueCaps,
                    ref count,
                    preparsedData) == HidConstants.StatusSuccess)
            {
                foreach (var cap in valueCaps.Take(count))
                {
                    var minimum = cap.IsRange != 0 ? cap.Union.UsageMinimum : cap.Union.Usage;
                    var maximum = cap.IsRange != 0 ? cap.Union.UsageMaximum : cap.Union.Usage;
                    if (maximum < minimum
                        || maximum - minimum > HidConstants.MaximumUsageRange) continue;
                    for (var usageValue = (int)minimum; usageValue <= maximum; usageValue++)
                    {
                        var usage = checked((ushort)usageValue);
                        var isHat = cap.UsagePage == HidConstants.GenericDesktopUsagePage
                            && usage == HidConstants.HatSwitchUsage;
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
        if (_preparsedData != IntPtr.Zero) HidNative.HidD_FreePreparsedData(_preparsedData);
        _handle?.Dispose();
    }

    internal sealed record Binding(
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
