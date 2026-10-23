namespace GWGUI.Emulation.Commodore.Common.Interfaces;
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorSettingsAdapter, IEmulatorInputMapper, IEmulatorConfigurationValidation, IEmulatorSettingsApplication, IEmulatorStorageSettings, IEmulatorFirmwareSettings, IEmulatorPointerCapabilities
{
    /// <summary>Annonce par défaut l’absence d’entrées de pointage ; le backend déclare sa prise en charge effective.</summary>
    bool IEmulatorPointerCapabilities.SupportsPointer(MachineConfiguration configuration) => false;

    /// <summary>Réutilise les associations Common ; la machine ajoute ensuite sa touche de bascule de session.</summary>
    EmulationInputSnapshot IEmulatorInputMapper.MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => InputSnapshotFunctions.Apply(snapshot, configuration.Input, false);

    ValueTask<EmulationMedia> PrepareMediaAsync(EmulationMedia media, string conversionDirectory);
    Task<MachineConfiguration> PrepareRuntimeConfigurationAsync(MachineConfiguration configuration, string conversionDirectory);
    IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates);
    EmulationConfigurationSummary Summarize(MachineConfiguration configuration);
    IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration);
    int GetControllerPortCount(MachineConfiguration configuration);
    MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration);
    IReadOnlySet<EmulationKey> SoftResetKeys { get; }
    /// <summary>Projette les réglages validés du module vers les valeurs textuelles attendues pendant l’exécution.</summary>
    /// <param name="configuration">Configuration de machine, conservée immuable par le module.</param>
    /// <returns>Associations de clés et valeurs techniques ; les nombres utilisent un format indépendant de la langue.</returns>
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
