using GWGUI.App.ViewModels.Conversion;
using GWGUI.App.ViewModels.Operations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace GWGUI.App.ViewModels.Main;

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
    private Visibility _globalProgressVisibility = Visibility.Visible;
    private Visibility _face0ProgressVisibility = Visibility.Collapsed;
    private Visibility _face1ProgressVisibility = Visibility.Collapsed;
    private double _face0ProgressValue;
    private double _face1ProgressValue;
    private string _face0ProgressText = "";
    private string _face1ProgressText = "";
    private Visibility _hostToolsUpdateVisibility = Visibility.Collapsed;
    private string _hostToolsUpdateText = "";
    private Visibility _timerVisibility = Visibility.Collapsed;
    private string _elapsedText = "00:00:00";

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
    public Visibility GlobalProgressVisibility { get => _globalProgressVisibility; set => Set(ref _globalProgressVisibility, value); }
    public Visibility Face0ProgressVisibility { get => _face0ProgressVisibility; set => Set(ref _face0ProgressVisibility, value); }
    public Visibility Face1ProgressVisibility { get => _face1ProgressVisibility; set => Set(ref _face1ProgressVisibility, value); }
    public double Face0ProgressValue { get => _face0ProgressValue; set => Set(ref _face0ProgressValue, value); }
    public double Face1ProgressValue { get => _face1ProgressValue; set => Set(ref _face1ProgressValue, value); }
    public string Face0ProgressText { get => _face0ProgressText; set => Set(ref _face0ProgressText, value); }
    public string Face1ProgressText { get => _face1ProgressText; set => Set(ref _face1ProgressText, value); }
    public Visibility HostToolsUpdateVisibility { get => _hostToolsUpdateVisibility; set => Set(ref _hostToolsUpdateVisibility, value); }
    public string HostToolsUpdateText { get => _hostToolsUpdateText; set => Set(ref _hostToolsUpdateText, value); }
    public Visibility TimerVisibility { get => _timerVisibility; set => Set(ref _timerVisibility, value); }
    public string ElapsedText { get => _elapsedText; set => Set(ref _elapsedText, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
