# Audit d’achèvement

Ce document relie le plan aux preuves présentes dans le dépôt. Un élément n’est déclaré terminé que lorsqu’une preuve directe existe.

## Prouvé

- Les onglets Lecture, Écriture, Conversion, Visualisation et Outils ainsi que les menus Options/Aide existent dans `MainWindow.xaml`.
- Les trois opérations principales construisent des commandes typées, affichent la commande, exécutent sans console externe, journalisent, progressent et peuvent être interrompues.
- Le profil système permanent Par défaut ne contient aucune option facultative; les profils utilisateur sont propres à leur opération.
- Lecture distingue SCP brut et format connu. SCP brut n’ajoute jamais `--format`, même si une ancienne sélection demeure en mémoire.
- `--drive` n’est ajouté que lorsque plusieurs lecteurs sont configurés; les configurations débranchées restent mémorisées.
- Écriture détecte le format, bloque les ambiguïtés, garde la vérification active par défaut et demande confirmation.
- Conversion utilise les cases pour le simple et le multiple, les extensions implicites/explicites, les tags, le traitement séquentiel, les conflits et le bilan. Une source sectorielle reconnue limite la sortie à sa géométrie; SCP/HFE gardent les formats décodables.
- Les diskdefs supplémentaires annoncés par les Host Tools actifs ou chargés depuis un fichier personnalisé ne sont plus masqués : ils sont ajoutés comme formats rares avec nom, tag et conteneur disponibles déterminés sans inventer une association machine/extension. Les imports préfixés personnalisés sont suivis et les cycles sont refusés.
- Lecture et Conversion utilisent le même dossier de destination pendant la session; il est réinitialisé depuis les Options au démarrage suivant.
- Les 14 actions officielles de `greaseweazle/cli.py` ont un parcours vérifié dans `gw-command-coverage.md`; les diagnostics, dont `align`, et le matériel utilisent des dialogues, tandis qu’Effacer et Nettoyer restent dans Outils.
- Les ressources FR/EN ont une parité automatisée et les deux langues ont été ouvertes et contrôlées sur le paquet portable final.
- ZIP portable, installateur Inno Setup et SHA-256 sont produits par `scripts/package.ps1`.
- La numérotation accepte un départ numérique ou alphabétique, poursuit après `Z` et ne s’incrémente qu’après succès.
- L’exécuteur de processus est testé avec sorties standard/erreur Unicode, ligne fragmentée, code non nul, concurrence et annulation.
- Les réglages avancés de Lecture, Écriture et Conversion possèdent des infobulles FR/EN indiquant l’argument `gw`, son rôle, les incompatibilités utiles et un exemple lorsqu’une valeur est attendue.
- La compilation Release et 209 tests automatisés réussissent; un test STA charge réellement le XAML principal, vérifie les chemins de liaison des trois opérations et contrôle séparément leur flux WPF bidirectionnel. Des tests sans fenêtre contrôlent le contrat/chargeur de document et le présentateur d’inspection SCP, le coordinateur commun d’opération (résultat, erreur, exclusion mutuelle et annulation), le présentateur de fin des commandes simples et des lots, le coordinateur séquentiel, ses échecs partiels et son annulation, ainsi que nommage, profils, multiconversion, extensions, tags et arguments; des vecteurs bit à bit contrôlent les extractions et intégrités valides, corrompues ou indisponibles ISO MFM/FM, Amiga MFM odd/even, Apple II 6-and-2, Commodore GCR, NorthStar — restitution des 512 octets et bloc tronqué —, Heathkit — en-tête et bloc de données de 256 octets —, Membrain — en-tête et bloc de données de 512 octets —, AED 6200P — taille variable et marques C0–C3 —, Centurion — clé, taille variable et bloc de données —, QD MO5 — numéro 16 bits, restitution des 128 octets et bloc tronqué —, E-mu Emulator, TYCOM, DEC RX02 FM/M²FM, Arburg et Victor 9000 GCR.

## Partiellement prouvé

- Le visualiseur SCP lit et rend le conteneur, les deux faces, pistes, structures, anomalies, zooms et inspecteur. Les familles déjà décodées ont des tests synthétiques, mais la couverture exhaustive des analyseurs HxC annoncée dans le plan n’est pas encore démontrée par un corpus physique libre.
- Le gestionnaire Host Tools est couvert par des tests de détection, version, téléchargement, checksum, extraction et retour arrière. Son comportement avec plusieurs installations réelles reste à vérifier.
- Le placement et le DPI ont été validés sur l’écran actuel à 125 %. Les matrices complètes multi-écran, clavier et lecteur d’écran restent à exécuter.

## Non prouvé sans environnement supplémentaire

- Contrôleurs et lecteurs Greaseweazle physiques multiples, déconnexion/reconnexion et changement de port COM.
- Exécution réelle de toutes les commandes avec Host Tools courant et précédent.
- Windows 10 et Windows 11, installation, désinstallation, mise à niveau et comportement SmartScreen.
- Décodage des formats rares sur un corpus de captures physiques libres et représentatives.

## Écart architectural restant

- La solution est séparée en Application, Domaine, Infrastructure, moteur SCP et Tests. Le contrat `IScpReader`, le chargement/préparation d’un document SCP et la présentation détaillée de son inspecteur sont extraits et testables sans fenêtre. L’état transversal et tout l’état éditable des trois opérations sont liés à des ViewModels observables; l’exécution séquentielle de Conversion, le cycle commun des opérations unitaires, la présentation de leurs résultats, les dialogues injectables, les décisions de conflits et les règles de compatibilité/classement du panneau dynamique de formats sont extraits. Le code-behind conserve encore l’ouverture des fenêtres Options, historique, À propos et outils matériels ainsi que la matérialisation WPF des lignes de formats; la migration MVVM avance donc sans être encore présentée comme achevée.
