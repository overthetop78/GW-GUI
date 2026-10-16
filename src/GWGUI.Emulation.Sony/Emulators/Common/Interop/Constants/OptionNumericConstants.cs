namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

internal static class OptionNumericConstants
{
    internal const string IntegerFormat = "0";
    internal const string SignedIntegerFormat = "+0;-0;0";
    internal const string OneDecimalPlaceFormat = "0.0";
    internal const string TwoDecimalPlacesFormat = "0.00";
    internal const string PercentageSuffix = "%";
    internal const string MultiplierSuffix = "x";
    internal const string PixelSuffix = "px";
    internal const string MillisecondsSuffix = " ms";
    internal const string MegabytesSuffix = " MB";
    internal const string GigabytesSuffix = " GB";
    internal const int BinaryMemoryUnitScale = 1024;
    internal static IReadOnlyList<int> DynarecOperationCycleCounts { get; } = Array.AsReadOnly<int>([2, 1]);
    internal static IReadOnlyList<int> DynarecSpuSampleCounts { get; } = Array.AsReadOnly<int>([1, 4, 16]);
}
