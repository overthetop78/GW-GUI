using System.IO;

namespace GWGUI.Emulation.Bandai.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private MediaConfiguration? _initialMedia;

    private void PrepareInitialMedia(MachineConfiguration configuration)
    {
        _initialMedia = configuration.Media.FirstOrDefault(media => media.IsInserted);
        var path = configuration.Model == MachineModel.TamagotchiP1 ? configuration.Firmwares.FirstOrDefault(firmware => firmware.Category == FirmwareCategory.System)?.Path : _initialMedia?.Path;
        if (path is null)
        {
            if (configuration.Model == MachineModel.TamagotchiP1)
                throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.RequiredFirmwareMissing);
            return;
        }
        var validated = ContentFunctions.Validate(path, _info.Extensions);
        if (configuration.Model == MachineModel.TamagotchiP1)
        {
            var size = new FileInfo(validated).Length;
            if (size <= 0 || size > MediaConstants.MaximumTamagotchiRomBytes
                || size % MediaConstants.TamagotchiInstructionBytes != 0)
                throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.UnknownFirmware);
        }
        _content = ContentFunctions.Create(validated, _info.NeedsFullPath, _info.Extensions);
        if (configuration.Family == MachineFamily.WonderSwan)
        {
            using var source = File.OpenRead(validated);
            var identity = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(source));
            _batterySavePath = Path.Combine(_saveDirectory!, identity + MediaConstants.BatterySaveExtension);
        }
    }

    // Media changes are handled by the owning machine through a core restart.
    public void InsertMedia(MediaConfiguration media) => throw DynamicMediaUnavailable();
    public void EjectMedia(EmulationMediaSlot slot) => throw DynamicMediaUnavailable();

    public void SelectDisk(int index)
    {
        if (index != 0 || GetDiskStatus().ImageCount == 0) throw DynamicMediaUnavailable();
    }

    public DiskStatus GetDiskStatus() => _configuration?.Family == MachineFamily.Playdia && _initialMedia is not null ? new(1, 0, false, [new(0, _initialMedia.Path, _initialMedia.Label)]) : new(0, -1, true, []);

    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        if (slot != EmulationMediaSlot.Cartridge0 || !_batteryMemoryReady) throw DynamicMediaUnavailable();
        WriteBatteryMemory(RequireExports());
    }

    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;

    private static EmulationException DynamicMediaUnavailable() => new(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.DynamicMediaUnsupported);
}
