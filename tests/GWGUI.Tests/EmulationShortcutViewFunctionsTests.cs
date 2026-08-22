using GWGUI.App.Contracts.Input;
using GWGUI.App.Functions.Emulation.Shortcuts;
using GWGUI.Emulation;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;

namespace GWGUI.Tests;

public sealed class EmulationShortcutViewFunctionsTests
{
    [Fact]
    public void BarDisplaysConfiguredChordInsteadOfDefault()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                IReadOnlyList<GlobalShortcutBinding> configured =
                [
                    new GlobalShortcutBinding(EmulationShortcutActions.QuickSave,
                        new KeyboardChord(ModifierKeys.Control | ModifierKeys.Shift,
                            [EmulationShortcutViewTestConstants.ConfiguredKey]))
                ];
                var group = EmulationShortcutViewFunctions.CreateGroup(configured,
                    (EmulationShortcutActions.QuickSave, EmulationShortcutViewTestConstants.ResourceKey));
                var panel = Assert.IsType<StackPanel>(group.Child);
                var hint = Assert.IsType<StackPanel>(Assert.Single(panel.Children));
                var key = Assert.IsType<Border>(hint.Children[EmulationShortcutViewTestConstants.KeyBorderIndex]);
                var text = Assert.IsType<TextBlock>(key.Child);

                Assert.Equal(EmulationShortcutViewTestConstants.ConfiguredChord, text.Text);
            }
            catch (Exception error)
            {
                failure = error;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) throw failure;
    }
}

internal static class EmulationShortcutViewTestConstants
{
    internal const string ResourceKey = "Emulation.Shortcut.QuickSave";
    internal const string ConfiguredChord = "Ctrl+Shift+S";
    internal const Key ConfiguredKey = Key.S;
    internal const int KeyBorderIndex = 1;
}
