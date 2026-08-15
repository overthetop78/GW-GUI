# 8 — Versionnement et workflows GitHub

## Décision de versionnement

GW GUI est publié comme un seul produit. Les projets restent séparés pour imposer leurs frontières architecturales, mais ils ne possèdent pas de cycles de publication indépendants.

Pour un paquet donné, l’EXE et toutes les DLL `GWGUI.*` partagent donc :

- la même version produit ;
- le même numéro de build ;
- la même révision Git ;
- le même état propre ou `dirty` de l’arbre source.

Chaque assembly conserve seulement son nom et son identité de composant. Une DLL ne recevra une version produit indépendante que si elle devient un produit public distribué séparément avec une API et un cycle de compatibilité propres.

Chaque build doit recevoir automatiquement une identité différente sans demander de numéro à l’utilisateur. La version produit de base change seulement lorsqu’une nouvelle version fonctionnelle est décidée ; le numéro de build et la révision rendent chaque compilation identifiable.

## Priorité de durée

- Un push ou une pull request doit effectuer uniquement le build et les tests rapides nécessaires.
- Une release doit restaurer, compiler, tester et publier chaque sortie une seule fois.
- Le ZIP, l’installateur et la release doivent réutiliser les mêmes fichiers publiés, sans recompilation intermédiaire.
- Le corpus local, les tests matériels et les validations interactives ne doivent jamais ralentir le CI ordinaire.
- Un ancien build de la même branche doit être annulé lorsqu’un nouveau commit le remplace.

## 8.1 — Identité de version du produit

Éléments déjà présents et qui ne constituent pas des tâches restantes :

- `scripts/package.ps1` transmet déjà la version produit reçue à `dotnet publish` et à Inno Setup ;
- les noms actuels du ZIP et de l’installateur utilisent déjà cette version produit.

- [ ] Ajouter un `Directory.Build.props` commun à tous les projets de production pour centraliser la version produit de base et les règles de version des assemblies.
- [ ] Calculer automatiquement une seule fois l’identité complète à chaque build, sans saisie manuelle.
  - [ ] Utiliser le numéro du run et de sa tentative dans GitHub Actions.
  - [ ] Générer automatiquement un identifiant de build local.
  - [ ] Ajouter le nombre de commits et le hash Git court constituant la révision.
  - [ ] Ajouter l’indicateur `dirty` lorsque l’arbre de travail contient des modifications.
- [ ] Produire pour tous les binaires `GWGUI.*` une `AssemblyVersion` stable, une `FileVersion` numérique cohérente et une `InformationalVersion` complète.
- [ ] Empêcher .NET d’ajouter une seconde fois le hash Git lorsque l’identité complète est fournie au build.
- [ ] Faire partager à tous les binaires d’un même paquet la même version produit, le même build et la même révision.
- [ ] Générer les noms du build testable, du ZIP et de l’installateur depuis la même identité calculée.
- [ ] Interdire seulement la publication officielle d’un build `dirty` ; un build local `dirty` reste autorisé et clairement identifié.
- [ ] Afficher dans la fenêtre À propos la version produit, le build et la révision sans supprimer les informations placées après `+`.
- [ ] Utiliser uniquement la version produit pour décider qu’une mise à jour fonctionnelle est disponible ; conserver le build et la révision comme identifiants précis du binaire.
- [ ] Ajouter un contrôle rapide vérifiant que l’EXE et toutes les DLL `GWGUI.*` portent la même identité calculée.

## 8.2 — Build continu

- [x] Auditer les scripts locaux `scripts/build.ps1` et `scripts/package.ps1`.
- [ ] Créer `.github/workflows/build.yml` pour les pushes et pull requests.
- [x] Valider dans le workflow de release l’utilisation d’un runner Windows et de la version .NET du projet.
- [x] Valider dans le workflow de release la restauration, la compilation Release et les tests compatibles CI.
- [x] Ne jamais inclure le corpus privé `image_test` dans le workflow ou les artifacts.
- [ ] Restaurer les dépendances une seule fois par run.
- [ ] Compiler en Release une seule fois par run et réutiliser cette sortie pour les tests et l’artifact testable.
- [ ] Exécuter seulement `GWGUI.Tests` dans le build continu ; conserver `GWGUI.LocalDiskImageTests` hors CI.
- [ ] Produire directement l’artifact testable portant l’identité automatique du build.
- [ ] Configurer un groupe de concurrence par workflow et branche, puis annuler un ancien run de la même branche.
- [x] Vérifier que toute erreur PowerShell ou commande native produit un code de sortie non nul.
- [ ] Ne pas construire le ZIP, l’installateur et les contrôles de publication sur chaque push ou pull request.

## 8.3 — Publication

- [x] Auditer le workflow de release existant et éviter les duplications fragiles.
- [ ] Utiliser les mêmes scripts rapides que le build continu sans déclencher deux workflows pour le même tag.
- [ ] Vérifier qu’un tag `vX.Y.Z` correspond exactement à la version produit transmise au packaging.
- [ ] Restaurer les dépendances, exécuter les tests rapides et publier l’application une seule fois.
- [ ] Construire le ZIP portable, l’installateur et `SHA256SUMS.txt` à partir de cette unique sortie publiée.
- [x] Vérifier que toutes les langues déclarées dans `installer/GWGUI.iss` disposent de leur fichier de messages et permettent la compilation de l’installateur.
- [x] Conserver les smoke tests d’installation en anglais et en français comme parcours représentatifs.
- [x] Vérifier qu’un échec de test, de packaging, d’accessibilité ou d’installation empêche toute création de release.
- [x] Publier une release non brouillon uniquement après le succès de tous les contrôles et l’envoi de tous les assets attendus.
- [ ] Mesurer la durée des étapes et supprimer toute restauration, compilation, copie ou compression exécutée deux fois.
