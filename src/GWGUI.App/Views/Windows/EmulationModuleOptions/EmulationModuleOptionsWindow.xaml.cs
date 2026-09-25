using System.Windows;
using System.ComponentModel;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Logging;
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
    private bool _cleanupStarted;
    private bool _cleanupCompleted;

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
        Closing += WindowClosing;
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

    private async void WindowClosing(object? sender, CancelEventArgs args)
    {
        if (_cleanupCompleted) return;
        args.Cancel = true;
        if (_cleanupStarted) return;
        _cleanupStarted = true;
        try
        {
            await _section.DisposeAsync();
        }
        catch (Exception error)
        {
            ErrorLog.Write(error, "Closing the emulation module options window");
        }
        finally
        {
            EmulationVideoShaderLoadingStatus.Changed -= VideoShaderLoadingChanged;
            _section.ConfigurationSaved -= ConfigurationSaved;
            _section.VideoConfigurationChanged -= VideoConfigurationChanged;
            ContentRendered -= LoadInitialConfiguration;
            ModuleContent.Content = null;
            Closing -= WindowClosing;
            _cleanupCompleted = true;
            _cleanupStarted = false;
            await Dispatcher.BeginInvoke(Close);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
