# Plan d’implémentation complet

## Objectif

Construire `overthetop78/GW-GUI`, application Windows 10/11 x64 en C#/.NET 10, WPF/MVVM et SkiaSharp. Elle remplace GreaseweazleGUI v2.129, couvre toutes les commandes `gw`, supprime les consoles externes et intègre lecture, écriture, multiconversion, maintenance, diagnostics et visualisation SCP complète.

Le dépôt est public sous licence MIT. Les préversions `0.x` servent au développement public; la version `1.0` n’est déclarée stable qu’après réalisation et validation de toutes les fonctions définies.

## Interface et comportements

- Fenêtre redimensionnable, minimum 1280×720, restaurant taille, position, écran, maximisation, thème et état de la console.
- Thèmes système, clair et sombre, couleur d’accent Windows et icône originale disquette + flux.
- Menus Options et Aide uniquement.
- Onglets Lecture, Écriture, Conversion, Visualisation et Outils.
- Exécuter/Arrêter en bas à droite de chaque opération.
- Console inférieure redimensionnable et réductible, restaurant son état, sans réouverture forcée.
- Barre d’état : port COM, lecteur, profil, diode d’état et progression par face masquée au repos.
- Sélecteur de lecteur dans chaque opération uniquement si plusieurs lecteurs sont configurés.
- Bannière non bloquante après lecture SCP réussie pour ouvrir le visualiseur.

## Architecture et contrats

- Projets Application WPF, Domaine, Infrastructure, Moteur SCP et Tests.
- `IGreaseweazleRunner` : exécution asynchrone, sortie temps réel, progression, annulation et code final.
- `IGwCommandBuilder` : arguments typés et rendu exact de la commande affichée.
- `IGwInstallationManager` : détection, téléchargement, sélection, mise à jour et retour arrière.
- `IHardwareRegistry` : contrôleurs, identifiants USB, ports COM, lecteurs et disponibilité.
- `IProfileStore<T>` : profil Par défaut et profils utilisateur propres à chaque opération.
- `IImageFormatCatalog` : familles, géométries, extensions, formats par défaut et compatibilités validées.
- `IScpReader`, `IFluxDecoder`, `IScpRenderer` : lecture défensive, décodage modulaire et rendu.
- Configuration JSON versionnée, écriture atomique, sauvegarde et migrations.
- Données dans le dossier du ZIP portable; `%AppData%`/`%LocalAppData%` pour l’installation classique.
- Journal rotatif : 10 fichiers de 5 Mio, exportable, aucune télémétrie.
- Une seule commande `gw` active; multiconversions séquentielles.
- Arrêt gracieux puis terminaison de l’arbre de processus en dernier recours.
- Catalogue convivial recoupé avec l’aide et les diskdefs de la version active de `gw`, puis validé par tests.
- Ressources `.resx` et catalogue extensible de langues; aucun texte visible codé dans les vues. Le français et l’anglais restent les références, puis l’application et l’installateur sont traduits dans plusieurs langues selon la feuille de route `remaining-work.md`.

## Fonctions

### Profils

- Par défaut/Default permanent, non modifiable et sans option facultative.
- Sauvegarde par nom; remplacement confirmé; gestion Renommer/Supprimer dans Options.
- Réinitialiser recharge le profil actif.

### Lecture

- Capture brute SCP ou format connu.
- Numérotation numérique/alphabetique avec masques `0/00/000` et `A/AA/AAA`.
- Incrément uniquement après succès.
- Conflit : Écraser, numéro suivant ou modifier le nom.
- Groupes avancés : pistes/faces, récupération, rotation/index, signal, matériel spécialisé.

### Écriture

- Détection du format avec modification manuelle et blocage si ambigu.
- Vérification active par défaut; `--no-verify` avancé et averti.
- Résumé obligatoire avant écriture.

### Conversion

- Tous les formats source supportés par `gw convert`.
- Sorties dynamiques compatibles; incompatibles visibles et désactivées.
- Sélection simple/multiple par cases, sans mode distinct.
- Extension implicite par défaut ou extensions explicites multiples.
- Tags prédéfinis et personnalisables; défaut `[FAMILLE-CAPACITÉ]`.
- Continuer après échec et produire un bilan; dialogue seulement en cas de conflits de fichiers.

### Visualiseur SCP

- Parser SCP défensif et rendu circulaire des faces, pistes, révolutions, structures et anomalies.
- Inspecteur latéral masquable; zoom indépendant avec option Lier.
- Détection automatique ou choix manuel du décodeur.
- Décodeurs modulaires couvrant flux brut, ISO MFM/FM, Amiga MFM, Apple II et Macintosh GCR, C64, Northstar, Micral N, DEC RX02, E-Emu, AED 6200P, TYCOM, Membrain, Heathkit, Arburg, Victor 9K, QD MO5, Centurion et les analyseurs HxC de référence.
- Aucune dépendance à HxCFloppyEmulator.exe ou libhxcfe.dll.

### Options, outils et matériel

- Options à navigation latérale : Général, Greaseweazle Tools, Contrôleurs/lecteurs, Profils.
- Assistant Télécharger/Choisir si `gw.exe` manque.
- Vérification quotidienne discrète des Host Tools; aucune installation automatique; conservation d’une version précédente.
- `Options > Diagnostics` : Info, Bande passante, RPM, Seek.
- `Options > Matériel` : Pin, Reset, Delays, Firmware.
- Dialogues avec résumé lisible, sortie brute repliable et commande visible.
- Outils principal : Effacer et Nettoyer les têtes, sans profils et avec confirmations adaptées.

## Publication

- Dépôt public `overthetop78/GW-GUI`, licence MIT.
- Publication autonome `win-x64` en dossier, ZIP portable et installateur Inno Setup.
- Notifications GitHub discrètes, sans mise à jour automatique.
- Builds non signés, sommes SHA-256 et explication SmartScreen.
- GitHub Actions Windows : compilation, tests, analyse, ZIP, installateur, SHA-256 et releases.

## Traductions de l’application et de l’installateur

- Remplacer la sélection binaire français/anglais par un catalogue de cultures extensible.
- Conserver une ressource `.resx` complète par langue avec contrôle automatique de parité des clés et repli vers l’anglais.
- Traduire tous les écrans, dialogues, messages dynamiques, infobulles et noms accessibles; ne pas traduire les arguments `gw` ni les identifiants techniques.
- Ajouter les mêmes langues à Inno Setup et étendre les tests silencieux, interactifs et de mise à niveau de l’installateur.
- Vérifier visuellement les textes longs, raccourcis clavier, formats numériques et captures dans chaque langue.
- Publier uniquement les traductions relues; conserver le glossaire et les règles de contribution dans la documentation.
- Langues demandées après les références française et anglaise : allemand, espagnol, italien, russe, chinois, japonais, portugais brésilien, néerlandais et polonais.
- Traduction et seconde passe de relecture assurées avec ChatGPT/Codex, puis contrôles automatiques de parité et vérification réelle de la mise en page.

## Version, build et révision

- Version produit choisie pour une publication; numéro de build propre à une compilation; révision liée au commit exact.
- Métadonnées centralisées et cohérentes pour l’EXE et toutes les DLL GW GUI, y compris lorsqu’un projet est compilé séparément.
- `AssemblyVersion` stable pour la compatibilité, `FileVersion` numérique Windows et `InformationalVersion` contenant build, révision numérique et hash court.
- Affichage complet dans À propos et contrôle automatique du paquet; convention détaillée dans `versioning.md`.

## Validation

- Tests unitaires des commandes, profils, migrations, compteurs, conflits, tags et compatibilités.
- Faux `gw` pour sortie fragmentée, Unicode, progression, erreurs, blocage et annulation.
- Tests d’intégration avec Host Tools courant et précédent.
- Corpus synthétique et libre couvrant formats, extensions et décodeurs SCP.
- Tests matériels sur Greaseweazle V4.1, déconnexion/reconnexion et changement de COM.
- Tests UI 1280×720, DPI, thèmes, clavier, lecteurs d’écran, français/anglais et chemins Unicode.
- Tests Windows 10/11 des deux distributions, désinstallation, mise à jour et retour arrière.

## Documentation continue

- Maintenir les décisions, spécifications visuelles, formats, guide utilisateur bilingue, aide `gw`, matériel et publication à jour pendant tout le développement.
- La documentation doit suffire à reconstruire l’application sans relire la conversation.
