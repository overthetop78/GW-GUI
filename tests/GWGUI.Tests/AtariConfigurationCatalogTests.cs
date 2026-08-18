using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GWGUI.App;
using GWGUI.App.Controls;
using GWGUI.Emulation.Atari;

namespace GWGUI.Tests;

[Collection(AtariNativeCoreTestConstants.CollectionName)]
public sealed class AtariConfigurationCatalogTests
{
    [Fact]
    public async Task LoadsCreatesUpdatesAndDeletesAtariConfigurations()
    {
        var root = CreateRoot();
        var store = new AtariConfigurationStore(root, root);
        var controller = new AtariConfigurationCatalogController(store);
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St);
        try
        {
            Assert.Empty(await controller.LoadAsync());
            await controller.SaveAsync(configuration);
            Assert.Equal(configuration.Id, Assert.Single(await controller.LoadAsync()).Id);

            var changed = AtariConfigurationCatalogFunctions.ChangeModel(configuration, AtariMachineModel.Ste);
            await controller.SaveAsync(changed);
            var saved = await controller.LoadAsync();
            Assert.Equal(2, saved.Count);
            Assert.Contains(saved, item => item.Id == configuration.Id && item.Model == AtariMachineModel.St);
            Assert.Contains(saved, item => item.Id == changed.Id && item.Model == AtariMachineModel.Ste);

            controller.Delete(configuration.Id);
            Assert.Equal(changed.Id, Assert.Single(await controller.LoadAsync()).Id);
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public async Task ActiveConfigurationCannotBeModifiedOrDeleted()
    {
        var root = CreateRoot();
        var store = new AtariConfigurationStore(root, root);
        var controller = new AtariConfigurationCatalogController(store);
        var configuration = new AtariMachineConfiguration(AtariMachineModel.Atari2600);
        try
        {
            await controller.SaveAsync(configuration);
            controller.ConfigureActiveCheck(id => id == configuration.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(() => controller.SaveAsync(configuration));
            Assert.Throws<InvalidOperationException>(() => controller.Delete(configuration.Id));
            Assert.Equal(configuration.Id, Assert.Single(await controller.LoadAsync()).Id);
        }
        finally { DeleteRoot(root); }
    }

    [Fact]
    public void ModelCatalogContainsEveryModelExactlyOnce()
    {
        var models = AtariConfigurationCatalogFunctions.Models();

        Assert.Equal(Enum.GetValues<AtariMachineModel>().Length, models.Count);
        Assert.Equal(models.Count, models.Select(item => item.Model).Distinct().Count());
        Assert.All(models, item => Assert.False(string.IsNullOrWhiteSpace(item.DisplayName)));
    }

    [Fact]
    public void SelectingSameModelPreservesDocumentAndChangingModelCreatesMachine()
    {
        var configuration = new AtariMachineConfiguration(AtariMachineModel.St,
            options: AtariConfigurationCatalogTestConstants.Options);

        Assert.Same(configuration,
            AtariConfigurationCatalogFunctions.ChangeModel(configuration, AtariMachineModel.St));
        var changed = AtariConfigurationCatalogFunctions.ChangeModel(configuration, AtariMachineModel.Falcon);
        Assert.NotEqual(configuration.Id, changed.Id);
        Assert.Equal(AtariMachineModel.Falcon, changed.Model);
        Assert.Empty(changed.Options);
    }

    [Fact]
    public void OptionsNavigationContainsOneAtariCatalogWithEveryModel()
    {
        RunOnSta(() =>
        {
            var app = Application.Current as GWGUI.App.App ?? new GWGUI.App.App();
            app.InitializeComponent();
            var options = new OptionsEmulationSection();
            var tabs = Assert.IsType<TabControl>(options.Content);
            var atariTabs = tabs.Items.OfType<TabItem>()
                .Where(item => item.Content is AtariConfigurationCatalogSection).ToArray();

            Assert.Single(atariTabs);
            var catalog = Assert.IsType<AtariConfigurationCatalogSection>(atariTabs[0].Content);
            var models = Descendants(catalog).OfType<ComboBox>()
                .Single(combo => combo.Items.Cast<object>().OfType<AtariModelItem>().Any());
            Assert.Equal(Enum.GetValues<AtariMachineModel>().Length, models.Items.Count);
        });
    }

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), AtariConfigurationCatalogTestConstants.RootPrefix
            + Guid.NewGuid().ToString(AtariConfigurationCatalogTestConstants.IdentifierFormat));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteRoot(string root)
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    private static IEnumerable<object> Descendants(object root)
    {
        yield return root;
        if (root is ContentControl content && content.Content is not null)
            foreach (var child in Descendants(content.Content)) yield return child;
        if (root is Panel panel)
            foreach (var element in panel.Children.Cast<object>())
                foreach (var child in Descendants(element)) yield return child;
        if (root is Decorator decorator && decorator.Child is not null)
            foreach (var child in Descendants(decorator.Child)) yield return child;
        if (root is ItemsControl items)
            foreach (var item in items.Items.Cast<object>())
                foreach (var child in Descendants(item)) yield return child;
    }

    private static void RunOnSta(Action action)
        => WpfTestHost.Run(action);
}

internal static class AtariConfigurationCatalogTestConstants
{
    internal const string RootPrefix = "gwgui-atari-catalog-";
    internal const string IdentifierFormat = "N";
    internal const string OptionKey = "test_option";
    internal const string OptionValue = "enabled";
    internal const int StaTimeoutMilliseconds = 10000;
    internal static readonly IReadOnlyDictionary<string, string> Options =
        new Dictionary<string, string> { [OptionKey] = OptionValue };
}
