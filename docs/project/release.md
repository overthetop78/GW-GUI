# Publier une release ou une snapshot sur GitHub

Les commandes de cette page s’exécutent depuis la racine du dépôt.

La commande de publication est **`scripts/publish-release.cmd`**. Elle demande les paramètres puis déclenche [le workflow de release](../../.github/workflows/release.yml), qui construit les paquets sur GitHub à partir de `main` distant.

GitHub CLI (`gh`) doit être installé et connecté à un compte autorisé à lancer ce workflow. Pour configurer la connexion si nécessaire :

```powershell
gh auth login
```

## 1. Préparer les notes de version

Créer `.github/release-notes/vX.Y.Z.md` à partir du [modèle de notes](../../.github/release-notes/TEMPLATE.md). Décrire les changements de cette version : nouveautés, améliorations, corrections et changements techniques utiles. Supprimer les rubriques vides et remplacer le lien de comparaison par celui des tags concernés.

Le numéro **X.Y.Z du nom de ce fichier est le numéro unique de publication**. Il doit rester identique dans les notes, le titre GitHub, le tag et les noms des paquets. Le type de publication peut ajouter un suffixe au tag ou au titre, sans changer ces trois chiffres.

Par exemple, avec `v0.1.3.md`, utiliser `0.1.3` partout. Pour une release stable succédant à `v0.1.2`, le lien de comparaison est `https://github.com/overthetop78/GW-GUI/compare/v0.1.2...v0.1.3`. Pour une snapshot, le tag cible de cet exemple devient `v0.1.3-snapshot`.

## 2. Envoyer le code et les notes sur main

Faire le commit du code à publier et des notes de version, puis les pousser sur `main` **avant de lancer le script**. Si ces changements sont déjà commités et poussés, cette étape est satisfaite.

Le workflow utilise les fichiers présents sur GitHub. Une modification conservée uniquement sur le poste local ne sera pas incluse dans la publication.

## 3. Lancer la commande et saisir la version

```powershell
.\scripts\publish-release.cmd
```

Répondre aux invites avec les chiffres du nom du fichier de notes :

| Invite | Valeur à saisir | Exemple avec `v0.1.3.md` |
|---|---|---|
| `Major` | X, premier nombre | `0` |
| `Minor` | Y, deuxième nombre | `1` |
| `Revision` | Z, troisième nombre | `3` |

Ces valeurs sont **des réponses à saisir dans la console**, pas des modifications à effectuer dans le script. Ici, `Revision` désigne le troisième nombre de la version, pas le hash du commit Git. Le script ne calcule pas automatiquement la prochaine version.

Il recherche les notes correspondant au numéro saisi. Si le fichier manque, il propose de continuer avec des notes générées par GitHub ; pour publier les notes préparées, corriger le fichier ou le numéro avant de poursuivre.

## 4. Choisir le type de publication

| Choix | Usage | Tag pour `0.1.3` | Titre GitHub |
|---|---|---|---|
| `1. Latest` | Release stable, marquée comme dernière version | `v0.1.3` | `GW GUI 0.1.3 Release` |
| `2. Pre-release` | Snapshot, marquée comme préversion | `v0.1.3-snapshot` | `GW GUI 0.1.3 Snapshot` |
| `3. Aucun label` | Publication sans label Latest ni Pre-release | `v0.1.3` | `GW GUI 0.1.3` |

Choisir le type demandé pour cette publication. S’il n’est pas précisé, le faire préciser avant de déclencher le workflow.

Une fois la commande acceptée, GitHub Actions prend en charge la construction, les contrôles et la publication des fichiers. Le script local rend la main ; il n’attend pas la fin du workflow.

Le workflow accepte également les pushs de tags `v*`, avec des notes générées automatiquement. La procédure ci-dessus utilise le déclenchement manuel sur `main` pour transmettre explicitement la version, les notes et le type de publication.

## 5. Publier un module d'émulation indépendamment

Une publication de module utilise sa propre version `X.Y.Z` lue dans son `module.json`.
Elle ne modifie pas la version de GW GUI et ne remplace pas une release de l'application comme
dernière release GitHub.

### Module officiel conservé dans ce dépôt

Pour tout identifiant `<id>` découvert dans `src/GWGUI.Emulation.*/module.json` :

- tag : `module-<id>-vX.Y.Z` ;
- notes : `.github/release-notes/modules/<id>/vX.Y.Z.md` ;
- archive : `GW-GUI-Module-<id>-X.Y.Z-win-x64.zip`.

Avant une publication réelle :

1. modifier uniquement la version du manifeste du module concerné ;
2. créer ses notes au chemin indiqué, avec les changements propres au module ;
3. commiter et pousser le manifeste, les changements et les notes ;
4. créer puis pousser le tag correspondant exactement à la version du manifeste.

Le workflow accepte tout tag `module-<id>-vX.Y.Z`, retrouve le projet par le manifeste et transmet
le même identifiant au packaging et au catalogue. Le push du tag relance les tests, reconstruit le
paquet, vérifie son empreinte et crée la release GitHub avec `--latest=false`. Un identifiant inconnu,
une version de tag différente du manifeste ou des notes absentes interrompt la publication.

Le déclenchement manuel de `module-release.yml` construit seulement un artefact sans publier de
release. Il ne fait pas partie de la procédure de publication et ne doit être utilisé que lorsqu'un
contrôle séparé est explicitement demandé.

Après la release, le workflow publie le catalogue propre au module sous
`module-<id>-catalog`. L’URL correspondante est inscrite dans le `module.json`; GW GUI ne la possède
pas dans ses constantes.

### Module maintenu dans un dépôt indépendant

Copier `sdk/module-template` dans le dépôt du module et remplacer les valeurs d’exemple. Ce projet
référence `GWGUI.Emulation.SDK` depuis NuGet.org et possède son propre `module.json`, ses notes, ses
tags, son workflow et son catalogue. Le tag `vX.Y.Z` déclenche le workflow modèle, qui crée la
release du module puis publie `update-catalog.json` sous le tag technique `module-catalog` de ce
dépôt. Aucun commit, tag ou workflow dans GW GUI n’est requis pour publier une nouvelle version de
ce module.

## 6. Catalogue de mises à jour et updater distribués

Ces fonctions sont réalisées par les workflows :

- une release d’application stable ou sans label met à jour l’actif `update-catalog.json` du tag
  technique `application-catalog` ;
- une snapshot produit son catalogue comme artefact sans remplacer le catalogue stable ;
- une release de l’application contient uniquement GW GUI, son lanceur et son updater, sans module ;
- une release de module inscrit son archive sous `module-<id>-vX.Y.Z` et met à jour uniquement
  `module-<id>-catalog` ;
- le paquet portable et l’installateur contiennent `Updater/gwgui.updater.exe`.

Les publications réelles de GW GUI 0.3.0, des modules Amiga et Atari 1.0.0 et du SDK 1.0.1 ont
confirmé les actifs, catalogues et URL produits.

## 7. Publier le SDK d’émulation

Le contrat public est distribué sur NuGet.org sous l’identifiant `GWGUI.Emulation.SDK`. Dans
NuGet.org, créer une stratégie **Trusted Publishing** appartenant au compte qui publie le paquet,
avec `overthetop78` comme propriétaire GitHub, `GW-GUI` comme dépôt, `sdk-release.yml` comme fichier
de workflow et aucun environnement. Autoriser la publication de nouveaux paquets et versions pour
`GWGUI.Emulation.SDK`. Dans GitHub Actions, définir la variable `NUGET_USER` avec le nom public du
profil NuGet.org. Aucune clé API permanente ni aucun secret `NUGET_API_KEY` ne sont nécessaires.

La version `X.Y.Z` doit être identique dans :

- `Version` de `src/GWGUI.Emulation/GWGUI.Emulation.csproj` ;
- `.github/release-notes/sdk/vX.Y.Z.md` ;
- le tag `sdk-vX.Y.Z` ;
- le paquet `GWGUI.Emulation.SDK.X.Y.Z.nupkg`.

Après commit et push sur `main`, créer et pousser le tag. `.github/workflows/sdk-release.yml` demande
un jeton OIDC GitHub, l’échange avec `NuGet/login@v1` contre une clé temporaire, exécute les tests,
crée le paquet, sa documentation XML et son README, publie sur NuGet.org, puis crée la release GitHub
du SDK avec ses notes. La politique de compatibilité est détaillée dans
[`emulation-sdk-versioning.md`](../architecture/emulation-sdk-versioning.md).

Le README affiché sur NuGet provient de `src/GWGUI.Emulation/README.md`. Il doit décrire le contrat
public actuel et être mis à jour dans le même commit lorsqu’une évolution du SDK change son usage.
