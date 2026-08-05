using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace GWGUI.App.ViewModels;

public sealed class MainWindowViewModel(string hardwareText, string operationText) : INotifyPropertyChanged
{
    private string _hardwareText = hardwareText;
    private Brush _hardwareBrush = Brushes.Gray;
    private string _profileText = "";
    private Visibility _profileVisibility = Visibility.Collapsed;
    private string _operationText = operationText;
    private Brush _operationBrush = Brushes.Gray;
    private Visibility _progressVisibility = Visibility.Collapsed;
    private bool _progressIndeterminate;
    private double _progressValue;
    private string _progressText = "";
    private Visibility _hostToolsUpdateVisibility = Visibility.Collapsed;
    private string _hostToolsUpdateText = "";

    public ReadOperationViewModel Read { get; } = new();
    public WriteOperationViewModel Write { get; } = new();
    public ConversionOperationViewModel Conversion { get; } = new();

    public string HardwareText { get => _hardwareText; set => Set(ref _hardwareText, value); }
    public Brush HardwareBrush { get => _hardwareBrush; set => Set(ref _hardwareBrush, value); }
    public string ProfileText { get => _profileText; set => Set(ref _profileText, value); }
    public Visibility ProfileVisibility { get => _profileVisibility; set => Set(ref _profileVisibility, value); }
    public string OperationText { get => _operationText; set => Set(ref _operationText, value); }
    public Brush OperationBrush { get => _operationBrush; set => Set(ref _operationBrush, value); }
    public Visibility ProgressVisibility { get => _progressVisibility; set => Set(ref _progressVisibility, value); }
    public bool ProgressIndeterminate { get => _progressIndeterminate; set => Set(ref _progressIndeterminate, value); }
    public double ProgressValue { get => _progressValue; set => Set(ref _progressValue, value); }
    public string ProgressText { get => _progressText; set => Set(ref _progressText, value); }
    public Visibility HostToolsUpdateVisibility { get => _hostToolsUpdateVisibility; set => Set(ref _hostToolsUpdateVisibility, value); }
    public string HostToolsUpdateText { get => _hostToolsUpdateText; set => Set(ref _hostToolsUpdateText, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
