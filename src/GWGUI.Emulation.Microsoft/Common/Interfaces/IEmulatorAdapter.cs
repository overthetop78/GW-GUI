namespace GWGUI.Emulation.Microsoft.Common.Interfaces;
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorSettings
{
    /// <summary>Réutilise la description Common existante pour le contrat commun de réglages.</summary>
    IReadOnlyList<EmulationSettingsBlock> IEmulatorSettings.DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context) => SettingsDescriptionFunctions.Create(configuration);
}
