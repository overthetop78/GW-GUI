# Version, compilation et révision

## État actuel vérifié

- `GWGUI.App.csproj` utilise `0.1.0` lorsque la propriété MSBuild `Version` n’est pas fournie.
- Les autres projets ne partagent pas encore une identité de version centralisée.
- Aucun `Directory.Build.props` commun n’existe actuellement.
- Aucun `scripts/version.ps1` ne calcule encore une identité unique.
- `scripts/build.ps1` produit les builds Debug ou Release sans calculer de version produit.
- `scripts/package.ps1` exige une version, la transmet aux publications App et Launcher, puis utilise la même valeur pour le ZIP portable et l’installateur.
- .NET peut ajouter automatiquement la révision Git à `AssemblyInformationalVersion` ; cette génération doit être maîtrisée lorsqu’une identité complète est fournie au build.

## Signification retenue

- **Version produit** : numéro fonctionnel de la publication.
- **Build** : fabrication précise d’un paquet.
- **Révision** : commit source exact, identifié par le nombre de commits et le hash Git court.
- **AssemblyVersion** : version de compatibilité .NET.
- **FileVersion** : version numérique Windows à quatre composantes.
- **InformationalVersion/ProductVersion** : identité lisible complète du binaire.

Une compilation contenant des modifications non commitées doit être marquée `dirty` et ne doit pas être promue comme paquet officiel.

## Cohérence des binaires

Tous les binaires `GWGUI.*` d’un même paquet doivent partager la version produit, le build et la révision. Une DLL distribuée uniquement avec l’application ne possède pas un cycle produit indépendant.

La comparaison fonctionnelle des mises à jour utilise la version produit. Le build et la révision servent à identifier exactement les fichiers employés.

## Travaux restants

La centralisation de la version, les workflows Snapshot et Stable, les contrôles d’identité des binaires et la promotion des artefacts sont suivis dans [Versionnement et publication](../tasks/release.md).

## Distinction importante

Un commit et une compilation ne sont pas équivalents : un même commit peut être compilé plusieurs fois et une compilation locale peut contenir des modifications non commitées. Le build et la révision restent donc deux informations distinctes.
