# Couverture détaillée des décodeurs SCP

Ce tableau distingue volontairement trois niveaux : détection de synchronisation ou de marque, extraction d’identité de secteur, et contrôle d’intégrité. La présence d’un décodeur dans la liste ne signifie donc pas automatiquement que tout son contenu logique est déjà décodé.

| Décodeur | Marques/structures | Identité de secteur | Intégrité actuellement contrôlée |
|---|---:|---:|---:|
| ISO MFM — Atari ST / IBM PC | Oui | C/H/R/N et taille | CRC16 d’en-tête |
| ISO FM | Oui | C/H/R/N et taille | CRC16 d’en-tête |
| NorthStar MFM à secteurs matériels | Oui | Piste, secteur, 512 octets | Checksum rotatif du bloc |
| Heathkit FM à secteurs matériels | Oui | Volume, cylindre, secteur, 256 octets | Checksum rotatif d’en-tête |
| Membrain MFM | Marques en-tête/données | Cylindre, face, secteur, 512 octets | CRC16 `0x8005` d’en-tête |
| AED 6200P MFM | Marques C6/données | Cylindre, secteur, taille variable | CRC-CCITT d’en-tête |
| Amiga MFM | Double synchronisation | Non | Non |
| Apple II GCR | Prologues adresse/données | Non | Non |
| Commodore GCR | Synchronisation et type de bloc | Non | Non |
| QD MO5 MFM | Marques en-tête/données | Non | Non |
| Centurion MFM | Marques secteur/données | Non | Non |
| E-mu Emulator FM | Marque secteur | Non | Non |
| TYCOM FM | En-tête et marques F8–FB | Non | Non |
| DEC RX02 M²FM | En-tête FM et marque données FD | Non | Non |
| Arburg | Blocs système/données | Non | Non |
| Victor 9000 GCR | Marques en-tête/données | Non | Non |
| Flux brut | Impulsions courtes et absences longues | Sans objet | Sans objet |

## Sources de qualification

Les structures NorthStar, Heathkit, Membrain et AED 6200P sont alignées sur leurs extracteurs homonymes de libhxcfe. Les tests synthétiques reconstruisent les encodages bit à bit, injectent une intégrité valide puis corrompue et vérifient les champs extraits, y compris les tailles variables AED. Les autres familles restent annoncées au niveau réellement atteint ci-dessus; leur extraction détaillée demande encore des vecteurs fiables ou un corpus physique libre.
