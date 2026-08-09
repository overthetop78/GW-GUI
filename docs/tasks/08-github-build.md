# 8 — Workflow GitHub de build

## 8.1 — Versionnement à terminer

Éléments déjà présents et qui ne constituent pas des tâches restantes :

- `scripts/package.ps1` transmet déjà la version produit reçue à `dotnet publish` et à Inno Setup ;
- les noms actuels du ZIP et de l’installateur utilisent déjà cette version produit.

- [ ] Ajouter un `Directory.Build.props` commun aux projets Application, Domain, Infrastructure et SCP pour centraliser la version produit, la société, le dépôt et les règles de version des assemblies.
- [ ] Calculer une seule fois, dans les scripts de build et de packaging, le nombre de commits et le hash Git court constituant la révision.
- [ ] Ajouter un numéro de build distinct de la version produit et de la révision Git, fourni par GitHub Actions en CI et généré ou reçu explicitement en local.
- [ ] Produire une `AssemblyVersion` stable, une `FileVersion` numérique cohérente et une `InformationalVersion` complète pour l’EXE et chacune des DLL GW GUI.
- [ ] Empêcher .NET d’ajouter une seconde fois le hash Git lorsque l’identité complète est fournie au build.
- [ ] Marquer `dirty` toute compilation provenant d’un arbre de travail contenant des modifications non commitées et interdire son utilisation comme paquet officiel.
- [ ] Faire partager à tous les binaires d’un même paquet la même version produit, le même build et la même révision, tout en conservant l’identité propre du composant recompilé séparément.
- [ ] Générer les noms du ZIP, de l’installateur et des artifacts depuis l’identité de version calculée une seule fois.
- [ ] Afficher dans la fenêtre À propos la version produit, le build et la révision sans supprimer les informations placées après `+`.
- [ ] Ajouter dans À propos une action permettant de copier toutes les informations de version utiles au diagnostic.
- [ ] Vérifier après chaque publication les métadonnées de l’EXE et des trois DLL GW GUI.
- [ ] Faire échouer le packaging si les versions, builds ou révisions des binaires GW GUI d’un même paquet ne correspondent pas.
- [ ] Tester une compilation complète, la compilation isolée de chacun des quatre projets et une compilation provenant d’un arbre `dirty`.
- [ ] Utiliser uniquement la version produit pour décider qu’une mise à jour fonctionnelle est disponible ; conserver le build et la révision comme identifiants précis du binaire.

## 8.2 — Workflow GitHub de build

- [ ] Auditer les scripts locaux `scripts/build.ps1` et `scripts/package.ps1`.
- [ ] Créer `.github/workflows/build.yml` pour les pushes et pull requests.
- [ ] Utiliser un runner Windows et la version .NET du projet.
- [ ] Restaurer les dépendances, compiler en Release et exécuter les tests compatibles CI.
- [ ] Exécuter les contrôles d’architecture, de ressources et d’encodage.
- [ ] Ne jamais inclure le corpus privé `image_test` dans le workflow ou les artifacts.
- [ ] Produire un artifact de build testable distinct du ZIP et de l’installateur final.
- [ ] Nommer l’artifact avec les informations de version, build et révision décidées.
- [ ] Configurer l’annulation d’un ancien run de la même branche.
- [ ] Vérifier que toute erreur PowerShell produit un code de sortie non nul.
- [ ] Auditer ensuite le workflow de release existant et éviter les duplications fragiles.
- [ ] Vérifier les traductions de l’installateur lors des builds de publication.
