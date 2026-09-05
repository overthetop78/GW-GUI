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
