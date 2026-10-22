using CommonCoreHostFunctions = GWGUI.Emulation.Bandai.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Bandai.Emulators.Common.Contracts;
using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Contracts;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace GWGUI.Emulation.Bandai.Emulators.Common.Interop.Functions;

internal static class EnvironmentFunctions
{
    internal static string CreateUnknownCommandDiagnostic(uint command) => string.Format(CultureInfo.InvariantCulture, ExceptionText.Get(EnvironmentConstants.UnknownCommandDiagnosticResourceKey), command);

    internal static IReadOnlyList<InputDescriptor> CopyInputDescriptors(nint data)
        => CommonCoreHostFunctions.EnvironmentProcessing.CopyInputDescriptors<InputDescriptor>(data, EnvironmentConstants.MaximumInputDescriptorCount, static (port, device, index, id, description) => new(port, device, index, id, description));

    internal static IReadOnlyList<ControllerPort> CopyControllerPorts(nint data)
        => CommonCoreHostFunctions.EnvironmentProcessing.CopyControllerPorts<ControllerPort, ControllerDevice>(data, EnvironmentConstants.MaximumControllerPortCount, EnvironmentConstants.MaximumControllerTypeCount, static (description, id) => new(description, id), static devices => new(devices));

    internal static IReadOnlyList<MemoryDescriptor> CopyMemoryMap(nint data)
        => CommonCoreHostFunctions.EnvironmentProcessing.CopyMemoryMap<MemoryDescriptor>(data, EnvironmentConstants.MaximumMemoryDescriptorCount, static (flags, pointer, offset, start, select, disconnect, length, addressSpace) => new(flags, pointer, offset, start, select, disconnect, length, addressSpace));

    internal static string CopyNativeLogTemplate(nint format)
        => CommonCoreHostFunctions.EnvironmentProcessing.CopyNativeLogTemplate(format, EnvironmentConstants.NativeLogArgumentMarker, EnvironmentConstants.NativeLogConversionCharacters);
}
