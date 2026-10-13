using GWGUI.Emulation.Apple.Emulators.AppleWin.Constants;
using GWGUI.Emulation.Apple.Emulators.AppleWin.Dictionaries;
using static GWGUI.Emulation.Apple.Emulators.AppleWin.Constants.ExpansionConstants;

namespace GWGUI.Emulation.Apple.Emulators.AppleWin.Functions;

internal static class ExpansionFunctions
{
    private static string Card(MachineConfiguration configuration, string slot) => configuration.Options.GetValueOrDefault(slot, AppleWinOptionCatalog.All.Single(option => option.Key == slot).DefaultValue);
    private static bool Available(MachineConfiguration configuration, string slot, string card) => Card(configuration, slot) is Empty || Card(configuration, slot) == card;

    internal static EmulationSettingsField Fixed(MachineConfiguration configuration, string id, string card, string slot, string label, string explanation, EmulationMachineTab tab) =>
        new(id, tab, tab.ToString(), label, EmulationSettingsEditor.Toggle, Card(configuration, slot) == card ? EmulationSettingConstants.Enabled : EmulationSettingConstants.Disabled,
            IsEnabled: Available(configuration, slot, card), ExplanationResourceKey: explanation, RefreshSettingsOnChange: true, RequiresRestart: true);

    internal static EmulationSettingsField AudioCard(MachineConfiguration configuration, string id, string card, string label)
    {
        var fourth = Card(configuration, OptionConstants.Slot4) == card;
        var fifth = Card(configuration, OptionConstants.Slot5) == card;
        var choices = new List<EmulationSettingsChoice> { new(NoSlot, SettingsChoiceConstants.ValueEmpty) };
        if (Available(configuration, OptionConstants.Slot4, card)) choices.Add(new(FourthSlot, string.Empty, FourthSlot));
        if (Available(configuration, OptionConstants.Slot5, card)) choices.Add(new(FifthSlot, string.Empty, FifthSlot));
        if (choices.Count == 3) choices.Add(new(BothSlots, string.Empty, "4 + 5"));
        return new(id, EmulationMachineTab.Audio, EmulationMachineTab.Audio.ToString(), label, EmulationSettingsEditor.Selection,
            fourth && fifth ? BothSlots : fourth ? FourthSlot : fifth ? FifthSlot : NoSlot, choices,
            IsEnabled: choices.Count > 1, RefreshSettingsOnChange: true, RequiresRestart: true);
    }

    internal static MachineConfiguration ApplySettings(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal);
        Apply(VideoField, VideoHd, [OptionConstants.Slot3], true);
        Apply(MouseField, Mouse, [OptionConstants.Slot4], true);
        Apply(Z80Field, Z80, [OptionConstants.Slot5], true);
        Apply(MockingboardField, Mockingboard, [OptionConstants.Slot4, OptionConstants.Slot5], false);
        Apply(PhasorField, Phasor, [OptionConstants.Slot4, OptionConstants.Slot5], false);
        Apply(SamField, Sam, [OptionConstants.Slot5], true);
        return configuration with { Options = options };

        void Apply(string id, string card, string[] slots, bool toggle)
        {
            options.Remove(id);
            if (!values.TryGetValue(id, out var value)) return;
            foreach (var slot in slots)
                if (Card(configuration with { Options = options }, slot) == card) options[slot] = Empty;
            string[] selected = toggle ? value == EmulationSettingConstants.Enabled ? slots : [] : value switch
            {
                NoSlot => [], FourthSlot => [OptionConstants.Slot4], FifthSlot => [OptionConstants.Slot5], BothSlots => slots,
                _ => throw new ArgumentException(nameof(values))
            };
            foreach (var slot in selected)
            {
                if (!Available(configuration with { Options = options }, slot, card)) throw new ArgumentException(nameof(values));
                options[slot] = card;
            }
        }
    }

    internal static MachineConfiguration ApplyStorage(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots)
    {
        var options = new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal)
        {
            [OptionConstants.Slot7] = slots.Contains(EmulationMediaSlot.HardDisk0) ? OptionConstants.HardDiskController : Empty
        };
        return configuration with { Options = options };
    }

    internal static void InitializeStorage(MachineConfiguration configuration, IDictionary<string, string> options) =>
        options[OptionConstants.Slot7] = configuration.Options.ContainsKey(StorageSettingsFunctionsConstants.StorageDevice + EmulationMediaSlot.HardDisk0)
            || configuration.Media.Any(media => media.Slot == EmulationMediaSlot.HardDisk0) ? OptionConstants.HardDiskController : Empty;
}
