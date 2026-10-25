namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Services;

internal sealed partial class Cpu6502
{
    /// <summary>Décode les instructions officielles, en réunissant les familles ayant le même adressage.</summary>
    private int Execute(byte opcode)
    {
        // Ces instructions occupent les positions irrégulières du groupe des décalages/chargements.
        switch (opcode)
        {
            case 0x8a: A = Nz(X); return 2;
            case 0x9a: S = X; return 2;
            case 0xaa: X = Nz(A); return 2;
            case 0xba: X = Nz(S); return 2;
            case 0xca: X = Nz((byte)(X - 1)); return 2;
            case 0xea: return 2;
        }
        if ((opcode & 3) == 1) return GroupOne(opcode);
        if ((opcode & 3) == 2) return GroupTwo(opcode);
        if ((opcode & 0x1f) == 0x10)
        {
            var test = (opcode >> 6) switch { 0 => Negative, 1 => Overflow, 2 => Carry, _ => Zero };
            var displacement = (sbyte)Fetch();
            if (((P & test) != 0) != ((opcode & 0x20) != 0)) return 2;
            var previous = PC;
            PC = (ushort)(PC + displacement);
            return (previous & 0xff00) == (PC & 0xff00) ? 3 : 4;
        }
        switch (opcode)
        {
            case 0x00:
                Fetch(); Push((byte)(PC >> 8)); Push((byte)PC); Push((byte)(P | Break | Unused)); P |= InterruptDisable; PC = ReadWord(0xfffe); return 7;
            case 0x08: Push((byte)(P | Break | Unused)); return 3;
            case 0x28: P = (byte)((Pop() & ~Break) | Unused); return 4;
            case 0x48: Push(A); return 3;
            case 0x68: A = Nz(Pop()); return 4;
            case 0x18: P &= unchecked((byte)~Carry); return 2;
            case 0x38: P |= Carry; return 2;
            case 0x58: P &= unchecked((byte)~InterruptDisable); return 2;
            case 0x78: P |= InterruptDisable; return 2;
            case 0xb8: P &= unchecked((byte)~Overflow); return 2;
            case 0xd8: P &= unchecked((byte)~Decimal); return 2;
            case 0xf8: P |= Decimal; return 2;
            case 0x20:
                var target = FetchWord(); var last = (ushort)(PC - 1); Push((byte)(last >> 8)); Push((byte)last); PC = target; return 6;
            case 0x40: P = (byte)((Pop() & ~Break) | Unused); var lo = Pop(); PC = (ushort)(lo | Pop() << 8); return 6;
            case 0x60: var low = Pop(); PC = (ushort)((low | Pop() << 8) + 1); return 6;
            case 0x4c: PC = FetchWord(); return 3;
            case 0x6c:
                var indirect = FetchWord(); var l = Read(indirect); PC = (ushort)(l | Read((ushort)((indirect & 0xff00) | (byte)(indirect + 1))) << 8); return 5;
            case 0x24: Bit(Read(Fetch())); return 3;
            case 0x2c: Bit(Read(FetchWord())); return 4;
            case 0x84: Write(Fetch(), Y); return 3;
            case 0x94: Write((byte)(Fetch() + X), Y); return 4;
            case 0x8c: Write(FetchWord(), Y); return 4;
            case 0xa0: Y = Nz(Fetch()); return 2;
            case 0xa4: Y = Nz(Read(Fetch())); return 3;
            case 0xb4: Y = Nz(Read((byte)(Fetch() + X))); return 4;
            case 0xac: Y = Nz(Read(FetchWord())); return 4;
            case 0xbc: var address = FetchWord(); Y = Nz(Read((ushort)(address + X))); return 4 + PageCross(address, X);
            case 0xc0: Compare(Y, Fetch()); return 2;
            case 0xc4: Compare(Y, Read(Fetch())); return 3;
            case 0xcc: Compare(Y, Read(FetchWord())); return 4;
            case 0xe0: Compare(X, Fetch()); return 2;
            case 0xe4: Compare(X, Read(Fetch())); return 3;
            case 0xec: Compare(X, Read(FetchWord())); return 4;
            case 0x88: Y = Nz((byte)(Y - 1)); return 2;
            case 0xc8: Y = Nz((byte)(Y + 1)); return 2;
            case 0xe8: X = Nz((byte)(X + 1)); return 2;
            case 0x98: A = Nz(Y); return 2;
            case 0xa8: Y = Nz(A); return 2;
            default: throw new InvalidOperationException($"6502: opcode {opcode:X2} non pris en charge à {(ushort)(PC - 1):X4}.");
        }
    }

    private static int PageCross(ushort address, byte index) => (address & 0xff00) == ((address + index) & 0xff00) ? 0 : 1;
    private void Bit(byte value) { Flag(Zero, (A & value) == 0); P = (byte)((P & 0x3f) | (value & 0xc0)); }

    /// <summary>ORA, AND, EOR, ADC, STA, LDA, CMP et SBC partagent huit formes d'adressage.</summary>
    private int GroupOne(byte opcode)
    {
        var operation = opcode >> 5;
        var mode = (opcode >> 2) & 7;
        ushort address;
        var extra = 0;
        var cycles = 0;
        switch (mode)
        {
            case 0: address = ZeroPageWord((byte)(Fetch() + X)); cycles = 6; break;
            case 1: address = Fetch(); cycles = 3; break;
            case 2:
                if (operation == 4) throw new InvalidOperationException("6502: STA immédiat invalide.");
                address = PC++; cycles = 2; break;
            case 3: address = FetchWord(); cycles = 4; break;
            case 4: var pointer = ZeroPageWord(Fetch()); extra = PageCross(pointer, Y); address = (ushort)(pointer + Y); cycles = operation == 4 ? 6 : 5; break;
            case 5: address = (byte)(Fetch() + X); cycles = 4; break;
            case 6: var ay = FetchWord(); extra = PageCross(ay, Y); address = (ushort)(ay + Y); cycles = operation == 4 ? 5 : 4; break;
            default: var ax = FetchWord(); extra = PageCross(ax, X); address = (ushort)(ax + X); cycles = operation == 4 ? 5 : 4; break;
        }
        if (operation == 4) { Write(address, A); return cycles; }
        var value = Read(address);
        switch (operation)
        {
            case 0: A = Nz((byte)(A | value)); break;
            case 1: A = Nz((byte)(A & value)); break;
            case 2: A = Nz((byte)(A ^ value)); break;
            case 3: Add(value); break;
            case 5: A = Nz(value); break;
            case 6: Compare(A, value); break;
            case 7: Subtract(value); break;
        }
        return cycles + extra;
    }

    /// <summary>Décalages, rotations, STX/LDX et décrémentations/incrémentations mémoire.</summary>
    private int GroupTwo(byte opcode)
    {
        var operation = opcode >> 5;
        var mode = (opcode >> 2) & 7;
        if (mode == 0 && operation == 5) { X = Nz(Fetch()); return 2; }
        var accumulator = mode == 2 && operation < 4;
        ushort address = 0;
        var cycles = 2;
        var extra = 0;
        if (!accumulator)
        {
            switch (mode)
            {
                case 1: address = Fetch(); cycles = operation is 4 or 5 ? 3 : 5; break;
                case 3: address = FetchWord(); cycles = operation is 4 or 5 ? 4 : 6; break;
                case 5: address = (byte)(Fetch() + (operation is 4 or 5 ? Y : X)); cycles = operation is 4 or 5 ? 4 : 6; break;
                case 7 when operation != 4: var a = FetchWord(); var index = operation == 5 ? Y : X; address = (ushort)(a + index); extra = operation == 5 ? PageCross(a, index) : 0; cycles = operation == 5 ? 4 : 7; break;
                default: throw new InvalidOperationException($"6502: opcode {opcode:X2} non pris en charge.");
            }
        }
        if (operation == 4) { Write(address, X); return cycles; }
        var old = accumulator ? A : Read(address);
        if (operation == 5) { X = Nz(old); return cycles + extra; }
        var c = P & Carry;
        byte result;
        switch (operation)
        {
            case 0: Flag(Carry, (old & 128) != 0); result = (byte)(old << 1); break;
            case 1: Flag(Carry, (old & 128) != 0); result = (byte)((old << 1) | c); break;
            case 2: Flag(Carry, (old & 1) != 0); result = (byte)(old >> 1); break;
            case 3: Flag(Carry, (old & 1) != 0); result = (byte)((old >> 1) | (c << 7)); break;
            case 6: result = (byte)(old - 1); break;
            default: result = (byte)(old + 1); break;
        }
        result = Nz(result);
        if (accumulator) A = result;
        else { Write(address, old); Write(address, result); }
        return cycles;
    }
}
