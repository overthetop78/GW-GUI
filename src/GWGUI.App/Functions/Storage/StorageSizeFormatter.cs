using System.Globalization;

namespace GWGUI.App.Functions.Storage;

internal static class StorageSizeFormatter
{
    private const long BytesPerKibibyte = 1024;
    private const long BytesPerMebibyte = BytesPerKibibyte * 1024;
    private const long BytesPerGibibyte = BytesPerMebibyte * 1024;

    internal static string KibibyteUnit => UsesFrenchBinaryUnits ? "Kio" : "KiB";
    internal static string MebibyteUnit => UsesFrenchBinaryUnits ? "Mio" : "MiB";
    internal static string GibibyteUnit => UsesFrenchBinaryUnits ? "Gio" : "GiB";

    internal static string FormatBytes(long bytes)
    {
        if (bytes < BytesPerKibibyte) return $"{bytes} B";
        if (bytes < BytesPerMebibyte) return $"{bytes / (double)BytesPerKibibyte:0.#} {KibibyteUnit}";
        return $"{bytes / (double)BytesPerMebibyte:0.##} {MebibyteUnit}";
    }

    internal static string FormatCapacity(long bytes) => bytes >= BytesPerGibibyte
        ? $"{bytes / (double)BytesPerGibibyte:0.##} {GibibyteUnit}"
        : $"{bytes / (double)BytesPerMebibyte:0.##} {MebibyteUnit}";

    internal static string FormatKibibytes(int kibibytes)
    {
        if (kibibytes < BytesPerKibibyte) return $"{kibibytes} {KibibyteUnit}";
        var mebibytes = kibibytes / (double)BytesPerKibibyte;
        return $"{mebibytes.ToString(mebibytes % 1 == 0 ? "0" : "0.##", CultureInfo.CurrentCulture)} {MebibyteUnit}";
    }

    private static bool UsesFrenchBinaryUnits =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "fr";
}
