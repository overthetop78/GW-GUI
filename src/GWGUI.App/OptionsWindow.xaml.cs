using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GWGUI.Domain.Settings;

namespace GWGUI.App;

public partial class OptionsWindow : Window
{
    private readonly AppSettings _settings;
    public ObservableCollection<HardwareRow> Hardware { get; } = [];

    public OptionsWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        ImagesFolderText.Text = settings.DefaultImagesFolder;
        GwPathText.Text = settings.GwExecutablePath;
        LanguageCombo.SelectedIndex = settings.Language == "en" ? 1 : 0;
        ThemeCombo.SelectedIndex = (int)settings.Theme;
        foreach (var controller in settings.Controllers)
        {
            var drives = settings.Drives.Where(x => x.ControllerUsbId == controller.UsbId).ToArray();
            if (drives.Length == 0) Hardware.Add(new(controller.LastPort, controller.UsbId, "Aucun lecteur défini", controller.IsAvailable));
            foreach (var drive in drives) Hardware.Add(new(controller.LastPort, controller.UsbId, $"{drive.Size} pouces · {drive.Density} · {drive.Selection}", controller.IsAvailable));
        }
        DrivesGrid.ItemsSource = Hardware;
    }

    private void Navigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GeneralPage is null) return;
        var pages = new FrameworkElement[] { GeneralPage, ToolsPage, HardwarePage, ProfilesPage };
        for (var index = 0; index < pages.Length; index++) pages[index].Visibility = index == Navigation.SelectedIndex ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BrowseGw_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Greaseweazle (gw.exe)|gw.exe|Exécutable (*.exe)|*.exe" };
        if (dialog.ShowDialog(this) == true) GwPathText.Text = dialog.FileName;
    }

    private void BrowseImagesFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Multiselect = false, Title = "Dossier d’images par défaut" };
        if (dialog.ShowDialog(this) == true) ImagesFolderText.Text = dialog.FolderName;
    }

    private void ScanHardware_Click(object sender, RoutedEventArgs e) => MessageBox.Show(this, "Le scan utilisera gw info et associera l’identifiant USB stable au dernier port COM.", "Scanner", MessageBoxButton.OK, MessageBoxImage.Information);
    private void AddDrive_Click(object sender, RoutedEventArgs e) => MessageBox.Show(this, "Le lecteur sera défini par contrôleur, sélection A/B ou 0/1, taille, densité et vitesse éventuelle.", "Ajouter un lecteur", MessageBoxButton.OK, MessageBoxImage.Information);

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _settings.DefaultImagesFolder = ImagesFolderText.Text.Trim();
        _settings.GwExecutablePath = string.IsNullOrWhiteSpace(GwPathText.Text) ? null : GwPathText.Text.Trim();
        _settings.Language = LanguageCombo.SelectedIndex == 1 ? "en" : "fr";
        _settings.Theme = (AppTheme)Math.Max(0, ThemeCombo.SelectedIndex);
        DialogResult = true;
    }
}

public sealed record HardwareRow(string Port, string UsbId, string Description, bool Available);
