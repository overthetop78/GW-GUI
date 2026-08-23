using System.IO;
using System.Reflection;

namespace GWGUI.App.Services.Input.GameInput;

internal static class GameControllerNameDatabase
{
    private const string ResourceName = "GWGUI.App.Assets.Input.gamecontrollerdb.txt";
    private static readonly Lazy<IReadOnlyDictionary<uint, GameControllerDatabaseEntry>> Entries = new(Load);

    internal static string? Find(ushort vendorId, ushort productId) =>
        FindEntry(vendorId, productId)?.Name;

    internal static GameControllerDatabaseEntry? FindEntry(ushort vendorId, ushort productId) =>
        Entries.Value.GetValueOrDefault(((uint)vendorId << 16) | productId);

    private static IReadOnlyDictionary<uint, GameControllerDatabaseEntry> Load()
    {
        var result = new Dictionary<uint, GameControllerDatabaseEntry>();
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
        if (stream is null) return result;
        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            if (line.Length < 34 || line[0] == '#' ||
                !line.Contains("platform:Windows", StringComparison.Ordinal))
                continue;
            var guid = line[..32];
            if (!guid.All(Uri.IsHexDigit) ||
                !TryReadLittleEndianId(guid.AsSpan(8, 8), out var vendorId) ||
                !TryReadLittleEndianId(guid.AsSpan(16, 8), out var productId))
                continue;
            var fields = line[33..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length == 0 || string.IsNullOrWhiteSpace(fields[0])) continue;
            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in fields.Skip(1))
            {
                var separator = field.IndexOf(':');
                if (separator <= 0 || separator == field.Length - 1) continue;
                var target = field[..separator].Trim();
                if (target.Equals("platform", StringComparison.OrdinalIgnoreCase) ||
                    target.Equals("hint", StringComparison.OrdinalIgnoreCase) ||
                    target.Equals("crc", StringComparison.OrdinalIgnoreCase))
                    continue;
                mappings.TryAdd(target, field[(separator + 1)..].Trim());
            }
            result.TryAdd(((uint)vendorId << 16) | productId,
                new GameControllerDatabaseEntry(fields[0].Trim(), mappings));
        }
        return result;
    }

    private static bool TryReadLittleEndianId(ReadOnlySpan<char> value, out ushort result)
    {
        result = 0;
        return byte.TryParse(value[..2], System.Globalization.NumberStyles.HexNumber, null, out var low) &&
            byte.TryParse(value.Slice(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var high) &&
            (result = (ushort)(low | high << 8)) >= 0;
    }
}
