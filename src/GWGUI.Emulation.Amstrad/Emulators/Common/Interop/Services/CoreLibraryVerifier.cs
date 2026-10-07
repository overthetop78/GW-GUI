using GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Constants;
using System.IO;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Interop.Services;

internal static class CoreLibraryVerifier
{
    internal static void VerifyWindowsX64Library(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        if (stream.Length < ExternalCoreConstants.MinimumDosHeaderLength || reader.ReadUInt16() != ExternalCoreConstants.DosHeaderSignature)
            throw new InvalidDataException(CommonExceptions.DownloadedCoreNotPe());
        stream.Position = ExternalCoreConstants.PeHeaderOffsetPosition;
        var peOffset = reader.ReadInt32();
        if (peOffset < ExternalCoreConstants.MinimumDosHeaderLength || peOffset > stream.Length - ExternalCoreConstants.PeSignatureAndMachineLength)
            throw new InvalidDataException(CommonExceptions.DownloadedCoreInvalidPe());
        stream.Position = peOffset;
        if (reader.ReadUInt32() != ExternalCoreConstants.PeHeaderSignature || reader.ReadUInt16() != ExternalCoreConstants.PeMachineX64)
            throw new InvalidDataException(CommonExceptions.DownloadedCoreWrongArchitecture());
    }

}
