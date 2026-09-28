using System.Globalization;
using System.IO;

namespace GWGUI.MediaEngine.Images.Formats.Optical.Gdi;

/// <summary>Lit le descripteur texte GDI et valide ses déclarations de pistes.</summary>
internal sealed class GdiDescriptorReader
{
    public async Task<GdiDescriptor> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var lines = await File.ReadAllLinesAsync(path, cancellationToken).ConfigureAwait(false);
        return Parse(lines);
    }

    public GdiDescriptor Parse(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var meaningful = lines
            .Select(line => line.Trim())
            .Where(line => line.Length != 0 && !line.StartsWith('#'))
            .ToArray();
        if (meaningful.Length < 2)
            throw new InvalidDataException("A GDI descriptor must contain a track count and at least one track.");
        if (!int.TryParse(meaningful[0], NumberStyles.None, CultureInfo.InvariantCulture, out var declaredCount)
            || declaredCount <= 0)
            throw new InvalidDataException("The GDI track count is missing or invalid.");
        if (meaningful.Length - 1 != declaredCount)
            throw new InvalidDataException("The GDI track count does not match the descriptor entries.");

        var tracks = new List<GdiTrackDeclaration>(declaredCount);
        var numbers = new HashSet<int>();
        long previousLba = -1;
        for (var index = 0; index < declaredCount; index++)
        {
            var fields = Tokenize(meaningful[index + 1]);
            if (fields.Count != GdiConstants.DescriptorFieldCount)
                throw new InvalidDataException($"The GDI track line {index + 2} does not contain six fields.");
            var number = ParsePositive(fields[0], "track number");
            var lba = ParseNonNegativeLong(fields[1], "track LBA");
            var control = ParseNonNegative(fields[2], "track control");
            var sectorSize = ParsePositive(fields[3], "sector size");
            var fileName = fields[4];
            var fileOffset = ParseNonNegativeLong(fields[5], "file offset");
            if (!numbers.Add(number))
                throw new InvalidDataException($"The GDI descriptor contains duplicate track number {number}.");
            if (lba <= previousLba)
                throw new InvalidDataException("GDI track LBAs must increase strictly.");
            if (control is not (GdiConstants.AudioControl or GdiConstants.DataControl))
                throw new NotSupportedException($"Unsupported GDI track control {control}.");
            if (sectorSize is not (GdiConstants.DataSectorSize or GdiConstants.RawSectorSize))
                throw new NotSupportedException($"Unsupported GDI sector size {sectorSize}.");
            if (fileName.Length == 0 || Path.IsPathFullyQualified(fileName))
                throw new InvalidDataException("A GDI track file name must be a relative file name.");
            tracks.Add(new GdiTrackDeclaration(number, lba, control, sectorSize, fileName, fileOffset));
            previousLba = lba;
        }

        return new GdiDescriptor(tracks);
    }

    private static IReadOnlyList<string> Tokenize(string line)
    {
        var values = new List<string>();
        var buffer = new System.Text.StringBuilder();
        var quoted = false;
        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                quoted = !quoted;
                continue;
            }
            if (char.IsWhiteSpace(character) && !quoted)
            {
                if (buffer.Length > 0)
                {
                    values.Add(buffer.ToString());
                    buffer.Clear();
                }
                continue;
            }
            buffer.Append(character);
        }
        if (quoted)
            throw new InvalidDataException("A GDI track file name has an unterminated quote.");
        if (buffer.Length > 0) values.Add(buffer.ToString());
        return values;
    }

    private static int ParsePositive(string value, string field) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) && result > 0
            ? result
            : throw new InvalidDataException($"The GDI {field} is invalid.");

    private static int ParseNonNegative(string value, string field) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) && result >= 0
            ? result
            : throw new InvalidDataException($"The GDI {field} is invalid.");

    private static long ParseNonNegativeLong(string value, string field) =>
        long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) && result >= 0
            ? result
            : throw new InvalidDataException($"The GDI {field} is invalid.");
}

internal sealed record GdiDescriptor(IReadOnlyList<GdiTrackDeclaration> Tracks);

internal sealed record GdiTrackDeclaration(
    int Number,
    long Lba,
    int Control,
    int SectorSize,
    string FileName,
    long FileOffset);
