# Évolutions futures des modules d'émulation

Le chargement actuel est décrit dans
[`../architecture/emulation-modules.md`](../architecture/emulation-modules.md) et son contrat complet
dans [`../architecture/emulation-module-authoring.md`](../architecture/emulation-module-authoring.md).

Le [découpage interne des moteurs](emulation-engine-organization.md) fait l'objet d'un chantier
différé distinct : conserver une base commune et isoler les adaptations des moteurs au sein
d'une seule DLL par famille, avec la même approche pour les futurs `GWGUI.Emulation.<Famille>`.

## Projets séparés et SDK

C'est utile, faisable et pratique : chaque module peut avoir son dépôt, sa version, ses tests et ses
publications. GW GUI doit d'abord publier un petit SDK versionné contenant uniquement les contrats
publics. Un module référencera ce SDK, jamais le projet principal.

Avant de promettre cette indépendance, il faut stabiliser :

- le paquet `GWGUI.Emulation.SDK` ;
- l'évolution de l'API hôte (version initiale `1.0` et contrôle par manifeste en place) ;
- le raccordement des textes propres au module (voir le mécanisme embarqué ci-dessous) ;
- le format du paquet et ses dépendances privées ;
- les tests exécutés contre chaque version de GW GUI prise en charge.

## Sous-dossier et manifeste

Le manifeste obligatoire et le sous-dossier par famille sont en place pour Amiga et Atari.
L'API actuelle et les bornes de ces deux modules valent `1.0`. Les DLL seules directement dans
`Modules` ne sont plus chargées. Le futur paquet autonome complétera cette structure avec les
dépendances privées :

```text
Modules/Commodore/
  module.json
  gwgui.emulation.commodore.dll
  dependance-privee.dll
```

Le sous-dossier évite de mélanger les fichiers de toutes les familles. `module.json` décrit le paquet
avant de charger son code :

```json
{
  "schemaVersion": 1,
  "id": "Commodore",
  "entryAssembly": "gwgui.emulation.commodore.dll",
  "moduleVersion": "1.2.0",
  "hostApiMinimum": "1.0",
  "hostApiMaximum": "1.0"
}
```

Il indique la DLL d'entrée et la compatibilité. Il ne contient aucun réglage de machine ou donnée
utilisateur.

## Version de l'API hôte

Elle sert bien au contrôle de compatibilité. Un module exigeant l'API 2 doit être refusé proprement
par une application limitée à l'API 1. La raison exacte est enregistrée dans le journal avant
d'instancier la factory. Cette version ne désigne ni GW GUI, ni le module, ni le cœur : elle désigne
uniquement leur langage commun.

La compatibilité avec de futures versions n'est pas présumée : minimum et maximum valent
actuellement `1.0`. Les bornes ne seront élargies qu'après vérification du module concerné.

## Diagnostic

Les logs sont obligatoires et suffisent. Une page graphique serait seulement une vue pratique des
mêmes informations : module chargé, version, chemin ou raison du refus. Elle reste facultative.

## Traductions et blocs graphiques

Les blocs graphiques sont déjà déclaratifs. Le module envoie onglet, bloc, éditeur, valeur, choix,
visibilité et règles ; GW GUI crée les contrôles. Le module n'envoie jamais de XAML ou de contrôle
WPF arbitraire.

Un catalogue propre au module concerne seulement les textes associés aux clés de ressources : noms
des machines, libellés et explications. Amiga et Atari embarquent leurs catalogues dans leur DLL
via `IEmulationModuleLocalization`, avec une base neutre et les 29 cultures. Les textes hôte
communs restent dans les ressources centrales. Le raccordement et son suivi de validation sont
décrits dans [emulation-module-localization.md](../architecture/emulation-module-localization.md)
et dans la [feuille de tâches](../tasks/emulation/module-autonomy.md).

Lorsqu'une clé existe à la fois dans le module et dans les ressources internes, la traduction du
module est prioritaire. Cela permet de corriger ou renommer un champ dans une nouvelle version du
module sans attendre une nouvelle version de GW GUI. Les ressources internes servent uniquement de
repli pour les textes communs. Le mécanisme doit être validé sur Amiga et Atari avant la création
d'un troisième module. Les catalogues embarqués remplacent la proposition de fichiers de langue
externes par module.

## Sélection des modules officiels

Cette idée correspondait à des cases dans l'installateur pour omettre Amiga ou Atari. Elle n'apporte
rien tant que le paquet complet reste raisonnable. Elle est différée jusqu'au moment où le nombre ou
la taille des modules justifiera une installation personnalisée. La première version devra extraire
les modules officiels déjà contenus dans l'installateur ; un installateur téléchargeant uniquement la
sélection ne sera utile que si la réduction de la taille du paquet devient nécessaire.

## `AssemblyLoadContext`

Un contexte dédié permet à chaque module d'utiliser ses propres versions de dépendances sans conflit.
Une variante « collectable » peut théoriquement décharger un module de la mémoire.

Le remplacement à chaud est fragile : machines, événements, tâches et DLL natives doivent tous être
libérés parfaitement. La décision retenue est donc de prévoir l'isolation des dépendances si elle
devient nécessaire, mais de mettre à jour les modules uniquement après fermeture et redémarrage.

## Mise à jour de l'application et des modules

GW GUI doit pouvoir annoncer séparément les mises à jour disponibles pour l'application et pour
chacun des modules installés. L'utilisateur sélectionne les mises à jour souhaitées, puis GW GUI
ouvre une application de mise à jour dédiée.

Le flux retenu est :

1. GW GUI recherche les mises à jour de l'application et de tous les modules installés ;
2. toutes les versions sélectionnées sont téléchargées, contrôlées et préparées ensemble ;
3. GW GUI lance l'application de mise à jour avec le plan complet ;
4. l'application de mise à jour demande à GW GUI de se fermer puis attend la fin réelle du processus ;
5. elle sauvegarde les fichiers actuels et remplace l'application, le SDK et tous les modules concernés ;
6. elle relance une seule fois la nouvelle version de GW GUI ;
7. si le nouveau démarrage échoue, elle peut restaurer les fichiers sauvegardés.

L'application de mise à jour est un exécutable minimal distinct : elle ne charge aucun module et ne
dépend pas des DLL qu'elle doit remplacer. Aucune mise à jour n'est appliquée pendant que GW GUI ou
une machine émulée fonctionne. Plusieurs mises à jour sont toujours regroupées dans une seule
fermeture et un seul redémarrage.

Cette fonction n'est à réaliser qu'après le manifeste, la version d'API et la vérification du paquet.
Sans ces trois éléments, une mise à jour indépendante serait inutilement fragile.
