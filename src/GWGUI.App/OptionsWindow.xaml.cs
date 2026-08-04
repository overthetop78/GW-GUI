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
    public ObservableCollection<ProfileOptionRow> Profiles { get; } = [];

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
        foreach (var operation in new[] { "Read", "Write", "Convert" }) Profiles.Add(new($"default-{operation.ToLowerInvariant()}", operation, "Par défaut", true));
        foreach (var profile in settings.Profiles) Profiles.Add(new(profile.Id, profile.Operation, profile.Name, false));
        ProfilesGrid.ItemsSource = Profiles;
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

    private void RenameProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesGrid.SelectedItem is not ProfileOptionRow row) return;
        if (row.IsSystem) { MessageBox.Show(this, "Le profil Par défaut est permanent et ne peut pas être renommé.", "Profil", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        var dialog = new ProfileNameWindow(row.Name) { Owner = this };
        if (dialog.ShowDialog() != true) return;
        if (Profiles.Any(x => x.Operation == row.Operation && x.Id != row.Id && string.Equals(x.Name, dialog.ProfileName, StringComparison.CurrentCultureIgnoreCase))) { MessageBox.Show(this, "Un profil de cet onglet porte déjà ce nom.", "Profil", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        var index = Profiles.IndexOf(row); Profiles[index] = row with { Name = dialog.ProfileName };
    }

    private void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesGrid.SelectedItem is not ProfileOptionRow row) return;
        if (row.IsSystem) { MessageBox.Show(this, "Le profil Par défaut est permanent et ne peut pas être supprimé.", "Profil", MessageBoxButton.OK, MessageBoxImage.Information); return; }
        if (MessageBox.Show(this, $"Supprimer le profil « {row.Name} » ?", "Profil", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) Profiles.Remove(row);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        _settings.DefaultImagesFolder = ImagesFolderText.Text.Trim();
        _settings.GwExecutablePath = string.IsNullOrWhiteSpace(GwPathText.Text) ? null : GwPathText.Text.Trim();
        _settings.Language = LanguageCombo.SelectedIndex == 1 ? "en" : "fr";
        _settings.Theme = (AppTheme)Math.Max(0, ThemeCombo.SelectedIndex);
        var retained = Profiles.Where(x => !x.IsSystem).ToDictionary(x => x.Id);
        _settings.Profiles = _settings.Profiles.Where(x => retained.ContainsKey(x.Id)).Select(x => { x.Name = retained[x.Id].Name; return x; }).ToList();
        DialogResult = true;
    }
}

public sealed record HardwareRow(string Port, string UsbId, string Description, bool Available);
public sealed record ProfileOptionRow(string Id, string Operation, string Name, bool IsSystem)
{
    public string OperationLabel => Operation switch { "Read" => "Lecture", "Write" => "Écriture", "Convert" => "Conversion", _ => Operation };
}
