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
- La compilation Release et 124 tests automatisés réussissent; un test STA charge réellement le XAML principal et vérifie le ViewModel, les liaisons de statut/progression et le menu Alignement. Des vecteurs bit à bit contrôlent les extractions et checksums valides comme corrompus NorthStar/Heathkit.

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

- La solution est séparée en Application, Domaine, Infrastructure, moteur SCP et Tests. L’état transversal de la fenêtre est désormais lié à un ViewModel observable, mais les formulaires d’opérations utilisent encore beaucoup de code-behind. La migration MVVM avance donc sans être encore présentée comme achevée.
