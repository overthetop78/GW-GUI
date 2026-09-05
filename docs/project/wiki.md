# Wiki utilisateur

L'aide utilisateur est uniquement en ligne. Le bouton Documentation ouvre le Wiki GitHub dans la langue de l'application, avec un repli anglais pour une langue inconnue.

Les sources sont dans `wiki/` : `Home.md` propose toutes les langues, chaque dossier de langue contient son guide et son sommaire, et `images/` contient les captures partagées. Les noms des pages sont uniques et portent le code complet de langue. Les guides repris constituent une base à revoir progressivement avec le code actuel.

## Préparer localement

Exécuter `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-wiki.ps1`.

Le script vérifie les langues, les noms des pages, les cibles des liens locaux et les images. Il prépare `build/wiki/`, conserve les dossiers et adapte les liens aux URL du Wiki GitHub. Cette commande prépare uniquement les fichiers locaux.

## Publier sur demande

Exécuter `scripts/publish-wiki.cmd`. Git doit être installé et authentifié avec un compte autorisé à écrire dans le wiki. Avant la première publication, le wiki doit être activé et posséder sa première page GitHub.

Le script prépare les fichiers, clone `GW-GUI.wiki.git`, synchronise les pages et les images, puis crée un commit dans le dépôt du wiki et le pousse. Un manifeste permet de retirer les anciens fichiers issus du script. Les corrections se font dans les sources `wiki/` du dépôt principal.

Le wiki se publie indépendamment des releases du logiciel. Les PDF ne sont plus générés ni distribués.
