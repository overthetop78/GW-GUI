using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public static class AtariShortcutFunctions
{
    public static IReadOnlyList<AtariShortcutRule> Rules(AtariMachineConfiguration configuration,
        bool statesAvailable, bool quickStateExists)
    {
        var rules = AtariShortcutConstants.CommonActions.Select(Available).ToList();
        rules.Add(new AtariShortcutRule(EmulationShortcutActions.QuickSave,
            Availability(statesAvailable)));
        rules.Add(new AtariShortcutRule(EmulationShortcutActions.QuickLoad,
            Availability(statesAvailable && quickStateExists)));

        var removable = AtariCompatibilityCatalog.Get(configuration.Model).Media
            .Any(rule => rule.Availability == AtariMediaAvailability.Available && IsRemovable(rule.Category));
        rules.Add(new AtariShortcutRule(EmulationShortcutActions.InsertMedia, Availability(removable)));
        rules.Add(new AtariShortcutRule(EmulationShortcutActions.EjectMedia,
            Availability(configuration.Media.Any(media => media.IsInserted && IsEjectable(media.Category)))));
        rules.Add(new AtariShortcutRule(EmulationShortcutActions.NextMedia,
            Availability(configuration.Media.Count(media => IsDiskSelectable(media.Category)) >
                         AtariShortcutConstants.MinimumMediaForSelection)));
        return rules;
    }

    public static bool IsAvailable(IReadOnlyList<AtariShortcutRule> rules, string action) =>
        rules.FirstOrDefault(rule => string.Equals(rule.Action, action, StringComparison.Ordinal))?.Availability ==
        AtariShortcutAvailability.Available;

    private static AtariShortcutRule Available(string action) =>
        new(action, AtariShortcutAvailability.Available);

    private static AtariShortcutAvailability Availability(bool available) => available
        ? AtariShortcutAvailability.Available
        : AtariShortcutAvailability.Unavailable;

    private static bool IsRemovable(AtariMediaCategory category) => category is AtariMediaCategory.Floppy or
        AtariMediaCategory.Cassette or AtariMediaCategory.Cartridge or AtariMediaCategory.CompactDisc;

    private static bool IsDiskSelectable(AtariMediaCategory category) => category == AtariMediaCategory.Floppy;

    private static bool IsEjectable(AtariMediaCategory category) =>
        category is AtariMediaCategory.Floppy or AtariMediaCategory.Cassette;
}
