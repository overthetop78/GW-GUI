using GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Nec.Emulators.NP2Kai.Constants;
using FirmwareConstants = GWGUI.Emulation.Nec.Emulators.NP2Kai.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Nec.Emulators.NP2Kai.Functions;

internal static class ContentFunctions
{
    internal static string Prepare(IReadOnlyList<MediaConfiguration> media, string contentDirectory, string systemDirectory) =>
        Pc98ContentFunctions.Prepare(media, contentDirectory, systemDirectory,
            FirmwareConstants.SystemSubdirectory, ContentConstants.Profiles, ContentConstants.HardDiskKeys,
            ContentConstants.ExecutableName, ContentConstants.CommandBufferLimit,
            StorageConstants.FirstDriveIndex, StorageConstants.DriveCount);
}
