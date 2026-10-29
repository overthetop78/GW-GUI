# Versionnement du SDK d’émulation

Le paquet public `GWGUI.Emulation.SDK` contient l’assembly `gwgui.emulation.dll` et sa documentation
XML. Les modules externes le référencent par `PackageReference`; ils ne copient aucune DLL depuis une
installation de GW GUI.

Le paquet contient également `src/GWGUI.Emulation/README.md` à sa racine. NuGet.org l'affiche comme
présentation du SDK avec son rôle, son installation et le point de départ d'un module.

## Versions actuelles

| Paquet SDK | API hôte | Schéma du manifeste |
|---|---|---|
| `1.3.0` | `1.3` | `2` |

L’API 1.3 fixe la nouvelle base binaire : chemins d’images externes, signatures des réglages et
périphériques, services cassette/disque, mécanique des lecteurs et caractères du clavier hôte.
Les 24 modules du dépôt déclarent la plage `1.3` à `1.3`. Les paquets publiés pour l’API 1.2 sont
refusés avant chargement ; leurs manifestes ne doivent pas être réétiquetés sans recompilation.
Les 24 modules et le SDK portent tous la version `1.3.0`. Toute version de module inférieure à
`1.3.0` est refusée au chargement, à l’installation et dans les choix de mise à jour. L’application
locale est en `0.4.1`.

La propriété MSBuild `Version` du SDK est locale à son projet afin de conserver `1.3.0` lorsqu’un
module est compilé avec son propre numéro de version. Le build et le catalogue de l’application
lisent tous deux l’API dans `EmulationHostApi.cs`.

L API 1.2 ajoute le contrat facultatif IEmulationVideoSettingsManager et EmulationGraphicsApi. Le module traduit la préférence vidéo de l application vers ses capacités natives, sans exposer un moteur concret au SDK. Les modules utilisant ce contrat exigent au minimum l API 1.2 ; les autres conservent leur minimum et déclarent leur maximum validé.

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

NuGet.org autorise `.github/workflows/sdk-release.yml` par une stratégie **Trusted Publishing** liée
à `overthetop78/GW-GUI`. Le workflow demande un jeton OIDC GitHub, que `NuGet/login@v1` échange
juste avant la publication contre une clé temporaire. Aucune clé permanente n’est créée ou stockée.
La variable GitHub Actions `NUGET_USER` contient uniquement le nom public du profil NuGet.org.
