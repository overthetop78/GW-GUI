using System.Security.Cryptography;
using GWGUI.Emulation.Acorn.Common.Exceptions;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.MemoryExtensionConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

/// <summary>Décrit les cartes mémoire et leurs prérequis sans modifier la capacité principale demandée.</summary>
internal static class MemoryExtensionFunctions
{
    /// <summary>Valide le choix de carte et le prérequis de RAM principale.</summary>
    internal static MemoryExtensionSettings Read(MachineConfiguration configuration)
    {
        var card = configuration.Options.GetValueOrDefault(C.CardOption, C.None);
        if (card is not (C.None or C.AriesB20 or C.Watford32)) throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        var result = new MemoryExtensionSettings(card);
        if (result.Enabled && BbcMicroMemoryFunctions.ModelABytes(configuration) != BbcMicroModelConstants.ThirtyTwoKibibytes)
            throw new ArgumentException(ExceptionText.Get(C.RequiresMainRam), nameof(configuration));
        return result;
    }

    /// <summary>Bloc RAM : une seule carte, ou Aucune ; la ROM est conservée quand la carte est retirée.</summary>
    internal static EmulationSettingsBlock Describe(MachineConfiguration configuration)
    {
        var settings = Read(configuration);
        return new(C.Group, EmulationMachineTab.Ram, C.Group,
        [
            new(C.CardOption, EmulationMachineTab.Ram, C.Group, C.CardLabel, EmulationSettingsEditor.Selection, settings.Card,
                Choices(),
                IsEnabled: BbcMicroMemoryFunctions.ModelABytes(configuration) == BbcMicroModelConstants.ThirtyTwoKibibytes,
                ExplanationResourceKey: C.Help, RequiresRestart: true, RefreshSettingsOnChange: true),
            new(C.Firmware, EmulationMachineTab.Ram, C.Group, C.FirmwareLabel, EmulationSettingsEditor.Path,
                configuration.Options.GetValueOrDefault(C.Firmware), IsEnabled: settings.Enabled && !SidewaysExpansionFunctions.ProvidesBank(configuration, FirmwareBank(configuration)), IsVisible: settings.Enabled,
                ExplanationResourceKey: C.FirmwareHelp, DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware, RequiresRestart: true)
        ]);
    }

    /// <summary>Capacité physique totale : mémoire principale plus carte, sans confondre leurs espaces d'adressage.</summary>
    internal static long TotalBytes(MachineConfiguration configuration) => BbcMicroMemoryFunctions.ModelABytes(configuration) + Read(configuration).Bytes + SidewaysExpansionFunctions.Read(configuration).RamBytes;

    private static IReadOnlyList<EmulationSettingsChoice> Choices() =>
    [
        new(C.None, C.NoneLabel, NumericValue: 0),
        new(C.AriesB20, C.AriesLabel, NumericValue: C.AriesBytes),
        new(C.Watford32, C.WatfordLabel, NumericValue: C.WatfordBytes)
    ];

    /// <summary>Banque physique occupée par le logiciel de gestion dans le montage courant.</summary>
    internal static int FirmwareBank(MachineConfiguration configuration) => SidewaysExpansionFunctions.NativeBank(configuration, C.FirmwareBank);

    /// <summary>Annonce la destination au sélecteur ROM existant dans l'onglet RAM.</summary>
    internal static FirmwareSlot Slot(MachineConfiguration configuration) => new(C.Firmware, string.Empty, C.FirmwareLabel,
        IsRequired: Read(configuration).Enabled, MachineIds: new HashSet<string> { configuration.Model.ToString() });

    /// <summary>Une ROM reconnue d'une autre carte mémoire n'est pas utilisable dans cette carte.</summary>
    internal static EmulationFirmwareCandidate Destination(MachineConfiguration configuration, EmulationFirmwareCandidate candidate)
    {
        var known = FirmwareCatalog.BySha1.GetValueOrDefault(candidate.Id.Split(':')[0]);
        return known?.Category == FirmwareCategory.MemoryExtension && known.Name != Read(configuration).FirmwareName
            ? candidate with { Compatibility = EmulationFirmwareCompatibility.Incompatible } : candidate;
    }

    /// <summary>Refuse un MOS connu trop ancien sans transformer le catalogue d'empreintes en liste blanche.</summary>
    internal static void ValidateMos(MachineConfiguration configuration, ReadOnlySpan<byte> mos)
    {
        if (!Read(configuration).Enabled) return;
        var known = FirmwareCatalog.BySha1.GetValueOrDefault(Convert.ToHexString(SHA1.HashData(mos)));
        if (known is { Category: FirmwareCategory.OperatingSystem, Version: not null }
            && Version.TryParse(known.Version, out var version) && version < Version.Parse(C.RequiredMosVersion))
            throw new ArgumentException(ExceptionText.Get(C.RequiresMos), nameof(configuration));
    }

    /// <summary>Conserve les emplacements BASIC/DFS/NFS et refuse une collision plutôt qu'un remplacement silencieux.</summary>
    internal static void AddFirmware(MachineConfiguration configuration, IDictionary<int, byte[]> banks)
    {
        var settings = Read(configuration);
        if (!settings.Enabled) return;
        var rom = SidewaysExpansionFunctions.ProvidesBank(configuration, FirmwareBank(configuration)) && banks.TryGetValue(FirmwareBank(configuration), out var chip) ? chip : FirmwareFunctions.Read(configuration, C.Firmware);
        var known = FirmwareCatalog.BySha1.GetValueOrDefault(Convert.ToHexString(SHA1.HashData(rom.AsSpan(0, rom.Length / 2))))
            ?? FirmwareCatalog.BySha1.GetValueOrDefault(Convert.ToHexString(SHA1.HashData(rom)));
        if (known is not null && (known.Category != FirmwareCategory.MemoryExtension || known.Name != settings.FirmwareName))
            throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(configuration));
        if (SidewaysExpansionFunctions.ProvidesBank(configuration, FirmwareBank(configuration))) { banks[FirmwareBank(configuration)] = rom; return; }
        if (!banks.TryAdd(FirmwareBank(configuration), rom)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(banks));
    }
}
