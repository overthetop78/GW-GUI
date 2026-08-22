using GWGUI.App.Contracts.Input;
using GWGUI.App.Functions.Input.Bindings;
using GWGUI.Emulation;

namespace GWGUI.Tests;

public sealed class AtariShortcutTests
{
    [Fact]
    public void ShortcutParsingIsSharedAcrossEmulationFamilies()
    {
        Assert.True(KeyboardChordFunctions.TryParse("Ctrl+F5", out var shortcut));

        Assert.Equal([System.Windows.Input.Key.F5], shortcut.Keys);
        Assert.True(shortcut.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control));
        Assert.False(shortcut.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt));
        Assert.False(shortcut.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift));
    }

    [Fact]
    public void ShortcutActionsUseTheCommonIdentifiers()
    {
        Assert.Equal("quick-save", EmulationShortcutActions.QuickSave);
        Assert.Equal("quick-load", EmulationShortcutActions.QuickLoad);
        Assert.Equal("soft-reset", EmulationShortcutActions.SoftReset);
        Assert.Equal("hard-reset", EmulationShortcutActions.HardReset);
    }
}
