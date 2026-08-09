# 8 — Workflow GitHub de build

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

