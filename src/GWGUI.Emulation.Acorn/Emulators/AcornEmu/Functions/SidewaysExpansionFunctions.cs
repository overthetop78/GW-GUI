using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;
using GWGUI.Emulation.Acorn.Common.Exceptions;
using System.Globalization;
using System.IO;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;
using E = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.EmulatorConstants;
using S = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;
using F = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.FloppyExtensionConstants;
using N = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.NetworkConstants;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.SidewaysExpansionConstants;
using D = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Constants.DiskConstants;
using M = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.MemoryExtensionConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

/// <summary>Configuration et images des puces réelles, sans interpréter une grosse puce comme une ROM 16 Kio.</summary>
internal static class SidewaysExpansionFunctions
{
    /// <summary>Décode exclusivement les montages de un à quatre modules réellement décrits.</summary>
    internal static SidewaysExpansionSettings Read(MachineConfiguration configuration)
    {
        var value = configuration.Options.GetValueOrDefault(C.Option, C.None);
        if (value == C.None) return new();
        if (C.AriesModels.TryGetValue(value, out var ariesRam))
        {
            if (BbcMicroMemoryFunctions.ModelABytes(configuration) != BbcMicroModelConstants.ThirtyTwoKibibytes) throw new ArgumentException(ExceptionText.Get(M.RequiresMainRam), nameof(configuration));
            return new(AriesB12: true, AriesRamBytes: ariesRam);
        }
        foreach (var ramRom in new[] { true, false })
        {
            var prefix = ramRom ? C.RamRomPrefix : C.EpromPrefix;
            if (value.StartsWith(prefix, StringComparison.Ordinal) && int.TryParse(value.AsSpan(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var count) && count is >= 1 and <= C.SocketCount && value == prefix + count.ToString(CultureInfo.InvariantCulture))
                return new(count, ramRom);
        }
        throw new ArgumentException(nameof(configuration));
    }

    /// <summary>Identifiant persistant d'une puce de support physique.</summary>
    internal static string ChipField(int socket) => C.ChipPrefix + socket.ToString(CultureInfo.InvariantCulture);

    private static string ChipField(SidewaysExpansionSettings settings, int socket) =>
        (settings.AriesB12 ? C.AriesChipPrefix : C.ChipPrefix) + socket.ToString(CultureInfo.InvariantCulture);

    private static string ChipLabel(SidewaysExpansionSettings settings, int socket) =>
        settings.AriesB12 ? C.AriesSocketLabels[socket] : C.SocketLabels[socket];

    /// <summary>Banque physique d'un rôle installé avant l'extension.</summary>
    internal static int NativeBank(MachineConfiguration configuration, int bank) => Read(configuration).MapNativeBank(bank);

    /// <summary>Choix du matériel installé, indépendant de l'affichage de sa capacité RAM.</summary>
    internal static EmulationSettingsBlock Describe(MachineConfiguration configuration)
    {
        _ = Read(configuration);
        var choices = new List<EmulationSettingsChoice> { new(C.None, M.NoneLabel, NumericValue: 0) };
        foreach (var ramRom in new[] { true, false })
            for (var count = 1; count <= C.SocketCount; count++)
                choices.Add(new((ramRom ? C.RamRomPrefix : C.EpromPrefix) + count.ToString(CultureInfo.InvariantCulture), string.Empty,
                    string.Format(CultureInfo.InvariantCulture, C.AssemblyDisplayFormat, ramRom ? C.RamRomName : C.EpromName, count), ramRom ? count * C.RamBytesPerModule : 0));
        if (BbcMicroMemoryFunctions.ModelABytes(configuration) == BbcMicroModelConstants.ThirtyTwoKibibytes)
            foreach (var model in C.AriesModels)
                choices.Add(new(model.Key, string.Empty, string.Format(CultureInfo.InvariantCulture, C.AriesDisplayFormat, model.Value / SettingsConstants.Kibibyte), model.Value));
        return new(C.ExtensionGroup, EmulationMachineTab.Extensions, C.ExtensionGroup,
            [new(C.Option, EmulationMachineTab.Extensions, C.ExtensionGroup, C.CardLabel, EmulationSettingsEditor.Selection, configuration.Options.GetValueOrDefault(C.Option, C.None), choices, ExplanationResourceKey: C.Help, RequiresRestart: true, RefreshSettingsOnChange: true)]);
    }

    /// <summary>La RAM physique ajoutée est comptée une seule fois, dans l'onglet RAM.</summary>
    internal static EmulationSettingsBlock DescribeRam(MachineConfiguration configuration)
    {
        var settings = Read(configuration);
        return new(C.Group, EmulationMachineTab.Ram, C.Group,
            [new(C.RamCapacity, EmulationMachineTab.Ram, C.Group, C.RamCapacity, EmulationSettingsEditor.Information,
                string.Create(CultureInfo.InvariantCulture, $"{settings.RamBytes / SettingsConstants.Kibibyte} KiB"), NumericValue: settings.RamBytes)],
            IsVisible: settings.Modules > 0 || settings.AriesB12);
    }

    /// <summary>Un chemin par puce, uniquement pour les supports utilisés ; les chemins retirés restent enregistrés.</summary>
    internal static EmulationSettingsBlock DescribeRoms(MachineConfiguration configuration)
    {
        var settings = Read(configuration);
        return new(C.RomGroup, EmulationMachineTab.Rom, C.RomGroup,
            Enumerable.Range(0, settings.RomSocketCount).Select(socket => new EmulationSettingsField(ChipField(settings, socket), EmulationMachineTab.Rom, C.RomGroup,
                ChipLabel(settings, socket), EmulationSettingsEditor.Path, configuration.Options.GetValueOrDefault(ChipField(settings, socket)), DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware, RequiresRestart: true)).ToArray(), IsVisible: settings.RomSocketCount > 0);
    }

    /// <summary>Annonce les fichiers de puce au sélecteur ROM commun.</summary>
    internal static IEnumerable<FirmwareSlot> Slots(MachineConfiguration configuration)
    {
        var settings = Read(configuration);
        return Enumerable.Range(0, settings.RomSocketCount).Select(socket => new FirmwareSlot(ChipField(settings, socket), string.Empty, ChipLabel(settings, socket)));
    }

    /// <summary>Présence explicite d'une image de puce contenant cette banque.</summary>
    internal static bool ProvidesBank(MachineConfiguration configuration, int bank) => Read(configuration).IsChipBank(bank)
        && !string.IsNullOrWhiteSpace(configuration.Options.GetValueOrDefault(ChipField(Read(configuration), Read(configuration).ChipSocket(bank))));

    /// <summary>Lit une banque de la puce choisie ; une petite puce répète ses lignes d'adresse non décodées.</summary>
    internal static byte[] ReadBank(MachineConfiguration configuration, int bank)
    {
        var settings = Read(configuration);
        if (!ProvidesBank(configuration, bank)) throw new ArgumentException(nameof(bank));
        var bytes = File.ReadAllBytes(configuration.Options[ChipField(settings, settings.ChipSocket(bank))]);
        if (!AcceptsLength(settings, bytes.Length)) throw InvalidSize(settings, configuration.Options[ChipField(settings, settings.ChipSocket(bank))]);
        return ExtractBank(bytes, bank, settings);
    }

    private static byte[] ExtractBank(ReadOnlySpan<byte> bytes, int bank, SidewaysExpansionSettings settings)
    {
        var group = settings.ChipGroup(bank);
        var result = new byte[C.BankBytes];
        for (var offset = 0; offset < result.Length; offset++) result[offset] = bytes[(group * C.BankBytes + offset) % bytes.Length];
        return result;
    }

    private static InvalidDataException InvalidSize(SidewaysExpansionSettings settings, string path) =>
        new(ExceptionText.Get(C.InvalidImageSize, path, settings.AriesB12 ? C.AriesImageSizes : settings.RamRom ? C.RamRomImageSizes : C.EpromImageSizes));

    /// <summary>Capacités documentées des puces compatibles avec chaque adaptateur.</summary>
    internal static bool AcceptsLength(SidewaysExpansionSettings settings, long length) => settings.AriesB12 ? length is C.MinimumRamRomImageBytes or C.BankBytes : settings.RamRom
        ? length is C.MinimumRamRomImageBytes or C.BankBytes or C.RamBytesPerModule
        : length is C.RamBytesPerModule or C.EpromBytes;

    /// <summary>Charge toutes les banques définies par une puce ; l'appelant ajoute les rôles non remplacés.</summary>
    internal static IReadOnlyDictionary<int, byte[]> ChipBanks(MachineConfiguration configuration)
    {
        var settings = Read(configuration);
        var banks = new Dictionary<int, byte[]>();
        for (var socket = 0; socket < settings.RomSocketCount; socket++)
        {
            var path = configuration.Options.GetValueOrDefault(ChipField(settings, socket));
            if (string.IsNullOrWhiteSpace(path)) continue;
            var bytes = File.ReadAllBytes(path);
            if (!AcceptsLength(settings, bytes.Length)) throw InvalidSize(settings, path);
            foreach (var bank in settings.BanksForSocket(socket)) banks.Add(bank, ExtractBank(bytes, bank, settings));
        }
        return banks;
    }

    /// <summary>Rôle de firmware existant remplacé par le contenu d'une puce explicitement choisie.</summary>
    internal static int? RoleBank(MachineConfiguration configuration, string field) => field switch
    {
        E.BasicFirmware => NativeBank(configuration, D.BasicBank),
        S.DfsFirmware or F.IntelRom or F.WdRom => NativeBank(configuration, D.DfsBank),
        N.Firmware => NativeBank(configuration, D.NetworkBank),
        M.Firmware => NativeBank(configuration, M.FirmwareBank),
        _ => null
    };
}
