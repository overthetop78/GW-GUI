using System.IO;
using GWGUI.App.Services.Storage;
using GWGUI.VideoPresentation.Constants;
using GWGUI.VideoPresentation.Services;

namespace GWGUI.App.Services.Emulation;

internal static class EmulationVideoPresentationProfiles
{
    internal static VideoPresentationProfileStore Store { get; } = new(
        Path.Combine(StoragePaths.EmulationDirectory, VideoPresentationStorageConstants.DirectoryName));
}