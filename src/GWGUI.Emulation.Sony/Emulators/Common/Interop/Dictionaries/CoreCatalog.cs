namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Dictionaries;

internal static class CoreCatalog
{
    internal static IReadOnlyList<Contracts.CoreDefinition> All { get; } =
    [
        BeetlePsx.Constants.CoreConstants.Definition,
        BeetlePsxHw.Constants.CoreConstants.Definition,
        PcsxRearmed.Constants.CoreConstants.Definition,
        Pcee2.Constants.CoreConstants.Definition,
        Play.Constants.CoreConstants.Definition,
        Rpcs3.Constants.CoreConstants.Definition,
        PokketStation.Constants.CoreConstants.Definition,
        SwanStation.Constants.CoreConstants.Definition
    ];

    internal static Contracts.CoreDefinition Get(string id) =>
        All.SingleOrDefault(core => core.Id.Equals(id, StringComparison.Ordinal))
        ?? throw new ArgumentOutOfRangeException(nameof(id), id, null);
}
