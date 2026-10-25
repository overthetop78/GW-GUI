using System.Windows.Input;

namespace GWGUI.App.Functions.Input.Keyboard;

internal sealed class KeyboardModifierState
{
    private int? _leftControlTimestamp;
    internal bool HasSyntheticLeftControl { get; private set; }

    internal void Press(Key key, int? timestamp, ISet<Key> pressed)
    {
        if (key == Key.LeftCtrl)
        {
            _leftControlTimestamp = timestamp;
            HasSyntheticLeftControl = false;
        }
        if (key == Key.RightAlt && timestamp is { } now && _leftControlTimestamp is { } control
            && pressed.Contains(Key.LeftCtrl) && unchecked((uint)(now - control)) <= 50)
        {
            // Windows generates LeftCtrl with AltGr. Use the timing distinction
            // also used by Microsoft Terminal; a separately held Ctrl remains real.
            HasSyntheticLeftControl = true;
            pressed.Remove(Key.LeftCtrl);
        }
    }

    internal void Release(Key key)
    {
        if (key == Key.LeftCtrl)
        {
            _leftControlTimestamp = null;
            HasSyntheticLeftControl = false;
        }
    }

    internal ModifierKeys Normalize(ModifierKeys modifiers, IReadOnlySet<Key> pressed) => HasSyntheticLeftControl && !pressed.Contains(Key.RightCtrl)
        ? modifiers & ~ModifierKeys.Control : modifiers;

    internal void Reset()
    {
        _leftControlTimestamp = null;
        HasSyntheticLeftControl = false;
    }
}
