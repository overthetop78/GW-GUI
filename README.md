# GW GUI

**Lire, écrire, convertir, explorer et émuler des images de disquettes depuis une interface Windows.**

GW GUI associe le pilotage du matériel Greaseweazle à un moteur de traitement des images disque et à l’émulation intégrée. Développée en C#/.NET 10 et WPF, l’application réunit les opérations, leurs réglages et leurs journaux dans une même interface.

[Télécharger une version](https://github.com/overthetop78/GW-GUI/releases) · [Guide utilisateur](https://github.com/overthetop78/GW-GUI/wiki) · [Documentation du projet](docs/README.md)

## Ce que permet l’application

| Fonction | Utilisation |
|---|---|
| **Lire une disquette** | Créer une capture de flux SCP ou une image dans un format compatible à partir d’une disquette, avec un contrôleur Greaseweazle et un lecteur. Régler les pistes, les faces et les options de récupération. |
| **Écrire une disquette** | Restaurer une image sur un support physique, choisir les paramètres du lecteur et de l’écriture, puis suivre l’opération. |
| **Convertir des images** | Produire une ou plusieurs sorties compatibles depuis une image disque, avec détection du format et choix des destinations. |
| **Visualiser les données** | Examiner les faces, pistes, flux et secteurs d’une image, selon les informations disponibles dans son format. |
| **Explorer les fichiers** | Parcourir les volumes, dossiers et fichiers lorsque le système de fichiers de l’image est reconnu. |
| **Émuler des machines** | Créer et enregistrer des configurations de machines, associer leurs médias et régler les entrées, la vidéo et l’audio. |
| **Entretenir et diagnostiquer le matériel** | Accéder à l’effacement, au nettoyage des têtes, aux informations du contrôleur, aux mesures de vitesse et aux autres outils Greaseweazle. |

Les profils permettent de retrouver les réglages des opérations. Les commandes et journaux intégrés facilitent le suivi et le diagnostic, tandis que les options avancées donnent accès aux paramètres techniques.

Le matériel Greaseweazle est nécessaire pour travailler sur les disquettes physiques. La conversion, la visualisation et l’exploration d’images existantes s’utilisent depuis leurs fichiers. Les opérations disponibles dépendent du format de l’image et, pour l’exploration, du système de fichiers reconnu.

## Installer et utiliser GW GUI

L’application est proposée pour **Windows x64**, avec une interface multilingue et deux distributions :

- **ZIP portable** : extraire l’archive et lancer `gwgui.exe`. Le marqueur `portable.flag` permet de conserver les réglages, journaux et outils gérés dans `Data`, à côté de l’application.
- **Installateur** : installer l’application avec l’assistant multilingue. Les données sont conservées dans les dossiers utilisateur Windows.

Le **Microsoft Windows Desktop Runtime .NET 10 x64** est nécessaire à l’exécution ; le runtime .NET n’est pas inclus dans les paquets.

Le [wiki utilisateur](https://github.com/overthetop78/GW-GUI/wiki) propose un choix parmi 29 langues. Le menu d’aide ouvre directement le guide dans la langue de l’application. Cette aide est uniquement en ligne ; les guides sont progressivement enrichis et révisés.

## Compiler le projet avec build.ps1

Prérequis : **Windows, PowerShell et le SDK .NET 10**. La sélection du SDK est définie dans [global.json](global.json). Exécuter les commandes depuis la racine du dépôt.

Le script compile l’application et son lanceur, puis range les bibliothèques et les ressources de langue pour obtenir un dossier prêt à lancer.

```powershell
# Construire Debug et Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1

# Construire uniquement Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Debug

# Construire uniquement Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Release
```

| Configuration | Exécutable produit |
|---|---|
| Debug | `build/Debug/GW GUI/gwgui.exe` |
| Release | `build/Release/GW GUI/gwgui.exe` |

Le script reconstruit le dossier de la configuration choisie et ferme au besoin l’application exécutée depuis ce dossier. Les sorties de compilation sont ignorées par Git.

## Créer les distributions avec package.ps1

Le script `package.ps1` compile en Release par défaut et produit le ZIP portable, l’installateur et leurs empreintes SHA-256. **Inno Setup 6** est nécessaire pour construire l’installateur.

Le paramètre `-Version` est obligatoire. Dans les exemples suivants, `0.1.3` est un numéro illustratif à remplacer par la version à préparer.

```powershell
# Créer le ZIP portable et l’installateur
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/package.ps1 -Version 0.1.3

# Créer uniquement le ZIP portable, sans nécessiter Inno Setup
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/package.ps1 -Version 0.1.3 -SkipInstaller
```

| Sortie | Contenu |
|---|---|
| `dist/GW-GUI-0.1.3-win-x64-portable.zip` | Archive portable à distribuer. |
| `dist/GW-GUI-0.1.3-win-x64-setup.exe` | Installateur, sauf avec `-SkipInstaller`. |
| `dist/SHA256SUMS.txt` | Empreintes des paquets produits. |
| `dist/portable/GW GUI/gwgui.exe` | Copie locale prête à lancer. |

Le dossier `Data` de la copie portable locale est conservé ; il n’est pas ajouté au ZIP distribué. La construction des paquets est locale : leur envoi sur GitHub utilise la procédure de publication ci-dessous.

## Documentation pour le développement

- [Développement avec .NET et organisation du dépôt](docs/project/development.md)
- [Publier une release ou une snapshot](docs/project/release.md)
- [Préparer et publier le wiki](docs/project/wiki.md)
- [Tests disponibles et contrôles de release](docs/project/testing.md)
- [Architecture, références et suivi des travaux](docs/README.md)

## Licence

GW GUI est distribué sous [licence MIT](LICENSE). Les informations concernant les composants tiers sont dans [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
