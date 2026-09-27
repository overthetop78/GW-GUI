namespace GWGUI.Emulation.Contracts;

public sealed record EmulationEmulatorInstallation(
    EmulationEmulatorDefinition Emulator,
    string? InstalledVersion)
{
    public EmulationEmulatorInstallation(string emulatorId, string? installedVersion)
        : this(new EmulationEmulatorDefinition(emulatorId, emulatorId,
            $"Emulation.Emulator.{emulatorId}.Description", new HashSet<string>(StringComparer.Ordinal)),
            installedVersion)
    {
    }

    public string EmulatorId => Emulator.Id;
    public string DisplayName => Emulator.DisplayName;
    public string DescriptionResourceKey => Emulator.DescriptionResourceKey;
}
