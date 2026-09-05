# Préparer et publier le wiki

Les commandes de cette page s’exécutent depuis la racine du dépôt.

Les sources du wiki sont conservées dans [wiki/](../../wiki/Home.md), avec un dossier par langue et un dossier `images/` partagé. Les modifications se font dans ces sources ; le dépôt `GW-GUI.wiki.git` reçoit le résultat de la préparation.

## Préparer les fichiers localement

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-wiki.ps1
```

Le script vérifie la présence des langues, l’unicité des noms de pages et les cibles des liens locaux. Il copie les pages et les images dans `build/wiki`, en convertissant les liens Markdown et les chemins des balises HTML `<img>` vers les URL adaptées à GitHub Wiki. Il conserve les dossiers par langue.

## Publier les fichiers sur GitHub

```powershell
.\scripts\publish-wiki.cmd
```

Cette commande lance `publish-wiki.ps1`, qui appelle d’abord `build-wiki.ps1`, clone le dépôt du wiki, synchronise les fichiers, puis effectue le commit et le push du wiki. Git doit disposer d’un accès en écriture au wiki ; le script reprend le nom et l’adresse de l’auteur configurés dans le dépôt principal.

Lors de la toute première mise en place, le wiki doit être activé et posséder une première page créée sur GitHub pour permettre son clonage.

La préparation locale et la publication sont séparées pour pouvoir vérifier les fichiers sans les envoyer. Un push du dépôt principal ou une release du logiciel ne publie pas le wiki : sa publication se lance avec sa propre commande.

Les noms de pages sont uniques et portent le code complet de langue. Les guides repris constituent une base à revoir progressivement avec le code actuel. L’aide de l’application ouvre directement la page de sa langue, avec un repli anglais pour une langue inconnue.

Le script de publication utilise un manifeste pour retirer les anciens fichiers issus de ses publications. Les PDF ne sont plus générés ni distribués.
