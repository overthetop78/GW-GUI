namespace GWGUI.Emulation.Interfaces;

public interface IEmulationRuntime
{
    IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity { get; }
    string EmulatorName { get; }
    string EmulatorVersion { get; }
    IReadOnlySet<string> SupportedContentExtensions { get; }
    IReadOnlyList<EmulationOption> AvailableOptions { get; }
    ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default);
}
