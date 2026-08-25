using GWGUI.App.Contracts.Services.Dialogs;
using GWGUI.App.Contracts.Services.Navigation;
using GWGUI.App.Enums.Services.Dialogs;
using GWGUI.App.Enums.Services.Navigation;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Interfaces.Services.Navigation;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Conversion;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Hardware;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Exploration.Scp;
using System.IO;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Hardware;
using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace GWGUI.Tests;

public abstract class CoreTestBase
{
    protected static string WindowsPowerShell => @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";

    protected sealed class ScriptedRunner(params GwExecutionResult[] results) : IGreaseweazleRunner
    {
        private readonly Queue<GwExecutionResult> _results = new(results);
        public List<GwCommand> Commands { get; } = [];
        public bool IsRunning { get; private set; }
        public Task<GwExecutionResult> RunAsync(GwCommand command, IProgress<GwOutputLine>? output = null, CancellationToken cancellationToken = default)
        {
            Commands.Add(command); IsRunning = true;
            try { return Task.FromResult(_results.Dequeue()); }
            finally { IsRunning = false; }
        }
    }

    protected sealed class BusyRunner : IGreaseweazleRunner
    {
        public bool IsRunning => true;
        public Task<GwExecutionResult> RunAsync(GwCommand command, IProgress<GwOutputLine>? output = null, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The shared runner is busy.");
    }

    protected sealed class StubScpReader(ScpImage image) : IScpReader
    {
        public string? Path { get; private set; }
        public Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default) { Path = path; return Task.FromResult(image); }
    }

    protected sealed class StaticSerialDeviceDiscovery(IReadOnlyList<SerialDevice> devices) : ISerialDeviceDiscovery
    {
        public IReadOnlyList<SerialDevice> FindSerialDevices() => devices;
    }

    protected sealed class StaticHardwareRegistry(IReadOnlyList<ControllerSettings> controllers, IReadOnlyList<ControllerSettings>? unconfigured = null) : IHardwareRegistry
    {
        public Task<HardwareScanResult> ScanAsync(string executable, IReadOnlyList<ControllerSettings> configuredControllers, CancellationToken cancellationToken = default) =>
            Task.FromResult(new HardwareScanResult(controllers, unconfigured ?? []));
    }

    protected sealed class MutableSerialDeviceDiscovery(IReadOnlyList<SerialDevice> devices) : ISerialDeviceDiscovery
    {
        public IReadOnlyList<SerialDevice> Devices { get; set; } = devices;
        public IReadOnlyList<SerialDevice> FindSerialDevices() => Devices;
    }

    protected sealed class DeviceInfoRunner(IReadOnlyDictionary<string, (string Serial, string Model)> devices) : IGreaseweazleRunner
    {
        public bool IsRunning { get; private set; }

        public Task<GwExecutionResult> RunAsync(GwCommand command, IProgress<GwOutputLine>? output = null, CancellationToken cancellationToken = default)
        {
            IsRunning = true;
            try
            {
                var deviceIndex = command.Arguments.ToList().IndexOf("--device");
                var port = deviceIndex >= 0 && deviceIndex + 1 < command.Arguments.Count ? command.Arguments[deviceIndex + 1] : "";
                if (!devices.TryGetValue(port, out var device)) return Task.FromResult(new GwExecutionResult(1, false, TimeSpan.Zero, []));
                GwOutputLine[] lines =
                [
                    new(DateTimeOffset.UtcNow, GwOutputStream.Standard, $"Model: {device.Model}"),
                    new(DateTimeOffset.UtcNow, GwOutputStream.Standard, $"Serial: {device.Serial}")
                ];
                return Task.FromResult(new GwExecutionResult(0, false, TimeSpan.Zero, lines));
            }
            finally { IsRunning = false; }
        }
    }

    protected sealed class RecordingMessageDialogService(UserDialogResult result = UserDialogResult.Ok) : IMessageDialogService
    {
        public List<(string Message, string Title, UserDialogButtons Buttons, UserDialogIcon Icon)> Requests { get; } = [];
        public UserDialogResult Show(string message, string title, UserDialogButtons buttons = UserDialogButtons.Ok, UserDialogIcon icon = UserDialogIcon.None)
        {
            Requests.Add((message, title, buttons, icon));
            return result;
        }
    }

    protected sealed class RecordingFileDialogService : IFileDialogService
    {
        public string? OpenResult { get; set; }
        public string? SaveResult { get; set; }
        public string? FolderResult { get; set; }
        public List<OpenFileRequest> OpenRequests { get; } = [];
        public List<SaveFileRequest> SaveRequests { get; } = [];
        public List<SelectFolderRequest> FolderRequests { get; } = [];
        public string? OpenFile(OpenFileRequest request) { OpenRequests.Add(request); return OpenResult; }
        public string? SaveFile(SaveFileRequest request) { SaveRequests.Add(request); return SaveResult; }
        public string? SelectFolder(SelectFolderRequest request) { FolderRequests.Add(request); return FolderResult; }
    }

    protected sealed class RecordingSettingsStore : ISettingsStore
    {
        public int SaveCount { get; private set; }
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(new AppSettings());
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) { SaveCount++; return Task.CompletedTask; }
    }

    protected sealed class DelayedSettingsStore : ISettingsStore
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(new AppSettings());
        public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) => await Task.Delay(20, cancellationToken);
    }

    protected sealed class FailingSettingsStore : ISettingsStore
    {
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(new AppSettings());
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default) => Task.FromException(new IOException("test save failure"));
    }

    protected sealed class RecordingBusinessDialogService : IBusinessDialogService
    {
        public string? ProfileNameResult { get; set; }
        public int ProfilePromptCount { get; private set; }
        public IReadOnlyList<ConversionConflictDecision>? ConflictResult { get; set; }
        public ReadConflictChoice? ReadConflictResult { get; set; }
        public MissingHardwareChoice MissingHardwareResult { get; set; } = MissingHardwareChoice.Continue;
        public List<IReadOnlyList<ControllerSettings>> MissingHardwareRequests { get; } = [];
        public string? PromptProfileName(string? initialName = null) { ProfilePromptCount++; return ProfileNameResult; }
        public ReadConflictChoice? ResolveReadConflict(string outputPath) => ReadConflictResult;
        public IReadOnlyList<ConversionConflictDecision>? ResolveConversionConflicts(IReadOnlyList<ConversionOutput> outputs) => ConflictResult;
        public MissingHardwareChoice ResolveMissingHardware(IReadOnlyList<ControllerSettings> controllers)
        {
            MissingHardwareRequests.Add(controllers);
            return MissingHardwareResult;
        }
    }

    protected sealed class RecordingWindowNavigationService : IWindowNavigationService
    {
        public bool OptionsResult { get; set; }
        public int AboutCount { get; private set; }
        public List<AppSettings> OptionsSettings { get; } = [];
        public List<string> LogDirectories { get; } = [];
        public List<GwToolWindowRequest> ToolRequests { get; } = [];
        public List<OptionsSection> OptionsSections { get; } = [];
        public bool ShowOptions(AppSettings settings, OptionsSection section = OptionsSection.General) { OptionsSettings.Add(settings); OptionsSections.Add(section); return OptionsResult; }
        public void ShowLogHistory(string logsDirectory) => LogDirectories.Add(logsDirectory);
        public void ShowAbout() => AboutCount++;
        public void ShowGwTool(GwToolWindowRequest request) => ToolRequests.Add(request);
    }

    protected sealed class ImmediateProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    protected static System.Windows.Controls.ScrollViewer GetScrollViewer(System.Windows.DependencyObject parent)
    {
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, index);
            if (child is System.Windows.Controls.ScrollViewer scrollViewer) return scrollViewer;
            var nested = GetScrollViewerOrDefault(child);
            if (nested is not null) return nested;
        }

        throw new InvalidOperationException("No ScrollViewer found in the visual tree.");
    }

    protected static System.Windows.Controls.ScrollViewer? GetScrollViewerOrDefault(System.Windows.DependencyObject parent)
    {
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, index);
            if (child is System.Windows.Controls.ScrollViewer scrollViewer) return scrollViewer;
            var nested = GetScrollViewerOrDefault(child);
            if (nested is not null) return nested;
        }

        return null;
    }

    protected static string EncodeMfmBytes(params byte[] values) { var result = new System.Text.StringBuilder(); var previous = 1; foreach (var value in values) for (var bit = 7; bit >= 0; bit--) { var data = (value >> bit) & 1; var clock = previous == 0 && data == 0 ? 1 : 0; result.Append(clock).Append(data); previous = data; } return result.ToString(); }
    protected static byte[] BuildSingleTrackScp(IReadOnlyList<uint> intervals)
    {
        if (intervals.Any(value => value is 0 or > ushort.MaxValue)) throw new ArgumentOutOfRangeException(nameof(intervals));
        var data = new byte[0x2c0 + intervals.Count * 2];
        data[0] = (byte)'S'; data[1] = (byte)'C'; data[2] = (byte)'P'; data[3] = 0x25; data[5] = 1; data[6] = 0; data[7] = 0; data[8] = (byte)ScpFlags.IndexAligned;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x10, 4), 0x2b0);
        data[0x2b0] = (byte)'T'; data[0x2b1] = (byte)'R'; data[0x2b2] = (byte)'K';
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2b4, 4), intervals.Aggregate(0u, (sum, value) => checked(sum + value)));
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2b8, 4), (uint)intervals.Count);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0x2bc, 4), 16);
        for (var index = 0; index < intervals.Count; index++) System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(0x2c0 + index * 2, 2), (ushort)intervals[index]);
        var checksum = ScpFormatAlgorithms.ComputeChecksum(data.AsSpan(ScpFormatConstants.TrackTableOffset));
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(ScpFormatConstants.ChecksumOffset, ScpFormatConstants.ChecksumLength), checksum);
        return data;
    }
    protected static string EncodeMfmBytesFromZero(params byte[] values) { var result = new System.Text.StringBuilder(); var previous = 0; foreach (var value in values) for (var bit = 7; bit >= 0; bit--) { var data = (value >> bit) & 1; var clock = previous == 0 && data == 0 ? 1 : 0; result.Append(clock).Append(data); previous = data; } return result.ToString(); }
    protected static string EncodeFmBytes(params byte[] values) => string.Concat(values.SelectMany(value => Enumerable.Range(0, 8).Select(bit => "1" + (((value >> (7 - bit)) & 1) != 0 ? "1" : "0"))));
    protected static List<uint> BitsToIntervals(string bits, uint cellTicks) { var result = new List<uint>(); var cells = 0; foreach (var bit in bits) { cells++; if (bit == '1') { result.Add((uint)cells * cellTicks); cells = 0; } } return result; }
    protected static ushort TestCrc16(IEnumerable<byte> values) => TestCrc16(values, 0x1021, 0xffff);
    protected static ushort TestCrc16(IEnumerable<byte> values, ushort polynomial, ushort initial) { var crc = initial; foreach (var value in values) { crc ^= (ushort)(value << 8); for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ polynomial : crc << 1); } return crc; }
}
