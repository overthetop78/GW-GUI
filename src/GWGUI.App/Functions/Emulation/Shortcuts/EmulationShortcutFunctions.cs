using GWGUI.App.Constants.Emulation.Shortcuts;
using GWGUI.App.Contracts.Emulation.Shortcuts;
using GWGUI.App.Contracts.Input;
using GWGUI.App.Enums.Emulation.Shortcuts;
using GWGUI.App.Functions.Input.Bindings;
using System.Windows.Input;

namespace GWGUI.App.Functions.Emulation.Shortcuts;

internal static class EmulationShortcutFunctions
{
    internal static EmulationShortcutMatch ResolveGlobal(IReadOnlyList<GlobalShortcutBinding> bindings,
        ModifierKeys modifiers, IReadOnlySet<Key> pressedKeys, Key source, IReadOnlySet<string> activeActions)
    {
        var exact = bindings.FirstOrDefault(binding => KeyboardChordFunctions.Matches(binding.Chord, modifiers, pressedKeys));
        if (exact is not null)
            return new EmulationShortcutMatch(EmulationShortcutMatchCategory.Global, exact.Action,
                !activeActions.Contains(exact.Action));
        return bindings.Any(binding => binding.Chord.Modifiers == modifiers && KeyboardChordFunctions.Contains(binding.Chord, source))
            ? new EmulationShortcutMatch(EmulationShortcutMatchCategory.ReservedForGlobal)
            : new EmulationShortcutMatch(EmulationShortcutMatchCategory.None);
    }

    internal static void ReleaseInactive(ISet<string> activeActions,
        IReadOnlyList<GlobalShortcutBinding> bindings, ModifierKeys modifiers, IReadOnlySet<Key> pressedKeys)
    {
        activeActions.RemoveWhere(action =>
        {
            var binding = bindings.FirstOrDefault(item => item.Action == action);
            return binding is null || !KeyboardChordFunctions.Matches(binding.Chord, modifiers, pressedKeys);
        });
    }

    internal static bool HasConflict(IEnumerable<GlobalShortcutBinding> bindings) => bindings
        .GroupBy(binding => KeyboardChordFunctions.Format(binding.Chord.Modifiers, binding.Chord.Keys),
            StringComparer.OrdinalIgnoreCase)
        .Any(group => group.Skip(EmulationShortcutConstants.UniqueChordBindingCount).Any());

    private static void RemoveWhere(this ISet<string> values, Func<string, bool> predicate)
    {
        foreach (var value in values.Where(predicate).ToArray()) values.Remove(value);
    }
}
