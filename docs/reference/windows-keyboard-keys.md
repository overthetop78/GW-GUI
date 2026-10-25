# Touches clavier Windows reconnues par WPF

Le projet utilise `System.Windows.Input.Key`, fourni par WPF, et non une enum intégrée au langage C#. Cette identité décrit une touche Windows ; elle ne désigne pas un code de cœur d'émulation.

Référence : [Microsoft — System.Windows.Input.Key](https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.key?view=windowsdesktop-10.0).

- `A` à `Z` : lettres logiques Windows. Leur position physique dépend de la disposition AZERTY, QWERTY, QWERTZ, Bépo, Dvorak, etc.
- `D0` à `D9` : touches numériques de la rangée supérieure, séparées de `NumPad0` à `NumPad9`.
- `F1` à `F24` : touches de fonction ; les claviers ordinaires proposent généralement F1 à F12.
- `LeftCtrl` / `RightCtrl`, `LeftAlt` / `RightAlt`, `LeftShift` / `RightShift` : côtés distincts. RightAlt correspond à Alt Gr sur les dispositions concernées ; SHIFT s'appelle MAJ en français.
- `LWin` / `RWin` : touches Windows, aucune correspondance implicite avec les touches Amiga. Elles restent réservées à Windows dans GW GUI.
- `Oem*` et `Abnt*` : identités dépendant de la disposition. Un alias tel que OemSemicolon ne prouve pas que le caractère produit soit un point-virgule sur un clavier français. Le caractère saisi relève de la disposition active et des événements de texte, pas du seul nom Key.
- `ImeProcessed`, `System`, `DeadCharProcessed` : valeurs de traitement ; récupérer la vraie touche depuis l'événement approprié, ne pas les présenter comme des touches physiques.

Le tableau ci-dessous liste les noms réellement présents dans l'enum, en regroupant les alias de même valeur. Il constitue un inventaire des identités WPF, pas une affirmation de prise en charge par chaque cœur ou par toutes les conversions existantes du SDK.

| Valeur WPF | Noms `Key` (alias regroupés) |
|---|---|
| 0 | `None` |
| 1 | `Cancel` |
| 2 | `Back` |
| 3 | `Tab` |
| 4 | `LineFeed` |
| 5 | `Clear` |
| 6 | `Return`, `Enter` |
| 7 | `Pause` |
| 8 | `Capital`, `CapsLock` |
| 9 | `KanaMode`, `HangulMode` |
| 10 | `JunjaMode` |
| 11 | `FinalMode` |
| 12 | `HanjaMode`, `KanjiMode` |
| 13 | `Escape` |
| 14 | `ImeConvert` |
| 15 | `ImeNonConvert` |
| 16 | `ImeAccept` |
| 17 | `ImeModeChange` |
| 18 | `Space` |
| 19 | `Prior`, `PageUp` |
| 20 | `Next`, `PageDown` |
| 21 | `End` |
| 22 | `Home` |
| 23 | `Left` |
| 24 | `Up` |
| 25 | `Right` |
| 26 | `Down` |
| 27 | `Select` |
| 28 | `Print` |
| 29 | `Execute` |
| 30 | `Snapshot`, `PrintScreen` |
| 31 | `Insert` |
| 32 | `Delete` |
| 33 | `Help` |
| 34 | `D0` |
| 35 | `D1` |
| 36 | `D2` |
| 37 | `D3` |
| 38 | `D4` |
| 39 | `D5` |
| 40 | `D6` |
| 41 | `D7` |
| 42 | `D8` |
| 43 | `D9` |
| 44 | `A` |
| 45 | `B` |
| 46 | `C` |
| 47 | `D` |
| 48 | `E` |
| 49 | `F` |
| 50 | `G` |
| 51 | `H` |
| 52 | `I` |
| 53 | `J` |
| 54 | `K` |
| 55 | `L` |
| 56 | `M` |
| 57 | `N` |
| 58 | `O` |
| 59 | `P` |
| 60 | `Q` |
| 61 | `R` |
| 62 | `S` |
| 63 | `T` |
| 64 | `U` |
| 65 | `V` |
| 66 | `W` |
| 67 | `X` |
| 68 | `Y` |
| 69 | `Z` |
| 70 | `LWin` |
| 71 | `RWin` |
| 72 | `Apps` |
| 73 | `Sleep` |
| 74 | `NumPad0` |
| 75 | `NumPad1` |
| 76 | `NumPad2` |
| 77 | `NumPad3` |
| 78 | `NumPad4` |
| 79 | `NumPad5` |
| 80 | `NumPad6` |
| 81 | `NumPad7` |
| 82 | `NumPad8` |
| 83 | `NumPad9` |
| 84 | `Multiply` |
| 85 | `Add` |
| 86 | `Separator` |
| 87 | `Subtract` |
| 88 | `Decimal` |
| 89 | `Divide` |
| 90 | `F1` |
| 91 | `F2` |
| 92 | `F3` |
| 93 | `F4` |
| 94 | `F5` |
| 95 | `F6` |
| 96 | `F7` |
| 97 | `F8` |
| 98 | `F9` |
| 99 | `F10` |
| 100 | `F11` |
| 101 | `F12` |
| 102 | `F13` |
| 103 | `F14` |
| 104 | `F15` |
| 105 | `F16` |
| 106 | `F17` |
| 107 | `F18` |
| 108 | `F19` |
| 109 | `F20` |
| 110 | `F21` |
| 111 | `F22` |
| 112 | `F23` |
| 113 | `F24` |
| 114 | `NumLock` |
| 115 | `Scroll` |
| 116 | `LeftShift` |
| 117 | `RightShift` |
| 118 | `LeftCtrl` |
| 119 | `RightCtrl` |
| 120 | `LeftAlt` |
| 121 | `RightAlt` |
| 122 | `BrowserBack` |
| 123 | `BrowserForward` |
| 124 | `BrowserRefresh` |
| 125 | `BrowserStop` |
| 126 | `BrowserSearch` |
| 127 | `BrowserFavorites` |
| 128 | `BrowserHome` |
| 129 | `VolumeMute` |
| 130 | `VolumeDown` |
| 131 | `VolumeUp` |
| 132 | `MediaNextTrack` |
| 133 | `MediaPreviousTrack` |
| 134 | `MediaStop` |
| 135 | `MediaPlayPause` |
| 136 | `LaunchMail` |
| 137 | `SelectMedia` |
| 138 | `LaunchApplication1` |
| 139 | `LaunchApplication2` |
| 140 | `Oem1`, `OemSemicolon` |
| 141 | `OemPlus` |
| 142 | `OemComma` |
| 143 | `OemMinus` |
| 144 | `OemPeriod` |
| 145 | `Oem2`, `OemQuestion` |
| 146 | `Oem3`, `OemTilde` |
| 147 | `AbntC1` |
| 148 | `AbntC2` |
| 149 | `Oem4`, `OemOpenBrackets` |
| 150 | `Oem5`, `OemPipe` |
| 151 | `Oem6`, `OemCloseBrackets` |
| 152 | `Oem7`, `OemQuotes` |
| 153 | `Oem8` |
| 154 | `Oem102`, `OemBackslash` |
| 155 | `ImeProcessed` |
| 156 | `System` |
| 157 | `OemAttn`, `DbeAlphanumeric` |
| 158 | `OemFinish`, `DbeKatakana` |
| 159 | `OemCopy`, `DbeHiragana` |
| 160 | `OemAuto`, `DbeSbcsChar` |
| 161 | `OemEnlw`, `DbeDbcsChar` |
| 162 | `OemBackTab`, `DbeRoman` |
| 163 | `Attn`, `DbeNoRoman` |
| 164 | `CrSel`, `DbeEnterWordRegisterMode` |
| 165 | `ExSel`, `DbeEnterImeConfigureMode` |
| 166 | `EraseEof`, `DbeFlushString` |
| 167 | `Play`, `DbeCodeInput` |
| 168 | `Zoom`, `DbeNoCodeInput` |
| 169 | `NoName`, `DbeDetermineString` |
| 170 | `Pa1`, `DbeEnterDialogConversionMode` |
| 171 | `OemClear` |
| 172 | `DeadCharProcessed` |
