using GWGUI.Emulation.Snk.Emulators.Common.Interop.Functions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Snk.Emulators.Common.Dictionaries;
using System.IO;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private void PrepareInitialMedia(MachineConfiguration configuration) => _content = MediaAdapter().PrepareContent(configuration, _sessionDirectory ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized), _info.Extensions);

    public void InsertMedia(MediaConfiguration media)
    {
        var configuration = RequireConfiguration();
        var adapter = MediaAdapter();
        if (!adapter.SupportsMediaExchange(configuration, media.Slot)) throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
        var next = configuration with { Media = configuration.Media.Where(item => item.Slot != media.Slot).Append(media with { IsInserted = true }).ToArray() };
        ConfigurationFunctions.Validate(next);
        var device = adapter.DescribeMediaDevices(configuration).SingleOrDefault(item => item.Slot == media.Slot);
        if (device is null || !configuration.ConfiguredSlots.Contains(media.Slot) || !device.AcceptedExtensions.Contains(Path.GetExtension(media.Path), StringComparer.OrdinalIgnoreCase)) throw new ArgumentException(ErrorMessages.IncompatibleMedia, nameof(media));
        var runtimePath = SessionMediaFunctions.Prepare(media, _sessionDirectory ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized), device.AcceptedExtensions);
        RequireCallbacks().DiskControl.Insert(runtimePath);
        _configuration = next;
    }

    public void EjectMedia(EmulationMediaSlot slot)
    {
        var configuration = RequireConfiguration();
        if (!configuration.ConfiguredSlots.Contains(slot)) throw new ArgumentOutOfRangeException(nameof(slot));
        if (!MediaAdapter().SupportsMediaExchange(configuration, slot)) throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
        RequireCallbacks().DiskControl.Eject();
        _configuration = configuration with { Media = configuration.Media.Select(media => media.Slot == slot ? media with { IsInserted = false } : media).ToArray() };
    }

    public void SelectDisk(int index) => RequireCallbacks().DiskControl.Select(index);
    public DiskStatus GetDiskStatus() => RequireCallbacks().DiskControl.GetStatus();

    private MachineConfiguration RequireConfiguration() => _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
    private LibretroMachineFactory MediaAdapter() => (LibretroMachineFactory)CoreCatalog.CreateAdapter(Emulator);
    /// <inheritdoc/>
    /// <remarks>La publication reprend le traitement de carte mémoire déjà employé à la fermeture.
    /// Les adaptations du format natif et les contrôles de protection restent dans son propriétaire.</remarks>
    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        var configuration = RequireConfiguration();
        if (!MediaAdapter().DescribeMediaDevices(configuration).Any(device => device.Slot == slot))
            throw new ArgumentOutOfRangeException(nameof(slot));
        if (HasUnsavedMediaChanges(slot)) _persistentMediaAdapter!.CompletePersistentMedia(_persistentMediaState);
    }
    /// <inheritdoc/>
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => slot.Category == EmulationMediaCategory.MemoryCard
        && _persistentMediaState is not null && RequireConfiguration().Media.Any(media => media.Slot == slot && media.IsInserted && !media.IsReadOnly);
}
