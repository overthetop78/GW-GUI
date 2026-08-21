using GWGUI.Domain.Settings;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

public sealed partial class EmulationSection
{
    private string? InitialMediaDirectory(
        EmulationConfigurationListItem selected, EmulationMediaDevice device)
    {
        var category = MediaFolderCategory(device.MediaType);
        return _settings.EmulationMediaFolders.FirstOrDefault(item =>
            string.Equals(item.ModuleId, selected.Module.Id, StringComparison.Ordinal)
            && string.Equals(item.MachineId, selected.Configuration.MachineId,
                StringComparison.Ordinal)
            && item.Category == category)?.Folder;
    }

    private void RememberMediaDirectory(
        EmulationConfigurationListItem selected, EmulationMediaDevice device, string directory)
    {
        var category = MediaFolderCategory(device.MediaType);
        var existing = _settings.EmulationMediaFolders.FirstOrDefault(item =>
            string.Equals(item.ModuleId, selected.Module.Id, StringComparison.Ordinal)
            && string.Equals(item.MachineId, selected.Configuration.MachineId,
                StringComparison.Ordinal)
            && item.Category == category);
        if (existing is null)
        {
            _settings.EmulationMediaFolders.Add(new EmulationMediaFolderSettings
            {
                ModuleId = selected.Module.Id,
                MachineId = selected.Configuration.MachineId,
                Category = category,
                Folder = directory
            });
            return;
        }
        existing.Folder = directory;
    }

    private static EmulationMediaFolderCategory MediaFolderCategory(
        EmulationMediaType mediaType) => mediaType switch
    {
        EmulationMediaType.Floppy => EmulationMediaFolderCategory.Floppy,
        EmulationMediaType.CompactDisc => EmulationMediaFolderCategory.CompactDisc,
        EmulationMediaType.HardDisk => EmulationMediaFolderCategory.HardDisk,
        EmulationMediaType.Cartridge => EmulationMediaFolderCategory.Cartridge,
        EmulationMediaType.Cassette => EmulationMediaFolderCategory.Cassette,
        _ => throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null)
    };
}
