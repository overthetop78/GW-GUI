using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace GWGUI.Emulation.Atari.Common.Functions;

internal static class EnvironmentFunctions
{
    internal static string CreateUnknownCommandDiagnostic(uint command) => string.Format(
        CultureInfo.InvariantCulture, EnvironmentConstants.UnknownCommandDiagnosticFormat, command);

    internal static IReadOnlyList<InputDescriptor> CopyInputDescriptors(nint data)
    {
        if (data == nint.Zero) return [];
        var result = new List<InputDescriptor>();
        var nativeSize = Marshal.SizeOf<ExternalCoreApi.InputDescriptor>();
        for (var index = CommonConstants.FirstCollectionIndex;
             index < EnvironmentConstants.MaximumInputDescriptorCount; index++)
        {
            var native = Marshal.PtrToStructure<ExternalCoreApi.InputDescriptor>(data + index * nativeSize);
            if (native.Description == nint.Zero) return result;
            result.Add(new(native.Port, native.Device, native.Index, native.Id,
                Marshal.PtrToStringUTF8(native.Description) ?? string.Empty));
        }
        return result;
    }

    internal static IReadOnlyList<ControllerPort> CopyControllerPorts(nint data)
    {
        if (data == nint.Zero) return [];
        var ports = new List<ControllerPort>();
        var portSize = Marshal.SizeOf<ExternalCoreApi.ControllerInfo>();
        var deviceSize = Marshal.SizeOf<ExternalCoreApi.ControllerDescription>();
        for (var portIndex = CommonConstants.FirstCollectionIndex;
             portIndex < EnvironmentConstants.MaximumControllerPortCount; portIndex++)
        {
            var port = Marshal.PtrToStructure<ExternalCoreApi.ControllerInfo>(data + portIndex * portSize);
            if (port.Types == nint.Zero || port.Count == CommonConstants.EmptyNativeCollectionCount) return ports;
            var devices = new List<ControllerDevice>();
            var count = Math.Min(port.Count, (uint)EnvironmentConstants.MaximumControllerTypeCount);
            for (var deviceIndex = CommonConstants.FirstCollectionIndex; deviceIndex < count; deviceIndex++)
            {
                var device = Marshal.PtrToStructure<ExternalCoreApi.ControllerDescription>(
                    port.Types + checked((int)deviceIndex) * deviceSize);
                devices.Add(new(Marshal.PtrToStringUTF8(device.Description) ?? string.Empty, device.Id));
            }
            ports.Add(new(devices));
        }
        return ports;
    }

    internal static IReadOnlyList<MemoryDescriptor> CopyMemoryMap(nint data)
    {
        if (data == nint.Zero) return [];
        var map = Marshal.PtrToStructure<ExternalCoreApi.MemoryMap>(data);
        if (map.Descriptors == nint.Zero || map.Count == CommonConstants.EmptyNativeCollectionCount) return [];
        var result = new List<MemoryDescriptor>();
        var count = Math.Min(map.Count, (uint)EnvironmentConstants.MaximumMemoryDescriptorCount);
        var size = Marshal.SizeOf<ExternalCoreApi.MemoryDescriptor>();
        for (var index = CommonConstants.FirstCollectionIndex; index < count; index++)
        {
            var item = Marshal.PtrToStructure<ExternalCoreApi.MemoryDescriptor>(
                map.Descriptors + checked((int)index) * size);
            result.Add(new(item.Flags, item.Pointer, item.Offset, item.Start, item.Select, item.Disconnect,
                item.Length, Marshal.PtrToStringUTF8(item.AddressSpace)));
        }
        return result;
    }

    internal static uint CurrentLanguage() => (uint)(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
    {
        EnvironmentFunctionsConstants.Ja => EnvironmentLanguage.Japanese, EnvironmentFunctionsConstants.Fr => EnvironmentLanguage.French,
        EnvironmentFunctionsConstants.Es => EnvironmentLanguage.Spanish, EnvironmentFunctionsConstants.De => EnvironmentLanguage.German,
        EnvironmentFunctionsConstants.It => EnvironmentLanguage.Italian, EnvironmentFunctionsConstants.Nl => EnvironmentLanguage.Dutch,
        EnvironmentFunctionsConstants.Pt when string.Equals(CultureInfo.CurrentUICulture.Name, EnvironmentFunctionsConstants.PtBR, StringComparison.OrdinalIgnoreCase)
            => EnvironmentLanguage.PortugueseBrazil,
        EnvironmentFunctionsConstants.Pt => EnvironmentLanguage.PortuguesePortugal, EnvironmentFunctionsConstants.Ru => EnvironmentLanguage.Russian,
        EnvironmentFunctionsConstants.Ko => EnvironmentLanguage.Korean,
        EnvironmentFunctionsConstants.Zh when CultureInfo.CurrentUICulture.Name.Contains(EnvironmentFunctionsConstants.Hant, StringComparison.OrdinalIgnoreCase)
            => EnvironmentLanguage.ChineseTraditional,
        EnvironmentFunctionsConstants.Zh => EnvironmentLanguage.ChineseSimplified, EnvironmentFunctionsConstants.Pl => EnvironmentLanguage.Polish,
        EnvironmentFunctionsConstants.Vi => EnvironmentLanguage.Vietnamese, EnvironmentFunctionsConstants.Ar => EnvironmentLanguage.Arabic,
        EnvironmentFunctionsConstants.El => EnvironmentLanguage.Greek, EnvironmentFunctionsConstants.Tr => EnvironmentLanguage.Turkish,
        EnvironmentFunctionsConstants.He => EnvironmentLanguage.Hebrew, EnvironmentFunctionsConstants.Fi => EnvironmentLanguage.Finnish,
        EnvironmentFunctionsConstants.Id => EnvironmentLanguage.Indonesian, EnvironmentFunctionsConstants.Sv => EnvironmentLanguage.Swedish,
        EnvironmentFunctionsConstants.Uk => EnvironmentLanguage.Ukrainian, EnvironmentFunctionsConstants.Cs => EnvironmentLanguage.Czech,
        EnvironmentFunctionsConstants.Hu => EnvironmentLanguage.Hungarian, EnvironmentFunctionsConstants.Nb => EnvironmentLanguage.Norwegian,
        EnvironmentFunctionsConstants.Th => EnvironmentLanguage.Thai,
        _ => EnvironmentLanguage.English
    });

    internal static string CopyNativeLogTemplate(nint format)
    {
        var source = Marshal.PtrToStringUTF8(format)?.Trim();
        if (string.IsNullOrEmpty(source)) return string.Empty;
        var result = new StringBuilder(source.Length);
        for (var index = CommonConstants.FirstCollectionIndex; index < source.Length; index++)
        {
            var character = source[index];
            if (character != '%')
            {
                result.Append(character);
                continue;
            }
            if (index + EnvironmentConstants.NextCharacterOffset < source.Length &&
                source[index + EnvironmentConstants.NextCharacterOffset] == '%')
            {
                result.Append('%');
                index++;
                continue;
            }
            result.Append(EnvironmentConstants.NativeLogArgumentMarker);
            while (index + EnvironmentConstants.NextCharacterOffset < source.Length)
            {
                index++;
                if (EnvironmentConstants.NativeLogConversionCharacters.Contains(source[index], StringComparison.Ordinal))
                    break;
            }
        }
        return result.ToString();
    }
}
