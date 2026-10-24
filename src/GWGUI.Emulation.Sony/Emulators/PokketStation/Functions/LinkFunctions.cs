using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Common.Exceptions;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Constants;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Enums;

namespace GWGUI.Emulation.Sony.Emulators.PokketStation.Functions;

internal static class LinkFunctions
{
    internal static IReadOnlyList<EmulationMedia> Cards(MachineConfiguration configuration) => EmulationMediaConversionFunctions.ToCommon(configuration.Media ?? [])
        .Where(media => media.Type == EmulationMediaType.MemoryCard && media.IsInserted).OrderBy(media => media.Slot.Index).ToArray();

    internal static bool IsLinked(MachineConfiguration configuration) => Cards(configuration).Any(card => card.Slot.Index == 1);

    internal static CoreContentLoadPlan? CreateLoadPlan(MachineConfiguration configuration)
    {
        if (!IsLinked(configuration)) return null;
        var cards = Cards(configuration);
        if (cards.Count != LinkConstants.UnitCount || cards[0].Slot.Index != 0 || cards[1].Slot.Index != 1)
            throw new InvalidOperationException(CoreExceptions.ContentRefused());
        var identity = CommonCoreHostFunctions.StateFiles.HashBytes(Encoding.UTF8.GetBytes(Path.GetFullPath(cards[1].Path).ToUpperInvariant()));
        return new(LinkConstants.SubsystemId, cards.Select(card => card.Path).ToArray(),
        [
            new(LinkConstants.UnitAFlashId, SaveMemoryConstants.FileName),
            new(LinkConstants.UnitBFlashId, LinkConstants.SecondarySavePrefix + identity + ".bin")
        ]);
    }

    internal static string ToNative(LinkLayout value) => value switch
    {
        LinkLayout.SideBySide => LinkConstants.SideBySide,
        LinkLayout.Stacked => LinkConstants.Stacked,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(LinkAudio value) => value switch
    {
        LinkAudio.BothUnits => LinkConstants.BothUnits,
        LinkAudio.UnitA => LinkConstants.UnitA,
        LinkAudio.UnitB => LinkConstants.UnitB,
        LinkAudio.Split => LinkConstants.SplitAudio,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static string ToNative(LinkIdentity value) => value switch
    {
        LinkIdentity.Different => LinkConstants.DifferentIdentity,
        LinkIdentity.Same => LinkConstants.SameIdentity,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
}
