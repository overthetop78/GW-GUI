# Versionnement du SDK d’émulation

Le paquet public `GWGUI.Emulation.SDK` contient l’assembly `gwgui.emulation.dll` et sa documentation
XML. Les modules externes le référencent par `PackageReference`; ils ne copient aucune DLL depuis une
installation de GW GUI.

## Versions actuelles

| Paquet SDK | API hôte | Schéma du manifeste |
|---|---|---|
| `1.0.0` | `1.0` | `2` |

La version du paquet suit SemVer avec trois nombres. La version d’API hôte possède deux nombres et
représente la compatibilité binaire et comportementale des contrats consommés par un module. Le
schéma du manifeste évolue séparément lorsque la structure de `module.json` change.

Une correction de documentation ou d’implémentation interne augmente la révision du paquet sans
changer l’API hôte. Un ajout de contrat rétrocompatible augmente la version mineure du paquet et
l’API hôte après validation. Une rupture de contrat augmente la version majeure du paquet et de
l’API hôte. Chaque module déclare explicitement `hostApiMinimum` et `hostApiMaximum`; aucune
compatibilité au-delà de cette plage n’est supposée.

## Changer et publier une version

1. Adapter les contrats et `EmulationHostApi.CurrentVersion` si la compatibilité hôte change.
2. Mettre à jour `Version` dans `src/GWGUI.Emulation/GWGUI.Emulation.csproj` et le tableau ci-dessus.
3. Adapter le modèle `sdk/module-template` à la nouvelle version si elle devient la référence.
4. Ajouter `.github/release-notes/sdk/vX.Y.Z.md`, commiter et pousser les changements sur `main`.
5. Créer puis pousser le tag `sdk-vX.Y.Z`. Le workflow vérifie la cohérence du tag, exécute les tests,
   produit le paquet et le publie sur NuGet.org.

Le secret GitHub `NUGET_API_KEY` contient une clé NuGet.org limitée à la publication de
`GWGUI.Emulation.SDK`. Sa valeur ne doit jamais être ajoutée au dépôt.
