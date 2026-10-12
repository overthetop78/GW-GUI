using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using Swan = GWGUI.Emulation.Sony.Emulators.SwanStation.Constants.OptionConstants;
using Rearmed = GWGUI.Emulation.Sony.Emulators.PcsxRearmed.Constants.OptionConstants;
using Beetle = GWGUI.Emulation.Sony.Emulators.BeetlePsx.Constants.OptionConstants;
using BeetleHw = GWGUI.Emulation.Sony.Emulators.BeetlePsxHw.Constants.OptionConstants;
using System.IO;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class MemoryCardFunctions
{
    internal static int Count(MachineConfiguration configuration) => configuration.Model == ModelConstants.Psp ? 1
        : configuration.EmulatorId is Emulators.BeetlePsx.Constants.CoreConstants.Id or Emulators.BeetlePsxHw.Constants.CoreConstants.Id ? ControllerFunctions.PortCount(configuration) : StorageSettingsFunctionsConstants.MemoryCardCount;

    internal static IReadOnlyList<string> NativeFileNames(MachineConfiguration configuration) => NativeFileNames(configuration.EmulatorId).Take(Count(configuration)).ToArray();

    internal static string? SelectedPath(MachineConfiguration configuration, int index) =>
        bool.TryParse(configuration.Options?.GetValueOrDefault(StorageSettingsFunctionsConstants.MemoryCardEnabledOptionPrefix + index), out var enabled) && enabled ? configuration.Options?.GetValueOrDefault(StorageSettingsFunctionsConstants.MemoryCardPathOptionPrefix + index) : null;

    internal static IReadOnlyList<string> NativeFileNames(string coreId) => coreId switch
    {
        Emulators.Pcsx2.Constants.Pcsx2Constants.Id or Emulators.Pcee2.Constants.CoreConstants.Id =>
            [MemoryCardConstants.Ps2FirstFile, MemoryCardConstants.Ps2SecondFile],
        Emulators.SwanStation.Constants.CoreConstants.Id => [MemoryCardConstants.SwanFirstFile, MemoryCardConstants.SwanSecondFile],
        Emulators.PcsxRearmed.Constants.CoreConstants.Id => [MemoryCardConstants.RearmedFirstFile, MemoryCardConstants.RearmedSecondFile],
        Emulators.BeetlePsx.Constants.CoreConstants.Id or Emulators.BeetlePsxHw.Constants.CoreConstants.Id =>
            Enumerable.Range(0, MemoryCardConstants.MaximumCards).Select(index => MemoryCardConstants.BeetleFilePrefix + index + StorageSettingsFunctionsConstants.MemoryCardExtension).ToArray(),
        _ => []
    };

    internal static MachineConfiguration Configure(MachineConfiguration configuration)
    {
        if (configuration.Model == ModelConstants.PlayStation2
            && configuration.EmulatorId == Emulators.Pcee2.Constants.CoreConstants.Id)
        {
            var ps2Options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal)
            {
                [MemoryCardConstants.PceeFirstEnabled] = SelectedPath(configuration, 0) is null ? "disabled" : "enabled",
                [MemoryCardConstants.PceeSecondEnabled] = SelectedPath(configuration, 1) is null ? "disabled" : "enabled",
                [MemoryCardConstants.PceeFirstFile] = MemoryCardConstants.Ps2FirstFile,
                [MemoryCardConstants.PceeSecondFile] = MemoryCardConstants.Ps2SecondFile
            };
            return configuration with { Options = ps2Options };
        }
        if (configuration.Model != ModelConstants.PlayStation) return configuration;
        var options = new Dictionary<string, string>(configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        var first = !string.IsNullOrWhiteSpace(SelectedPath(configuration, 0));
        var second = !string.IsNullOrWhiteSpace(SelectedPath(configuration, 1));
        switch (configuration.EmulatorId)
        {
            case Emulators.SwanStation.Constants.CoreConstants.Id:
                options[Swan.MemoryCardsCard1TypeKey] = first ? Swan.MemoryCardsCard1TypeShared : Swan.MemoryCardsCard1TypeNone;
                options[Swan.MemoryCardsCard2TypeKey] = second ? Swan.MemoryCardsCard2TypeShared : Swan.MemoryCardsCard2TypeNone;
                break;
            case Emulators.PcsxRearmed.Constants.CoreConstants.Id:
                options[Rearmed.Memcard1Key] = first ? Rearmed.Memcard1Shared : Rearmed.Memcard1None;
                options[Rearmed.Memcard2Key] = second ? Rearmed.Memcard2Shared : Rearmed.Memcard2None;
                break;
            case Emulators.BeetlePsx.Constants.CoreConstants.Id:
                ConfigureBeetle(Beetle.UseMednafenMemcard0MethodKey, Beetle.UseMednafenMemcard0MethodMednafen, Beetle.EnableMemcard1Key, Beetle.SharedMemoryCardsKey, Beetle.MemcardLeftIndexKey, Beetle.MemcardRightIndexKey);
                break;
            case Emulators.BeetlePsxHw.Constants.CoreConstants.Id:
                ConfigureBeetle(BeetleHw.UseMednafenMemcard0MethodKey, BeetleHw.UseMednafenMemcard0MethodMednafen, BeetleHw.EnableMemcard1Key, BeetleHw.SharedMemoryCardsKey, BeetleHw.MemcardLeftIndexKey, BeetleHw.MemcardRightIndexKey);
                break;
        }
        return configuration with { Options = options };

        void ConfigureBeetle(string methodKey, string method, string secondKey, string sharedKey, string firstIndexKey, string secondIndexKey)
        {
            options[methodKey] = method;
            options[secondKey] = second ? ToggleValueConstants.Enabled : ToggleValueConstants.Disabled;
            options[sharedKey] = ToggleValueConstants.Enabled;
            options[firstIndexKey] = MemoryCardConstants.FirstIndex;
            options[secondIndexKey] = MemoryCardConstants.SecondIndex;
        }
    }

    internal static bool IsManagedOption(string key) => key is
        MemoryCardConstants.PceeFirstEnabled or MemoryCardConstants.PceeSecondEnabled
        or MemoryCardConstants.PceeFirstFile or MemoryCardConstants.PceeSecondFile
        or Swan.MemoryCardsCard1TypeKey or Swan.MemoryCardsCard2TypeKey or Swan.MemoryCardsUsePlaylistTitleKey
        or Rearmed.Memcard1Key or Rearmed.Memcard2Key
        or Beetle.UseMednafenMemcard0MethodKey or Beetle.EnableMemcard1Key or Beetle.SharedMemoryCardsKey
        or Beetle.MemcardLeftIndexKey or Beetle.MemcardRightIndexKey
        or BeetleHw.UseMednafenMemcard0MethodKey or BeetleHw.EnableMemcard1Key or BeetleHw.SharedMemoryCardsKey
        or BeetleHw.MemcardLeftIndexKey or BeetleHw.MemcardRightIndexKey;

    internal static void PreparePs2(MachineConfiguration configuration, string systemDirectory,
        ICollection<(string Source, string Native)> files)
    {
        files.Clear();
        var names = NativeFileNames(configuration);
        if (names.Count == 0) return;
        var directory = Path.Combine(systemDirectory, MemoryCardConstants.Ps2CoreDirectory, MemoryCardConstants.Ps2Directory);
        Directory.CreateDirectory(directory);
        for (var index = 0; index < names.Count; index++)
        {
            var source = SelectedPath(configuration, index);
            if (string.IsNullOrWhiteSpace(source)) continue;
            if (new FileInfo(source).Length != StorageSettingsFunctionsConstants.Ps2MemoryCardSize)
                throw new InvalidDataException(source);
            var destination = Path.Combine(directory, names[index]);
            File.Copy(source, destination, overwrite: true);
            files.Add((source, destination));
        }
    }

    internal static void PersistFiles(IEnumerable<(string Source, string Native)> files, int size)
    {
        List<Exception>? errors = null;
        foreach (var (source, native) in files)
        {
            try
            {
                if (!File.Exists(native)) continue;
                var data = File.ReadAllBytes(native);
                if (data.Length != size) throw new InvalidDataException(native);
                var temporary = source + SaveMemoryConstants.TemporaryFileSuffix;
                try
                {
                    File.WriteAllBytes(temporary, data);
                    File.Move(temporary, source, overwrite: true);
                }
                finally { if (File.Exists(temporary)) File.Delete(temporary); }
            }
            catch (Exception error) { (errors ??= []).Add(error); }
        }
        if (errors is not null) throw new AggregateException(errors);
    }
}
