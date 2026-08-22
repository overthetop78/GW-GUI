using GWGUI.Domain.Commands.Progress;
using GWGUI.App.Contracts.Progress;
using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using GWGUI.App.Contracts.Services.PhysicalDiskWriting;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Visualization;
using System.Windows;
using System.Windows.Media;

using GWGUI.MediaEngine.Exploration.Contracts;


namespace GWGUI.App.Services.Operations;

public sealed class OperationProgressController(
    MainWindowViewModel viewModel,
    TrackProgressStrip face0,
    TrackProgressStrip face1,
    Func<string, object[], string> localize)
{
    private readonly GwProgressTracker _tracker = new();
    private readonly Dictionary<(int Cylinder, int Head), ExternalTrackReadState> _externalTrackStates = [];
    private bool _needsConfiguration;
    private int _completedPhysicalTracks;

    public IEtatLectureDisquette? CurrentReadState { get; private set; }

    public event EventHandler<IEtatLectureDisquette>? ReadStateChanged;

    public void Begin()
    {
        _tracker.Reset();
        _externalTrackStates.Clear();
        face0.ResetToPending();
        face1.ResetToPending();
        _needsConfiguration = true;
        _completedPhysicalTracks = 0;
        CurrentReadState = null;
        SetState("Status.Running", Color.FromRgb(45, 125, 210));
        viewModel.ProgressVisibility = Visibility.Visible;
        viewModel.ProgressIndeterminate = true;
        viewModel.ProgressValue = 0;
        viewModel.ProgressText = "";
    }

    public void Accept(string output)
    {
        var progress = _tracker.Accept(output);
        if (progress is null)
        {
            var current = CurrentReadState;
            Publish(new ExternalReadState(
                current?.Etape ?? "Acquiring",
                current?.NombrePistesTerminees ?? _completedPhysicalTracks,
                current?.NombrePistesTotal ?? 0,
                current?.Cylindre,
                current?.Face,
                current?.Tentative ?? 1,
                current?.EtatsPistes ?? [],
                output));
            return;
        }

        _completedPhysicalTracks = Math.Max(_completedPhysicalTracks, progress.CompletedTracks);
        Publish(CreateExternalReadState(progress, output));

        if (progress.TotalOnHead is int totalOnHead)
        {
            viewModel.GlobalProgressVisibility = Visibility.Collapsed;
            viewModel.Face0ProgressVisibility = progress.Head0Expected ? Visibility.Visible : Visibility.Collapsed;
            viewModel.Face1ProgressVisibility = progress.Head1Expected ? Visibility.Visible : Visibility.Collapsed;
            ConfigureFaces(progress);

            var text = localize("Status.FaceTrackProgress", [progress.Head, progress.Cylinder, progress.CompletedOnHead, totalOnHead]);
            var strip = progress.Head == 0 ? face0 : face1;
            strip.SetState(progress.Cylinder, ToSegmentState(progress.State));
            if (progress.State == GwTrackState.Retry)
            {
                face0.ClearActive();
                face1.ClearActive();
            }
            else if (progress.NextCylinder is int nextCylinder && progress.NextHead is int nextHead)
                (nextHead == 0 ? face0 : face1).SetActive(nextCylinder);

            if (progress.Head == 0)
            {
                viewModel.Face0ProgressValue = progress.HeadFraction.GetValueOrDefault() * 100;
                viewModel.Face0ProgressText = text;
            }
            else
            {
                viewModel.Face1ProgressValue = progress.HeadFraction.GetValueOrDefault() * 100;
                viewModel.Face1ProgressText = text;
            }
            return;
        }

        if (progress.TotalTracks is int total)
        {
            viewModel.ProgressIndeterminate = false;
            viewModel.ProgressValue = progress.Fraction.GetValueOrDefault() * 100;
            viewModel.ProgressText = localize("Status.TrackProgress", [progress.Cylinder, progress.Head, progress.CompletedTracks, total]);
        }
        else
            viewModel.ProgressText = localize("Status.TrackUnknown", [progress.Cylinder, progress.Head, progress.CompletedTracks]);
    }

    public void Accept(PhysicalTrackWriteProgress progress)
    {
        viewModel.ProgressIndeterminate = false;
        viewModel.ProgressValue = progress.TotalTracks == 0
            ? 0
            : progress.CompletedTracks * 100d / progress.TotalTracks;
        viewModel.ProgressText = localize("Status.TrackProgress",
            [progress.Cylinder, progress.Head, progress.CompletedTracks, progress.TotalTracks]);
    }

    public void Accept(PhysicalDiskReadOperationProgress progress)
    {
        Publish(progress);
        if (progress.Tracks is { Count: > 0 })
        {
            ConfigureFaces(progress.Tracks);
        }

        if (progress.Cylinder is int cylinder
            && progress.Head is int head
            && progress.Tracks is { Count: > 0 })
        {
            var strip = head == 0 ? face0 : face1;
            face0.ClearActive();
            face1.ClearActive();
            if (progress.CompletedTracks > _completedPhysicalTracks)
            {
                strip.SetState(cylinder, TrackSegmentState.Success);
            }
            else if (progress.Attempt > 1)
            {
                strip.SetState(cylinder, TrackSegmentState.Retry);
            }
            else
            {
                strip.SetActive(cylinder);
            }

            _completedPhysicalTracks = Math.Max(_completedPhysicalTracks, progress.CompletedTracks);
            UpdateFaceProgress(head, cylinder);
        }

        viewModel.ProgressIndeterminate = false;
        viewModel.ProgressValue = progress.TotalTracks == 0
            ? 0
            : progress.CompletedTracks * 100d / progress.TotalTracks;
        if (progress.Cylinder is int progressCylinder && progress.Head is int progressHead)
        {
            viewModel.ProgressText = localize(
                "Status.TrackProgress",
                [progressCylinder, progressHead, progress.CompletedTracks, progress.TotalTracks]);
        }
        else
        {
            viewModel.ProgressText = localize("Status.Running", []);
        }
    }

    public void End()
    {
        viewModel.ProgressIndeterminate = false;
        viewModel.ProgressValue = 100;
        viewModel.ProgressVisibility = Visibility.Collapsed;
        viewModel.GlobalProgressVisibility = Visibility.Visible;
        viewModel.Face0ProgressVisibility = Visibility.Collapsed;
        viewModel.Face1ProgressVisibility = Visibility.Collapsed;
    }

    public void SetState(string resourceKey, Color color)
    {
        viewModel.OperationText = localize(resourceKey, []);
        viewModel.OperationBrush = new SolidColorBrush(color);
    }

    private void ConfigureFaces(GwTrackProgress progress)
    {
        if ((_needsConfiguration || face0.Segments.Count == 0) && progress.Head0Expected)
            face0.Configure(0, progress.Cylinders, localize("Visual.Side", [0]));
        if ((_needsConfiguration || face1.Segments.Count == 0) && progress.Head1Expected)
            face1.Configure(1, progress.Cylinders, localize("Visual.Side", [1]));
        _needsConfiguration = false;
    }

    private void ConfigureFaces(IReadOnlyList<PhysicalDiskTrackAddress> tracks)
    {
        if (!_needsConfiguration)
        {
            return;
        }

        var face0Cylinders = tracks
            .Where(track => track.Head == 0)
            .Select(track => track.Cylinder)
            .Distinct()
            .Order()
            .ToArray();
        var face1Cylinders = tracks
            .Where(track => track.Head == 1)
            .Select(track => track.Cylinder)
            .Distinct()
            .Order()
            .ToArray();
        viewModel.GlobalProgressVisibility = Visibility.Collapsed;
        viewModel.Face0ProgressVisibility = face0Cylinders.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        viewModel.Face1ProgressVisibility = face1Cylinders.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        if (face0Cylinders.Length > 0)
        {
            face0.Configure(0, face0Cylinders, localize("Visual.Side", [0]));
        }
        if (face1Cylinders.Length > 0)
        {
            face1.Configure(1, face1Cylinders, localize("Visual.Side", [1]));
        }

        _needsConfiguration = false;
    }

    private void UpdateFaceProgress(int head, int cylinder)
    {
        var strip = head == 0 ? face0 : face1;
        var completed = strip.Segments.Count(segment => segment.State == TrackSegmentState.Success);
        var total = strip.Segments.Count;
        var value = total == 0 ? 0 : completed * 100d / total;
        var text = localize("Status.FaceTrackProgress", [head, cylinder, completed, total]);
        if (head == 0)
        {
            viewModel.Face0ProgressValue = value;
            viewModel.Face0ProgressText = text;
            return;
        }

        viewModel.Face1ProgressValue = value;
        viewModel.Face1ProgressText = text;
    }

    private static TrackSegmentState ToSegmentState(GwTrackState state) => state switch
    {
        GwTrackState.Retry => TrackSegmentState.Retry,
        GwTrackState.Failed => TrackSegmentState.Failed,
        _ => TrackSegmentState.Success
    };

    private ExternalReadState CreateExternalReadState(GwTrackProgress progress, string output)
    {
        EnsureExternalTrackStates(progress);
        var key = (progress.Cylinder, progress.Head);
        var attempts = 1;
        if (_externalTrackStates.TryGetValue(key, out var previous))
        {
            attempts = previous.Tentatives;
            if (progress.State == GwTrackState.Retry)
            {
                attempts++;
            }

            attempts = Math.Max(attempts, 1);
        }
        _externalTrackStates[key] = new ExternalTrackReadState(
            progress.Cylinder,
            progress.Head,
            TrackState(progress.State),
            attempts);
        var states = _externalTrackStates.Values
            .OrderBy(state => state.Cylindre)
            .ThenBy(state => state.Face)
            .Cast<IEtatPisteLecture>()
            .ToArray();

        return new ExternalReadState(
            "Acquiring",
            progress.CompletedTracks,
            progress.TotalTracks ?? 0,
            progress.Cylinder,
            progress.Head,
            progress.State == GwTrackState.Retry ? 2 : 1,
            states,
            output);
    }

    private void EnsureExternalTrackStates(GwTrackProgress progress)
    {
        foreach (var cylinder in progress.Cylinders)
        {
            if (progress.Head0Expected)
            {
                _externalTrackStates.TryAdd(
                    (cylinder, 0),
                    new ExternalTrackReadState(cylinder, 0, "pending", 0));
            }

            if (progress.Head1Expected)
            {
                _externalTrackStates.TryAdd(
                    (cylinder, 1),
                    new ExternalTrackReadState(cylinder, 1, "pending", 0));
            }
        }
    }

    private static string TrackState(GwTrackState state)
    {
        return state switch
        {
            GwTrackState.Retry => "retry",
            GwTrackState.Failed => "failed",
            _ => "completed"
        };
    }

    private void Publish(IEtatLectureDisquette state)
    {
        CurrentReadState = state;
        ReadStateChanged?.Invoke(this, state);
    }

    private sealed record ExternalReadState(
        string Etape,
        int NombrePistesTerminees,
        int NombrePistesTotal,
        int? Cylindre,
        int? Face,
        int Tentative,
        IReadOnlyList<IEtatPisteLecture> EtatsPistes,
        string? MessageExterne) : IEtatLectureDisquette
    {
        public IPiste? PisteAcquise => null;

        public string? CodeMessage => null;

        public IReadOnlyDictionary<string, string> ParametresMessage { get; } =
            new Dictionary<string, string>();
    }

    private sealed record ExternalTrackReadState(
        int Cylindre,
        int Face,
        string Etat,
        int Tentatives) : IEtatPisteLecture;
}
