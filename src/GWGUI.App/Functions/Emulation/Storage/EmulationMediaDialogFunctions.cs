using System.Security.Cryptography;
using System.Text;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Emulation.Storage;

internal static class EmulationMediaDialogFunctions
{
    internal static Guid ClientGuid(string moduleId, string machineId, EmulationMediaSlot slot)
    {
        var key = $"GW GUI\0Emulation media\0{moduleId}\0{machineId}\0{slot}";
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(key), hash);
        return new Guid(hash[..16]);
    }
}
