using GWGUI.Tests.Interface.Navigation;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Threading;
using GWGUI.App.Localization.Extensions;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Tests.Interface.ReadViews;
using GWGUI.Tests.Interface.WriteViews;
using GWGUI.Tests.Interface.ConversionViews;
namespace GWGUI.Tests.Interface.Accessibility;
internal static class AccessibleControlsScenarios
{
    public static async Task OperationStates(int kind, bool failure)
    {
        switch (kind)
        {
            case 0:
                using (var context = new ReadOperationScenarios.Context())
                    await CheckStates(context.View, context.View.ExecuteActionButton, "ReadExecuteButton", context.Controller.ExecuteAsync, context.Started.Task, context.Pending, failure);
                break;
            case 1:
                var write = new WriteOperationScenarios.Context();
                await CheckStates(write.View, write.View.ExecuteActionButton, "WriteExecuteButton", write.Controller.ExecuteAsync, write.Started.Task, write.Pending, failure);
                break;
            default:
                var convert = new ConversionOperationScenarios.Context();
                await CheckStates(convert.View, convert.View.ExecuteActionButton, "ConvertExecuteButton", convert.Controller.ExecuteAsync, convert.Started.Task, convert.Pending, failure);
                break;
        }
    }

    private static async Task CheckStates(UserControl section, Button button, string id, Func<Task> execute,
        Task started, TaskCompletionSource<GwExecutionResult> pending, bool failure)
    {
        await Dispatcher.Yield(DispatcherPriority.ContextIdle);
        Validate(section);
        var peer = UIElementAutomationPeer.CreatePeerForElement(button)!;
        Assert.Equal(id, peer.GetAutomationId()); Assert.True(button.Focusable); Assert.True(KeyboardNavigation.GetIsTabStop(button));
        var running = execute(); await Task.WhenAny(started, running); Assert.True(started.IsCompleted);
        Validate(section); Assert.True(button.IsEnabled); Assert.Equal(LocExtension.Get("Common.Stop"), button.Content);
        Assert.Equal(id, peer.GetAutomationId()); Assert.False(string.IsNullOrWhiteSpace(peer.GetName()));
        pending.SetResult(new(failure ? 5 : 0, false, TimeSpan.Zero, [])); await running;
        Validate(section); Assert.True(button.IsEnabled); Assert.Equal(LocExtension.Get("Common.Execute"), button.Content);
        Assert.Equal(id, peer.GetAutomationId()); Assert.True(KeyboardNavigation.GetIsTabStop(button));
    }

    public static void Controls(int tab)
    {
        var shell = NavigationScenarios.Shell;
        Assert.IsType<TabControl>(shell.FindName("MainTabs")).SelectedIndex = tab;
        WindowLayoutScenarios.ArrangeShell(1360, 820);
        var section = Assert.IsAssignableFrom<FrameworkElement>(shell.FindName(NavigationScenarios.SectionNames[tab]));
        Validate(section);
    }

    private static void Validate(FrameworkElement section)
    {
        section.Measure(new Size(1280, 720)); section.Arrange(new Rect(0, 0, 1280, 720));
        var controls = Descendants(section).OfType<Control>().Where(control => control.TemplatedParent is null && control is Button or TextBox or ComboBox or CheckBox or RadioButton or Slider).Where(DeclaredVisible).ToArray();
        Assert.NotEmpty(controls);
        var ids = controls.Select(AutomationProperties.GetAutomationId).Where(id => !string.IsNullOrEmpty(id)).ToArray();
        Assert.Equal(ids.Length, ids.Distinct(StringComparer.Ordinal).Count());
        foreach (var control in controls)
        {
            var peer = UIElementAutomationPeer.CreatePeerForElement(control);
            Assert.NotNull(peer);
            Assert.False(string.IsNullOrWhiteSpace(peer.GetName()), $"Missing accessible name: {section.Name}/{control.GetType().Name}/{control.Name}");
            var expected = control switch { TextBox => AutomationControlType.Edit, ComboBox => AutomationControlType.ComboBox, CheckBox => AutomationControlType.CheckBox, RadioButton => AutomationControlType.RadioButton, Slider => AutomationControlType.Slider, _ => AutomationControlType.Button };
            Assert.Equal(expected, peer.GetAutomationControlType());
        }
    }
    private static bool DeclaredVisible(DependencyObject element)
    {
        for (DependencyObject? current = element; current is not null; current = VisualTreeHelper.GetParent(current))
            if (current is UIElement { Visibility: not Visibility.Visible }) return false;
        return true;
    }
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        yield return root;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
            foreach (var child in Descendants(VisualTreeHelper.GetChild(root, index))) yield return child;
    }
}
