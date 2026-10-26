using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Functions;

/// <summary>Chargement direct BBC commun aux modèles utilisant le contrat MOS OSFILE et un bus 6502.</summary>
internal static class CassetteFunctions
{
    /// <summary>Charge un fichier validé lors d'OSFILE 255, sans modifier la ROM ni consommer la bande si la RAM ne convient pas.</summary>
    /// <remarks>LOAD et CHAIN utilisent ce contrat. Les chargeurs propriétaires restent exécutés par le matériel.
    /// Le bus et le CPU sont empruntés ; le modèle possède leurs ressources et leur horloge.</remarks>
    internal static bool TryDirectLoad(Cpu6502 cpu, ICpuBus bus, Cassette cassette)
    {
        if (cassette.Settings.Mode != EmulationCassettePlaybackMode.Direct || cpu.PC != 0xffdd || cpu.A != 255) return false;
        var block = cpu.X | cpu.Y << 8;
        var pointer = bus.Read((ushort)block) | bus.Read((ushort)(block + 1)) << 8;
        var name = new System.Text.StringBuilder();
        for (var index = 0; index < 255; index++)
        {
            var value = bus.Read((ushort)(pointer + index));
            if (value == 13) break;
            if (value != '"') name.Append((char)value);
        }
        if (!cassette.TryReadFile(name.ToString().Trim(), out var data, out var load, out var execution, out var nextPosition)) return false;
        var address = bus.Read((ushort)(block + 6)) == 0 ? bus.Read((ushort)(block + 2)) | bus.Read((ushort)(block + 3)) << 8 : (int)(load & 0xffff);
        if (address + data.Length > 0x8000) return false;
        for (var index = 0; index < data.Length; index++) bus.Write((ushort)(address + index), data[index]);
        void Store(int offset, uint value) { for (var index = 0; index < 4; index++) bus.Write((ushort)(block + offset + index), (byte)(value >> (8 * index))); }
        Store(2, load); Store(6, execution); Store(10, (uint)data.Length); Store(14, 0);
        cassette.CompleteDirectRead(nextPosition); cpu.CompleteHostCall(1);
        return true;
    }
}
