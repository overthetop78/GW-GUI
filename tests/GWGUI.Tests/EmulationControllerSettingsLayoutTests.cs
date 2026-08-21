using System.Windows.Controls;
using GWGUI.App.Controls;

namespace GWGUI.Tests;

public sealed class EmulationControllerSettingsLayoutTests
{
    [Fact]
    public void ControllerPageCanBeRebuiltWithTheSameEditorsAndBehaviorControls()
    {
        WpfTestHost.Run(() =>
        {
            var section = new EmulationControllerSettingsSection();
            var port = EmulationControllerSettingsSection.CreatePort(1,
                InputCaptureSources.Keyboard | InputCaptureSources.Controller, true,
                "Action", "Search");
            var behavior = new CheckBox { Content = "Parallel adapter" };
            var behaviorField = new EmulationSettingsControlField("Adapter", behavior);

            var firstPage = section.Build([port.Settings], behaviorField);
            var secondPage = section.Build([port.Settings], behaviorField);

            Assert.NotSame(firstPage, secondPage);
            Assert.Same(behavior, behaviorField.Control);
        });
    }
}
