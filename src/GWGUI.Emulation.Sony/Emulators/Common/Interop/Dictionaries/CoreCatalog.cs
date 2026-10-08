namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Dictionaries;

internal static class CoreCatalog
{
    internal static IReadOnlyList<Contracts.CoreDefinition> All { get; } =
    [
        SwanStation.Constants.CoreConstants.Definition
    ];

    internal static Contracts.CoreDefinition Get(string id) =>
        All.SingleOrDefault(core => core.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);
}
