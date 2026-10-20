using GWGUI.Emulation.Apple.Emulators.Common.Contracts;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Interop;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;

internal static class EnvironmentFunctions
{
    internal static string CreateUnknownCommandDiagnostic(uint command) => string.Format(CultureInfo.InvariantCulture, ExceptionText.Get(EnvironmentConstants.UnknownCommandDiagnosticResourceKey), command);

    internal static IReadOnlyList<InputDescriptor> CopyInputDescriptors(nint data)
        => GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.CopyInputDescriptors<InputDescriptor>(data, EnvironmentConstants.MaximumInputDescriptorCount, static (port, device, index, id, description) => new(port, device, index, id, description));

    internal static IReadOnlyList<ControllerPort> CopyControllerPorts(nint data)
        => GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.CopyControllerPorts<ControllerPort, ControllerDevice>(data, EnvironmentConstants.MaximumControllerPortCount, EnvironmentConstants.MaximumControllerTypeCount, static (description, id) => new(description, id), static devices => new(devices));

    internal static IReadOnlyList<MemoryDescriptor> CopyMemoryMap(nint data)
        => GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.CopyMemoryMap<MemoryDescriptor>(data, EnvironmentConstants.MaximumMemoryDescriptorCount, static (flags, pointer, offset, start, select, disconnect, length, addressSpace) => new(flags, pointer, offset, start, select, disconnect, length, addressSpace));

    internal static string CopyNativeLogTemplate(nint format)
        => GWGUI.Emulation.Functions.ExternalCoreEnvironmentFunctions.CopyNativeLogTemplate(format, EnvironmentConstants.NativeLogArgumentMarker, EnvironmentConstants.NativeLogConversionCharacters);
}
