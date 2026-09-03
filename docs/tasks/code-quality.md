# Qualité et refactorisation du code

Les phases ci-dessous restent exécutées dans leur ordre. Chaque case conserve son état actuel.

## Phase 02 — Refactorisation et rangement du code

Cette phase reprend entièrement la refactorisation. Les extractions déjà réalisées ne sont pas considérées comme validées tant que leur place, leur responsabilité et leurs dépendances n’ont pas été contrôlées dans cette nouvelle liste.

`AtariScpSectorImageReader.cs`, `MainWindow.xaml.cs`, la racine de `GWGUI.App` et le dossier `GWGUI.MediaEngine/Images` sont des exemples visibles du problème. Ils ne limitent pas le périmètre : **tous les projets et tous les fichiers de production doivent être examinés et rangés**.

### Règles de travail de cette phase

Ces points sont des règles, pas des tâches à cocher.

- Respecter l’ordre du présent document.
- Ne pas recommencer la cartographie générale validée dans la tâche 01.
- Avant de déplacer du code, écrire la structure cible dans un document Markdown, la contrôler, la corriger si nécessaire, puis la faire valider.
- Ne pas relire un fichier ou un document inchangé qui vient déjà d’être analysé. Revenir uniquement sur la partie modifiée ou sur une information précise manquante.
- Ne pas modifier le comportement fonctionnel pendant un déplacement architectural.
- Ne pas transformer une sélection explicite en détection exclusive : les images et disquettes multiformats doivent rester prises en charge.
- Ne pas appliquer automatiquement un modèle unique à tous les cas. Choisir entre registre, stratégie, polymorphisme, `switch` ou conditions simples selon la responsabilité réelle.
- Ne pas créer du code uniquement pour faire diminuer le nombre de lignes. Chaque extraction doit donner un propriétaire clair à une responsabilité.
- Ne pas créer une seconde implémentation pendant une extraction. Le nouveau chemin remplace l’ancien lorsqu’il est raccordé.
- Cocher chaque case immédiatement après sa réalisation et sa vérification, sans attendre la fin de la phase.
- Regrouper les contrôles techniques par bloc cohérent. Ne pas relancer toute la suite de tests après chaque modification mineure.
- Faire un commit lorsqu’une tâche terminée forme une modification autonome.
- Pousser lorsqu’une ou plusieurs tâches terminées forment un bloc complet et cohérent.
- Les constantes et textes bruts relèvent de la tâche 03, les modèles et contrats de la tâche 04, les fonctions et services de la tâche 05, et le rangement des traductions de la tâche 06. La phase 02 doit signaler leurs emplacements, sans exécuter ces phases à leur place.

### 2.1 Définir et valider la structure cible

#### 2.1.1 Préparer le document de structure

- [x] Établir la structure cible initiale de la phase 02.
  - [x] Y reprendre les projets existants sans refaire leur cartographie fonctionnelle complète.
  - [x] Y représenter l’arborescence actuelle seulement au niveau nécessaire pour comprendre les déplacements.
  - [x] Y représenter l’arborescence cible proposée.
  - [x] Y indiquer le rôle de chaque dossier cible.
  - [x] Y indiquer les dépendances autorisées entre les projets.
  - [x] Y indiquer les dépendances interdites entre les projets.

#### 2.1.2 Préparer le plan de déplacement

- [ ] Ajouter au document une table de correspondance entre l’emplacement actuel et l’emplacement cible.
  - [ ] Couvrir `GWGUI.App`.
  - [ ] Couvrir `GWGUI.Domain`.
  - [ ] Couvrir `GWGUI.Infrastructure`.
  - [ ] Couvrir `GWGUI.MediaEngine`.
  - [ ] Couvrir les fichiers de production présents à la racine de chaque projet.
  - [ ] Distinguer les fichiers qui restent à leur place.
  - [ ] Distinguer les fichiers qui doivent seulement être déplacés.
  - [ ] Distinguer les fichiers qui doivent être renommés.
  - [ ] Distinguer les fichiers qui doivent être séparés en plusieurs responsabilités.
  - [ ] Distinguer les fichiers dont plusieurs parties doivent rejoindre des modules existants.

#### 2.1.3 Contrôler la structure proposée

- [ ] Vérifier la structure cible avant toute modification du code.
  - [ ] Vérifier qu’un dossier ne mélange pas interface, domaine, infrastructure et formats de disquette.
  - [ ] Vérifier que les dossiers génériques comme `Controls`, `Services` et `Images` ne redeviennent pas des dossiers fourre-tout.
  - [ ] Vérifier que chaque famille de fichiers possède un emplacement évident.
  - [ ] Vérifier qu’un nouveau format pourra être ajouté sans modifier plusieurs dossiers sans rapport.
  - [ ] Vérifier que les composants réellement communs ne sont pas dupliqués dans chaque fonction.
  - [ ] Vérifier que les composants spécifiques ne sont pas placés artificiellement dans un dossier commun.
  - [ ] Corriger le document tant qu’un fichier important n’a pas de destination claire.
- [ ] Faire valider la structure cible avant de commencer les déplacements.

### 2.2 Réorganiser `GWGUI.App`

#### 2.2.1 Nettoyer la racine du projet

- [ ] Examiner tous les fichiers actuellement placés à la racine de `GWGUI.App` à partir de la cartographie de la tâche 01.
  - [ ] Laisser à la racine uniquement les fichiers qui appartiennent réellement au démarrage ou à la définition du projet.
  - [ ] Ranger les fenêtres secondaires selon leur fonction.
    - [ ] Fenêtre À propos.
    - [ ] Fenêtres de conflits de Lecture et Conversion.
    - [ ] Fenêtre des problèmes de l’Explorateur.
    - [ ] Fenêtres liées aux outils GW.
    - [ ] Fenêtre de matériel indisponible.
    - [ ] Fenêtre d’historique des journaux.
    - [ ] Fenêtre de nommage des profils.
    - [ ] Fenêtres et vues SCP.
  - [ ] Ranger les contrôles actuellement isolés à la racine dans leur fonction réelle.
  - [ ] Ranger `StoragePaths` et `ThemeManager` selon leur responsabilité approuvée dans la structure cible.
  - [ ] Mettre à jour les namespaces, références XAML et ressources après chaque groupe de déplacements.

#### 2.2.2 Réorganiser les composants visuels

- [ ] Remplacer le dossier `Controls` unique par la structure fonctionnelle validée.
  - [ ] Séparer les composants communs réutilisables.
  - [ ] Regrouper les composants de Lecture.
  - [ ] Regrouper les composants d’Écriture.
  - [ ] Regrouper les composants de Conversion.
  - [ ] Regrouper les composants du Visualisateur.
  - [ ] Regrouper les composants de l’Explorateur.
  - [ ] Regrouper les composants des Outils.
  - [ ] Regrouper les composants des Options.
  - [ ] Regrouper menu, terminal, barre d’état et progression selon la structure validée.
  - [ ] Vérifier que chaque composant XAML et son code-behind restent ensemble.
  - [ ] Vérifier que les composants réutilisés n’embarquent pas l’état d’un onglet particulier.

#### 2.2.3 Reprendre complètement `MainWindow`

- [ ] Établir dans le document de structure la liste des responsabilités encore présentes dans `MainWindow.xaml.cs`.
  - [ ] Initialisation de la fenêtre.
  - [ ] Navigation entre les onglets.
  - [ ] Lecture.
  - [ ] Écriture.
  - [ ] Conversion.
  - [ ] Visualisation.
  - [ ] Exploration.
  - [ ] Outils et maintenance.
  - [ ] Profils.
  - [ ] Matériel et sélection du lecteur.
  - [ ] Exécution et arrêt des commandes.
  - [ ] Terminal et journaux.
  - [ ] Progression et barre d’état.
  - [ ] Placement et dimensions de la fenêtre.
  - [ ] Synchronisation d’une image entre Explorateur et Visualisateur.
- [ ] Attribuer un propriétaire cible à chacune de ces responsabilités.
- [ ] Extraire la logique de chaque fonction hors de `MainWindow` dans l’ordre défini par le document de structure.
  - [ ] Déplacer son état.
  - [ ] Déplacer ses traitements.
  - [ ] Déplacer ses gestionnaires d’événements.
  - [ ] Exposer uniquement les commandes, données et événements nécessaires à la fenêtre principale.
  - [ ] Remplacer les accès directs de `MainWindow` aux contrôles internes par l’interface publique du composant concerné.
  - [ ] Vérifier qu’aucun gestionnaire n’est abonné deux fois.
  - [ ] Vérifier qu’aucun service ou composant n’est construit deux fois.
  - [ ] Supprimer de `MainWindow` l’ancien code après raccordement du nouveau propriétaire.
- [ ] Réduire le rôle final de `MainWindow` à la composition et à la coordination réellement globale.
  - [ ] Conserver la création des grands blocs de la fenêtre.
  - [ ] Conserver uniquement la navigation globale qui ne dépend d’aucun onglet particulier.
  - [ ] Conserver uniquement les échanges nécessaires entre deux fonctions distinctes.
  - [ ] Documenter les responsabilités qui doivent encore y rester et pourquoi.

#### 2.2.4 Reprendre `OptionsWindow`

- [ ] Vérifier les responsabilités restantes dans `OptionsWindow.xaml.cs` sans refaire celles déjà correctement extraites.
  - [ ] Général.
  - [ ] Contrôleurs et lecteurs.
  - [ ] Profils.
  - [ ] Journaux.
  - [ ] Host Tools.
  - [ ] Tags.
  - [ ] Sauvegarde immédiate et fermeture.
- [ ] Déplacer chaque responsabilité restante vers le composant ou contrôleur approuvé.
- [ ] Conserver dans la fenêtre uniquement la composition des pages et la fermeture globale.
- [ ] Vérifier que les changements automatiques restent appliqués sans bouton Enregistrer général.

#### 2.2.5 Réorganiser services et ViewModels de l’application

- [ ] Classer tous les fichiers de `Services` selon leur consommateur et leur portée.
  - [ ] Services réellement globaux.
  - [ ] Services de Lecture.
  - [ ] Services d’Écriture.
  - [ ] Services de Conversion.
  - [ ] Services du Visualisateur.
  - [ ] Services de l’Explorateur.
  - [ ] Services matériels.
  - [ ] Services de fenêtre et navigation.
  - [ ] Services d’exécution, progression, terminal et journaux.
- [ ] Classer tous les fichiers de `ViewModels` selon leur fonction.
- [ ] Vérifier qu’un ViewModel ne manipule pas directement un contrôle WPF.
- [ ] Vérifier qu’un service spécifique à un onglet n’est pas présenté comme service global.
- [ ] Vérifier que les services globaux ne dépendent pas d’un onglet concret.

### 2.3 Réorganiser `GWGUI.MediaEngine`

#### 2.3.1 Reprendre entièrement le dossier `Images`

- [x] Classer chaque fichier actuellement présent directement dans `Images`.
  - [x] Lecteurs de conteneurs et images sectorielles.
  - [x] Géométries et signatures.
  - [x] Métadonnées, systèmes et protections.
  - [x] Exploration des images.
  - [x] Détection et interprétation.
  - [x] Normalisation des images reconnues.
  - [x] Conversion interne.
  - [x] Visualisation des images sectorielles.
  - [x] Contrats et résultats partagés.
- [x] Déplacer chaque fichier vers la catégorie approuvée dans la structure cible.
- [x] Créer des sous-dossiers par famille uniquement lorsque plusieurs fichiers spécialisés de cette famille doivent rester ensemble.
  - [x] Acorn/BBC.
  - [x] Amiga.
  - [x] Amstrad.
  - [x] Apple.
  - [x] Atari.
  - [x] Commodore.
  - [x] DEC.
  - [x] Epson.
  - [x] IBM PC et compatibles.
  - [x] MSX.
  - [x] UCSD.
  - [x] Autres familles déjà prises en charge.
- [x] Ne pas ranger un lecteur commun dans une famille particulière uniquement parce qu’elle a été la première à l’utiliser.
- [x] Vérifier que l’ajout d’un nouveau lecteur possède un point d’enregistrement clairement identifié.

#### 2.3.2 Reprendre les conteneurs

- [x] Vérifier chaque politique de `Images/Containers`.
  - [x] Un conteneur doit seulement reconnaître et ouvrir son type d’image.
  - [x] Une politique de conteneur ne doit pas analyser le système de fichiers à la place du module prévu.
  - [x] Une politique générique ne doit pas porter le nom d’une machine particulière.
  - [x] Les délégations doivent avoir une destination unique et visible.
- [x] Regrouper les contrats, contextes, registres et implémentations sans les mélanger.

#### 2.3.3 Reprendre la détection et l’interprétation

- [x] Vérifier séparément les responsabilités de `ScpDetection` et `Interpretations`.
  - [x] Détection des familles compatibles.
  - [x] Production de plusieurs résultats compatibles pour une image multiformat.
  - [x] Classement des résultats sans supprimer les autres résultats valides.
  - [x] Interprétation supplémentaire d’une image déjà ouverte.
  - [x] Normalisation des résultats reconnus.
- [x] Retirer les essais successifs inutiles lorsqu’un conteneur ou une famille permet de limiter les candidats.
- [x] Conserver plusieurs candidats lorsque le support peut réellement contenir plusieurs systèmes.
- [x] Vérifier que la sélection manuelle choisit le traitement demandé sans détruire la capacité multiformat de la détection automatique.
- [x] Documenter le chemin automatique et le chemin manuel après leur réorganisation.

#### 2.3.4 Reprendre `SectorImages`

- [x] Classer tous les lecteurs et reconstructeurs sectoriels par responsabilité réelle.
  - [x] Contrats et modèles sectoriels communs.
  - [x] Reconstruction ISO FM/MFM commune.
  - [x] Politiques ISO propres aux machines.
  - [x] Reconstruction Apple.
  - [x] Reconstruction Commodore.
  - [x] Reconstruction DEC.
  - [x] Autres reconstructions spécialisées.
- [x] Vérifier `AtariScpSectorImageReader.cs` comme exemple, puis contrôler tous les autres lecteurs.
  - [x] Ne conserver dans un lecteur Atari que les décisions propres à Atari.
  - [x] Placer Amstrad dans son module.
  - [x] Placer BBC/Acorn dans son module.
  - [x] Placer Epson dans son module.
  - [x] Placer IBM PC dans son module.
  - [x] Placer UCSD dans son module.
  - [x] Placer les opérations ISO communes dans un composant au nom neutre.
- [x] Rechercher le même défaut dans chaque autre lecteur nommé selon une machine.
- [x] Corriger chaque nom qui ne représente pas le contenu réel du fichier.
- [x] Vérifier qu’une règle de géométrie spécialisée ne fuit pas dans le constructeur sectoriel commun.

#### 2.3.5 Reprendre décodage et encodage

- [x] Vérifier l’organisation complète de `Decoding`.
  - [x] Un décodeur spécialisé par fichier.
  - [x] Bases partagées séparées des décodeurs concrets.
  - [x] Modèles et contrats séparés des implémentations.
  - [x] Registre unique des décodeurs disponibles.
- [x] Vérifier l’organisation complète de `Encoding`.
  - [x] Un encodeur spécialisé par fichier.
  - [x] Bases partagées séparées des encodeurs concrets.
  - [x] Modèles et contrats séparés des implémentations.
  - [x] Registre unique des encodeurs disponibles.
- [x] Comparer les registres de décodage et d’encodage sans supposer que toutes les capacités sont forcément symétriques.
- [x] Vérifier que les primitives partagées ne contiennent aucune décision propre à une machine.
- [x] Vérifier que les implémentations spécialisées ne recopient pas une primitive déjà disponible.

#### 2.3.6 Reprendre les systèmes de fichiers

- [x] Réorganiser `FileSystems` pour distinguer clairement contrats, modèles, registre, aides communes et lecteurs concrets.
- [x] Classer les lecteurs concrets selon la structure cible approuvée.
- [x] Vérifier qu’un lecteur de système de fichiers ne prend pas en charge un conteneur.
- [x] Vérifier qu’un lecteur de système de fichiers ne décide pas du rendu visuel.
- [x] Vérifier qu’une aide commune n’encode pas une règle propre à une seule famille.
- [x] Vérifier que le registre est l’unique point de découverte des lecteurs disponibles.

#### 2.3.7 Reprendre la visualisation technique

- [x] Vérifier la séparation entre reconstruction des données et classification visuelle.
- [x] Classer les politiques de visualisation selon la structure cible.
- [x] Conserver dans le registre le point unique de sélection de la politique.
- [x] Vérifier que les couleurs et catégories visuelles ne modifient pas le résultat du décodage.
- [x] Vérifier que le Visualisateur ne connaît pas directement chaque lecteur concret.

#### 2.3.8 Reprendre les modèles SCP et primitives

- [x] Vérifier la séparation entre lecteur SCP, modèles SCP, informations de capture et constantes de structure.
- [x] Vérifier que `ScpReader` ne contient que la lecture du conteneur SCP.
- [x] Vérifier que les primitives de bits et CRC ne dépendent ni de l’interface ni d’une machine précise.
- [ ] Signaler dans la tâche 03 les constantes encore mal placées sans les traiter dans cette phase.
- [ ] Signaler dans la tâche 04 les modèles ou contrats encore mélangés sans les traiter dans cette phase.

### 2.4 Réorganiser `GWGUI.Domain`

#### 2.4.1 Vérifier les frontières fonctionnelles

- [ ] Examiner chaque dossier du domaine à partir de la cartographie validée.
  - [ ] Commandes.
  - [ ] Conversion.
  - [ ] Formats.
  - [ ] Matériel.
  - [ ] Host Tools.
  - [ ] Maintenance.
  - [ ] Nommage.
  - [ ] Profils.
  - [ ] Lecture.
  - [ ] Réglages.
  - [ ] Écriture.
- [ ] Vérifier que chaque fichier appartient réellement à son dossier.
- [ ] Déplacer les fichiers mal rangés vers la fonction qui les possède.
- [ ] Vérifier qu’aucun fichier du domaine ne dépend de WPF.
- [ ] Vérifier qu’aucun fichier du domaine ne dépend d’une implémentation Windows ou d’un stockage concret.

#### 2.4.2 Reprendre les commandes et opérations

- [ ] Distinguer construction, validation, planification et exécution des commandes.
- [ ] Vérifier séparément Lecture, Écriture et Conversion.
- [ ] Vérifier que les options communes ne sont pas recopiées dans chaque constructeur.
- [ ] Vérifier que les différences propres aux opérations restent dans leur fonction.
- [ ] Vérifier que la compatibilité d’un format ne dépend pas d’un contrôle WPF.

#### 2.4.3 Reprendre formats et capacités

- [ ] Vérifier les responsabilités du catalogue intégré, du catalogue tenant compte de GW et des modèles de format.
- [ ] Conserver une source commune pour les formats proposés à Lecture, Écriture, Conversion, Explorateur et Visualisateur.
- [ ] Vérifier que les capacités propres à GW restent distinctes des capacités internes de GW GUI.
- [ ] Vérifier que les définitions de disquette intégrées ont un propriétaire unique.

#### 2.4.4 Reprendre réglages, profils et matériel

- [ ] Vérifier que les réglages sont répartis par domaine sans recréer un fichier monolithique.
- [ ] Vérifier que les profils restent séparés par opération.
- [ ] Vérifier que la description physique d’un lecteur ne devient pas une option GW.
- [ ] Vérifier que routage matériel, registre matériel et découverte Windows restent séparés.

### 2.5 Réorganiser `GWGUI.Infrastructure`

#### 2.5.1 Vérifier chaque implémentation technique

- [ ] Classer les implémentations par domaine technique.
  - [ ] Découverte matérielle Windows.
  - [ ] Registre matériel Greaseweazle.
  - [ ] Installation et capacités Host Tools.
  - [ ] Exécution des processus.
  - [ ] Journaux d’opération.
  - [ ] Stockage des réglages.
- [ ] Vérifier que chaque implémentation réalise un contrat du domaine ou un besoin clairement identifié de l’application.
- [ ] Vérifier que l’infrastructure ne contient aucune décision d’affichage WPF.
- [ ] Vérifier que l’infrastructure ne décide pas du format métier à la place des catalogues du domaine.
- [ ] Vérifier que les classes propres à Windows sont identifiables par leur emplacement et leur nom.

### 2.6 Supprimer les duplications architecturales

#### 2.6.1 Rechercher les doubles chemins

- [ ] Rechercher les responsabilités possédant encore deux implémentations après les déplacements.
  - [ ] Détection de format.
  - [ ] Catalogue des formats.
  - [ ] Sélection machine/format/protection.
  - [ ] Ouverture d’image.
  - [ ] Reconstruction sectorielle.
  - [ ] Exploration des systèmes de fichiers.
  - [ ] Classification visuelle.
  - [ ] Gestion des profils.
  - [ ] Exécution des commandes.
  - [ ] Progression.
  - [ ] Terminal et journaux.
  - [ ] Placement des fenêtres.
- [ ] Désigner un seul propriétaire pour chaque responsabilité dupliquée.
- [ ] Raccorder tous les consommateurs au propriétaire retenu.
- [ ] Supprimer l’ancien chemin après le raccordement.

#### 2.6.2 Rechercher les algorithmes recopiés

- [ ] Identifier les blocs identiques ou presque identiques entre familles.
- [ ] Distinguer une vraie primitive commune d’une ressemblance accidentelle.
- [ ] Extraire uniquement les primitives réellement communes.
- [ ] Paramétrer les différences simples déjà représentées par un identifiant de machine ou de format.
- [ ] Conserver des implémentations séparées lorsque les règles du format diffèrent réellement.
- [ ] Reporter dans les tâches 03, 04 ou 05 les changements qui appartiennent aux constantes, modèles, contrats, fonctions ou services.

### 2.7 Réorganiser les tests sans les multiplier inutilement

#### 2.7.1 Ranger les tests selon le code de production

- [ ] Définir dans le document de structure l’arborescence cible des tests.
- [ ] Regrouper les tests par projet et fonction de production.
- [ ] Séparer les tests unitaires ciblés des tests utilisant le corpus d’images.
- [ ] Conserver les images externes hors du dépôt selon les règles déjà décidées.
- [ ] Ne pas créer plusieurs tests qui vérifient exactement le même comportement.

#### 2.7.2 Définir des blocs de contrôle rapides

- [ ] Définir un bloc de contrôle pour `GWGUI.App`.
- [ ] Définir un bloc de contrôle pour `GWGUI.Domain`.
- [ ] Définir un bloc de contrôle pour `GWGUI.Infrastructure`.
- [ ] Définir un bloc de contrôle pour `GWGUI.MediaEngine`.
- [ ] Définir un bloc ciblé pour la détection multiformat.
- [ ] Définir un bloc ciblé pour les registres de formats, décodeurs, encodeurs et systèmes de fichiers.
- [ ] Utiliser le bloc concerné après une série cohérente de déplacements.
- [ ] Réserver la compilation complète et la suite complète à la clôture d’un grand bloc ou de la phase.

### 2.8 Contrôler chaque bloc de refactorisation

#### 2.8.1 Contrôle après chaque tâche autonome

- [ ] Vérifier uniquement les fichiers et dépendances modifiés.
- [ ] Exécuter le bloc de tests ciblé correspondant.
- [ ] Vérifier qu’aucun abonnement, enregistrement ou appel n’est dupliqué.
- [ ] Vérifier que l’ancien chemin a été supprimé lorsqu’il n’est plus utilisé.
- [ ] Mettre à jour le document de structure si la destination finale a dû être corrigée.
- [ ] Cocher immédiatement les sous-tâches réellement terminées.
- [ ] Créer le commit de la tâche autonome terminée.

#### 2.8.2 Contrôle à la fin d’un bloc cohérent

- [ ] Compiler les projets concernés ensemble.
- [ ] Exécuter les groupes de tests concernés ensemble.
- [ ] Contrôler les dépendances entre projets.
- [ ] Vérifier que le comportement observable est conservé.
- [ ] Pousser le bloc complet.

### 2.9 Clôturer réellement la phase 02

#### 2.9.1 Vérifier la structure finale

- [ ] Comparer l’arborescence obtenue à la structure cible validée.
- [ ] Expliquer dans le document toute différence conservée volontairement.
- [ ] Vérifier qu’aucun fichier de production n’a été oublié dans la table de déplacement.
- [ ] Vérifier que les racines des projets ne contiennent plus de fichiers mal rangés.
- [ ] Vérifier que les dossiers génériques ne sont plus des dossiers fourre-tout.
- [ ] Vérifier que `MainWindow` est réellement limité à son rôle final documenté.
- [ ] Vérifier que `OptionsWindow` est réellement limité à son rôle final documenté.
- [ ] Vérifier que `Images` est réellement structuré par responsabilités.
- [ ] Vérifier que les comportements multiformats sont toujours présents.

#### 2.9.2 Validation finale de la phase

- [ ] Compiler la solution complète une seule fois pour la clôture.
- [ ] Exécuter la suite de tests complète une seule fois pour la clôture.
- [ ] Corriger les régressions provoquées par le refactor.
- [ ] Refaire uniquement les contrôles concernés par une correction de clôture.
- [ ] Mettre à jour la documentation d’architecture avec la structure réellement obtenue.
- [ ] Vérifier que toutes les cases correspondent à un travail réellement terminé.
- [ ] Créer le commit final de documentation de la phase.
- [ ] Pousser le bloc complet de la phase 02.

### Résultat attendu

La phase 02 sera terminée uniquement lorsque :

- la structure cible aura été écrite, contrôlée et validée avant les déplacements ;
- chaque fichier de production aura une destination et une responsabilité claires ;
- `MainWindow` et `OptionsWindow` ne concentreront plus les traitements propres aux fonctions ;
- `GWGUI.App`, `GWGUI.Domain`, `GWGUI.Infrastructure` et `GWGUI.MediaEngine` seront rangés selon leurs responsabilités réelles ;
- les dossiers génériques visibles aujourd’hui ne seront plus des listes plates de fichiers sans organisation suffisante ;
- la détection automatique, la sélection manuelle et les disquettes multiformats conserveront leur comportement ;
- les contrôles auront été effectués par blocs rapides, puis une fois complètement à la clôture ;
- la documentation représentera exactement le code final.

## Phase 03 — Constantes et textes techniques

### 3.1 Constantes

- [x] Inventorier toutes les valeurs fixes trouvées pendant l’audit.
- [ ] Créer les fichiers de constantes nécessaires pour chaque domaine identifié par l’audit.
- [ ] Séparer notamment identifiants de machines, formats, codecs, protections, systèmes de fichiers, conteneurs, extensions, commandes `gw`, géométries et contrôles d’intégrité.
- [ ] Remplacer les chaînes et nombres recopiés par ces définitions.
- [ ] Documenter la source des valeurs techniques non évidentes.
- [x] Intégrer dans les données embarquées toute définition spéciale de disquette nécessaire au produit.

### 3.2 Aucun texte brut visible

- [x] Retirer tout libellé, message, infobulle, titre ou erreur visible écrit directement dans C# ou XAML.
- [x] Envoyer chaque texte utilisateur vers la ressource de traduction de son domaine.
- [ ] Garder les noms techniques non traduisibles dans un catalogue neutre commun.
- [ ] Vérifier que le même nom technique n’est pas recopié dans trente langues s’il doit rester identique.
- [x] Distinguer messages de journaux techniques et messages destinés à l’utilisateur.

### 3.3 Contrôles

- [ ] Ajouter un contrôle des identifiants utilisés mais absents des catalogues.
- [ ] Ajouter un contrôle des constantes dupliquées.
- [ ] Ajouter un contrôle des textes visibles codés en dur avec une liste blanche technique documentée.

## Phase 04 — Enums, modèles et contrats

### 4.1 Modèles de données

- [ ] Créer des DTO ou records distincts pour machine, format, géométrie, média, codec, protection, conteneur et système de fichiers.
- [ ] Créer des modèles distincts pour piste physique, secteur décodé, intégrité, révolution et avertissement.
- [ ] Séparer les modèles publics partagés des structures internes propres à un algorithme.
- [ ] Ajouter aux modèles les métadonnées nécessaires aux images multiformats et protégées.
- [ ] Définir les invariants et validations à la construction des données.

### 4.2 Enums et identifiants extensibles

- [ ] Utiliser des enums pour les ensembles fermés et stables : état d’intégrité, type de média, face, état d’opération et capacité.
- [ ] Utiliser des identifiants catalogués pour les formats, protections et définitions extensibles.
- [ ] Vérifier que les formats provenant des `diskdefs` et autres définitions extensibles restent catalogués dynamiquement.

### 4.3 Interfaces

- [ ] Définir des contrats distincts pour lecture/écriture de conteneur, décodage/encodage de piste, reconstruction sectorielle, système de fichiers, détection, visualisation et conversion.
- [ ] Justifier chaque nouvelle interface par une frontière réelle ou par des implémentations interchangeables.
- [ ] Vérifier les dépendances autorisées entre projets.

## Phase 05 — Fonctions et services

- [ ] Relever les fonctions trop longues ou qui réalisent plusieurs opérations indépendantes.
- [ ] Extraire les fonctions communes dans le service propriétaire de la responsabilité, pas dans un fourre-tout `Helpers`.
- [ ] Séparer parsing, validation, transformation, sélection et présentation.
- [ ] Regrouper dans un même fichier les petites fonctions qui constituent une seule primitive cohérente.
- [ ] Isoler les accès disque, processus, réglages, journaux et dialogues derrière les services existants ou des contrats justifiés.
- [ ] Réutiliser le même résultat d’analyse entre Explorateur et Visualisateur.
- [ ] Annuler immédiatement l’analyse précédente lorsqu’une nouvelle image est chargée.
- [ ] Vérifier que chaque service a un propriétaire, une responsabilité et des tests ciblés.

## Phase 06 — Traductions

### 6.1 Arborescence

- [x] Organiser les ressources sous `Resources/00-Base` pour les catalogues neutres et sous un dossier par culture distribuée.
- [x] Séparer les ressources dans les 21 catalogues actuels : `About`, `Actions`, `Advanced`, `Common`, `Conversion`, `Emulation`, `Errors`, `Explorer`, `ExplorerWarnings`, `Formats`, `Hardware`, `HostTools`, `Logs`, `Menus`, `Options`, `Profiles`, `Read`, `Shell`, `Tools`, `Visualizer` et `Write`.
- [x] Fournir pour chaque catalogue une ressource neutre et une ressource par culture distribuée.
- [ ] Garder dans `Common` uniquement ce qui est réellement commun et basique.
- [ ] Placer une erreur spécialisée dans son catalogue fonctionnel ou dans `Errors` uniquement lorsqu’elle est réellement partagée.

### 6.2 Chargeur de ressources

- [x] Charger les catalogues actuels sans changer leurs clés.
- [x] Conserver le repli géré par `ResourceManager` vers la culture parente puis la ressource neutre.
- [x] Refuser les clés dupliquées entre catalogues lors de la construction de l’index.
- [ ] Vérifier le changement de langue immédiat dans toutes les fenêtres déjà ouvertes.
- [ ] Vérifier que le sélecteur conserve le nom natif de chaque langue.

### 6.3 Vérification

- [ ] Vérifier la parité des clés de toutes les langues.
- [ ] Vérifier les placeholders, retours à la ligne et valeurs vides.
- [ ] Détecter les corruptions d’encodage et le mojibake.
- [ ] Vérifier les langues de droite à gauche.
- [ ] Vérifier séparément les traductions de l’application et celles de l’installateur.
- [ ] Vérifier que les listes de formats des cinq fonctions utilisent les mêmes noms du catalogue central.

## Phase 07 — Interface, robustesse et maintenance

Ces tâches viennent après le refactor principal et avant la validation finale des images.

### 7.1 Interface

- [ ] Vérifier chaque fenêtre à taille normale, réduite, maximisée et avec plusieurs DPI.
- [ ] Vérifier qu’aucun contrôle ne dépasse de son cadre et qu’aucun texte n’est tronqué.
- [ ] Vérifier les défilements : visibles uniquement lorsque nécessaires et fonctionnement correct à la molette.
- [ ] Vérifier l’indépendance des composants réutilisés entre onglets.
- [ ] Vérifier les onglets, focus, survols, icônes, cadres, alignements et états désactivés.
- [ ] Reprendre le thème sombre plus tard, sans le mélanger au refactor fonctionnel.
- [ ] Vérifier la restauration de position et taille avant l’affichage de la fenêtre.
- [ ] Vérifier les silhouettes de disquettes et leur correspondance avec le média détecté.
- [ ] Vérifier les sélecteurs automatiques et manuels Machine, Format et Protection.

### 7.2 Performance

- [ ] Mesurer séparément conteneur, décodage, reconstruction, système de fichiers et rendu.
- [ ] Supprimer les analyses répétées inutiles.
- [ ] Mettre en cache uniquement les résultats immuables réutilisables.
- [ ] Afficher progressivement les données lorsque le traitement est long.
- [ ] Vérifier changement rapide d’image, annulation et fermeture pendant une analyse.
- [ ] Vérifier spécialement la détection multiformat sur les images de flux sans ralentir les images sectorielles ciblées.

### 7.3 Erreurs, journaux et persistance

- [x] Recenser les dialogues et états visibles qui présentaient le chemin du journal comme seule explication.
- [x] Centraliser l'interprétation des exceptions courantes sans exposer leur message technique non traduit.
- [x] Décrire les erreurs réseau, HTTP, disque, fichiers, permissions, données et état de l'application dans la langue sélectionnée.
- [x] Générer les 19 descriptions dans les 29 cultures distribuées avec Argos et vérifier leurs placeholders.
- [x] Tester en français le délai de connexion réseau et le fichier absent, sans mention du journal.
- [x] Terminer le groupe « descriptions localisées des erreurs ».
- [ ] Localiser tous les messages utilisateur et conserver le détail technique dans les journaux.
- [ ] Remplacer la boîte d’erreur d’un format simplement non reconnu par l’état d’interface prévu.
- [ ] Vérifier un journal par action, rotation, archivage et ouverture du dossier Logs.
- [ ] Vérifier nettoyage des fichiers temporaires et partiels.
- [ ] Vérifier les réglages absents, anciens, partiels ou corrompus.
- [ ] Ajouter et vérifier les migrations qui conservent les réglages existants.

### 7.4 Documentation et crédits

- [ ] Maintenir les documents actuels après chaque bloc terminé.
- [ ] Mettre à jour l’état du projet et la liste des formats réellement pris en charge.
- [ ] Maintenir les crédits, licences et liens des projets réellement utilisés ou étudiés.

