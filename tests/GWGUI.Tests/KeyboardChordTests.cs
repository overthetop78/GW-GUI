using System.Windows.Input;
using GWGUI.App.Input;

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
        Assert.True(KeyboardChord.TryParse(text, out var chord));
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
        Assert.True(KeyboardChord.TryParse(text, out var chord));
        Assert.True(KeyboardChord.IsWindowsReserved(chord));
    }

    [Fact]
    public void MatchesOnlyTheCompleteChord()
    {
        Assert.True(KeyboardChord.TryParse("F5+F6", out var chord));
        Assert.False(chord.Matches(ModifierKeys.None, new HashSet<Key> { Key.F5 }));
        Assert.True(chord.Matches(ModifierKeys.None, new HashSet<Key> { Key.F5, Key.F6 }));
    }
}
