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

Les prochaines familles doivent suivre la même règle : signature justifiée par une référence primaire, corpus synthétique ciblé et résultat visuel réellement exploitable.
