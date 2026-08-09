# Matrice de refactorisation

Cette matrice ne déclenche aucun déplacement. Elle prépare la phase 2 en indiquant les dépendances et validations obligatoires.

| Source actuelle | Destination proposée | Risque | Dépendances à préserver | Tests avant/après |
|---|---|---:|---|---|
| `MainWindow.xaml.cs` | un contrôle/ViewModel par onglet + services d’orchestration ; `MainWindow` garde coque/état global | très élevé | contrôles nommés, stores profils, runner unique, document partagé, console/statut | suite complète + tests ciblés Read/Write/Convert/Visual/Explorer + lancement réel UI |
| `MainWindow.xaml` | `Tabs/ReadTab`, `WriteTab`, `ConvertTab`, `VisualizerTab`, `ToolsTab` | élevé | DataContext, ressources, noms et événements | compilation XAML, changement de langue/thème, navigation sans recréation |
| `OptionsWindow.xaml/.cs` | pages Général, Matériel/Host Tools, Profils ; modèles de lignes séparés | élevé | autosauvegarde, modalité, scan, changement immédiat langue/thème | tests réglages + scan matériel simulé + fermeture croix/bouton |
| `DiskImageExplorer.cs` | registre de lecteurs, service détection, service reconstruction, service FS, projection Explorer | très élevé | ordre/score actuel, multiformat, annulation, avertissements | tous tests d’images + corpus ciblé une image à la fois |
| `AtariScpSectorImageReader.cs` | collecteur ISO commun + reconstructeurs Atari ST, Atari 8 bits, IBM, Amstrad, BBC, Epson, UCSD | très élevé | marques ISO, CRC, choix révolution, géométries | tests par famille + comparaison des secteurs avant/après |
| `AppleScpSectorImageReader.cs` | stratégies Apple II standard, RWTS18 et Macintosh | élevé | GCR circulaire, protection, ordre physique/logique | tests Apple standard/protégé/NIB/WOZ/SCP |
| `AppleDiskImageReader.cs` | lecteurs de conteneurs Apple II/III, 2MG, DiskCopy/Mac/Lisa et service d’ordre | élevé | signatures, endianess, ordre DOS/ProDOS | tests Apple par extension/conteneur |
| `ScpImage.cs` | `Containers/Scp/ScpModels`, `ScpReader` | élevé | validation checksum, offsets, timings | tous tests SCP et corpus réel |
| `ImageFormatCatalog.cs` | modèles, interface, built-in, capabilities, catalogues de données | moyen | ordre visible, défauts, extensions, compatibilités | tests formats + commandes + localisation |
| `DiskClassificationCatalog.cs` + connaissances dispersées | catalogue unique machine/format/codec/protection/conteneur | élevé | identifiants stables et multiformat | tests de parité des listes Read/Write/Convert/Explorer/Visualizer |
| `ExplorerFileIconClassifier.cs` | profils de types par système de fichiers/machine hors de la vue | moyen | règles spécifiques `.bat`, `.prg`, `.info`, types Apple | tests par système de fichiers |
| `SectorImageFluxVisualizer.cs` | stratégie par classification/codec | élevé | couleurs, géométrie, synthèse de flux | tests visualisateur + vérification visuelle |
| `ExplorerSection.xaml.cs` | contrôle + modèles de vue + formateur | moyen | binding, sélection uniquement dans la liste droite | tests de présentation et UI |
| `ExplorerDetailsPanel.xaml.cs` | contrôle + presenter + records | faible | formatage localisé | tests localisation/UI |
| `AppSettings.cs` | racine + fichiers UI/Hardware/Operations/Profiles/Logs + contrat séparé | élevé | compatibilité JSON et migration | fixtures de tous schémas connus |
| `WriteRequest.cs` | détecteur, modèles, requête, builder | moyen | commande identique octet pour octet | tests WriteCommandBuilder/détection |
| `ReadRequest.cs` | requête/résultat, builder, tokenizer commun | moyen | commande identique | tests Lecture/options/tokenizer |
| `ConversionPlanner.cs` | modèles + planificateur gw + planificateur interne | moyen | ordre sorties, extensions défaut, conflits | tests multiconversion |
| `IProfileStore.cs` | contrat Domain + implémentation dans couche appropriée | moyen | profil système immuable, portée par onglet | tests profiles par onglet |
| `AdfImageReader.cs` | `ISectorImageReader.cs` + lecteur ADF | faible | API publique | compilation + tests ADF |
| `FluxDecoderRegistry` / `FluxEncoderRegistry` | registres alimentés par définitions communes | élevé | ordre de détection et parité | tests codecs complets |
| `FileSystemRegistry` | registre déclaratif + résultat multiple/score explicite | élevé | toutes interprétations valides | tests multiformat et FS concurrents |
| `CoreTests.cs` | dossiers de tests par domaine + doubles partagés | moyen, produit faible | noms/fixtures/couverture | nombre de tests et suite complète identiques |
| scripts de corpus | utilitaires communs sans batch opaque | faible | sortie par image demandée | exécuter chaque script sur une image |
| versioning dispersé | `Directory.Build.props` + service/script d’identité | élevé pour packaging | compatibilité update/installer | build solution, projets isolés, dirty, package |

## Séquence de validation de chaque déplacement

1. capturer les tests ciblés et le comportement observable avant déplacement ;
2. déplacer uniquement la responsabilité, sans réécrire l’algorithme ;
3. compiler et exécuter les tests ciblés ;
4. vérifier les ressources/localisations et les contrats publics ;
5. seulement ensuite supprimer une duplication réellement identique ;
6. refaire les tests ciblés puis la suite complète à la fin du bloc ;
7. commit à la fin de la tâche ; push à la fin du bloc cohérent, conformément aux règles du projet.

## Décisions à demander avant implémentation

- priorité exacte entre préférence manuelle et autres interprétations d’une image multiformat ;
- représentation UI de plusieurs systèmes reconnus simultanément ;
- politique de cache disque et limite mémoire ;
- emplacement final de certains contrats dont l’usage traverse Domain/App/Scp ;
- toute modification du résultat d’un codec, d’une géométrie ou du score de détection.

Ces décisions ne bloquent pas l’inventaire, mais elles bloquent les changements de comportement correspondants.

