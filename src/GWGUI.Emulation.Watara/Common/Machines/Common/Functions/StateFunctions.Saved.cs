using CommonCoreHostFunctions = GWGUI.Emulation.Watara.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Watara.Common.Machines.Common.Functions;
internal static class SavedStateFunctions
{
    internal static SavedStateHeader CreateHeader(MachineConfiguration configuration,
        IEmulatorCore core,
        ReadOnlySpan<byte> state) => new(StateConstants.CurrentFormatVersion, core.Emulator, core.CoreName, core.CoreVersion, core.CoreSha256, configuration.Model, ConfigurationHash(configuration), ContentHash(configuration), HashBytes(state));
    internal static void Validate(SavedStateHeader header, MachineConfiguration configuration, IEmulatorCore core)
    {
        if (!EmulationSdk.StateFiles.IsCompatible(header, configuration.Model, core.Emulator, core,
                () => ConfigurationHash(configuration), () => ContentHash(configuration)))
            throw Invalid(EmulationMessageCode.SavedStateIncompatible, ErrorMessages.StateIncompatible);
    }

    internal static void ValidatePayloadSize(int length)
    {
        if (length <= StateConstants.EmptyLength)
            throw Invalid(EmulationMessageCode.SavedStateInvalid, ErrorMessages.StateInvalid);
        if (length > Array.MaxLength)
            throw Invalid(EmulationMessageCode.SavedStateInvalid, ErrorMessages.StateInvalid);
    }

    internal static EmulationMessageException Invalid(EmulationMessageCode code,
        string message,
        Exception? innerException = null) => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.SavedState, code, innerException ?? new InvalidDataException(message));
    internal static string HashBytes(ReadOnlySpan<byte> bytes) => EmulationSdk.StateFiles.HashBytes(bytes);
    internal static string ConfigurationHash(MachineConfiguration configuration)
    {
        var fingerprint = new StateConfigurationFingerprint(
            configuration.SchemaVersion,
            configuration.Model,
            configuration.Core,
            configuration.AudioEnabled,
            ContentEntries(configuration),
            configuration.Options.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray(),
            InputFingerprint(configuration.Input),
            configuration.Media.OrderBy(MediaOrder).ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase).ToArray()
        );
        return EmulationSdk.StateFiles.HashDocument(fingerprint);
    }

    internal static string ContentHash(MachineConfiguration configuration) => EmulationSdk.StateFiles.HashDocument(ContentEntries(configuration));
    private static IReadOnlyList<StateContentEntry> ContentEntries(MachineConfiguration configuration) => configuration.Media.Where(media => media.IsInserted).OrderBy(MediaOrder).ThenBy(media => media.Path,
        StringComparer.OrdinalIgnoreCase).Select(media => new StateContentEntry(StateConstants.MediaCategory, $"{media.Category}:{media.Slot}", HashPath(media.Path))).ToArray();
    private static EmulationMediaSlot MediaOrder(MediaConfiguration media) => media.Slot;
    private static StateInputFingerprint InputFingerprint(InputConfiguration input) => new((input.Controllers ?? []).OrderBy(controller => controller.Port).Select(controller => new StateControllerFingerprint(controller.Port,
        controller.Peripheral,
        controller.DeviceId,
        controller.DeadZonePercent,
        EmulationSdk.StateFiles.OrderInputMappings(controller.Mappings).ToArray())).ToArray());
    private static string HashPath(string path) =>
        EmulationSdk.StateFiles.HashPath(path, StateConstants.AllFilesSearchPattern, StateConstants.CanonicalDirectorySeparator, StateConstants.HashBufferSize, () => new FileNotFoundException(ErrorMessages.ContentFileMissing, path));


}
