using System.Windows.Input;
using GWGUI.App.Input;
using GWGUI.Emulation;

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
        Assert.Equal(EmulationShortcutMatchKind.Global, inactive.Kind);
        Assert.Equal(EmulationShortcutActions.HardReset, inactive.Action);
        Assert.True(inactive.ShouldExecute);

        var active = EmulationShortcutFunctions.ResolveGlobal(bindings, ModifierKeys.Control, pressed, Key.R,
            new HashSet<string> { EmulationShortcutActions.HardReset });
        Assert.Equal(EmulationShortcutMatchKind.Global, active.Kind);
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
        Assert.Equal(EmulationShortcutMatchKind.ReservedForGlobal, match.Kind);
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
}
