namespace GWGUI.Emulation.Example.Common.Machines.Common.Constants;

/// <summary>Identité de module exposée par le contrat de configuration de machine.</summary>
/// <remarks>La configuration et le registre doivent désigner le même module.
/// Cette définition reprend l’identité déclarée par EmulationModuleConstants.</remarks>
internal static class MachineConfigurationConstants
{
    /// <summary>Identifiant technique retourné par IEmulationConfiguration.ModuleId.</summary>
    internal const string ModuleId = EmulationModuleConstants.ModuleId;
}
