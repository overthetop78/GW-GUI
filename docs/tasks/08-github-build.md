# 8 — Versionnement et workflows GitHub

## Décision de versionnement

GW GUI est publié comme un seul produit. Les projets restent séparés pour imposer leurs frontières architecturales, mais ils ne possèdent pas de cycles de publication indépendants.

Pour un paquet donné, l’EXE et toutes les DLL `GWGUI.*` partagent donc :

- la même version produit ;
- le même numéro de build ;
- la même révision Git ;
- le même état propre ou `dirty` de l’arbre source.

Chaque assembly conserve seulement son nom et son identité de composant. Une DLL ne recevra une version produit indépendante que si elle devient un produit public distribué séparément avec une API et un cycle de compatibilité propres.

## 8.1 — Identité de version du produit

Éléments déjà présents et qui ne constituent pas des tâches restantes :

- `scripts/package.ps1` transmet déjà la version produit reçue à `dotnet publish` et à Inno Setup ;
- les noms actuels du ZIP et de l’installateur utilisent déjà cette version produit.

- [ ] Ajouter un `Directory.Build.props` commun à tous les projets de production pour centraliser la version produit, la société, le dépôt et les règles de version des assemblies.
- [ ] Calculer une seule fois l’identité du build dans un script partagé par la compilation locale, le packaging et GitHub Actions.
  - [ ] Calculer le nombre de commits et le hash Git court constituant la révision.
  - [ ] Recevoir le numéro de build de GitHub Actions en CI.
  - [ ] Générer ou recevoir explicitement un numéro de build local hors CI.
  - [ ] Détecter un arbre de travail modifié et ajouter l’indicateur `dirty` à son identité.
- [ ] Produire pour tous les binaires `GWGUI.*` une `AssemblyVersion` stable, une `FileVersion` numérique cohérente et une `InformationalVersion` complète.
- [ ] Empêcher .NET d’ajouter une seconde fois le hash Git lorsque l’identité complète est fournie au build.
- [ ] Faire partager à tous les binaires d’un même paquet la même version produit, le même build et la même révision.
- [ ] Conserver le nom et l’identité propres de chaque assembly lorsqu’un projet est compilé séparément.
- [ ] Générer les noms du build testable, du ZIP, de l’installateur et des artifacts depuis la même identité calculée.
- [ ] Interdire la création d’un paquet officiel depuis un arbre `dirty`, tout en autorisant explicitement les builds locaux identifiés comme tels.
- [ ] Afficher dans la fenêtre À propos la version produit, le build et la révision sans supprimer les informations placées après `+`.
- [ ] Ajouter dans À propos une action permettant de copier toutes les informations de version utiles au diagnostic.
- [ ] Utiliser uniquement la version produit pour décider qu’une mise à jour fonctionnelle est disponible ; conserver le build et la révision comme identifiants précis du binaire.
- [ ] Ajouter des tests automatisés pour l’identité de version.
  - [ ] Tester une compilation propre et une compilation `dirty`.
  - [ ] Tester la compilation de la solution et de chaque projet de production pris comme point d’entrée.
  - [ ] Vérifier les métadonnées de l’EXE et de toutes les DLL `GWGUI.*` produites.
  - [ ] Faire échouer le packaging si les versions, builds ou révisions des binaires d’un même paquet ne correspondent pas.
  - [ ] Tester que la comparaison de mises à jour ignore le build et la révision lorsque la version produit est identique.

## 8.2 — Build continu

- [x] Auditer les scripts locaux `scripts/build.ps1` et `scripts/package.ps1`.
- [ ] Créer `.github/workflows/build.yml` pour les pushes et pull requests.
- [x] Valider dans le workflow de release l’utilisation d’un runner Windows et de la version .NET du projet.
- [x] Valider dans le workflow de release la restauration, la compilation Release et les tests compatibles CI.
- [ ] Exécuter des contrôles nommés et vérifiables pour les frontières entre projets, la parité des ressources et l’encodage des fichiers texte.
- [x] Ne jamais inclure le corpus privé `image_test` dans le workflow ou les artifacts.
- [ ] Produire un artifact de build testable distinct du ZIP et de l’installateur final.
- [ ] Nommer cet artifact avec la version produit, le build et la révision calculés en 8.1.
- [ ] Définir une durée de conservation explicite pour les artifacts de build.
- [ ] Configurer un groupe de concurrence par workflow et branche, puis annuler un ancien run de la même branche.
- [ ] Définir un délai maximal d’exécution pour éviter les jobs bloqués.
- [x] Vérifier que toute erreur PowerShell ou commande native produit un code de sortie non nul.
- [ ] Donner au workflow de build uniquement les permissions GitHub en lecture nécessaires.
- [ ] Publier les résultats ou journaux de tests en cas d’échec lorsqu’ils sont disponibles, sans exposer de données privées.

## 8.3 — Publication

- [x] Auditer le workflow de release existant et éviter les duplications fragiles.
- [ ] Faire réutiliser à la publication les mêmes scripts et contrôles que le build continu, sans maintenir une seconde suite divergente.
- [ ] Vérifier qu’un tag `vX.Y.Z` correspond exactement à la version produit transmise au packaging.
- [ ] Refuser une version vide, invalide, déjà publiée ou différente du tag demandé.
- [ ] Réserver `contents: write` au job qui crée effectivement la GitHub Release.
- [ ] Construire le ZIP portable, l’installateur et `SHA256SUMS.txt` à partir du même build validé.
- [ ] Vérifier que `SHA256SUMS.txt` contient exactement tous les fichiers publiés et que chaque somme correspond au fichier final.
- [x] Vérifier que toutes les langues déclarées dans `installer/GWGUI.iss` disposent de leur fichier de messages et permettent la compilation de l’installateur.
- [x] Conserver les smoke tests d’installation en anglais et en français comme parcours représentatifs.
- [x] Vérifier qu’un échec de test, de packaging, d’accessibilité ou d’installation empêche toute création de release.
- [x] Publier une release non brouillon uniquement après le succès de tous les contrôles et l’envoi de tous les assets attendus.
- [ ] Définir une durée de conservation explicite pour les artifacts intermédiaires de publication.
