using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Functions;

internal static class FirmwareFunctions
{
    internal static string Field(FirmwareCategory role) => FirmwareConstants.FieldPrefix + role;
    internal static bool IsField(string field) => Enum.GetValues<FirmwareCategory>().Any(role => field == Field(role));
    internal static IReadOnlyList<FirmwareCategory> Roles(MachineModel model) { _ = ConfigurationFunctions.Family(model); return [FirmwareCategory.Executive, FirmwareCategory.Graphics]; }
    internal static IReadOnlyList<FirmwareConfiguration> Apply(MachineConfiguration configuration, IReadOnlyDictionary<string,string?> values)
    {
        var result=configuration.Firmwares.ToList();
        foreach(var role in Roles(configuration.Model)) {
            if(!values.TryGetValue(Field(role),out var path)) continue;
            result.RemoveAll(item=>item.Category==role);
            if(!string.IsNullOrWhiteSpace(path)) result.Add(new(role,path,true));
        }
        return result;
    }
    internal static async ValueTask<EmulationFirmwareCandidate> ScanFileAsync(string path,MachineModel model,Emulator core,CancellationToken cancellationToken)
    {
        _=ConfigurationFunctions.Family(model);
        if (!ConfigurationFunctions.IsCompatible(model, core)) throw new ArgumentOutOfRangeException(nameof(core));
        var absolute=Path.GetFullPath(path);
        string? hash=null, destination=null;
        var compatibility=EmulationFirmwareCompatibility.Incompatible;
        try {
            var length=new FileInfo(absolute).Length;
            if(length==FirmwareConstants.ExecutiveBytes || length==FirmwareConstants.GraphicsBytes) {
                var bytes=await File.ReadAllBytesAsync(absolute,cancellationToken).ConfigureAwait(false);
                hash=Convert.ToHexString(SHA256.HashData(bytes));
                var md5=Convert.ToHexString(MD5.HashData(bytes));
                if(length==FirmwareConstants.ExecutiveBytes && md5.Equals(FirmwareConstants.ExecutiveMd5,StringComparison.OrdinalIgnoreCase)) destination=Field(FirmwareCategory.Executive);
                if(length==FirmwareConstants.GraphicsBytes && md5.Equals(FirmwareConstants.GraphicsMd5,StringComparison.OrdinalIgnoreCase)) destination=Field(FirmwareCategory.Graphics);
                if(destination is not null) compatibility=EmulationFirmwareCompatibility.Compatible;
            }
        } catch(IOException) { } catch(UnauthorizedAccessException) { }
        return new(hash??absolute,absolute,Path.GetFileName(absolute),null,compatibility,destination);
    }
    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate)
    {
        if(candidate.Compatibility!=EmulationFirmwareCompatibility.Compatible || !IsField(candidate.DestinationFieldId??string.Empty))
            throw new EmulationException(ErrorCategory.Firmware,ErrorCode.FirmwareInvalid,ErrorMessages.FirmwareCannotBeSelected);
        var role=Enum.GetValues<FirmwareCategory>().Single(role=>Field(role)==candidate.DestinationFieldId);
        return new(role,candidate.Path,true,true);
    }
}
