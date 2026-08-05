# Version, compilation et révision

## État constaté

- Le projet Application définit `Version=0.1.0` par défaut.
- Les projets Domain, Infrastructure et SCP n’ont pas de version commune déclarée et prennent `1.0.0` lorsqu’ils sont compilés seuls.
- `scripts/package.ps1` transmet la valeur reçue à `dotnet publish` et à Inno Setup.
- .NET ajoute automatiquement le hash complet du commit à `AssemblyInformationalVersion`.
- Le numéro `0.1.0` ne change donc pas automatiquement à chaque commit.
- Lorsqu’un hash est déjà inclus manuellement dans la valeur passée au packaging, .NET peut le rajouter une seconde fois dans les DLL publiées.

Le paquet doit corriger cette incohérence avant la prochaine publication.

## Signification retenue

- **Version produit** : numéro fonctionnel choisi pour une publication, par exemple `0.1.0`, `0.2.0` ou `1.0.0`.
- **Build de compilation** : identifiant d’une fabrication précise du paquet. En CI, il peut utiliser le numéro d’exécution GitHub Actions. En local, le script en génère ou en reçoit un explicitement.
- **Révision** : commit source exact. Elle comporte le nombre de commits du dépôt et le hash Git court, par exemple `rev.121-f5d1672`.
- **AssemblyVersion** : version de compatibilité .NET, volontairement stable pendant une série compatible.
- **FileVersion** : version numérique Windows à quatre composantes, par exemple `0.1.0.121` lorsque la révision numérique vaut 121.
- **InformationalVersion/ProductVersion** : identité lisible complète, par exemple `0.1.0+build.42.rev.121.f5d1672`.

Une compilation d’un arbre de travail modifié mais non commité doit être marquée `dirty` et ne doit pas être utilisée comme paquet officiel.

## Application et DLL

L’EXE et les DLL font partie du même produit et doivent partager la version produit et la révision du paquet. Cela permet de savoir immédiatement si les fichiers proviennent du même build.

Une DLL recompilée indépendamment porte néanmoins sa propre identité de fichier et le commit réellement utilisé. Il n’est pas nécessaire de créer quatre versions produit indépendantes tant que les DLL ne sont pas distribuées séparément. Des cycles indépendants rendraient les diagnostics et les dépendances plus difficiles sans bénéfice actuel.

Pour chaque binaire GW GUI, les propriétés doivent donc exposer :

- le même `ProductVersion` complet pour un paquet donné;
- une `FileVersion` numérique cohérente;
- une `AssemblyVersion` stable compatible;
- le nom du composant déjà fourni par le fichier et l’assembly;
- le même hash source si tous les composants viennent du même commit.

Si une DLL devient un jour un produit public distribué séparément, elle recevra alors son propre cycle de version sémantique.

## Travail à réaliser

- Ajouter un `Directory.Build.props` qui centralise version produit, société, dépôt et règles d’assemblage pour les quatre projets.
- Calculer une seule fois révision numérique et hash court dans le script de build.
- Empêcher le double ajout du hash par `IncludeSourceRevisionInInformationalVersion` ou en ne passant plus un hash dans `Version`.
- Ajouter un numéro de build distinct de la révision Git.
- Générer les noms du ZIP et de l’installateur depuis la même identité.
- Afficher dans À propos la version, le build et la révision, au lieu de supprimer tout ce qui suit `+`.
- Ajouter un bouton Copier les informations de version pour le diagnostic.
- Vérifier après publication les métadonnées de l’EXE et des trois DLL.
- Faire échouer le packaging si un binaire GW GUI possède une version ou une révision différente.
- Tester une compilation complète, une compilation isolée de chaque projet et un paquet construit depuis un arbre `dirty`.
- Utiliser la version produit pour la comparaison des mises à jour; le build et la révision servent à identifier précisément un exécutable, pas à annoncer une fausse nouvelle version fonctionnelle.

## Point à ne pas confondre

Un commit et une compilation ne sont pas toujours équivalents : un même commit peut être compilé plusieurs fois, et une compilation locale peut contenir des modifications non commitées. C’est pourquoi le build et la révision sont deux informations distinctes.
