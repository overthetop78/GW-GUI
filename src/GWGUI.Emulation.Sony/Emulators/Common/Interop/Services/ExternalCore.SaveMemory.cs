using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Common.Exceptions;
using NativeMemoryCards = GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.MemoryCardFunctions;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private ExternalCoreApi.GetMemoryData? _getMemoryData;
    private ExternalCoreApi.GetMemorySize? _getMemorySize;
    private IReadOnlyList<(uint Id, string Path)> _saveMemoryRegions = [];
    private bool _saveMemoryReady;
    private readonly List<(string Source, string Native)> _memoryCardFiles = [];

    private string PrepareMemoryCards(MachineConfiguration configuration, string saveDirectory, string systemDirectory)
    {
        _memoryCardFiles.Clear();
        if (configuration.Model == ModelConstants.PlayStation2)
        {
            NativeMemoryCards.PreparePs2(configuration, systemDirectory, _memoryCardFiles);
            return saveDirectory;
        }
        if (configuration.Model != ModelConstants.PlayStation) return saveDirectory;
        var directory = Path.Combine(_sessionDirectory!, MemoryCardConstants.Directory);
        Directory.CreateDirectory(directory);
        var names = NativeMemoryCards.NativeFileNames(configuration);
        for (var index = 0; index < names.Count; index++)
        {
            var source = NativeMemoryCards.SelectedPath(configuration, index);
            if (string.IsNullOrWhiteSpace(source)) continue;
            if (new FileInfo(source).Length != StorageSettingsFunctionsConstants.MemoryCardSize)
                throw new InvalidDataException(source);
            var destination = Path.Combine(directory, names[index]);
            File.Copy(source, destination, true);
            _memoryCardFiles.Add((source, destination));
        }
        return directory;
    }

    private void PersistMemoryCards()
    {
        if (!_saveMemoryReady) return;
        NativeMemoryCards.PersistFiles(_memoryCardFiles, _configuration?.Model == ModelConstants.PlayStation2 ? StorageSettingsFunctionsConstants.Ps2MemoryCardSize : StorageSettingsFunctionsConstants.MemoryCardSize);
    }

    private (nint Pointer, int Size) SaveMemory(uint id)
    {
        var size = _getMemorySize?.Invoke(id) ?? ExternalCoreInteropConstants.EmptyNativeSize;
        if (size == ExternalCoreInteropConstants.EmptyNativeSize) return default;
        if (size > SaveMemoryConstants.MaximumManagedBufferSize)
            throw new InvalidOperationException(ErrorMessages.InvalidStateSize(size));
        var pointer = _getMemoryData?.Invoke(id) ?? nint.Zero;
        return pointer == nint.Zero ? default : (pointer, checked((int)size));
    }

    private void RestoreSaveMemory()
    {
        if (_configuration?.Model == ModelConstants.PlayStation) return;
        foreach (var region in _saveMemoryRegions)
        {
            if (!File.Exists(region.Path)) continue;
            var memory = SaveMemory(region.Id);
            if (memory.Pointer == nint.Zero) continue;
            var bytes = File.ReadAllBytes(region.Path);
            if (bytes.Length != memory.Size)
                throw new InvalidDataException(ErrorMessages.InvalidStateSize((nuint)bytes.Length));
            Marshal.Copy(bytes, BufferConstants.FirstBufferIndex, memory.Pointer, bytes.Length);
        }
    }

    private void PersistSaveMemory()
    {
        if (_configuration?.Model == ModelConstants.PlayStation) return;
        if (!_saveMemoryReady) return;
        var errors = new List<Exception>();
        foreach (var region in _saveMemoryRegions)
            try { PersistSaveMemory(region.Id, region.Path); }
            catch (Exception error) { errors.Add(error); }
        if (errors.Count > 0) throw new AggregateException(errors);
    }

    private void PersistSaveMemory(uint id, string path)
    {
        var memory = SaveMemory(id);
        if (memory.Pointer == nint.Zero) return;
        var bytes = new byte[memory.Size];
        Marshal.Copy(memory.Pointer, bytes, BufferConstants.FirstBufferIndex, bytes.Length);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporaryPath = path + SaveMemoryConstants.TemporaryFileSuffix;
        try
        {
            File.WriteAllBytes(temporaryPath, bytes);
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
    /// <summary>Contrôle la session et la validité de l’identité de lecteur reçue.</summary>
    private void ValidateMediaSaveSlot(EmulationMediaSlot slot)
    {
        if (_library is null) throw new InvalidOperationException();
        if (!Enum.IsDefined(slot.Category) || slot.Index < 0)
            throw new ArgumentOutOfRangeException(nameof(slot));
    }
    /// <inheritdoc/>
    /// <remarks>La carte sélectionnée est publiée par le traitement atomique existant. Les autres cartes,
    /// la mémoire de sauvegarde du cœur et leurs règles de fermeture restent indépendantes.</remarks>
    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        ValidateMediaSaveSlot(slot);
        if (MemoryCardForSave(slot) is not { } card) return;
        NativeMemoryCards.PersistFiles([card], _configuration!.Model == ModelConstants.PlayStation2 ? StorageSettingsFunctionsConstants.Ps2MemoryCardSize : StorageSettingsFunctionsConstants.MemoryCardSize);
    }
    /// <inheritdoc/>
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot)
    {
        ValidateMediaSaveSlot(slot);
        return MemoryCardForSave(slot).HasValue;
    }
    /// <summary>Résout la copie de carte modifiable déjà préparée pour le lecteur demandé.</summary>
    /// <param name="slot">Lecteur logique reçu par la file de commandes du cœur.</param>
    /// <returns>Paire de chemins déjà possédée par la session, ou null sans carte modifiable à publier.</returns>
    private (string Source, string Native)? MemoryCardForSave(EmulationMediaSlot slot)
    {
        if (!_saveMemoryReady || slot.Category != EmulationMediaCategory.MemoryCard) return null;
        if (_configuration!.Media?.Any(media => media.Category == MediaCategory.MemoryCard && (media.SlotIndex ?? media.MountOrder) == slot.Index && media.IsReadOnly) == true) return null;
        var source = NativeMemoryCards.SelectedPath(_configuration, slot.Index);
        foreach (var card in _memoryCardFiles)
            if (string.Equals(card.Source, source, StringComparison.OrdinalIgnoreCase)) return card;
        return null;
    }
}
