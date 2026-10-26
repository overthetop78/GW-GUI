using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Interfaces;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

/// <summary>CPU NMOS 6502 : registres et accès au bus, sans dépendance envers un modèle BBC.</summary>
/// <remarks>Les instructions officielles sont décodées dans le fichier associé. Les cycles internes font également avancer
/// le bus. Ce premier moteur n'est pas encore une reproduction de toutes les lectures transitoires du circuit NMOS.</remarks>
internal sealed partial class Cpu6502(ICpuBus bus)
{
    private const byte Carry = 1, Zero = 2, InterruptDisable = 4, Decimal = 8, Break = 16, Unused = 32, Overflow = 64, Negative = 128;

    /// <summary>Registres du CPU ; la pile matérielle occupe la page 01 du bus.</summary>
    internal byte A { get; private set; }
    internal byte X { get; private set; }
    internal byte Y { get; private set; }
    internal byte S { get; private set; } = 0xfd;
    internal byte P { get; private set; } = InterruptDisable | Unused;
    internal ushort PC { get; private set; }
    internal long Cycles { get; private set; }
    /// <summary>Termine un appel MOS traité explicitement par l'assemblage de la machine, comme un RTS matériel.</summary>
    /// <remarks>Le CPU ne décide pas quel appel intercepter. Le modèle choisit le contrat MOS et les données à charger.</remarks>
    internal void CompleteHostCall(byte result)
    {
        A = result; P &= unchecked((byte)~Carry);
        var start = Cycles; var low = Pop(); PC = (ushort)((low | Pop() << 8) + 1); Pad(start, 6);
    }

    /// <summary>Applique RESET et lit le vecteur FFFC/FFFD. Le bus et les périphériques sont réinitialisés par leur propriétaire.</summary>
    internal void Reset()
    {
        _nmiPending = false; _nmiLevel = bus.Nmi;
        S = 0xfd;
        P = (byte)(P | InterruptDisable | Unused);
        for (var i = 0; i < 5; i++) Tick();
        PC = ReadWord(0xfffc);
    }

    /// <summary>Exécute une instruction, une NMI ou une IRQ et renvoie au planificateur de la machine.</summary>
    internal void Step()
    {
        var start = Cycles;
        if (_nmiPending)
        {
            _nmiPending = false;
            EnterInterrupt(start, 0xfffa);
            return;
        }
        if (bus.Irq && (P & InterruptDisable) == 0)
        {
            EnterInterrupt(start, 0xfffe);
            return;
        }
        var opcode = Fetch();
        var expected = Execute(opcode);
        Pad(start, expected);
        P |= Unused;
    }

    private void Tick() { bus.Tick(); Cycles++; SampleNmi(); }
    private byte Read(ushort address) { Tick(); return bus.Read(address); }
    private void Write(ushort address, byte value) { Tick(); bus.Write(address, value); }
    private byte Fetch() => Read(PC++);
    private ushort FetchWord() { var low = Fetch(); return (ushort)(low | Fetch() << 8); }
    private ushort ReadWord(ushort address) { var low = Read(address); return (ushort)(low | Read((ushort)(address + 1)) << 8); }
    private ushort ZeroPageWord(byte address) { var low = Read(address); return (ushort)(low | Read((byte)(address + 1)) << 8); }
    private void Push(byte value) => Write((ushort)(0x100 | S--), value);
    private byte Pop() => Read((ushort)(0x100 | ++S));
    private void Pad(long start, int length) { while (Cycles - start < length) Tick(); }
    private void Flag(byte flag, bool set) => P = set ? (byte)(P | flag) : (byte)(P & ~flag);
    private byte Nz(byte value) { Flag(Zero, value == 0); Flag(Negative, (value & 0x80) != 0); return value; }
    private void Compare(byte left, byte right) { Flag(Carry, left >= right); Nz((byte)(left - right)); }

    /// <summary>ADC NMOS : les indicateurs ne suivent pas tous le résultat décimal corrigé.</summary>
    private void Add(byte value)
    {
        var c = P & Carry;
        var binary = A + value + c;
        if ((P & Decimal) != 0)
        {
            var low = (A & 15) + (value & 15) + c;
            if (low > 9) low += 6;
            var sum = (A & 0xf0) + (value & 0xf0) + (low & 15) + (low > 15 ? 16 : 0);
            Flag(Zero, (byte)binary == 0);
            Flag(Negative, (sum & 0x80) != 0);
            Flag(Overflow, (~(A ^ value) & (A ^ sum) & 0x80) != 0);
            if (sum > 0x99) sum += 0x60;
            Flag(Carry, sum > 255);
            A = (byte)sum;
        }
        else
        {
            Flag(Overflow, (~(A ^ value) & (A ^ binary) & 0x80) != 0);
            Flag(Carry, binary > 255);
            A = Nz((byte)binary);
        }
    }

    /// <summary>SBC conserve les indicateurs de la soustraction binaire en mode décimal NMOS.</summary>
    private void Subtract(byte value)
    {
        var borrow = (P & Carry) == 0 ? 1 : 0;
        var binary = A - value - borrow;
        Flag(Overflow, ((A ^ value) & (A ^ binary) & 0x80) != 0);
        Flag(Carry, binary >= 0);
        Nz((byte)binary);
        if ((P & Decimal) != 0)
        {
            var low = (A & 15) - (value & 15) - borrow;
            var high = (A >> 4) - (value >> 4);
            if (low < 0) { low -= 6; high--; }
            if (high < 0) high -= 6;
            A = (byte)((high << 4) | (low & 15));
        }
        else A = (byte)binary;
    }
}
