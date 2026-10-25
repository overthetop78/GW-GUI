namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries;

/// <summary>Matrice BBC A/B : nibble haut = ligne, nibble bas = colonne ; les identités viennent du contrat d'entrée existant.</summary>
/// <remarks>Comparaison : MAME bbc_kbd.cpp, révision 84981cc15cfc1429e48f098d80c6dd97bc82a0bc. Aucun code de touche Windows n'est conservé.</remarks>
internal static class KeyboardDictionary
{
    internal static IReadOnlyDictionary<EmulationKey, byte> Matrix { get; } = new Dictionary<EmulationKey, byte>
    {
        [EmulationKey.LeftShift] = 0x00, [EmulationKey.RightShift] = 0x00, [EmulationKey.LeftControl] = 0x01, [EmulationKey.RightControl] = 0x01,
        [EmulationKey.Q] = 0x10, [EmulationKey.AcornF0] = 0x20, [EmulationKey.D1] = 0x30, [EmulationKey.CapsLock] = 0x40,
        [EmulationKey.AcornShiftLock] = 0x50, [EmulationKey.Tab] = 0x60, [EmulationKey.Escape] = 0x70,
        [EmulationKey.D3] = 0x11, [EmulationKey.W] = 0x21, [EmulationKey.D2] = 0x31, [EmulationKey.A] = 0x41,
        [EmulationKey.S] = 0x51, [EmulationKey.Z] = 0x61, [EmulationKey.F1] = 0x71,
        [EmulationKey.D4] = 0x12, [EmulationKey.E] = 0x22, [EmulationKey.D] = 0x32, [EmulationKey.X] = 0x42,
        [EmulationKey.C] = 0x52, [EmulationKey.Space] = 0x62, [EmulationKey.F2] = 0x72,
        [EmulationKey.D5] = 0x13, [EmulationKey.T] = 0x23, [EmulationKey.R] = 0x33, [EmulationKey.F] = 0x43,
        [EmulationKey.G] = 0x53, [EmulationKey.V] = 0x63, [EmulationKey.F3] = 0x73,
        [EmulationKey.F4] = 0x14, [EmulationKey.D7] = 0x24, [EmulationKey.D6] = 0x34, [EmulationKey.Y] = 0x44,
        [EmulationKey.H] = 0x54, [EmulationKey.B] = 0x64, [EmulationKey.F5] = 0x74,
        [EmulationKey.D8] = 0x15, [EmulationKey.I] = 0x25, [EmulationKey.U] = 0x35, [EmulationKey.J] = 0x45,
        [EmulationKey.N] = 0x55, [EmulationKey.M] = 0x65, [EmulationKey.F6] = 0x75,
        [EmulationKey.F7] = 0x16, [EmulationKey.D9] = 0x26, [EmulationKey.O] = 0x36, [EmulationKey.K] = 0x46,
        [EmulationKey.L] = 0x56, [EmulationKey.Comma] = 0x66, [EmulationKey.F8] = 0x76,
        [EmulationKey.Minus] = 0x17, [EmulationKey.D0] = 0x27, [EmulationKey.P] = 0x37, [EmulationKey.LeftBracket] = 0x38,
        [EmulationKey.Semicolon] = 0x57, [EmulationKey.Period] = 0x67, [EmulationKey.F9] = 0x77,
        [EmulationKey.Equals] = 0x18, [EmulationKey.Backquote] = 0x28, [EmulationKey.RightBracket] = 0x58, [EmulationKey.Quote] = 0x48,
        [EmulationKey.Backslash] = 0x78, [EmulationKey.Slash] = 0x68, [EmulationKey.Left] = 0x19, [EmulationKey.Down] = 0x29,
        [EmulationKey.Up] = 0x39, [EmulationKey.Return] = 0x49, [EmulationKey.Backspace] = 0x59, [EmulationKey.Delete] = 0x59,
        [EmulationKey.AcornCopy] = 0x69, [EmulationKey.Right] = 0x79
    };

    /// <summary>Caractères imprimables de la matrice BBC : contact et état de SHIFT nécessaires, indépendants du clavier hôte.</summary>
    /// <remarks>Table matérielle vérifiée dans bbc_kbd.cpp (MAME). Les accents absents du BBC ne sont pas remplacés par une autre touche.</remarks>
    internal static IReadOnlyDictionary<char, (byte Position, bool Shift)> Characters { get; } = new Dictionary<char, (byte, bool)>
    {
        ['0'] = (0x27, false), ['1'] = (0x30, false), ['2'] = (0x31, false), ['3'] = (0x11, false), ['4'] = (0x12, false),
        ['5'] = (0x13, false), ['6'] = (0x34, false), ['7'] = (0x24, false), ['8'] = (0x15, false), ['9'] = (0x26, false),
        ['!'] = (0x30, true), ['"'] = (0x31, true), ['#'] = (0x11, true), ['$'] = (0x12, true), ['%'] = (0x13, true),
        ['&'] = (0x34, true), ['\''] = (0x24, true), ['('] = (0x15, true), [')'] = (0x26, true),
        ['-'] = (0x17, false), ['='] = (0x17, true), ['@'] = (0x47, false), [';'] = (0x57, false), ['+'] = (0x57, true),
        [','] = (0x66, false), ['<'] = (0x66, true), ['.'] = (0x67, false), ['>'] = (0x67, true),
        ['^'] = (0x18, false), ['~'] = (0x18, true), ['_'] = (0x28, false), ['£'] = (0x28, true),
        ['['] = (0x38, false), ['{'] = (0x38, true), [']'] = (0x58, false), ['}'] = (0x58, true),
        [':'] = (0x48, false), ['*'] = (0x48, true), ['/'] = (0x68, false), ['?'] = (0x68, true),
        ['\\'] = (0x78, false), ['|'] = (0x78, true), [' '] = (0x62, false)
    };

    /// <summary>Repli du pavé numérique lorsque l'appelant ne fournit pas de caractères hôtes.</summary>
    internal static IReadOnlyDictionary<EmulationKey, char> Keypad { get; } = new Dictionary<EmulationKey, char>
    {
        [EmulationKey.Numpad0] = '0', [EmulationKey.Numpad1] = '1', [EmulationKey.Numpad2] = '2', [EmulationKey.Numpad3] = '3',
        [EmulationKey.Numpad4] = '4', [EmulationKey.Numpad5] = '5', [EmulationKey.Numpad6] = '6', [EmulationKey.Numpad7] = '7',
        [EmulationKey.Numpad8] = '8', [EmulationKey.Numpad9] = '9', [EmulationKey.NumpadPeriod] = '.', [EmulationKey.NumpadSeparator] = ',',
        [EmulationKey.NumpadPlus] = '+', [EmulationKey.NumpadMinus] = '-', [EmulationKey.NumpadMultiply] = '*', [EmulationKey.NumpadDivide] = '/'
    };
}
