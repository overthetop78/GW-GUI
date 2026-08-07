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
| Micral N FM | `micraln_fm_track.c` | trois octets nuls, synchronisation `FF`, secteur, cylindre, 128 octets et checksum à retenue de fin |
| E-mu Emulator FM | `emu_emulator_fm_track.c` | `45 45 55 55 45 54 54 45` |
| TYCOM FM | `tycom_fm_track.c` | `55 11 15 54` et marques de données `55 11 14 xx` |
| DEC RX02 M²FM | `dec_rx02_track.c` | `55 11 15 54` et marque M²FM `55 11 15 45` |
| Arburg | `arburg_track.c` | `44 44 44 44 55 55 55 55` et `55 55 55 55 55 24 92 49` |
| Victor 9000 GCR | `victor9k_gcr_track.c` | Marques `55 55 55 55 55 55 11 11` / `55 55 55 55 55 55 11 04`, table GCR 4/5, en-tête de 6 octets, données de 512 octets et somme additive 16 bits little-endian |
| Apple II GCR | `apple2_gcr_track.c` | Prologues `D5 AA 96` / `D5 AA AD`, adresse 4-and-4, table 6-and-2, reconstruction de 256 octets et chaîne XOR |
| Apple Macintosh GCR | `apple_mac_gcr_track.c` | Prologues échantillonnés `D5 AA 96` / `D5 AA AD`, en-tête 6 bits, dénibblisation de 524 octets, 12 octets de tags, 512 octets utiles et quatre checksums |
| Commodore GCR | `c64_gcr_track.c` | Synchronisations, table GCR 4/5, blocs `0x08`/`0x07`, en-tête de 6 octets, données de 256 octets et checksums XOR |
| Amiga MFM | `amiga_mfm_track.c` | Double sync `4489 4489`, identité et données odd/even, bloc de 512 octets et parités XOR séparées |
| ISO MFM/FM | Greaseweazle `codec/ibm` et conventions WD/IBM | Marques FE/FB/F8, géométrie CHRN, CRC-CCITT avec préfixe A1×3 en MFM et sans préfixe en FM |

La comparaison exhaustive confirme que les 18 extracteurs `*_track.c` possèdent un décodeur dédié. Chaque ajout suit la même règle : signature justifiée par la référence primaire, corpus synthétique ciblé et résultat visuel réellement exploitable.

## Références Greaseweazle complémentaires

La comparaison du catalogue HxC avec les codecs Greaseweazle a identifié trois familles supplémentaires :

| Famille GW GUI | Source Greaseweazle étudiée |
|---|---|
| HP MMFM | `src/greaseweazle/codec/hp/hp_mmfm.py` |
| Data General 2F | `src/greaseweazle/codec/datageneral/datageneral.py` |
| Micropolis MFM | `src/greaseweazle/codec/micropolis/micropolis.py` |

Révisions locales de contrôle utilisées le 7 août 2026 :

- Greaseweazle : `26690f89967d519e0106ab9566019a026b920bb4` ;
- HxCFloppyEmulator : `b1eee4cd73391ceaf2ad4ac57e28bf11c91333ba`.

Les algorithmes ont été réimplémentés en C# dans l'architecture de GW GUI. Le code HxC sous GPL n'est pas copié dans le projet. Les encodeurs possèdent chacun leur propre classe et partagent uniquement les primitives réellement communes : construction de cellules, conversion en intervalles, CRC, checksums et inversion de bits.
