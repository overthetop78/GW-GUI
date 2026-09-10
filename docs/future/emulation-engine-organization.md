# Organisation interne des moteurs d'émulation — chantier différé

## Décision et périmètre

Ce document conserve la direction retenue pour les futurs `GWGUI.Emulation.<Famille>`
(Amstrad, Commodore, etc.) et pour une évolution ultérieure d'Atari et d'Amiga.
Le chantier est mis de côté : il ne demande pas de refactorisation immédiate.

Une famille conserve une seule DLL de module. Ses moteurs sont séparés dans le code,
sans créer une DLL ou un projet par moteur. Cette organisation interne est indépendante
du chargement et de la publication des modules de familles.

## Base commune à préserver

- `GWGUI.Emulation` conserve les contrats et les traitements génériques entre App et les
  modules, dans les deux sens. App ne doit pas récupérer les traitements propres aux moteurs.
- Chaque famille conserve une base commune pour l'orchestration et les comportements
  réellement partagés. Rattacher les moteurs à cette base est le fonctionnement souhaité.
- Les particularités d'un moteur appartiennent à son implémentation : préparation des médias,
  options, commandes, reset et autres adaptations spécifiques.
- Les échanges avec un cœur externe peuvent être mutualisés entre moteurs qui utilisent le
  même protocole. Un futur moteur interne ne doit pas être obligé de passer par libretro.

Organisation indicative à appliquer de manière cohérente aux nouvelles familles, en respectant
les conventions du dépôt et les besoins réels :

```text
GWGUI.Emulation.<Famille> — une DLL
  Base commune et orchestration
  Contrats des moteurs
  Implémentations des moteurs et comportements spécifiques
  Catalogue, configurations et enregistrement
```

L'objectif est d'ajouter un moteur avec son implémentation, ses descriptions et son
enregistrement, sans multiplier les conditions sur son identité dans la base commune.
Une capacité réellement nouvelle peut nécessiter de faire évoluer les contrats ou la base :
ils ne sont pas considérés comme exhaustifs. Ne pas inventer des abstractions uniquement
pour des besoins hypothétiques.

## Atari

Le découpage est faisable mais demande davantage qu'un déplacement des factories.
Les factories des moteurs actuels partagent `AtariProcessCore` et `AtariMachine` ; cette
construction commune est normale. En revanche, `AtariExternalCore` et `AtariMachine`
contiennent aussi des comportements spécifiques à certains moteurs, à isoler lorsque
ce chantier sera repris.

Le rangement est utile avec les moteurs déjà présents, mais n'est pas urgent. Le reprendre
avant l'ajout d'un nouveau moteur, notamment interne, ou progressivement lors d'un travail
sur les comportements concernés. Il ne bloque pas les manifestes, la compatibilité ni les
traductions des modules.

## Amiga et nouvelles familles

Le même principe s'applique à Amiga. Avec un seul moteur actuellement, le découpage complet
peut attendre un second moteur ou un moteur interne. Sa facilité exacte doit être évaluée
dans le code avant de planifier une extraction.

Pour les prochaines familles, conserver dès le départ une distinction lisible entre la base
commune et les adaptations du moteur présent, sans construire à l'avance un mécanisme complexe
pour plusieurs moteurs. Réutiliser les traitements de `GWGUI.Emulation` lorsqu'ils sont génériques.

## Conditions de reprise et validation du chantier

Ce chantier ne devient une tâche active qu'après une décision explicite, notamment lors de l'ajout
d'un second moteur dans une famille ou d'un moteur interne. À ce moment-là, une nouvelle feuille de
tâches devra être écrite à partir du code alors présent.

1. Recenser les comportements spécifiques dispersés dans les classes communes.
2. Définir les points d'extension nécessaires à partir de ces comportements existants.
3. Déplacer les adaptations moteur par moteur en conservant la base partagée.
4. Vérifier les configurations existantes, le lancement, l'arrêt, les médias, les entrées,
   l'audio, la vidéo et les états sauvegardés selon les capacités du moteur concerné.
5. Vérifier les autres moteurs après toute modification d'un comportement commun.

Ce découpage réduit le risque de régression mais ne garantit pas à lui seul l'absence de
régression et n'isole pas les plantages natifs. Les validations doivent porter sur le
fonctionnement réel, en complément des tests automatisés pertinents.
