using GWGUI.Emulation.Nec.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Factories;

internal sealed class BeetlePcfxMachineFactory : CoreMachineFactory, IEmulatorPersistentStorage, IEmulatorMachineDefaults, IEmulatorConfigurationValidation
{
    /// <inheritdoc/>
    public override Emulator Emulator => Emulator.BeetlePcfx;
    public void ValidateConfiguration(MachineConfiguration configuration)
    {
        if (BeetlePcfxStorageFunctions.ExternalCardPath(configuration) is { } path) PcFxBackupMemoryFunctions.ValidateExternalCard(path);
    }
    public bool IsDefaultFor(string machineId) => Definition.MachineIds.Contains(machineId);

    public void EnsurePersistentStorage(MachineConfiguration configuration, string directory)
    {
        if (Definition.MachineIds.Contains(configuration.Model)) PcFxBackupMemoryFunctions.EnsureInternalFile(directory);
    }
    protected override CoreReleaseSettings ReleaseSettings => BeetlePcfxConstants.ReleaseSettings;
    public override string EmulatorId => BeetlePcfxConstants.Id;
    public override EmulationEmulatorDefinition Definition { get; } = new(BeetlePcfxConstants.Id, BeetlePcfxConstants.DisplayName, BeetlePcfxConstants.DescriptionResourceKey, new[] { "PcFx" }.ToHashSet(StringComparer.Ordinal));

    public override ValueTask<string?> FindInstalledCorePathAsync(EmulatorManagementContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.HttpClient);
        return base.FindInstalledCorePathAsync(context, cancellationToken);
    }

    protected override MachineConfiguration Configure(MachineConfiguration configuration) => BeetlePcfxOptionFunctions.ToNative(configuration);
}