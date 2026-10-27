using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;
using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.FloppyExtensionConstants;
using S = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.StorageConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Functions;

/// <summary>Description et autorisations de l'extension machine ; aucun réglage indépendant par lecteur.</summary>
internal static class FloppyExtensionFunctions
{
    /// <summary>Contrôleur actif commun aux deux lecteurs.</summary>
    internal static bool UsesWd1770(MachineConfiguration configuration) => configuration.Options.GetValueOrDefault(S.ControllerOption, S.Intel8271) == S.AcornWd1770;

    /// <summary>Présence de la carte double, distincte de son commutateur de sélection.</summary>
    internal static bool HasSolidisk(MachineConfiguration configuration) => bool.Parse(configuration.Options.GetValueOrDefault(C.Solidisk, bool.FalseString));

    /// <summary>Champ ROM correspondant au commutateur commun.</summary>
    internal static string ActiveRom(MachineConfiguration configuration) => UsesWd1770(configuration) ? C.WdRom : C.IntelRom;

    /// <summary>Autorise les deux champs avec Solidisk, sinon seulement celui du contrôleur sélectionné.</summary>
    internal static bool Allows(MachineConfiguration configuration, string field) => HasSolidisk(configuration) || field == ActiveRom(configuration);

    /// <summary>Relit l'ancien champ unique uniquement avant l'enregistrement des nouveaux champs séparés.</summary>
    internal static string RomPath(MachineConfiguration configuration, string field)
    {
        if (configuration.Options.TryGetValue(field, out var path)) return path;
        return !configuration.Options.ContainsKey(C.IntelRom) && !configuration.Options.ContainsKey(C.WdRom)
            && field == ActiveRom(configuration) ? configuration.Options.GetValueOrDefault(S.DfsFirmware, string.Empty) : string.Empty;
    }

    /// <summary>Une ROM sélectionnée installe l'interface même sans lecteur connecté.</summary>
    internal static bool HasInterface(MachineConfiguration configuration) => !string.IsNullOrWhiteSpace(RomPath(configuration, ActiveRom(configuration)));

    /// <summary>Déclare la carte, son commutateur et ses deux ROM externes dans Extensions.</summary>
    internal static EmulationSettingsBlock Describe(MachineConfiguration configuration) => new(C.Group, EmulationMachineTab.Extensions, C.Group,
    [
        new(S.ControllerOption, EmulationMachineTab.Extensions, C.Group, S.InterfaceResource, EmulationSettingsEditor.Selection,
            configuration.Options.GetValueOrDefault(S.ControllerOption, S.Intel8271),
            [new(S.Intel8271, string.Empty, S.Intel8271Display), new(S.AcornWd1770, string.Empty, S.Wd1770Display)],
            RequiresRestart: true, RefreshSettingsOnChange: true, UseRadioButtons: true),
        RomField(configuration, C.IntelRom, C.IntelRomLabel),
        RomField(configuration, C.WdRom, C.WdRomLabel),
        new(C.Solidisk, EmulationMachineTab.Extensions, C.Group, C.SolidiskLabel, EmulationSettingsEditor.Toggle,
            HasSolidisk(configuration).ToString(), EnabledValue: bool.TrueString, DisabledValue: bool.FalseString,
            RefreshSettingsOnChange: true, RequiresRestart: true)
    ]);

    /// <summary>Décrit un chemin conservé même lorsque son contrôleur n'est pas autorisé.</summary>
    private static EmulationSettingsField RomField(MachineConfiguration configuration, string field, string label) => new(
        field, EmulationMachineTab.Extensions, C.Group, label, EmulationSettingsEditor.Path, RomPath(configuration, field),
        IsEnabled: Allows(configuration, field) && !SidewaysExpansionFunctions.ProvidesBank(configuration, SidewaysExpansionFunctions.NativeBank(configuration, Machines.BbcMicro.Common.Constants.DiskConstants.DfsBank)), DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware, RequiresRestart: true);

    /// <summary>Annonce les deux destinations au gestionnaire existant, même lorsqu'un champ est grisé.</summary>
    internal static IReadOnlyList<FirmwareSlot> Slots(MachineConfiguration configuration) =>
    [
        new(C.IntelRom, string.Empty, C.IntelRomLabel, IsRequired: StorageFunctions.NeedsDiskRom(configuration) && !UsesWd1770(configuration), MachineIds: new HashSet<string> { configuration.Model.ToString() }),
        new(C.WdRom, string.Empty, C.WdRomLabel, IsRequired: StorageFunctions.NeedsDiskRom(configuration) && UsesWd1770(configuration), MachineIds: new HashSet<string> { configuration.Model.ToString() })
    ];

    /// <summary>Projette une identité disque vers chaque interface sans changer sa compatibilité avec la machine.</summary>
    internal static IReadOnlyList<EmulationFirmwareCandidate> Destinations(EmulationFirmwareCandidate candidate)
    {
        var identity = candidate.Id.Split(':')[0];
        var known = FirmwareCatalog.BySha1.GetValueOrDefault(identity);
        return new[] { C.IntelRom, C.WdRom }.Select(field => candidate with
        {
            Id = candidate.Id + ":" + field,
            DestinationFieldId = field,
            Compatibility = known is { DiskController: not FirmwareDiskController.Unknown }
                && !known.DiskController.HasFlag(field == C.IntelRom ? FirmwareDiskController.Intel8271 : FirmwareDiskController.AcornWd1770)
                ? EmulationFirmwareCompatibility.Incompatible : candidate.Compatibility
        }).ToArray();
    }
}
