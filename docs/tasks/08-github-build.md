# 8 — Versionnement et workflows GitHub

## Décision de versionnement

GW GUI est publié comme un seul produit. Les projets restent séparés pour imposer leurs frontières architecturales, mais ils ne possèdent pas de cycles de publication indépendants.

Pour un paquet donné, l’EXE et toutes les DLL `GWGUI.*` partagent donc :

- la même version produit ;
- le même numéro de build ;
- la même révision Git ;
- le même état propre ou `dirty` de l’arbre source.

Chaque assembly conserve seulement son nom et son identité de composant. Une DLL ne recevra une version produit indépendante que si elle devient un produit public distribué séparément avec une API et un cycle de compatibilité propres.

Chaque commit change la révision Git, sans changer automatiquement la version produit. Lorsqu’une vraie fonctionnalité prépare une nouvelle version, le workflow calcule la prochaine version sans demander de saisir son numéro.

Deux canaux sont conservés :

- **Snapshot** : build demandé manuellement pour tester un état jugé suffisamment bon ; il porte la version candidate et la révision exacte ;
- **Stable** : publication finale décidée manuellement à partir d’une snapshot validée.

Un push, un commit ou une pull request ne déclenche seul ni build, ni snapshot, ni release.

## Priorité de durée

- Une snapshot demandée doit restaurer, compiler, tester et fabriquer ses fichiers une seule fois.
- La publication stable doit promouvoir les fichiers de la snapshot validée, sans les recompiler.
- Le corpus local, les tests matériels et les validations interactives ne doivent jamais ralentir ces workflows.
- Aucun workflow inutile ne doit être lancé lors d’un simple push.

## 8.1 — Identité de version du produit

Éléments déjà présents et qui ne constituent pas des tâches restantes :

- `scripts/package.ps1` transmet déjà la version produit reçue à `dotnet publish` et à Inno Setup ;
- les noms actuels du ZIP et de l’installateur utilisent déjà cette version produit.

- [ ] Ajouter un `Directory.Build.props` commun à tous les projets de production pour centraliser les règles de version des assemblies ; la dernière version stable provient du dernier tag Git, pas d’une copie maintenue dans chaque projet.
- [ ] Créer `scripts/version.ps1` comme point unique de calcul de version pour les workflows et le packaging local.
  - [ ] Lire automatiquement la dernière version stable depuis les tags Git.
  - [ ] Recevoir uniquement le type de build demandé : correction snapshot, fonctionnalité snapshot ou promotion stable.
  - [ ] Retourner la version produit, la révision, la version informative et les noms de fichiers sans modifier manuellement les projets.
- [ ] Calculer automatiquement une seule fois l’identité complète lorsqu’une snapshot est demandée.
  - [ ] Conserver la version stable courante pour une snapshot de corrections.
  - [ ] Calculer automatiquement la prochaine version de fonctionnalité pour une snapshot contenant une nouvelle fonction, sans saisir son numéro.
  - [ ] Utiliser le numéro du run et de sa tentative comme identifiant technique du build.
  - [ ] Ajouter le nombre de commits et le hash Git court constituant la révision.
  - [ ] Ajouter l’indicateur `dirty` lorsque l’arbre de travail contient des modifications.
- [ ] Produire pour tous les binaires `GWGUI.*` une `AssemblyVersion` stable, une `FileVersion` numérique cohérente et une `InformationalVersion` complète.
- [ ] Empêcher .NET d’ajouter une seconde fois le hash Git lorsque l’identité complète est fournie au build.
- [ ] Faire partager à tous les binaires d’un même paquet la même version produit, le même build et la même révision.
- [ ] Générer les noms du build testable, du ZIP et de l’installateur depuis la même identité calculée.
- [ ] Refuser la promotion stable si la snapshot ne correspond pas exactement au commit et aux fichiers qui ont été validés.
- [ ] Afficher dans la fenêtre À propos la version produit, le build et la révision sans supprimer les informations placées après `+`.
- [ ] Utiliser uniquement la version produit pour décider qu’une mise à jour fonctionnelle est disponible ; conserver le build et la révision comme identifiants précis du binaire.
- [ ] Ajouter un contrôle rapide vérifiant que l’EXE et toutes les DLL `GWGUI.*` portent la même identité calculée.

## 8.2 — Snapshot demandée manuellement

- [x] Auditer les scripts locaux `scripts/build.ps1` et `scripts/package.ps1`.
- [ ] Créer un workflow manuel `snapshot.yml` qui ne se déclenche jamais sur un push, un commit, une pull request ou une planification.
- [ ] Faire appeler `scripts/version.ps1` automatiquement par ce workflow dès que la snapshot est demandée depuis GitHub Actions.
- [ ] Permettre de choisir si la snapshot représente seulement une nouvelle révision ou une nouvelle fonctionnalité, sans saisir de numéro de version.
- [x] Valider dans le workflow de release l’utilisation d’un runner Windows et de la version .NET du projet.
- [x] Valider dans le workflow de release la restauration, la compilation Release et les tests compatibles CI.
- [x] Ne jamais inclure le corpus privé `image_test` dans le workflow ou les paquets produits.
- [ ] Restaurer les dépendances une seule fois par run.
- [ ] Compiler et publier en Release une seule fois, puis réutiliser cette sortie pour tous les fichiers de la snapshot.
- [ ] Exécuter seulement `GWGUI.Tests` ; conserver `GWGUI.LocalDiskImageTests` hors CI.
- [ ] Produire le ZIP portable, l’installateur et `SHA256SUMS.txt` avec la version candidate et la révision calculées.
- [ ] Publier chaque snapshot demandée comme une GitHub Prerelease distincte portant sa version candidate et sa révision.
- [ ] Enregistrer dans la snapshot le commit, l’identité complète et les sommes des fichiers nécessaires à une promotion stable.
- [x] Vérifier que toute erreur PowerShell ou commande native produit un code de sortie non nul.

## 8.3 — Promotion stable demandée manuellement

- [x] Auditer le workflow de release existant et éviter les duplications fragiles.
- [ ] Permettre de choisir manuellement la snapshot à finaliser en version stable.
- [ ] Vérifier que la snapshot sélectionnée correspond encore exactement à son commit, à sa version candidate et à ses sommes SHA-256.
- [ ] Créer automatiquement le tag stable depuis la version candidate, sans demander de saisir le numéro.
- [ ] Réutiliser exactement le ZIP, l’installateur et `SHA256SUMS.txt` validés dans la snapshot, sans restauration, compilation, test ou packaging supplémentaire.
- [x] Vérifier que toutes les langues déclarées dans `installer/GWGUI.iss` disposent de leur fichier de messages et permettent la compilation de l’installateur.
- [x] Conserver les smoke tests d’installation en anglais et en français comme parcours représentatifs.
- [x] Vérifier qu’un échec de test, de packaging, d’accessibilité ou d’installation empêche toute création de release.
- [ ] Publier la GitHub Release stable avec les fichiers promus et les notes de version.
- [ ] Mettre à jour la version stable courante seulement après la réussite de la promotion.
- [ ] Conserver la snapshot jusqu’à la fin de la promotion, puis la retirer ou la remplacer lors de la prochaine snapshot.
