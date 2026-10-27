using C = GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants.SidewaysExpansionConstants;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Contracts;

/// <summary>Montage physique sur les supports ROM, indépendant de la RAM principale et de la carte Shadow.</summary>
internal sealed record SidewaysExpansionSettings(int Modules = 0, bool RamRom = false, bool AriesB12 = false, int AriesRamBytes = 0)
{
    /// <summary>Nombre total d'octets RAM effectivement présents.</summary>
    internal int RamBytes => AriesB12 ? AriesRamBytes : RamRom ? Modules * C.RamBytesPerModule : 0;
    /// <summary>Nombre de supports de puces ROM du montage.</summary>
    internal int RomSocketCount => AriesB12 ? C.AriesRomSockets : Modules;
    /// <summary>Capacité RAM effectivement installée sur une banque.</summary>
    internal int RamBankBytes(int bank) => AriesB12 ? bank == 0 ? AriesRamBytes : 0
        : RamRom && bank >= 0 && bank < C.RamBanksPerModule * C.SocketCount && bank % C.SocketCount < Modules ? C.BankBytes : 0;
    /// <summary>Présence physique d'une banque RAM.</summary>
    internal bool IsRamBank(int bank) => RamBankBytes(bank) > 0;
    /// <summary>Présence d'un support ROM correspondant à la banque.</summary>
    internal bool IsChipBank(int bank) => AriesB12 ? bank is >= 1 and <= C.AriesRomSockets
        : bank >= (RamRom ? C.RamRomFirstRomBank : 0) && bank < C.SocketCount * C.SocketCount && bank % C.SocketCount < Modules;
    /// <summary>Support physique contenant une banque ROM.</summary>
    internal int ChipSocket(int bank) => AriesB12 ? bank - 1 : bank % C.SocketCount;
    /// <summary>Position d'une banque dans le contenu binaire de sa puce.</summary>
    internal int ChipGroup(int bank) => AriesB12 ? 0 : bank / C.SocketCount - (RamRom ? C.RamRomFirstRomBank / C.SocketCount : 0);
    /// <summary>Réaffectation des ROM d'origine lorsque les quatre supports sont repris sur B12.</summary>
    internal int MapNativeBank(int bank) => AriesB12 && bank is >= C.NativeFirstBank and < C.NativeFirstBank + C.SocketCount ? bank - C.AriesNativeBankOffset : bank;
    /// <summary>Banques alimentées par une puce physique.</summary>
    internal IEnumerable<int> BanksForSocket(int socket)
    {
        if (AriesB12) { yield return socket + 1; yield break; }
        for (var bank = socket; bank < C.SocketCount * C.SocketCount; bank += C.SocketCount)
            if (IsChipBank(bank)) yield return bank;
    }
}
