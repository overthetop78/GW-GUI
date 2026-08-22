using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Emulation.Shortcuts;
using GWGUI.App.Functions.Emulation.Shortcuts;
using GWGUI.Emulation;
using System.Windows.Input;

namespace GWGUI.Tests;

public sealed class EmulationShortcutFunctionsTests
{
    [Fact]
    public void GlobalActionHasPriorityAndDoesNotRepeatWhileHeld()
    {
        var chord = new KeyboardChord(ModifierKeys.Control, [Key.R]);
        IReadOnlyList<GlobalShortcutBinding> bindings =
        [
            new GlobalShortcutBinding(EmulationShortcutActions.HardReset, chord)
        ];
        IReadOnlySet<Key> pressed = new HashSet<Key> { Key.R };
        var inactive = EmulationShortcutFunctions.ResolveGlobal(bindings, ModifierKeys.Control, pressed, Key.R,
            new HashSet<string>());
        Assert.Equal(EmulationShortcutMatchCategory.Global, inactive.Category);
        Assert.Equal(EmulationShortcutActions.HardReset, inactive.Action);
        Assert.True(inactive.ShouldExecute);

        var active = EmulationShortcutFunctions.ResolveGlobal(bindings, ModifierKeys.Control, pressed, Key.R,
            new HashSet<string> { EmulationShortcutActions.HardReset });
        Assert.Equal(EmulationShortcutMatchCategory.Global, active.Category);
        Assert.False(active.ShouldExecute);
    }

    [Fact]
    public void PartialGlobalChordIsReservedBeforeMachineInput()
    {
        IReadOnlyList<GlobalShortcutBinding> bindings =
        [
            new GlobalShortcutBinding(EmulationShortcutActions.HardReset,
                new KeyboardChord(ModifierKeys.Control, [Key.R, Key.H]))
        ];
        var match = EmulationShortcutFunctions.ResolveGlobal(bindings, ModifierKeys.Control,
            new HashSet<Key> { Key.R }, Key.R, new HashSet<string>());
        Assert.Equal(EmulationShortcutMatchCategory.ReservedForGlobal, match.Category);
    }

    [Fact]
    public void ReleaseAndConflictDetectionAreDeterministic()
    {
        var chord = new KeyboardChord(ModifierKeys.Control, [Key.R]);
        IReadOnlyList<GlobalShortcutBinding> bindings =
        [
            new GlobalShortcutBinding(EmulationShortcutActions.HardReset, chord),
            new GlobalShortcutBinding(EmulationShortcutActions.SoftReset, chord)
        ];
        Assert.True(EmulationShortcutFunctions.HasConflict(bindings));

        ISet<string> active = new HashSet<string> { EmulationShortcutActions.HardReset };
        EmulationShortcutFunctions.ReleaseInactive(active, bindings, ModifierKeys.Control, new HashSet<Key>());
        Assert.Empty(active);
    }

    [Fact]
    public void ConfiguredGlobalHostBindingReplacesTheDefaultChord()
    {
        var bindings = EmulationShortcutMap.GlobalShortcuts(new Dictionary<string, string>
        {
            [EmulationShortcutActions.SoftReset] = "Ctrl+F8"
        });

        var configured = EmulationShortcutFunctions.ResolveGlobal(bindings, ModifierKeys.Control,
            new HashSet<Key> { Key.F8 }, Key.F8, new HashSet<string>());
        Assert.Equal(EmulationShortcutMatchCategory.Global, configured.Category);
        Assert.Equal(EmulationShortcutActions.SoftReset, configured.Action);
        Assert.True(configured.ShouldExecute);

        var formerDefault = EmulationShortcutFunctions.ResolveGlobal(bindings,
            ModifierKeys.Control | ModifierKeys.Alt, new HashSet<Key> { Key.R }, Key.R,
            new HashSet<string>());
        Assert.Equal(EmulationShortcutMatchCategory.None, formerDefault.Category);
    }
}
