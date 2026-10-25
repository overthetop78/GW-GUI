using System.Windows.Input;
using GWGUI.App.Contracts.Input;

namespace GWGUI.App.Dictionaries.Input.Bindings;

internal static class ReservedKeyboardChordCatalog
{
    // A required modifier set also protects variants with extra modifiers.
    internal static IReadOnlyList<KeyboardChord> WindowsExceptions { get; } =
    [
        new(ModifierKeys.Windows, []),
        new(ModifierKeys.None, [Key.LWin]),
        new(ModifierKeys.None, [Key.RWin]),
        new(ModifierKeys.None, [Key.Sleep]),
        new(ModifierKeys.Alt, [Key.Tab]),
        new(ModifierKeys.Alt, [Key.Escape]),
        new(ModifierKeys.Alt, [Key.F4]),
        new(ModifierKeys.Control, [Key.Escape]),
        new(ModifierKeys.Control | ModifierKeys.Shift, [Key.Escape]),
        new(ModifierKeys.Control | ModifierKeys.Alt, [Key.Delete])
    ];

    internal static bool Contains(KeyboardChord chord) => WindowsExceptions.Any(reserved => (chord.Modifiers & reserved.Modifiers) == reserved.Modifiers && reserved.Keys.All(chord.Keys.Contains));
}
