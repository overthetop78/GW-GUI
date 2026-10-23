namespace GWGUI.Emulation.Snk.Common.Interfaces;
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorSettingsAdapter, IEmulatorPointerCapabilities
{
    /// <summary>Annonce par défaut l’absence d’entrées de pointage ; le backend déclare sa prise en charge effective.</summary>
    bool IEmulatorPointerCapabilities.SupportsPointer(MachineConfiguration configuration) => false;

    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
