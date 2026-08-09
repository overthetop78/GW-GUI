using System.Windows;
using System.Windows.Media;
using GWGUI.App.Controls;
using GWGUI.App.ViewModels;
using GWGUI.Domain.Commands;

namespace GWGUI.App.Services;

public sealed class OperationProgressController(
    MainWindowViewModel viewModel,
    TrackProgressStrip face0,
    TrackProgressStrip face1,
    Func<string, object[], string> localize)
{
    private readonly GwProgressTracker _tracker = new();
    private bool _needsConfiguration;

    public void Begin()
    {
        _tracker.Reset();
        face0.ResetToPending();
        face1.ResetToPending();
        _needsConfiguration = true;
        SetState("Status.Running", Color.FromRgb(45, 125, 210));
        viewModel.ProgressVisibility = Visibility.Visible;
        viewModel.ProgressIndeterminate = true;
        viewModel.ProgressValue = 0;
        viewModel.ProgressText = "";
    }

    public void Accept(string output)
    {
        var progress = _tracker.Accept(output);
        if (progress is null) return;

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

    private static TrackSegmentState ToSegmentState(GwTrackState state) => state switch
    {
        GwTrackState.Retry => TrackSegmentState.Retry,
        GwTrackState.Failed => TrackSegmentState.Failed,
        _ => TrackSegmentState.Success
    };
}
