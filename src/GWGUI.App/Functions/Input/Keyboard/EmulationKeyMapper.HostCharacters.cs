using System.Runtime.InteropServices;
using System.Windows.Input;
using GWGUI.Emulation;

namespace GWGUI.App.Functions.Input.Keyboard;

public static partial class EmulationKeyMapper
{
    /// <summary>Associe aux touches imprimables maintenues les caractères de la disposition active du thread de saisie.</summary>
    /// <remarks>Les touches restent transmises séparément pour les commandes et les adaptateurs physiques.
    /// ToUnicodeEx utilise le drapeau 4 pour préserver l'état des touches mortes de Windows.
    /// Les raccourcis CTRL/ALT restent physiques ; AltGr conserve sa traduction de caractères.</remarks>
    internal static IReadOnlyDictionary<EmulationKey, char> HostCharacters(IReadOnlySet<EmulationKey> keys)
    {
        var result = new Dictionary<EmulationKey, char>();
        var state = new byte[256];
        if (!GetKeyboardState(state)) return result;
        var altGr = keys.Contains(EmulationKey.RightAlt);
        if (!altGr && (keys.Contains(EmulationKey.LeftControl) || keys.Contains(EmulationKey.RightControl) || keys.Contains(EmulationKey.LeftAlt))) return result;
        state[0x10] = (byte)(keys.Contains(EmulationKey.LeftShift) || keys.Contains(EmulationKey.RightShift) ? 128 : 0);
        if (altGr) { state[0x11] = 128; state[0x12] = 128; }
        else { state[0x11] = 0; state[0x12] = 0; }
        var layout = GetKeyboardLayout(0);
        foreach (var key in keys)
        {
            if (!TryMap(key, out var hostKey)) continue;
            var vk = KeyInterop.VirtualKeyFromKey(hostKey);
            if (vk is not (>= 0x30 and <= 0x5a or >= 0x60 and <= 0x6f or >= 0xba and <= 0xe2) && hostKey != Key.Space) continue;
            var character = TranslateHostCharacter(vk, state, layout);
            result[key] = character;
        }
        return result;
    }

    /// <summary>Traduit une touche avec une disposition et un état explicites, sans modifier le clavier Windows.</summary>
    internal static char TranslateHostCharacter(int virtualKey, byte[] state, IntPtr layout)
    {
        var text = new char[8];
        var count = ToUnicodeEx((uint)virtualKey, MapVirtualKeyEx((uint)virtualKey, 0, layout), state, text, text.Length, 4, layout);
        return count == 1 || count == -1 ? text[0] : '\0';
    }

    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetKeyboardState([Out] byte[] state);
    [DllImport("user32.dll")] private static extern IntPtr GetKeyboardLayout(uint threadId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern uint MapVirtualKeyEx(uint code, uint mapType, IntPtr layout);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int ToUnicodeEx(uint virtualKey, uint scanCode, byte[] state, [Out] char[] text, int capacity, uint flags, IntPtr layout);
}
