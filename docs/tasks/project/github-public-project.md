# Organisation GitHub et suivi public du projet — tâches

> **Plan reporté.** La création de l’organisation, le transfert du dépôt, les formulaires Issues et
> le Project GitHub demandent des changements importants. Ils ne bloquent ni le développement ni
> les publications actuelles et ne doivent commencer qu’après une nouvelle décision explicite.
> Les commits, pushs, tags et opérations GitHub décrits dans ce plan exigent aussi une demande
> explicite; les cases ne constituent pas une autorisation de les effectuer.

Ce document décrit le passage de `overthetop78/GW-GUI` vers une organisation GitHub dédiée, puis
la mise en place d’un suivi public fondé sur Issues et Projects. Les opérations sont ordonnées pour
ne pas casser les mises à jour de GW GUI, les modules, le wiki, les releases ni la publication du
SDK sur NuGet.org.

GitHub Pro reste attaché au compte personnel `overthetop78`. Une organisation GitHub Free suffit
pour les dépôts publics envisagés. Aucun abonnement GitHub Team ne doit être souscrit sans besoin
ultérieur explicite.

Les workflows de l’application et des modules utilisent déjà `${{ github.repository }}` pour leurs
URL de publication et s’adapteront au nouveau propriétaire. Les adresses écrites en dur dans le
code, les manifestes, les scripts et la documentation doivent en revanche être remplacées. La
stratégie NuGet Trusted Publishing doit également être réassociée au nouveau propriétaire GitHub.

- [ ] 1. Définir l’identité et la structure GitHub définitives
  - [ ] 1.1 Choisir l’organisation
    - [ ] 1.1.1 Fixer son identité publique
      - [ ] Créer `docs/project/github-organization.md` et y inscrire le nom affiché, l’identifiant GitHub disponible, l’adresse publique, le compte propriétaire `overthetop78` et le choix initial du plan GitHub Free.
    - [ ] 1.1.2 Fixer le périmètre de l’organisation
      - [ ] Modifier `docs/project/github-organization.md` pour préciser que l’organisation regroupe GW GUI, son SDK, ses modules officiels et leurs documents publics, sans accueillir les autres projets sans rapport avec GW GUI.
  - [ ] 1.2 Définir les dépôts cibles
    - [ ] 1.2.1 Définir le dépôt principal
      - [ ] Modifier `docs/project/github-organization.md` pour inscrire `<ORGANISATION>/GW-GUI` comme dépôt de l’application, de son catalogue de mise à jour et de son wiki.
    - [ ] 1.2.2 Définir la destination du SDK
      - [ ] Modifier `docs/project/github-organization.md` pour décider si le code du SDK reste d’abord dans `GW-GUI` ou rejoint ultérieurement `<ORGANISATION>/GWGUI.Emulation.SDK`, tout en conservant `GWGUI.Emulation.SDK` comme identifiant NuGet.
    - [ ] 1.2.3 Définir la destination des modules officiels
      - [ ] Modifier `docs/project/github-organization.md` pour inscrire les dépôts prévus pour Amiga et Atari, avec un dépôt et un catalogue de mises à jour propres à chaque module, puis réserver la même convention aux futurs modules officiels.

- [ ] 2. Créer l’organisation sans déplacer encore le dépôt
  - [ ] 2.1 Créer et configurer l’organisation
    - [ ] 2.1.1 Créer l’espace GitHub
      - [ ] Créer l’organisation depuis le compte `overthetop78`, conserver le plan GitHub Free et modifier `docs/project/github-organization.md` pour consigner son URL et la date de création.
    - [ ] 2.1.2 Configurer son identité visible
      - [ ] Créer le fichier `profile/README.md` dans le dépôt public spécial `<ORGANISATION>/.github` avec une présentation courte de GW GUI, puis inscrire son URL dans `docs/project/github-organization.md`.
    - [ ] 2.1.3 Conserver l’administration du projet
      - [ ] Modifier `docs/project/github-organization.md` pour consigner `overthetop78` comme propriétaire de l’organisation et administrateur du dépôt principal.

- [ ] 3. Préparer le dépôt au changement de propriétaire
  - [ ] 3.1 Remplacer les adresses utilisées par l’application
    - [ ] 3.1.1 Modifier la source de mise à jour de GW GUI
      - [ ] Modifier `src/GWGUI.App/Constants/Updates/UpdateEndpoints.cs` pour faire pointer le catalogue de l’application vers `<ORGANISATION>/GW-GUI`.
    - [ ] 3.1.2 Modifier l’adresse du guide utilisateur
      - [ ] Modifier `src/GWGUI.App/Services/Documentation/UserGuideLocator.cs` pour ouvrir le wiki de `<ORGANISATION>/GW-GUI`.
    - [ ] 3.1.3 Aligner les tests des adresses publiques
      - [ ] Modifier `tests/GWGUI.Tests/Interface/Help/HelpTargetScenarios.cs` pour vérifier les nouvelles URL du wiki.
  - [ ] 3.2 Remplacer les adresses des modules officiels
    - [ ] 3.2.1 Modifier la source du module Amiga
      - [ ] Modifier `src/GWGUI.Emulation.Amiga/module.json` pour utiliser le catalogue Amiga encore publié par `<ORGANISATION>/GW-GUI`, sans anticiper son futur dépôt indépendant.
    - [ ] 3.2.2 Modifier la source du module Atari
      - [ ] Modifier `src/GWGUI.Emulation.Atari/module.json` pour utiliser le catalogue Atari encore publié par `<ORGANISATION>/GW-GUI`, sans anticiper son futur dépôt indépendant.
  - [ ] 3.3 Remplacer les adresses des outils de publication
    - [ ] 3.3.1 Modifier les scripts du wiki
      - [ ] Modifier `scripts/build-wiki.ps1` et `scripts/publish-wiki.ps1` pour lire, publier et afficher le wiki sous `<ORGANISATION>/GW-GUI`.
    - [ ] 3.3.2 Modifier les métadonnées de l’application
      - [ ] Modifier `installer/GWGUI.iss` et `src/GWGUI.App/GWGUI.App.csproj` pour utiliser l’organisation comme éditeur ou adresse de dépôt, sans remplacer le titulaire du copyright sans décision explicite.
    - [ ] 3.3.3 Modifier les métadonnées du SDK
      - [ ] Modifier `src/GWGUI.Emulation/GWGUI.Emulation.csproj` pour utiliser la nouvelle URL de dépôt tout en conservant le propriétaire et l’identifiant actuels du paquet NuGet tant qu’ils ne sont pas transférés sur NuGet.org.
  - [ ] 3.4 Mettre à jour les liens documentaires actifs
    - [ ] 3.4.1 Modifier les liens d’entrée du projet
      - [ ] Modifier `README.md` et `src/GWGUI.Emulation/README.md` pour remplacer les liens de releases, wiki, modèle de module et guides par leurs adresses définitives.
    - [ ] 3.4.2 Modifier la documentation de publication
      - [ ] Modifier `.codex/config.toml`, `docs/project/release.md` et `docs/architecture/emulation-sdk-versioning.md` pour employer le nouveau propriétaire GitHub dans les procédures de release et de Trusted Publishing.
    - [ ] 3.4.3 Modifier les documents actuels sur les modules indépendants
      - [ ] Modifier `docs/architecture/emulation-modules.md`, `docs/architecture/emulation-module-updates.md`, `docs/project/release.md` et `docs/tasks/emulation/remaining-validations.md` pour remplacer les URL devenues anciennes sans altérer les résultats déjà inscrits.
    - [ ] 3.4.4 Réviser les anciennes notes de version
      - [ ] Modifier les fichiers concernés sous `.github/release-notes` pour que leurs liens de comparaison et de wiki utilisent le dépôt transféré, tout en conservant leur contenu historique.
  - [ ] 3.5 Contrôler l’inventaire des anciennes adresses
    - [ ] 3.5.1 Éliminer les références actives restantes
      - [ ] Modifier `docs/project/github-organization.md` pour y inscrire le résultat d’une recherche complète de `overthetop78/GW-GUI`, en distinguant les références remplacées du nom d’auteur ou du copyright volontairement conservé.

- [ ] 4. Préparer un état transférable et traçable
  - [ ] 4.1 Valider le dépôt avant transfert
    - [ ] 4.1.1 Vérifier les modifications GitHub et documentaires
      - [ ] Créer `docs/tasks/project/github-public-project-validation.md` avec le résultat de `git diff --check`, des tests ciblés sur les URL, du build Debug standard et de la vérification du dossier `Modules` vide.
    - [ ] 4.1.2 Identifier le commit à transférer
      - [ ] Modifier `docs/tasks/project/github-public-project-validation.md` après le commit et le push sur `main` pour inscrire le hash transféré et confirmer que le dépôt distant contient toutes les modifications préparées.

- [ ] 5. Transférer `GW-GUI` vers l’organisation
  - [ ] 5.1 Effectuer le transfert GitHub
    - [ ] 5.1.1 Changer le propriétaire du dépôt
      - [ ] Transférer `overthetop78/GW-GUI` vers `<ORGANISATION>/GW-GUI`, puis modifier `docs/tasks/project/github-public-project-validation.md` pour inscrire l’URL obtenue et confirmer la présence du code, des branches, tags, releases, Issues, pull requests et paramètres.
    - [ ] 5.1.2 Corriger le dépôt distant local
      - [ ] Modifier la configuration Git locale `remote.origin.url` vers `https://github.com/<ORGANISATION>/GW-GUI.git`, puis inscrire la valeur contrôlée dans `docs/tasks/project/github-public-project-validation.md`.
  - [ ] 5.2 Vérifier les services GitHub transférés
    - [ ] 5.2.1 Vérifier GitHub Actions
      - [ ] Modifier `docs/tasks/project/github-public-project-validation.md` pour consigner l’état actif de `.github/workflows/release.yml`, `.github/workflows/module-release.yml` et `.github/workflows/sdk-release.yml`, leurs permissions et la présence de la variable `NUGET_USER`.
    - [ ] 5.2.2 Vérifier le wiki et les releases
      - [ ] Modifier `docs/tasks/project/github-public-project-validation.md` avec les URL répondant correctement pour le wiki, les releases existantes et les tags techniques de catalogues.
    - [ ] 5.2.3 Vérifier les redirections de l’ancienne adresse
      - [ ] Modifier `docs/tasks/project/github-public-project-validation.md` avec le résultat des anciennes URL GitHub et confirmer que la nouvelle adresse reste la seule inscrite dans le code actif.

- [ ] 6. Réassocier NuGet Trusted Publishing
  - [ ] 6.1 Mettre à jour la stratégie OIDC
    - [ ] 6.1.1 Autoriser le dépôt transféré
      - [ ] Modifier ou recréer sur NuGet.org la stratégie Trusted Publishing avec `<ORGANISATION>` comme propriétaire GitHub, `GW-GUI` comme dépôt et `sdk-release.yml` comme workflow, puis consigner exactement ces valeurs dans `docs/tasks/project/github-public-project-validation.md`.
    - [ ] 6.1.2 Conserver le propriétaire NuGet décidé
      - [ ] Modifier `docs/project/github-organization.md` pour préciser si `GWGUI.Emulation.SDK` reste la propriété NuGet de `overthetop78` ou est transféré à une organisation NuGet distincte, puis aligner la stratégie Trusted Publishing sur ce choix.
    - [ ] 6.1.3 Valider lors de la prochaine vraie version du SDK
      - [ ] Modifier `docs/tasks/project/github-public-project-validation.md` après la prochaine publication utile du SDK pour inscrire la réussite OIDC depuis le nouveau dépôt, sans créer une version NuGet factice uniquement pour ce contrôle.

- [ ] 7. Mettre en place les formulaires d’Issues
  - [ ] 7.1 Créer le formulaire de rapport de bug
    - [ ] 7.1.1 Demander les informations nécessaires au diagnostic
      - [ ] Créer `.github/ISSUE_TEMPLATE/bug-report.yml` avec les champs composant concerné, version de GW GUI, version du module, version de Windows, étapes de reproduction, résultat obtenu, résultat attendu, journaux et captures facultatives.
  - [ ] 7.2 Créer le formulaire de demande d’amélioration
    - [ ] 7.2.1 Faire décrire le besoin avant la solution
      - [ ] Créer `.github/ISSUE_TEMPLATE/feature-request.yml` avec les champs composant concerné, problème rencontré, comportement souhaité, exemple d’usage et informations complémentaires.
  - [ ] 7.3 Configurer l’écran de création d’Issue
    - [ ] 7.3.1 Présenter uniquement les entrées utiles
      - [ ] Créer `.github/ISSUE_TEMPLATE/config.yml` pour afficher les deux formulaires, conserver ou refuser les Issues libres selon la décision inscrite dans `docs/project/github-organization.md` et fournir les liens d’aide réellement disponibles.
  - [ ] 7.4 Ajouter les libellés associés
    - [ ] 7.4.1 Créer une classification minimale
      - [ ] Créer sur GitHub les labels `bug`, `enhancement`, `application`, `sdk`, `module`, `amiga`, `atari` et `needs-triage`, puis modifier `docs/project/github-organization.md` pour documenter leur usage.

- [ ] 8. Créer le Project de suivi de GW GUI
  - [ ] 8.1 Créer le tableau central
    - [ ] 8.1.1 Définir les colonnes de travail
      - [ ] Créer dans l’organisation un Project consacré à GW GUI avec les états `À étudier`, `À faire`, `En cours`, `À tester` et `Terminé`, puis inscrire son URL dans `docs/project/github-organization.md`.
    - [ ] 8.1.2 Définir les champs de classement
      - [ ] Ajouter au Project les champs `Zone`, `Priorité` et `Version visée`, avec les valeurs de zone `Application`, `SDK`, `Amiga`, `Atari`, `Autre module`, `Publication` et `Documentation`, puis les documenter dans `docs/project/github-organization.md`.
    - [ ] 8.1.3 Créer les vues utiles
      - [ ] Créer les vues `Tableau`, `Feuille de route` et `Par module`, puis modifier `docs/project/github-organization.md` pour expliquer le rôle et le filtre de chacune.
  - [ ] 8.2 Automatiser le suivi courant
    - [ ] 8.2.1 Ajouter automatiquement les nouveaux éléments
      - [ ] Configurer le Project pour ajouter les nouvelles Issues et pull requests de `<ORGANISATION>/GW-GUI`, puis consigner la règle dans `docs/project/github-organization.md`.
    - [ ] 8.2.2 Synchroniser les états simples
      - [ ] Configurer le Project pour placer les éléments fermés dans `Terminé`, puis consigner cette automatisation dans `docs/project/github-organization.md`.

- [ ] 9. Migrer les tâches Markdown vers Issues et Projects
  - [ ] 9.1 Inventorier les travaux encore ouverts
    - [ ] 9.1.1 Établir la correspondance sans perdre de tâche
      - [ ] Créer `docs/project/github-task-migration.md` avec chaque document sous `docs/tasks`, ses cases encore ouvertes, l’Issue cible, son état de migration et le document technique à conserver éventuellement.
  - [ ] 9.2 Créer les Issues dans l’ordre des tâches existantes
    - [ ] 9.2.1 Migrer chaque ensemble cohérent
      - [ ] Créer une Issue par travail cohérent encore ouvert, utiliser des sous-Issues lorsque la hiérarchie existante l’exige, ajouter chaque Issue au Project et modifier `docs/project/github-task-migration.md` après chaque création avec son numéro et son URL.
    - [ ] 9.2.2 Conserver les spécifications nécessaires
      - [ ] Déplacer sous `docs/architecture`, `docs/reference` ou `docs/future` le contenu technique durable qui ne constitue pas une tâche, puis modifier chaque Issue concernée pour pointer vers ce document.
    - [ ] 9.2.3 Archiver les suivis terminés
      - [ ] Créer `docs/archive/tasks` et y déplacer les documents de tâches entièrement terminés qui doivent rester dans l’historique, puis mettre à jour leurs liens entrants.
  - [ ] 9.3 Établir la nouvelle source de vérité
    - [ ] 9.3.1 Remplacer l’index local des tâches actives
      - [ ] Modifier `docs/tasks/README.md` pour désigner le Project GitHub comme suivi principal, conserver uniquement les feuilles locales dont la migration n’est pas terminée et lier `docs/project/github-task-migration.md`.
    - [ ] 9.3.2 Éviter les doublons de suivi
      - [ ] Modifier `docs/project/github-task-migration.md` après contrôle complet pour confirmer que chaque case ouverte possède une Issue et qu’aucune tâche active n’est maintenue simultanément dans deux sources de vérité.

- [ ] 10. Préparer les contributions publiques lorsque le projet sera annoncé
  - [ ] 10.1 Expliquer comment contribuer
    - [ ] 10.1.1 Documenter le parcours d’une contribution
      - [ ] Créer `CONTRIBUTING.md` avec la création d’une Issue, le fork, la branche, le build, les tests pertinents, la pull request et les règles propres aux modules indépendants.
    - [ ] 10.1.2 Guider la description d’une pull request
      - [ ] Créer `.github/PULL_REQUEST_TEMPLATE.md` avec le problème traité, les changements, la validation réalisée, les effets sur les versions et les liens vers les Issues associées.
  - [ ] 10.2 Vérifier automatiquement les pull requests
    - [ ] 10.2.1 Ajouter une intégration continue sans publication
      - [ ] Créer `.github/workflows/ci.yml` déclenché par `pull_request` et par les changements de `main`, restaurant les dépendances, compilant le projet et exécutant les tests pertinents sans créer de release ni modifier un catalogue.
    - [ ] 10.2.2 Documenter le contrôle automatique
      - [ ] Modifier `docs/project/development.md` pour expliquer le workflow `ci.yml`, ses déclencheurs et les contrôles exigés avant fusion.
  - [ ] 10.3 Protéger la branche principale
    - [ ] 10.3.1 Ajouter la règle après validation du workflow CI
      - [ ] Créer une règle GitHub pour `main` exigeant la réussite du workflow `ci.yml` avant fusion, puis modifier `docs/tasks/project/github-public-project-validation.md` pour consigner son nom et ses contrôles obligatoires.
    - [ ] 10.3.2 Définir la propriété du code
      - [ ] Créer `.github/CODEOWNERS` avec `@overthetop78` comme propriétaire initial des fichiers, puis l’étendre uniquement lorsque d’autres responsables rejoignent le projet.
  - [ ] 10.4 Publier les règles de sécurité
    - [ ] 10.4.1 Fournir un canal de signalement adapté
      - [ ] Créer `SECURITY.md` avec les versions prises en charge et une méthode privée réelle de signalement des vulnérabilités, sans demander de publier une vulnérabilité exploitable dans une Issue publique.

- [ ] 11. Séparer les dépôts officiels lorsque cette organisation est souhaitée
  - [ ] 11.1 Préparer la séparation des dépendances
    - [ ] 11.1.1 Définir le traitement de `GWGUI.MediaEngine`
      - [ ] Créer `docs/architecture/official-module-repositories.md` avec l’inventaire des API de `GWGUI.MediaEngine` utilisées par Amiga et Atari, puis y inscrire la dépendance distribuable retenue avant de déplacer leurs sources.
    - [ ] 11.1.2 Remplacer les références directes au dépôt principal
      - [ ] Modifier les projets Amiga et Atari pour référencer `GWGUI.Emulation.SDK` depuis NuGet.org et la dépendance MediaEngine définie au point 11.1.1, sans conserver de `ProjectReference` vers un chemin destiné à disparaître.
  - [ ] 11.2 Créer et publier les dépôts autonomes
    - [ ] 11.2.1 Créer les dépôts officiels
      - [ ] Créer `<ORGANISATION>/GWGUI.Module.Amiga` et `<ORGANISATION>/GWGUI.Module.Atari`, y ajouter les sources et tests propres à chaque module, puis inscrire leurs URL dans `docs/project/github-organization.md`.
    - [ ] 11.2.2 Installer le workflow autonome
      - [ ] Copier et adapter `sdk/module-template/.github/workflows/release-module.yml` dans chaque dépôt de module, avec son `module.json`, son identifiant, sa version et son URL `module-catalog` propres.
    - [ ] 11.2.3 Ajouter les notes de publication propres au module
      - [ ] Créer dans chaque dépôt la documentation de version et la procédure qui produit une release, un ZIP, un SHA-256 et le catalogue du seul module sans publier GW GUI.
    - [ ] 11.2.4 Publier et vérifier les premiers paquets autonomes
      - [ ] Modifier `docs/tasks/project/github-public-project-validation.md` avec les URL et résultats de publication, d’installation et de chargement des premiers paquets Amiga et Atari produits par leurs dépôts respectifs.
  - [ ] 11.3 Basculer GW GUI vers les catalogues autonomes
    - [ ] 11.3.1 Modifier les manifestes officiels
      - [ ] Modifier les `module.json` Amiga et Atari encore utilisés dans le dépôt principal pour pointer vers leurs catalogues autonomes vérifiés, puis contrôler leur installation et leur recherche de mise à jour.
    - [ ] 11.3.2 Supprimer les sources devenues externes
      - [ ] Supprimer de `GW-GUI` les projets officiels déplacés seulement après avoir installé et chargé leurs paquets publiés depuis leurs nouveaux dépôts, puis modifier la solution, les scripts et les tests qui les référençaient.
    - [ ] 11.3.3 Conserver un développement local explicite
      - [ ] Modifier `scripts/build.ps1`, `scripts/package.ps1` et leur documentation pour que les modules externes de développement puissent être fournis explicitement sans réintroduire une liste de modules dans l’application.
  - [ ] 11.4 Séparer éventuellement le dépôt du SDK
    - [ ] 11.4.1 Déplacer les sources et la publication du SDK
      - [ ] Créer `<ORGANISATION>/GWGUI.Emulation.SDK` avec les sources, le README NuGet, les notes et un workflow Trusted Publishing propre, puis modifier les références du dépôt principal et des modules vers ce dépôt.
    - [ ] 11.4.2 Réassocier la politique NuGet au dépôt du SDK
      - [ ] Modifier la stratégie NuGet Trusted Publishing pour autoriser le workflow du dépôt SDK, puis consigner ses propriétaire, dépôt, workflow et portée dans `docs/project/github-organization.md`.

- [ ] 12. Valider l’ensemble après mise en place
  - [ ] 12.1 Vérifier les parcours publics
    - [ ] 12.1.1 Contrôler la navigation et le suivi
      - [ ] Modifier `docs/tasks/project/github-public-project-validation.md` avec le résultat de la création d’une Issue de chaque type, de son ajout automatique au Project, de son changement d’état et de sa fermeture.
    - [ ] 12.1.2 Contrôler les contributions lorsque leur phase est activée
      - [ ] Modifier `docs/tasks/project/github-public-project-validation.md` avec le résultat d’une pull request de contrôle montrant le modèle, l’exécution de `ci.yml` et l’application de la règle de `main`, puis fermer cette pull request sans fusion si elle ne contient aucun changement utile.
  - [ ] 12.2 Vérifier les publications et mises à jour réelles
    - [ ] 12.2.1 Contrôler GW GUI depuis la nouvelle organisation
      - [ ] Modifier `docs/tasks/project/github-public-project-validation.md` après une prochaine vraie release pour inscrire la release, son catalogue et une recherche de mise à jour réussie depuis une version antérieure.
    - [ ] 12.2.2 Contrôler chaque module indépendant
      - [ ] Modifier `docs/tasks/project/github-public-project-validation.md` après une prochaine vraie release de module pour inscrire son dépôt, sa version, son catalogue, son installation et sa mise à jour sans téléchargement d’une nouvelle version de GW GUI.
    - [ ] 12.2.3 Clore la migration documentaire
      - [ ] Modifier `docs/tasks/README.md`, `docs/project/github-task-migration.md` et le présent document pour cocher la migration uniquement lorsque toutes les tâches ouvertes sont suivies dans GitHub et que les fichiers locaux ne constituent plus une seconde liste active.
