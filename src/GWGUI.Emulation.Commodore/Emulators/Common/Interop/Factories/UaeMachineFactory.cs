
namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Factories;

internal abstract class UaeMachineFactory : MachineFactory
{
    internal override MachineConfiguration PrepareConfiguration(MachineConfiguration configuration) =>
        UaeOptionFunctions.ToNative(configuration);

    internal override IReadOnlyDictionary<string, string> NativeOptions(MachineConfiguration configuration,
        IReadOnlyList<MediaConfiguration> media)
    {
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(),
            StringComparer.Ordinal)
        {
            [UaeOptionConstants.Model] = ModelCatalog.BackendModelFor(configuration.Model),
            [UaeOptionConstants.Kickstart] = ExternalCoreConstants.Auto,
            [UaeOptionConstants.MouseToggle] = ExternalCoreConstants.RightControl
        };
        var floppies = media.Where(item => item.Category == MediaCategory.Floppy).ToArray();
        if (floppies.Length > 1)
            options[UaeOptionConstants.FloppyMultidrive] = configuration.MountFloppiesInSeparateDrives
                ? SettingsDescriptionFunctionsConstants.Enabled : SettingsDescriptionFunctionsConstants.Disabled;
        if (floppies.Length > 0 && floppies.All(item => item.IsReadOnly))
            options[UaeOptionConstants.FloppyWriteProtection] = SettingsDescriptionFunctionsConstants.Enabled;
        return options;
    }
}
