using GWGUI.Emulation.Mattel.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Mattel.Emulators.Common.Interop.Functions;
using System.IO;

namespace GWGUI.Emulation.Mattel.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private void PrepareInitialMedia(MachineConfiguration configuration)
    {
        var media = configuration.Media.SingleOrDefault(value => value.IsInserted);
        if (media is null) throw InvalidContent();
        var source = ContentFunctions.Validate(media.Path, _info.Extensions);
        var size = new FileInfo(source).Length;
        if (size <= 0 || size > MediaRuntimeConstants.MaximumCartridgeBytes) throw InvalidContent();
        var bytes = File.ReadAllBytes(source);
        if (bytes[0] == MediaRuntimeConstants.IntellicartMagic || (bytes.Length >= MediaRuntimeConstants.IntellicartHeaderBytes && bytes[1] == (byte)(bytes[2] ^ byte.MaxValue)))
        {
            if (bytes.Length < MediaRuntimeConstants.IntellicartHeaderBytes
                || bytes[1] != (byte)(bytes[2] ^ byte.MaxValue)) throw InvalidContent();
            var position = MediaRuntimeConstants.IntellicartHeaderBytes;
            for (var segment = 0; segment < bytes[1]; segment++)
            {
                if (position + MediaRuntimeConstants.SegmentHeaderBytes > bytes.Length) throw InvalidContent();
                var firstPage = bytes[position++];
                var lastPage = bytes[position++];
                if (lastPage < firstPage) throw InvalidContent();
                position += (lastPage - firstPage + 1) * MediaRuntimeConstants.WordsPerPage * MediaRuntimeConstants.BytesPerWord + MediaRuntimeConstants.SegmentChecksumBytes;
                if (position > bytes.Length) throw InvalidContent();
            }
        }
        else if (bytes.Length % MediaRuntimeConstants.BytesPerWord != 0) throw InvalidContent();
        _content = ContentFunctions.Create(source, _info.NeedsFullPath, _info.Extensions);
    }

    public void InsertMedia(MediaConfiguration media) => throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
    public void EjectMedia(EmulationMediaSlot slot) => throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
    public void SelectDisk(int index) => throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
    public DiskStatus GetDiskStatus() => RequireCallbacks().DiskControl.GetStatus();
    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        if (!ConfigurationFunctions.DefaultSlots(RequireConfiguration().Model).Contains(slot))
            throw new ArgumentOutOfRangeException(nameof(slot));
    }
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;
    private MachineConfiguration RequireConfiguration() => _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
    private static EmulationException InvalidContent() => new(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
}
