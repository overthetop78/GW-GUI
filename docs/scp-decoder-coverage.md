# Couverture détaillée des décodeurs SCP

Ce tableau distingue volontairement trois niveaux : détection de synchronisation ou de marque, extraction d’identité de secteur, et contrôle d’intégrité. La présence d’un décodeur dans la liste ne signifie donc pas automatiquement que tout son contenu logique est déjà décodé.

| Décodeur | Marques/structures | Identité de secteur | Intégrité actuellement contrôlée |
|---|---:|---:|---:|
| ISO MFM — Atari ST / IBM PC | Marques FE/FB/F8, C/H/R/N, taille et données | CRC16-CCITT des en-têtes et des données, avec préfixe A1×3 ; état valide, incorrect ou indisponible | Oui, y compris les données supprimées F8 |
| ISO FM | Marques FE/FB/F8, C/H/R/N, taille et données | CRC16-CCITT des en-têtes et des données ; état valide, incorrect ou indisponible | Oui, y compris les données supprimées F8 |
| NorthStar MFM à secteurs matériels | Enregistrement unique marque/identité/données | Piste, secteur et restitution du bloc de 512 octets | Checksum rotatif du bloc ; état valide, incorrect ou indisponible si tronqué |
| Heathkit FM à secteurs matériels | Marques FD d’en-tête et de données associées | Volume, cylindre, secteur et bloc de 256 octets, avec inversion bit à bit | Checksums rotatifs distincts de l’en-tête et des données ; état valide, incorrect ou indisponible |
| Membrain MFM | Marques en-tête/données associées | Cylindre, face, secteur et bloc de 512 octets | CRC16 `0x8005` de l’en-tête et du bloc de données ; état valide, incorrect ou indisponible |
| AED 6200P MFM | Marque d’en-tête C6 et marques de données C0–C3 associées | Cylindre, secteur et bloc de taille variable | CRC-CCITT de l’en-tête et des données ; état valide, incorrect ou indisponible |
| Amiga MFM | Double synchronisation, identité odd/even, cylindre, face, secteur, secteurs restants et 512 octets | Parités XOR odd/even de l’en-tête/label et des données ; état valide, incorrect ou indisponible | Oui pour l’identité, les données et les deux checksums |
| Apple II GCR 16 secteurs | Adresse 4-and-4 avec volume, piste et secteur ; bloc de 256 octets décodé en 6-and-2 | XOR de l’adresse et chaîne XOR des 343 symboles GCR ; état valide, incorrect ou indisponible | Oui pour l’identité, les données et les deux checksums |
| Commodore GCR | Synchronisations, blocs `0x08`/`0x07`, piste, secteur, identifiant disque et 256 octets | XOR des cinq champs d’en-tête et XOR des données avec l’octet stocké ; état valide, incorrect ou indisponible | Oui pour l’identité, les données et les deux checksums |
| QD MO5 MFM | En-tête et bloc de données associés | Numéro de secteur sur 16 bits et restitution des 128 octets | Somme 8 bits du marqueur et des données ; état valide, incorrect ou indisponible ; aucun CRC d’en-tête |
| Centurion MFM | En-tête et bloc de données associés, clé et taille 16 bits | Cylindre, secteur et bloc de taille variable | CRC16 XMODEM de l’en-tête et de `taille + données` ; état valide, incorrect ou indisponible ; seule la clé non chiffrée `0` documentée est décodée |
| E-mu Emulator FM | Cylindre, face, secteur unique et restitution des 3584 octets | CRC16 `0x8005` de l’en-tête et des données ; état valide, incorrect ou indisponible | Oui pour l’identité, la cadence FM quadruplée, les données et les deux CRC |
| TYCOM FM | Cylindre, secteur, marques F8–FB et restitution des 128 octets | CRC-CCITT de l’en-tête et des données ; état valide, incorrect ou indisponible | Oui pour l’identité, les marques, la cadence FM quadruplée, les données et les deux CRC |
| DEC RX02 FM/M²FM | Cylindre, face, secteur, code de taille, marques F8–FD ; restitution de 128 octets FM ou 256 octets M²FM pour F9/FD | CRC-CCITT de l’en-tête et des données ; état valide, incorrect ou indisponible | Oui, y compris les données et la substitution DEC M²FM sur 11 bits |
| Arburg | Bloc de données FM de 2560 octets et bloc système variable de 3840 octets ; identité fixe de bloc unique | Somme additive 16 bits little-endian sur 2558 ou 3838 octets ; état valide, incorrect ou indisponible | Oui pour les deux encodages et leurs sommes |
| Victor 9000 GCR | En-tête GCR de 6 octets, cylindre, secteur et bloc de 512 octets | Contrôle arithmétique de l’en-tête et somme additive 16 bits little-endian des données ; état valide, incorrect ou indisponible | Oui pour l’identité, l’encodage GCR à demi-cellules et les deux contrôles |
| Flux brut | Impulsions courtes et absences longues | Sans objet | Sans objet |

## Sources de qualification

Les structures Amiga, NorthStar, Heathkit, Membrain, AED 6200P, Apple II 6-and-2, Commodore et Victor 9000 sont alignées sur leurs extracteurs homonymes de libhxcfe. Les tests synthétiques reconstruisent les encodages bit à bit, injectent une intégrité valide puis corrompue et vérifient les champs extraits, y compris les restitutions exactes Amiga, Apple II, Commodore, Membrain et AED, les tailles variables et marques C0–C3 AED, ainsi que l’échantillonnage GCR Victor à un bit utile sur deux. Les autres familles restent annoncées au niveau réellement atteint ci-dessus; leur extraction détaillée demande encore des vecteurs fiables ou un corpus physique libre.
