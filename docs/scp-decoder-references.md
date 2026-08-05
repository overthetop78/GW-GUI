# Références des décodeurs SCP

Les décodeurs rares sont implémentés à partir des caractéristiques de leur encodage, sans intégrer le code de HxC dans GW GUI.

## Référence principale

- Projet officiel : <https://github.com/jfdelnero/HxCFloppyEmulator>
- Copie de contrôle étudiée : branche principale consultée le 5 août 2026.
- Fichiers de référence : `libhxcfe/sources/tracks/track_formats/*_track.c` et `libhxcfe/sources/tracks/encoding/`.

## Correspondances actuellement vérifiées

| Décodeur GW GUI | Référence HxC étudiée | Marques utilisées |
|---|---|---|
| Membrain MFM | `membrain_mfm_track.c` | `44 89 55 54`, `44 89 55 4A` |
| AED 6200P MFM | `aed6200p_track.c` | `50 94`, `A5 08` |
| QD MO5 MFM | `qd_mo5_track.c` | cinq répétitions `A9 14`, puis `44 91` ou `91 44` |
| Centurion MFM | `centurion_mfm_track.c` | `91 22 44 89`, `AA AA AA A9` |
| NorthStar MFM | `northstar_mfm_track.c` | sept octets nuls suivis de `FB`, encodés en MFM |
| Heathkit FM | `heathkit_fm_track.c` | trois octets nuls suivis de `FD` inversé bit à bit, encodés en FM |
| E-mu Emulator FM | `emu_emulator_fm_track.c` | `45 45 55 55 45 54 54 45` |
| TYCOM FM | `tycom_fm_track.c` | `55 11 15 54` et marques de données `55 11 14 xx` |
| DEC RX02 M²FM | `dec_rx02_track.c` | `55 11 15 54` et marque M²FM `55 11 15 45` |
| Arburg | `arburg_track.c` | `44 44 44 44 55 55 55 55` et `55 55 55 55 55 24 92 49` |
| Victor 9000 GCR | `victor9k_gcr_track.c` | Marques `55 55 55 55 55 55 11 11` / `55 55 55 55 55 55 11 04`, table GCR 4/5, en-tête de 6 octets, données de 512 octets et somme additive 16 bits little-endian |
| Apple II GCR | `apple2_gcr_track.c` | Prologues `D5 AA 96` / `D5 AA AD`, adresse 4-and-4, table 6-and-2, reconstruction de 256 octets et chaîne XOR |
| Commodore GCR | `c64_gcr_track.c` | Synchronisations, table GCR 4/5, blocs `0x08`/`0x07`, en-tête de 6 octets, données de 256 octets et checksums XOR |

Les prochaines familles doivent suivre la même règle : signature justifiée par une référence primaire, corpus synthétique ciblé et résultat visuel réellement exploitable.
