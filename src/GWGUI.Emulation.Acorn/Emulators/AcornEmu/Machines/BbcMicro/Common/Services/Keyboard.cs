using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Dictionaries;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Machines.BbcMicro.Common.Services;

/// <summary>Matrice des touches maintenues ; la carte l'interroge depuis le bus lent du VIA.</summary>
internal sealed class Keyboard
{
    private readonly bool[] _matrix = new bool[128];
    internal bool BreakPressed { get; private set; }
    /// <summary>Traduit les caractères hôtes puis applique les commandes physiques non traduites.</summary>
    /// <remarks>SHIFT BBC peut différer de SHIFT hôte (guillemet AZERTY ou chiffre avec SHIFT).
    /// La matrice possède un seul contact SHIFT : les symboles simultanés doivent partager cet état.
    /// Reconstruire chaque instantané assure le relâchement des touches et du SHIFT synthétique.</remarks>
    internal void SetInput(IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<EmulationKey, char>? hostCharacters = null)
    {
        Array.Clear(_matrix);
        bool? characterShift = null;
        foreach (var key in keys)
        {
            var hasCharacter = hostCharacters?.TryGetValue(key, out _) == true;
            var character = hasCharacter ? hostCharacters![key] : BbcMicroKeyboardDictionary.Keypad.GetValueOrDefault(key);
            // Les lettres du contrat suivent déjà les identités hôtes ; CAPS LOCK et SHIFT LOCK restent gérés par MOS.
            if (hasCharacter && key is >= EmulationKey.A and <= EmulationKey.Z && char.ToUpperInvariant(character) == key.ToString()[0])
            {
                _matrix[BbcMicroKeyboardDictionary.Matrix[key]] = true;
                continue;
            }
            if (hasCharacter || character != '\0')
            {
                if (BbcMicroKeyboardDictionary.Characters.TryGetValue(character, out var contact))
                {
                    // L'espace est identique avec ou sans SHIFT ; il ne doit pas changer une autre touche maintenue.
                    if (character == ' ') { _matrix[contact.Position] = true; continue; }
                    if (characterShift is not null && characterShift != contact.Shift) continue;
                    characterShift = contact.Shift;
                    _matrix[contact.Position] = true;
                }
                continue;
            }
            if (key == EmulationKey.NumpadEnter) _matrix[0x49] = true;
            else if (BbcMicroKeyboardDictionary.Matrix.TryGetValue(key, out var position)) _matrix[position] = true;
        }
        if (characterShift.HasValue) _matrix[0] = characterShift.Value;
        BreakPressed = keys.Contains(EmulationKey.Break);
    }
    /// <summary>PA0–PA6 sélectionnent la touche et PA7 rapporte son état.</summary>
    internal byte Read(byte selection) => (byte)((selection & 127) | (_matrix[selection & 127] ? 128 : 0));
    /// <summary>Recherche dans la colonne, hors SHIFT/CTRL, pour la détection automatique.</summary>
    internal bool ColumnPressed(int column)
    {
        for (var row = 1; row < 8; row++) if (_matrix[row * 16 + (column & 15)]) return true;
        return false;
    }
}
