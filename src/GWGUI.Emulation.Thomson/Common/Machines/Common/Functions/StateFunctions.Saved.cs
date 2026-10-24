using CommonCoreHostFunctions = GWGUI.Emulation.Thomson.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Functions;
internal static class SavedStateFunctions
{
    internal static SavedStateHeader CreateHeader(MachineConfiguration configuration,
        IEmulatorCore core,
        ReadOnlySpan<byte> state) => new(StateConstants.CurrentFormatVersion, core.Emulator, core.CoreName, core.CoreVersion, core.CoreSha256, configuration.Model, ConfigurationHash(configuration), ContentHash(configuration), StateStore.HashBytes(state));
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
    private static IReadOnlyList<StateContentEntry> ContentEntries(MachineConfiguration configuration)
    {
        IReadOnlyList<string> initialPaths = configuration.InitialContent is null ? [] : new Engine().SelectedAdapter(configuration)?.ResolveInitialContentPaths(configuration) ?? [configuration.InitialContent];
        var entries = configuration.Media.OrderBy(MediaOrder).ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase).Select(media => new StateContentEntry(StateConstants.MediaCategory, $"{media.Category}:{media.Slot}", StateStore.HashPath(media.Path))).Concat(initialPaths.Select((path,
            index) => new StateContentEntry(StateConstants.InitialContentCategory, index.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + Path.GetFileName(path), StateStore.HashPath(path)))).ToArray();
        return entries;
    }

    private static EmulationMediaSlot MediaOrder(MediaConfiguration media) => media.Slot;
    private static StateInputFingerprint InputFingerprint(InputConfiguration input) => new(
        EmulationSdk.StateFiles.OrderInputMappings(input.Keyboard).ToArray(),
        (input.Controllers ?? []).OrderBy(controller => controller.Port).Select(controller => new StateControllerFingerprint(controller.Port,
            controller.Peripheral,
            controller.DeviceId,
            controller.DeadZonePercent,
            EmulationSdk.StateFiles.OrderInputMappings(controller.Mappings).ToArray())).ToArray(),
        EmulationSdk.StateFiles.OrderInputMappings(input.Mouse).ToArray(),
        input.MouseSpeedPercent
    );



}
