using System.Windows;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Views.Controls.Emulation.Options;
using GWGUI.Emulation;

namespace GWGUI.App.Views.Windows.EmulationModuleOptions;

public partial class EmulationModuleOptionsWindow : Window
{
    private readonly IEmulationModule _module;
    private readonly EmulationModuleSettingsSection _section;
    private readonly IEmulationConfiguration? _initialConfiguration;
    private bool _initialConfigurationLoaded;

    public EmulationModuleOptionsWindow(IEmulationModule module,
        IEmulationConfiguration? initialConfiguration = null,
        Action<Exception>? showError = null,
        GWGUI.VideoPresentation.Services.VideoPresentationProfileStore? profiles = null)
    {
        InitializeComponent();
        _module = module;
        _initialConfiguration = initialConfiguration;
        _section = new EmulationModuleSettingsSection(module, profiles, showError);
        _section.ConfigurationSaved += ConfigurationSaved;
        _section.VideoConfigurationChanged += VideoConfigurationChanged;
        EmulationVideoShaderLoadingStatus.Changed += VideoShaderLoadingChanged;
        ModuleContent.Content = _section;
        ContentRendered += LoadInitialConfiguration;
        Closed += WindowClosed;
        RefreshLocalizedContent();
    }

    internal void RefreshLocalizedContent()
    {
        Title = LocExtension.Get("Options.EmulationModuleTitle",
            LocExtension.GetForModule(_module, _module.DisplayResourceKey));
        _section.RefreshLocalizedContent();
    }

    private async void LoadInitialConfiguration(object? sender, EventArgs e)
    {
        if (_initialConfigurationLoaded || _initialConfiguration is null) return;
        _initialConfigurationLoaded = true;
        await _section.EditConfigurationAsync(_initialConfiguration);
    }

    private static void ConfigurationSaved(object? sender, EmulationConfigurationSavedEventArgs args) =>
        EmulationPreferencesSection.RaiseConfigurationSaved(sender ?? typeof(EmulationModuleOptionsWindow), args);

    private static void VideoConfigurationChanged(object? sender, EmulationConfigurationSavedEventArgs args) =>
        EmulationPreferencesSection.RaiseVideoConfigurationChanged(sender ?? typeof(EmulationModuleOptionsWindow), args);

    private void VideoShaderLoadingChanged(object? sender, EmulationVideoShaderLoadingChangedEventArgs args) =>
        _section.SetVideoShaderLoading(args.ModuleId, args.ConfigurationId, args.IsLoading);

    private void WindowClosed(object? sender, EventArgs e)
    {
        EmulationVideoShaderLoadingStatus.Changed -= VideoShaderLoadingChanged;
        _section.ConfigurationSaved -= ConfigurationSaved;
        _section.VideoConfigurationChanged -= VideoConfigurationChanged;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
