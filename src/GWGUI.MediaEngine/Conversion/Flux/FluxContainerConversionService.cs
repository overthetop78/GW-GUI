using GWGUI.MediaEngine.Containers.Hfe;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Conversion.Flux;

/// <summary>Convertit directement les conteneurs de flux sans construire d'image sectorielle.</summary>
public sealed class FluxContainerConversionService(
    ScpReader scpReader,
    ScpWriter scpWriter,
    HfeReader hfeReader,
    HfeWriter hfeWriter)
{
    public static bool CanConvert(string sourcePath, string formatId, string targetExtension)
    {
        var source = Path.GetExtension(sourcePath);
        var target = Normalize(targetExtension);
        if (source.Equals(DiskImageFileExtensions.Scp, StringComparison.OrdinalIgnoreCase))
            return target is DiskImageFileExtensions.Scp or DiskImageFileExtensions.Hfe;
        if (!source.Equals(DiskImageFileExtensions.Hfe, StringComparison.OrdinalIgnoreCase))
            return false;
        return target == DiskImageFileExtensions.Scp &&
               formatId.Equals(DiskImageFormatIds.RawScp, StringComparison.OrdinalIgnoreCase) ||
               target == DiskImageFileExtensions.Hfe &&
               formatId.Equals(DiskImageFormatIds.RawHfe, StringComparison.OrdinalIgnoreCase);
    }

    public async Task ConvertAsync(
        string sourcePath,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        var source = Normalize(Path.GetExtension(sourcePath));
        var target = Normalize(Path.GetExtension(outputPath));
        if (source == DiskImageFileExtensions.Scp && target == DiskImageFileExtensions.Scp)
        {
            await CopyScpAsync(sourcePath, outputPath, cancellationToken).ConfigureAwait(false);
            return;
        }
        if (source == DiskImageFileExtensions.Hfe && target == DiskImageFileExtensions.Hfe)
        {
            await CopyHfeAsync(sourcePath, outputPath, cancellationToken).ConfigureAwait(false);
            return;
        }
        if (source == DiskImageFileExtensions.Hfe && target == DiskImageFileExtensions.Scp)
        {
            await ConvertHfeToScpAsync(sourcePath, outputPath, cancellationToken).ConfigureAwait(false);
            return;
        }
        if (source == DiskImageFileExtensions.Scp && target == DiskImageFileExtensions.Hfe)
        {
            await ConvertScpToHfeAsync(sourcePath, outputPath, cancellationToken).ConfigureAwait(false);
            return;
        }
        throw new NotSupportedException($"La conversion de flux {source} vers {target} n'est pas prise en charge.");
    }

    private async Task CopyScpAsync(
        string sourcePath,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        var source = await scpReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await AtomicFileWriter.WriteAsync(outputPath, bytes, cancellationToken).ConfigureAwait(false);
        var target = await scpReader.ReadAsync(outputPath, cancellationToken).ConfigureAwait(false);
        ScpFluxParityValidator.Validate(source, target);
        var copied = await File.ReadAllBytesAsync(outputPath, cancellationToken).ConfigureAwait(false);
        if (!bytes.AsSpan().SequenceEqual(copied))
            throw new InvalidDataException("Le conteneur SCP n'a pas été copié à l'identique.");
    }

    private async Task CopyHfeAsync(
        string sourcePath,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        var source = await hfeReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        await AtomicFileWriter.WriteAsync(outputPath, bytes, cancellationToken).ConfigureAwait(false);
        var target = await hfeReader.ReadAsync(outputPath, cancellationToken).ConfigureAwait(false);
        HfeFluxParityValidator.Validate(source, target);
        var copied = await File.ReadAllBytesAsync(outputPath, cancellationToken).ConfigureAwait(false);
        if (!bytes.AsSpan().SequenceEqual(copied))
            throw new InvalidDataException("Le conteneur HFE n'a pas été copié à l'identique.");
    }

    private async Task ConvertHfeToScpAsync(
        string sourcePath,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var source = await hfeReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        var tracks = source.Tracks.Select(ToScpTrack).OrderBy(track => track.TrackNumber).ToArray();
        var start = tracks.Min(track => track.TrackNumber);
        var end = tracks.Max(track => track.TrackNumber);
        var heads = source.Heads == 1 ? ScpHeadSelection.Side0 : ScpHeadSelection.Both;
        var header = new ScpHeader(
            ScpWriterDefaults.Version,
            (byte)ScpDiskType.Other720,
            ScpWriterDefaults.RevolutionCount,
            start,
            end,
            ScpFlags.IndexAligned | ScpFlags.Writable | ScpFlags.ThirdPartyCreator,
            ScpBitCellEncoding.Default16Bit,
            heads,
            ScpWriterDefaults.Resolution,
            ScpFormatConstants.MissingChecksum);
        var image = new ScpImage(header, tracks, true, ScpWriterDefaults.InitialFileSize);
        await scpWriter.WriteAsync(outputPath, image, cancellationToken).ConfigureAwait(false);
        var target = await scpReader.ReadAsync(outputPath, cancellationToken).ConfigureAwait(false);
        HfeScpParityValidator.Validate(source, target);
    }

    private async Task ConvertScpToHfeAsync(
        string sourcePath,
        string outputPath,
        CancellationToken cancellationToken)
    {
        var source = await scpReader.ReadAsync(sourcePath, cancellationToken).ConfigureAwait(false);
        if (source.Header.Revolutions != 1 || source.Tracks.Any(track => track.Revolutions.Count != 1))
            throw new NotSupportedException("HFE v1 ne peut pas conserver plusieurs révolutions SCP séparées.");
        var scale = checked((uint)(source.Header.ResolutionNanoseconds / HfeFormat.TickNanoseconds));
        var values = source.Tracks.SelectMany(track =>
            track.Revolutions[0].FluxIntervals
                .Append(track.Revolutions[0].IndexTimeTicks)
                .Select(value => checked(value * scale)));
        var bitCellTicks = FluxBitCellConverter.GreatestCommonDivisor(values);
        if (bitCellTicks == 0)
            throw new InvalidDataException("Le SCP ne contient aucun timing représentable.");
        var bitRate = CalculateBitRate(bitCellTicks);
        var tracks = source.Tracks.Select(track => ToHfeTrack(track, scale, bitCellTicks)).ToArray();
        EnsureHfeTrackLengthsAreExact(tracks, source.Header);
        var image = new HfeImage(
            HfeFormat.Revision,
            tracks.Max(track => track.Cylinder) + 1,
            tracks.Max(track => track.Head) + 1,
            ScpHfeEncodingResolver.Resolve(source.Header.DiskType),
            bitRate,
            tracks);
        await hfeWriter.WriteAsync(image, outputPath, cancellationToken).ConfigureAwait(false);
        var target = await hfeReader.ReadAsync(outputPath, cancellationToken).ConfigureAwait(false);
        HfeFluxParityValidator.Validate(image, target);
    }

    private static ScpTrack ToScpTrack(HfeTrack track)
    {
        var indexTime = checked((uint)(track.Bits.Count * (long)track.BitCellTicks));
        var intervals = FluxBitCellConverter.ToIntervals(track.Bits, track.BitCellTicks);
        var revolution = new ScpRevolution(indexTime, checked((uint)intervals.Count), intervals);
        return new ScpTrack(
            ScpFormatConstants.ToTrackNumber(track.Cylinder, track.Head),
            track.Cylinder,
            track.Head,
            [revolution]);
    }

    private static HfeTrack ToHfeTrack(ScpTrack track, uint scale, uint bitCellTicks)
    {
        var revolution = track.Revolutions[0];
        var intervals = revolution.FluxIntervals.Select(value => checked(value * scale)).ToArray();
        var indexTime = checked(revolution.IndexTimeTicks * scale);
        var bits = FluxBitCellConverter.ToBits(intervals, indexTime, bitCellTicks);
        return new HfeTrack(track.Cylinder, track.Head, bits, bitCellTicks);
    }

    private static void EnsureHfeTrackLengthsAreExact(
        IReadOnlyList<HfeTrack> tracks,
        ScpHeader header)
    {
        if (tracks.Any(track => track.Bits.Count % HfeFormat.BitsPerByte != 0))
            throw new NotSupportedException("HFE v1 imposerait un remplissage à la fin d'une piste SCP.");
        for (var cylinder = header.StartTrack / 2; cylinder <= header.EndTrack / 2; cylinder++)
        {
            var sides = tracks.Where(track => track.Cylinder == cylinder).ToArray();
            if (sides.Length == 0)
                throw new NotSupportedException($"HFE v1 exige la piste {cylinder}, absente du SCP.");
            if (sides.Select(track => track.Bits.Count).Distinct().Count() != 1)
                throw new NotSupportedException("HFE v1 imposerait un remplissage différent entre les faces.");
        }
    }

    private static ushort CalculateBitRate(uint bitCellTicks)
    {
        var value = HfeFormat.NanosecondsPerSecond /
            (HfeFormat.BitsPerDataBit * HfeFormat.TickNanoseconds * 1000L * bitCellTicks);
        if (value is < 1 or > ushort.MaxValue)
            throw new NotSupportedException("Le timing SCP ne possède pas de bitrate HFE v1 valide.");
        return checked((ushort)value);
    }

    private static string Normalize(string extension) => extension.StartsWith('.')
        ? extension.ToLowerInvariant()
        : "." + extension.ToLowerInvariant();
}
