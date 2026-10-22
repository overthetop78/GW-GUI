using CommonCoreHostFunctions = GWGUI.Emulation.Sharp.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Contracts;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;
internal static class EnvironmentFunctions
{
    internal static string CreateUnknownCommandDiagnostic(uint command) => string.Format(CultureInfo.InvariantCulture, ExceptionText.Get(EnvironmentConstants.UnknownCommandDiagnosticResourceKey), command);
    internal static IReadOnlyList<InputDescriptor> CopyInputDescriptors(nint data, int maximumCount = EnvironmentConstants.MaximumInputDescriptorCount)
        => CommonCoreHostFunctions.EnvironmentProcessing.CopyInputDescriptors<InputDescriptor>(data, maximumCount, static (port, device, index, id, description) => new(port, device, index, id, description));

    internal static IReadOnlyList<ControllerPort> CopyControllerPorts(nint data)
        => CommonCoreHostFunctions.EnvironmentProcessing.CopyControllerPorts<ControllerPort, ControllerDevice>(data, EnvironmentConstants.MaximumControllerPortCount, EnvironmentConstants.MaximumControllerTypeCount, static (description, id) => new(description, id), static devices => new(devices));

    internal static IReadOnlyList<MemoryDescriptor> CopyMemoryMap(nint data)
        => CommonCoreHostFunctions.EnvironmentProcessing.CopyMemoryMap<MemoryDescriptor>(data, EnvironmentConstants.MaximumMemoryDescriptorCount, static (flags, pointer, offset, start, select, disconnect, length, addressSpace) => new(flags, pointer, offset, start, select, disconnect, length, addressSpace));
    internal static string CopyNativeLogTemplate(nint format)
        => CommonCoreHostFunctions.EnvironmentProcessing.CopyNativeLogTemplate(format, EnvironmentConstants.NativeLogArgumentMarker, EnvironmentConstants.NativeLogConversionCharacters);
}