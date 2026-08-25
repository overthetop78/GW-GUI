using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace GWGUI.Emulation.Atari;

internal static class AtariEnvironmentFunctions
{
    internal static string CreateUnknownCommandDiagnostic(uint command) => string.Format(
        CultureInfo.InvariantCulture, AtariEnvironmentConstants.UnknownCommandDiagnosticFormat, command);

    internal static IReadOnlyList<AtariInputDescriptor> CopyInputDescriptors(nint data)
    {
        if (data == nint.Zero) return [];
        var result = new List<AtariInputDescriptor>();
        var nativeSize = Marshal.SizeOf<ExternalCoreApi.InputDescriptor>();
        for (var index = AtariConstants.FirstCollectionIndex;
             index < AtariEnvironmentConstants.MaximumInputDescriptorCount; index++)
        {
            var native = Marshal.PtrToStructure<ExternalCoreApi.InputDescriptor>(data + index * nativeSize);
            if (native.Description == nint.Zero) return result;
            result.Add(new(native.Port, native.Device, native.Index, native.Id,
                Marshal.PtrToStringUTF8(native.Description) ?? string.Empty));
        }
        return result;
    }

    internal static IReadOnlyList<AtariControllerPort> CopyControllerPorts(nint data)
    {
        if (data == nint.Zero) return [];
        var ports = new List<AtariControllerPort>();
        var portSize = Marshal.SizeOf<ExternalCoreApi.ControllerInfo>();
        var deviceSize = Marshal.SizeOf<ExternalCoreApi.ControllerDescription>();
        for (var portIndex = AtariConstants.FirstCollectionIndex;
             portIndex < AtariEnvironmentConstants.MaximumControllerPortCount; portIndex++)
        {
            var port = Marshal.PtrToStructure<ExternalCoreApi.ControllerInfo>(data + portIndex * portSize);
            if (port.Types == nint.Zero || port.Count == AtariConstants.EmptyNativeCollectionCount) return ports;
            var devices = new List<AtariControllerDevice>();
            var count = Math.Min(port.Count, (uint)AtariEnvironmentConstants.MaximumControllerTypeCount);
            for (var deviceIndex = AtariConstants.FirstCollectionIndex; deviceIndex < count; deviceIndex++)
            {
                var device = Marshal.PtrToStructure<ExternalCoreApi.ControllerDescription>(
                    port.Types + checked((int)deviceIndex) * deviceSize);
                devices.Add(new(Marshal.PtrToStringUTF8(device.Description) ?? string.Empty, device.Id));
            }
            ports.Add(new(devices));
        }
        return ports;
    }

    internal static IReadOnlyList<AtariMemoryDescriptor> CopyMemoryMap(nint data)
    {
        if (data == nint.Zero) return [];
        var map = Marshal.PtrToStructure<ExternalCoreApi.MemoryMap>(data);
        if (map.Descriptors == nint.Zero || map.Count == AtariConstants.EmptyNativeCollectionCount) return [];
        var result = new List<AtariMemoryDescriptor>();
        var count = Math.Min(map.Count, (uint)AtariEnvironmentConstants.MaximumMemoryDescriptorCount);
        var size = Marshal.SizeOf<ExternalCoreApi.MemoryDescriptor>();
        for (var index = AtariConstants.FirstCollectionIndex; index < count; index++)
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
        "ja" => AtariEnvironmentLanguage.Japanese, "fr" => AtariEnvironmentLanguage.French,
        "es" => AtariEnvironmentLanguage.Spanish, "de" => AtariEnvironmentLanguage.German,
        "it" => AtariEnvironmentLanguage.Italian, "nl" => AtariEnvironmentLanguage.Dutch,
        "pt" when string.Equals(CultureInfo.CurrentUICulture.Name, "pt-BR", StringComparison.OrdinalIgnoreCase)
            => AtariEnvironmentLanguage.PortugueseBrazil,
        "pt" => AtariEnvironmentLanguage.PortuguesePortugal, "ru" => AtariEnvironmentLanguage.Russian,
        "ko" => AtariEnvironmentLanguage.Korean,
        "zh" when CultureInfo.CurrentUICulture.Name.Contains("Hant", StringComparison.OrdinalIgnoreCase)
            => AtariEnvironmentLanguage.ChineseTraditional,
        "zh" => AtariEnvironmentLanguage.ChineseSimplified, "pl" => AtariEnvironmentLanguage.Polish,
        "vi" => AtariEnvironmentLanguage.Vietnamese, "ar" => AtariEnvironmentLanguage.Arabic,
        "el" => AtariEnvironmentLanguage.Greek, "tr" => AtariEnvironmentLanguage.Turkish,
        "he" => AtariEnvironmentLanguage.Hebrew, "fi" => AtariEnvironmentLanguage.Finnish,
        "id" => AtariEnvironmentLanguage.Indonesian, "sv" => AtariEnvironmentLanguage.Swedish,
        "uk" => AtariEnvironmentLanguage.Ukrainian, "cs" => AtariEnvironmentLanguage.Czech,
        "hu" => AtariEnvironmentLanguage.Hungarian, "nb" => AtariEnvironmentLanguage.Norwegian,
        "th" => AtariEnvironmentLanguage.Thai,
        _ => AtariEnvironmentLanguage.English
    });

    internal static string CopyNativeLogTemplate(nint format)
    {
        var source = Marshal.PtrToStringUTF8(format)?.Trim();
        if (string.IsNullOrEmpty(source)) return string.Empty;
        var result = new StringBuilder(source.Length);
        for (var index = AtariConstants.FirstCollectionIndex; index < source.Length; index++)
        {
            var character = source[index];
            if (character != '%')
            {
                result.Append(character);
                continue;
            }
            if (index + AtariEnvironmentConstants.NextCharacterOffset < source.Length &&
                source[index + AtariEnvironmentConstants.NextCharacterOffset] == '%')
            {
                result.Append('%');
                index++;
                continue;
            }
            result.Append(AtariEnvironmentConstants.NativeLogArgumentMarker);
            while (index + AtariEnvironmentConstants.NextCharacterOffset < source.Length)
            {
                index++;
                if (AtariEnvironmentConstants.NativeLogConversionCharacters.Contains(source[index], StringComparison.Ordinal))
                    break;
            }
        }
        return result.ToString();
    }
}
