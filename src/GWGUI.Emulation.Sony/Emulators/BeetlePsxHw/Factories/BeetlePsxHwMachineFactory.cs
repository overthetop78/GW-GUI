using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants;
using ToggleValues = GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants.ToggleValueConstants;

namespace GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Factories;

internal sealed class BeetlePsxHwMachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;
    public override int GetControllerPortCount(MachineConfiguration configuration) =>
        base.GetControllerPortCount(configuration)
        + AdditionalControllers(OptionConstants.EnableMultitapPort1Key, configuration)
        + AdditionalControllers(OptionConstants.EnableMultitapPort2Key, configuration);

    private static int AdditionalControllers(string key, MachineConfiguration configuration) =>
        configuration.Options?.GetValueOrDefault(key) == ToggleValues.Enabled
            ? ControllerPortConstants.AdditionalControllersPerMultitap
            : ControllerPortConstants.NoAdditionalControllers;
}
