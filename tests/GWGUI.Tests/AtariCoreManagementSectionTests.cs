using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using GWGUI.App.Controls;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Tests;

[Collection(AtariNativeCoreTestConstants.CollectionName)]
public sealed class AtariCoreManagementSectionTests
{
    private const int UiThreadTimeoutSeconds = 15;
    private const string TestReleaseId = "official-test";
    private const string TestReleaseVersion = "test-version";
    private const string AlternateReleaseId = "official-alternate";
    private const string AlternateReleaseVersion = "alternate-version";
    private const string LocalizedPrefix = "localized:";
    private const long CompletedProgressBytes = 1;
    private const int AlternateReleaseIndex = 1;
    private static readonly DateTimeOffset TestPublishedUtc =
        new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    public static TheoryData<AtariMachineModel, AtariCoreKind> CoreModels => new()
    {
        { AtariMachineModel.St, AtariCoreKind.Hatari },
        { AtariMachineModel.Atari800, AtariCoreKind.Atari800 },
        { AtariMachineModel.Atari2600, AtariCoreKind.Stella },
        { AtariMachineModel.Atari7800, AtariCoreKind.ProSystem },
        { AtariMachineModel.Lynx, AtariCoreKind.BeetleLynx },
        { AtariMachineModel.Jaguar, AtariCoreKind.VirtualJaguar }
    };

    [Theory]
    [MemberData(nameof(CoreModels))]
    public void ControlSearchesSelectsInstallsAndRefreshesEachRequiredCore(
        AtariMachineModel model, AtariCoreKind expectedKind)
    {
        RunOnUiThread(() =>
        {
            var service = new FakeReleaseService();
            var control = new AtariCoreManagementSection(service, Localize);
            AtariCoreInstallationChangedEventArgs? change = null;
            control.InstallationChanged += (_, args) => change = args;

            control.SetModelAsync(model).GetAwaiter().GetResult();
            Assert.Equal(expectedKind, control.RequiredKind);
            Assert.Contains(AtariCoreCatalog.Get(expectedKind).LibraryName, control.RequiredText,
                StringComparison.Ordinal);
            Assert.Contains(AtariCoreManagementConstants.NotInstalledResource, control.InstalledText,
                StringComparison.Ordinal);
            Assert.Equal(Visibility.Collapsed, control.VersionsVisibility);
            Assert.Equal(Visibility.Collapsed, control.DownloadVisibility);

            control.SearchAsync().GetAwaiter().GetResult();
            Assert.Equal(expectedKind, service.LastSearchedKind);
            Assert.Equal(Visibility.Visible, control.VersionsVisibility);
            Assert.Equal(Visibility.Visible, control.DownloadVisibility);
            Assert.Equal(TestReleaseVersion, Assert.Single(control.Versions).DeclaredVersion);

            control.DownloadSelectedAsync().GetAwaiter().GetResult();
            Assert.NotNull(change);
            Assert.Equal(expectedKind, change!.Kind);
            Assert.Contains(TestReleaseVersion, control.InstalledText, StringComparison.Ordinal);
            Assert.Contains(change.Paths.LibraryPath, control.InstalledText, StringComparison.Ordinal);
            Assert.Contains(AtariCoreManagementConstants.InstalledPathResource, control.StatusText,
                StringComparison.Ordinal);
            Assert.Equal(TestReleaseVersion, Path.GetFileName(change.Paths.VersionDirectory));
        });
    }

    [Fact]
    public void FailedSearchKeepsResultsHiddenAndShowsTheTechnicalCause()
    {
        RunOnUiThread(() =>
        {
            var service = new FakeReleaseService { SearchError = new HttpRequestException("offline-detail") };
            var control = new AtariCoreManagementSection(service, Localize);
            control.SetModelAsync(AtariMachineModel.St).GetAwaiter().GetResult();

            control.SearchAsync().GetAwaiter().GetResult();

            Assert.Equal(Visibility.Collapsed, control.VersionsVisibility);
            Assert.Equal(Visibility.Collapsed, control.DownloadVisibility);
            Assert.False(string.IsNullOrWhiteSpace(control.StatusText));
        });
    }

    [Fact]
    public void ControlInstallsTheVersionSelectedByTheUser()
    {
        RunOnUiThread(() =>
        {
            var service = new FakeReleaseService { IncludeAlternateRelease = true };
            var control = new AtariCoreManagementSection(service, Localize);
            control.SetModelAsync(AtariMachineModel.St).GetAwaiter().GetResult();
            control.SearchAsync().GetAwaiter().GetResult();

            control.SelectVersion(AlternateReleaseIndex);
            control.DownloadSelectedAsync().GetAwaiter().GetResult();

            Assert.Equal(AlternateReleaseVersion, service.LastInstalledVersion);
            Assert.Contains(AlternateReleaseVersion, control.InstalledText, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void CancellationAndProgressAreVisibleWhileInstallingAndCancellationIsReported()
    {
        RunOnUiThread(() =>
        {
            AtariCoreManagementSection? control = null;
            var service = new FakeReleaseService
            {
                InstallError = new OperationCanceledException(),
                OnInstallStarted = () =>
                {
                    Assert.Equal(Visibility.Visible, control!.ProgressVisibility);
                    Assert.Equal(Visibility.Visible, control.CancelVisibility);
                }
            };
            control = new AtariCoreManagementSection(service, Localize);
            control.SetModelAsync(AtariMachineModel.St).GetAwaiter().GetResult();
            control.SearchAsync().GetAwaiter().GetResult();

            control.DownloadSelectedAsync().GetAwaiter().GetResult();

            Assert.Equal(Visibility.Collapsed, control.ProgressVisibility);
            Assert.Equal(Visibility.Collapsed, control.CancelVisibility);
            Assert.Contains(AtariCoreManagementConstants.CancelledResource, control.StatusText,
                StringComparison.Ordinal);
        });
    }

    private static string Localize(string key, object[] arguments) =>
        LocalizedPrefix + key + (arguments.Length == 0 ? string.Empty : $":{string.Join('|', arguments)}");

    private static void RunOnUiThread(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception error)
            {
                failure = error;
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(UiThreadTimeoutSeconds)),
            "The Atari core management control test timed out.");
        if (failure is not null) throw failure;
    }

    private sealed class FakeReleaseService : IAtariCoreReleaseService
    {
        private AtariCoreInstallationPaths? _active;
        internal AtariCoreKind? LastSearchedKind { get; private set; }
        internal string? LastInstalledVersion { get; private set; }
        internal Exception? SearchError { get; init; }
        internal Exception? InstallError { get; init; }
        internal Action? OnInstallStarted { get; init; }
        internal bool IncludeAlternateRelease { get; init; }

        public Task<IReadOnlyList<AtariCoreRelease>> GetAvailableAsync(AtariCoreKind kind,
            CancellationToken cancellationToken = default)
        {
            LastSearchedKind = kind;
            if (SearchError is not null) return Task.FromException<IReadOnlyList<AtariCoreRelease>>(SearchError);
            var releases = new List<AtariCoreRelease>
            {
                new(kind, TestReleaseId, TestReleaseVersion,
                    AtariCoreCatalog.Get(kind).ArchiveUri, TestPublishedUtc, null)
            };
            if (IncludeAlternateRelease)
            {
                releases.Add(new AtariCoreRelease(kind, AlternateReleaseId, AlternateReleaseVersion,
                    AtariCoreCatalog.Get(kind).ArchiveUri, TestPublishedUtc, null));
            }
            return Task.FromResult<IReadOnlyList<AtariCoreRelease>>(releases);
        }

        public Task<AtariCoreInstallationPaths> InstallAsync(AtariCoreRelease release,
            IProgress<AtariCoreInstallProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            OnInstallStarted?.Invoke();
            if (InstallError is not null) return Task.FromException<AtariCoreInstallationPaths>(InstallError);
            LastInstalledVersion = release.DeclaredVersion;
            _active = new AtariCoreInstallationPaths(Path.Combine("cores", release.Kind.ToString(),
                release.DeclaredVersion), Path.Combine("cores", release.Kind.ToString(),
                release.DeclaredVersion, AtariCoreCatalog.Get(release.Kind).DllName),
                Path.Combine("cores", release.Kind.ToString(), release.DeclaredVersion,
                    AtariCoreCatalogConstants.ManifestFileName));
            progress?.Report(new AtariCoreInstallProgress(CompletedProgressBytes, CompletedProgressBytes));
            return Task.FromResult(_active);
        }

        public Task<AtariCoreInstallationPaths?> GetActiveInstallationAsync(AtariCoreKind kind,
            CancellationToken cancellationToken = default) => Task.FromResult(_active);
    }
}
