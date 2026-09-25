using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed partial class ExternalCore : IEmulatorCore
{
public void InsertMedia(MediaConfiguration media)
    {
        var configuration = _configuration ??
            throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        _mediaAdapter.ValidateInsertion(configuration, media);
        if (media.Category == MediaCategory.CompactDisc)
        {
            ReplacePreparedContent(media);
            return;
        }
        if (CartridgeFunctions.Supports(Emulator))
        {
            ReplaceCartridge(media);
            return;
        }
        if (!_mediaAdapter.SupportsDiskControl(media))
            throw new NotSupportedException(ErrorMessages.HatariFloppyRequired);
        var sessionDirectory = _sessionDirectory ??
            throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        var preparedMedia = _mediaAdapter.PrepareInsertedMedia(configuration, media, sessionDirectory, _info)
            ?? throw new NotSupportedException(ErrorMessages.HatariFloppyRequired);
        RequireCallbacks().DiskControl.Insert(preparedMedia.RuntimePath);
        MediaRuntimeFunctions.Register(_mountedMedia, media with { IsInserted = true });
        _sessionMedia.Add(preparedMedia);
    }

    public void EjectMedia(EmulationMediaSlot slot)
    {
        if (!_mediaAdapter.SupportsEjection(slot))
            throw new NotSupportedException(CartridgeErrors.EjectionUnsupported);
        RequireCallbacks().DiskControl.Eject();
        MediaRuntimeFunctions.MarkEjected(_mountedMedia, slot);
    }

    public void SelectDisk(int index)
    {
        if (!_mediaAdapter.SupportsDiskControlOperations)
            throw new NotSupportedException(ErrorMessages.HatariFloppyRequired);
        RequireCallbacks().DiskControl.Select(index);
    }

    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        var media = _sessionMedia.LastOrDefault(item => item.Configuration.Slot == slot) ??
            throw new InvalidOperationException(SessionMediaErrors.ExplicitSaveRequired);
        SessionMediaFunctions.Save(media);
    }

    public DiskStatus GetDiskStatus()
    {
        if (!_mediaAdapter.SupportsDiskControlOperations)
            throw new NotSupportedException(ErrorMessages.HatariFloppyRequired);
        return RequireCallbacks().DiskControl.GetStatus();
    }

    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) =>
        _sessionMedia.LastOrDefault(item => item.Configuration.Slot == slot)?.RequiresExplicitSave == true;

    public byte[] SaveState()
    {
        var exports = RequireExports();
        var size = exports.GetSerializedSize();
        if (size == nuint.Zero || size > CommonConstants.MaximumStateSize)
            throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid,
                ErrorMessages.StateSizeInvalid);
        var state = GC.AllocateUninitializedArray<byte>(checked((int)size));
        var buffer = Marshal.AllocHGlobal(state.Length);
        try
        {
            if (!exports.Serialize(buffer, size))
                throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid,
                    ErrorMessages.StateSaveFailed);
            Marshal.Copy(buffer, state, CommonConstants.FirstBufferIndex, state.Length);
            return state;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void LoadState(ReadOnlySpan<byte> state)
    {
        if (state.IsEmpty || state.Length > CommonConstants.MaximumStateSize)
            throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid,
                ErrorMessages.StateSizeInvalid);
        var bytes = state.ToArray();
        var buffer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, CommonConstants.FirstBufferIndex, buffer, bytes.Length);
            if (!RequireExports().Unserialize(buffer, (nuint)bytes.Length))
                throw new EmulationException(ErrorCategory.State, ErrorCode.StateIncompatible,
                    ErrorMessages.StateLoadFailed);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void Stop()
    {
        if (_gameLoaded && _exports is not null)
        {
            _exports.UnloadGame();
            _gameLoaded = false;
        }
        _supportsSaveStates = false;
        _content?.Dispose();
        _content = null;
        _mediaAdapter.CleanupPreparedContent(_preparedContent);
        _preparedContent = null;
        _cartridge = null;
        _mountedMedia.Clear();
        _sessionMedia.Clear();
        _configuration = null;
    }
}
