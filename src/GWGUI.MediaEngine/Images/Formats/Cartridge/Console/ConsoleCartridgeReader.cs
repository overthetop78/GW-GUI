using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaFileSystems.Contracts;

namespace GWGUI.MediaEngine.Images.Formats.Cartridge.Console;

/// <summary>Lit les cartouches console par banques de 16 Kio.</summary>
public sealed class ConsoleCartridgeReader : IMediaImageReader
{
    private static readonly IReadOnlyDictionary<string, string> ExtensionFormats =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [DiskImageFileExtensions.Nes] = DiskImageFormatIds.NintendoNes,
            [DiskImageFileExtensions.Sfc] = DiskImageFormatIds.NintendoSnes,
            [DiskImageFileExtensions.Smc] = DiskImageFormatIds.NintendoSnes,
            [DiskImageFileExtensions.N64] = DiskImageFormatIds.NintendoN64,
            [DiskImageFileExtensions.Z64] = DiskImageFormatIds.NintendoN64,
            [DiskImageFileExtensions.V64] = DiskImageFormatIds.NintendoN64,
            [DiskImageFileExtensions.Gb] = DiskImageFormatIds.NintendoGameBoy,
            [DiskImageFileExtensions.Gbc] = DiskImageFormatIds.NintendoGameBoyColor,
            [DiskImageFileExtensions.Cgb] = DiskImageFormatIds.NintendoGameBoyColor,
            [DiskImageFileExtensions.Gba] = DiskImageFormatIds.NintendoGameBoyAdvance,
            [DiskImageFileExtensions.Nds] = DiskImageFormatIds.NintendoNds,
            [DiskImageFileExtensions.Mgw] = DiskImageFormatIds.NintendoGameWatch,
            [DiskImageFileExtensions.ThreeDs] = DiskImageFormatIds.Nintendo3Ds,
            [DiskImageFileExtensions.Cia] = DiskImageFormatIds.Nintendo3Ds,
            [DiskImageFileExtensions.ThreeDsx] = DiskImageFormatIds.Nintendo3Ds,
            [DiskImageFileExtensions.Vb] = DiskImageFormatIds.NintendoVirtualBoy,
            [DiskImageFileExtensions.Sms] = DiskImageFormatIds.SegaMasterSystem,
            [DiskImageFileExtensions.Sg] = DiskImageFormatIds.SegaSg1000,
            [DiskImageFileExtensions.Md] = DiskImageFormatIds.SegaMegaDrive,
            [DiskImageFileExtensions.Gen] = DiskImageFormatIds.SegaMegaDrive,
            [DiskImageFileExtensions.Gg] = DiskImageFormatIds.SegaGameGear,
            [DiskImageFileExtensions.ThirtyTwoX] = DiskImageFormatIds.SegaThirtyTwoX,
            [DiskImageFileExtensions.Pce] = DiskImageFormatIds.NecPcEngine,
            [DiskImageFileExtensions.A26] = DiskImageFormatIds.Atari2600,
            [DiskImageFileExtensions.A52] = DiskImageFormatIds.Atari5200,
            [DiskImageFileExtensions.A78] = DiskImageFormatIds.Atari7800,
            [DiskImageFileExtensions.Ws] = DiskImageFormatIds.BandaiWonderSwan,
            [DiskImageFileExtensions.Wsc] = DiskImageFormatIds.BandaiWonderSwan
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlySet<string> SupportedFormatIds =
        ExtensionFormats.Values.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        ExtensionFormats.Keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.Cartridge }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentations;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.Length <= 0 || !SupportedExtensions.Contains(context.Extension))
            return ValueTask.FromResult(false);
        if (context.RequestedFormatId is null)
            return ValueTask.FromResult(true);
        return ValueTask.FromResult(SupportedFormatIds.Contains(context.RequestedFormatId)
            && ExtensionFormats[context.Extension].Equals(context.RequestedFormatId, StringComparison.OrdinalIgnoreCase));
    }

    Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ExtensionFormats.TryGetValue(context.Extension, out var formatId)
            || context.Length <= 0)
            throw new InvalidDataException("The selected console cartridge is not readable.");
        if (context.RequestedFormatId is not null
            && !context.RequestedFormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The selected cartridge extension does not match its format.");

        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        var bankCount = checked((int)((context.Length + ConsoleCartridgeConstants.BankLength - 1)
            / ConsoleCartridgeConstants.BankLength));
        var capacity = checked((long)bankCount * ConsoleCartridgeConstants.BankLength);
        var ranges = new List<MediaDataRange>(bankCount);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ConsoleCartridgeConstants.SystemId] = SystemFor(formatId),
            [ConsoleCartridgeConstants.MediaRole] = "cartridge",
            [ConsoleCartridgeConstants.FormatId] = formatId,
            [ConsoleCartridgeConstants.BankSize] = ConsoleCartridgeConstants.BankLength.ToString(CultureInfo.InvariantCulture),
            [ConsoleCartridgeConstants.BankCount] = bankCount.ToString(CultureInfo.InvariantCulture),
            [ConsoleCartridgeConstants.StoredLength] = context.Length.ToString(CultureInfo.InvariantCulture),
            [ConsoleCartridgeConstants.SourceExtension] = context.Extension
        };
        for (var bank = 0; bank < bankCount; bank++)
        {
            var offset = bank * (long)ConsoleCartridgeConstants.BankLength;
            var length = (int)Math.Min(ConsoleCartridgeConstants.BankLength, context.Length - offset);
            ranges.Add(new MediaDataRange(offset, length, MediaDataRangeKind.Stored, source, offset));
            metadata[$"{ConsoleCartridgeConstants.BankPrefix}{bank}{ConsoleCartridgeConstants.BankLengthSuffix}"] =
                length.ToString(CultureInfo.InvariantCulture);
            metadata[$"{ConsoleCartridgeConstants.BankPrefix}{bank}{ConsoleCartridgeConstants.BankNameSuffix}"] =
                $"bank{bank:D2}";
        }

        var volume = new MediaVolumeDescriptor(0, capacity, MediaVolumeOrigins.DirectVolume, DisplayName(formatId));
        return Task.FromResult(new MediaImageDocument(
            context.Source,
            formatId,
            MediaKind.Cartridge,
            new BlockMediaImageRepresentation(capacity, ConsoleCartridgeConstants.BankLength, ranges),
            [volume],
            [],
            metadata));
    }

    private static string SystemFor(string formatId) => formatId switch
    {
        var id when id.StartsWith("nintendo.", StringComparison.Ordinal) => "nintendo",
        var id when id.StartsWith("sega.", StringComparison.Ordinal) => "sega",
        var id when id.StartsWith("atari.", StringComparison.Ordinal) => "atari",
        DiskImageFormatIds.NecPcEngine => "nec",
        DiskImageFormatIds.BandaiWonderSwan => "bandai",
        _ => "console"
    };

    private static string DisplayName(string formatId) => formatId switch
    {
        DiskImageFormatIds.NintendoNes => "Nintendo NES",
        DiskImageFormatIds.NintendoSnes => "Nintendo SNES",
        DiskImageFormatIds.NintendoN64 => "Nintendo 64",
        DiskImageFormatIds.NintendoGameBoy => "Nintendo Game Boy",
        DiskImageFormatIds.NintendoGameBoyColor => "Nintendo Game Boy Color",
        DiskImageFormatIds.NintendoGameBoyAdvance => "Nintendo Game Boy Advance",
        DiskImageFormatIds.NintendoNds => "Nintendo DS",
        DiskImageFormatIds.NintendoGameWatch => "Nintendo Game & Watch",
        DiskImageFormatIds.Nintendo3Ds => "Nintendo 3DS",
        DiskImageFormatIds.NintendoVirtualBoy => "Nintendo Virtual Boy",
        DiskImageFormatIds.SegaSg1000 => "Sega SG-1000",
        DiskImageFormatIds.SegaMasterSystem => "Sega Master System",
        DiskImageFormatIds.SegaMegaDrive => "Sega Mega Drive / Genesis",
        DiskImageFormatIds.SegaGameGear => "Sega Game Gear",
        DiskImageFormatIds.SegaThirtyTwoX => "Sega 32X",
        DiskImageFormatIds.NecPcEngine => "NEC PC Engine / TurboGrafx",
        DiskImageFormatIds.Atari2600 => "Atari 2600",
        DiskImageFormatIds.Atari5200 => "Atari 5200",
        DiskImageFormatIds.Atari7800 => "Atari 7800",
        DiskImageFormatIds.BandaiWonderSwan => "Bandai WonderSwan",
        _ => "Console cartridge"
    };
}
