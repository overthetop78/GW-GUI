using GWGUI.App.Contracts.Input;
using GWGUI.App.Functions.Input.Bindings;
using System.Windows.Input;

namespace GWGUI.Tests;

public sealed class KeyboardChordTests
{
    [Theory]
    [InlineData("F12", ModifierKeys.None, 1)]
    [InlineData("Ctrl+Alt+F12", ModifierKeys.Control | ModifierKeys.Alt, 1)]
    [InlineData("F5+F6", ModifierKeys.None, 2)]
    [InlineData("Win+Shift+X", ModifierKeys.Windows | ModifierKeys.Shift, 1)]
    public void ParsesModifierAndMultiKeyChords(string text, ModifierKeys modifiers, int keyCount)
    {
        Assert.True(KeyboardChordFunctions.TryParse(text, out var chord));
        Assert.Equal(modifiers, chord.Modifiers);
        Assert.Equal(keyCount, chord.Keys.Count);
    }

    [Theory]
    [InlineData("Alt+F4")]
    [InlineData("Alt+Tab")]
    [InlineData("Ctrl+Escape")]
    [InlineData("Ctrl+Shift+Escape")]
    [InlineData("Ctrl+Alt+Delete")]
    [InlineData("Win+Shift+X")]
    public void RejectsWindowsReservedChords(string text)
    {
        Assert.True(KeyboardChordFunctions.TryParse(text, out var chord));
        Assert.True(KeyboardChordFunctions.IsWindowsReserved(chord));
    }

    [Fact]
    public void MatchesOnlyTheCompleteChord()
    {
        Assert.True(KeyboardChordFunctions.TryParse("F5+F6", out var chord));
        Assert.False(KeyboardChordFunctions.Matches(chord, ModifierKeys.None, new HashSet<Key> { Key.F5 }));
        Assert.True(KeyboardChordFunctions.Matches(chord, ModifierKeys.None, new HashSet<Key> { Key.F5, Key.F6 }));
    }
}
