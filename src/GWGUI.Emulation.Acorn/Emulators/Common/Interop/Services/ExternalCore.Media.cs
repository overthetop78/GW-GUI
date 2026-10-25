using CommonCoreHostFunctions = GWGUI.Emulation.Acorn.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Acorn.Emulators.Common.Functions;
using GWGUI.Emulation.Acorn.Emulators.Common.Exceptions;
namespace GWGUI.Emulation.Acorn.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    public void InsertMedia(MediaConfiguration media)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        ConfigurationFunctions.Validate(configuration with { Media = [media] });
        var prepared = SessionMediaFunctions.Prepare(media, _sessionDirectory ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized), _info.Extensions);
        RequireCallbacks().DiskControl.Insert(prepared.RuntimePath);
        _sessionMedia.Add(prepared);
    }

    public void EjectMedia(EmulationMediaSlot slot) => throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.DynamicMediaUnsupported);

    public void SelectDisk(int index) => RequireCallbacks().DiskControl.Select(index);
    public DiskStatus GetDiskStatus() => RequireCallbacks().DiskControl.GetStatus();

    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        var media = _sessionMedia.LastOrDefault(item => item.Configuration.Slot == slot) ?? throw new InvalidOperationException(SessionMediaErrors.ExplicitSaveRequired);
        SessionMediaFunctions.Save(media);
    }

    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => _sessionMedia.LastOrDefault(item => item.Configuration.Slot == slot)?.RequiresExplicitSave == true;

    public byte[] SaveState() => CommonCoreHostFunctions.MachineExecution.SerializeState(RequireExports, SupportsSaveStates,
        (nuint)Array.MaxLength,
        static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateUnavailable),
        static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateSaveFailed));

    public void LoadState(ReadOnlySpan<byte> state) => CommonCoreHostFunctions.MachineExecution.UnserializeState(RequireExports,
        SupportsSaveStates, (nuint)Array.MaxLength, state,
        static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateSizeInvalid),
        static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateLoadFailed));
}
