using GWGUI.App.Views.Controls.Emulation.Options;
using System.Collections;
using System.Reflection;
using System.Windows.Controls;

namespace GWGUI.Tests;

public sealed class OptionsEmulationSectionLazyLoadingTests
{
    [Fact]
    public void BuildsEachModuleOnlyWhenItsTabIsFirstSelected()
    {
        WpfTestHost.Run(() =>
        {
            var section = new OptionsEmulationSection();
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var tabs = Assert.IsType<TabControl>(
                typeof(OptionsEmulationSection).GetField("_tabs", flags)!.GetValue(section));
            var loaded = Assert.IsAssignableFrom<IDictionary>(
                typeof(OptionsEmulationSection).GetField("_moduleSections", flags)!.GetValue(section));

            Assert.Empty(loaded.Keys.Cast<object>());

            tabs.SelectedIndex = 3;
            Assert.Single(loaded.Keys.Cast<object>());

            tabs.SelectedIndex = 0;
            tabs.SelectedIndex = 3;
            Assert.Single(loaded.Keys.Cast<object>());
        });
    }
}
