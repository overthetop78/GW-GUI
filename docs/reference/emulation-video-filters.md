# Recherche sur les filtres vidéo d’émulation

## Statut et périmètre

Recherche effectuée le 1er septembre 2026 pour le point 7 de
`docs/tasks/interface/emulation-improvements.md`.

Ce document constitue le catalogue et la proposition d’architecture à faire valider. Il ne valide
pas à lui seul les choix fonctionnels et ne déclenche aucune implémentation. Conformément à la
checklist, aucun contrat, shader, réglage de configuration ou contrôle d’interface n’est créé avant
validation des groupes, compatibilités, présélections, réglages et de l’architecture proposée.

Le périmètre couvre les traitements réalisés par GW GUI après réception d’une `VideoFrame`, pour
les machines actuelles et futures. Les choix qui modifient la machine ou le signal qu’elle produit
restent des options de son émulateur. GW GUI ne doit donc pas recréer sous forme de filtre ses
options RGB, composite, S-Video, RF, PAL, NTSC, monochrome ou équivalentes. Un effet inspiré d’un
signal ne pourra être étudié que comme simulation distincte et explicitement nommée.

## Sources consultées

- [Guide officiel des shaders Libretro](https://docs.libretro.com/guides/shaders/) : paramètres
  modifiables en direct, présélections et chaînes composées de plusieurs passes.
- [Catalogue officiel slang-shaders](https://github.com/libretro/slang-shaders) et sa
  [spécification](https://github.com/libretro/slang-shaders/blob/master/README.md) : familles,
  textures intermédiaires, tailles, historique de trames, paramètres et échantillonnage.
- [Catalogue officiel common-shaders](https://github.com/libretro/common-shaders) : CRT, handheld,
  NTSC/PAL, interpolation, dithering, sharpen, ScaleFX et xBR.
- [Discussion de licence de slang-shaders](https://github.com/libretro/slang-shaders/issues/150) :
  licences multiples, parfois absentes, à vérifier pour chaque fichier et dépendance.
- [Documentation officielle de PUAE](https://docs.libretro.com/library/puae/) et
  [documentation officielle de Hatari](https://github.com/libretro/hatari/blob/master/doc/hatari.1).
- [Shaders portables de Veldrid](https://github.com/mellinoe/veldrid-docs/blob/master/articles/portable-shaders.md)
  et [backends Veldrid](https://github.com/veldrid/veldrid).
- [Documentation Microsoft de `ShaderEffect`](https://learn.microsoft.com/dotnet/api/system.windows.media.effects.shadereffect)
  et [spécification OpenGL 4.6](https://registry.khronos.org/OpenGL/specs/gl/glspec46.core.pdf).

Le code examiné comprend `IEmulationVideoSurface`, `MachineVideoPresenter`, les surfaces WPF,
OpenGL et Veldrid, `EmulationVideoSurfaceFactory`, `VideoFrame`, les descriptions vidéo Amiga et
Atari, ainsi que le raccordement d’enregistrement et d’application aux instances ouvertes.

## Enseignements des collections Libretro

Libretro distingue une présélection de la chaîne qu’elle décrit. Une chaîne empile des passes ;
chaque passe choisit son échantillonnage et sa résolution, et expose éventuellement des paramètres.
Ce modèle confirme la séparation utile entre catalogue stable, valeurs enregistrées par machine,
chaîne ordonnée et exécution propre au renderer.

Les fichiers Slang ne sont toutefois pas réutilisables tels quels : ils supposent le runtime de
présélections Libretro, ses ressources, ses uniformes et parfois l’historique des trames. GW GUI
devrait reprendre le modèle de chaîne, pas intégrer silencieusement un interpréteur RetroArch.

### Politique de licence proposée

GW GUI est sous licence MIT, tandis que les collections n’ont pas de licence unique. Par exemple,
[`newpixie-crt.slang`](https://github.com/libretro/slang-shaders/blob/master/crt/shaders/newpixie/newpixie-crt.slang)
propose MIT ou domaine public,
[`crt-geom.slang`](https://github.com/libretro/slang-shaders/blob/master/crt/shaders/crt-geom.slang)
est GPL-2.0-or-later et
[`lcd1x.slang`](https://github.com/libretro/slang-shaders/blob/master/handheld/shaders/lcd1x.slang)
contient une modification GPL-2.0-or-later d’un travail annoncé domaine public.

Règles proposées : aucune copie sans inventaire des dépendances et preuve de licence compatible ;
aucun shader GPL dans le code MIT sans décision explicite ; un fichier sans licence sert seulement
de référence ; privilégier une implémentation originale ou une source permissive vérifiée ;
conserver auteur, origine, licence, dépendances et notices de toute source finalement retenue.

## Catalogue fonctionnel

Ce catalogue est extensible et ne promet pas que toutes les familles seront livrées d’emblée.

| Famille | Effet et réglages utiles | Dépendances et combinaisons | Licence de référence |
|---|---|---|---|
| Image générale | luminosité, contraste, gamma, saturation, netteté ; neutre 0 | toujours disponible, passe commune | à développer dans GW GUI |
| Échantillonnage | nearest, bilinéaire, sharp-bilinear, bicubique, échelle entière | une méthode à la fois ; préalable aux motifs | vérifier `interpolation`, `bicubic`, `windowed` |
| Scalers pixel art | xBR, xBRZ, HQx, ScaleFX, ScaleNx et SABR ; facteur, seuil, lissage | multi-passe, un scaler à la fois ; avant la technologie d’affichage | licences hétérogènes, à vérifier séparément pour chaque algorithme |
| Scanlines | orientation, intensité, épaisseur, phase, compensation | compatible avec CRT ; éviter deux générateurs | licences par fichier dans `scanlines`/`crt` |
| CRT — faisceau | largeur, intensité, diffusion, halo | lumière linéaire recommandée ; avec scanlines et masque | GPL, MIT, domaine public ou inconnue |
| CRT — masque | grille d’ouverture, shadow/slot mask ; motif, force, triades, RGB/BGR | dépend de la résolution de sortie | vérifier shader, LUT et textures |
| CRT — géométrie | courbure, coins, vignette, overscan visuel | préserve le rapport logique ; peut accentuer le moiré | `crt-geom` est GPL-2.0-or-later |
| Écran à pixels fixes | sous-choix LCD, LCD/LED ou OLED ; couleur ou palette monochrome, grille, sous-pixels, espace inter-pixels, ordre RGB/BGR, noir, rétroéclairage et réponse | exclusif du CRT, Plasma et Vectoriel ; paramètres propres affichés seulement s’ils produisent une différence réelle | licences hétérogènes dans `handheld` |
| Plasma | cellules, diffusion, tramage temporel et rémanence | technologie principale exclusive ; historique nécessaire au temporel | à développer dans GW GUI après validation des paramètres |
| Écran vectoriel | renforcement des lignes, halo et persistance | approximation raster exclusive des autres technologies ; historique nécessaire | à développer dans GW GUI tant que le moteur ne fournit pas de primitives vectorielles |
| VFD | phosphores lumineux, couleur, halo et persistance | affichage spécialisé à ajouter lorsqu’une machine le demande | à développer dans GW GUI |
| Matrice LED | cellules LED, espacement, diffusion et couleur | technologie spécialisée exclusive, compatible avec scaler, restauration et réglages généraux | implémentation raster originale GW GUI, MIT |
| Matrice de points | palette, forme et taille des points, contraste et réponse | technologie spécialisée exclusive ; compatible avec scaler, restauration et réglages généraux | implémentation raster originale GW GUI, MIT |
| Affichage à segments | disposition 7/14/16, teinte, épaisseur, contraste, halo et réponse | technologie spécialisée exclusive ; compatible avec scaler, restauration et réglages généraux | implémentation raster originale GW GUI, MIT |
| Papier électronique | modes monochrome, 16 gris ou 4096 couleurs, contraste, tramage, rafraîchissement et image fantôme | technologie secondaire exclusive avec historique temporel | implémentation raster originale GW GUI, MIT |
| Projection | flou optique, diffusion, texture de toile et convergence RGB | technologie secondaire exclusive, compatible avec scaler, restauration et réglages généraux | implémentation raster originale GW GUI, MIT |
| Moiré/trame volontaire | orientation, fréquence, phase, intensité | distinct du moiré accidentel ; incompatible avec LCD | à développer dans GW GUI |
| Désentrelacement | bob, weave, blend, détection | peut exiger la trame précédente ; ne remplace pas le moteur | licences variables |
| Rémanence générale | traînée lumineuse indépendante, intensité | exige une image d’historique bornée ; compatible avec toute technologie | implémentation temporelle originale GW GUI, MIT |
| Flou de mouvement | mélange de la frame courante avec la précédente, intensité | exige une image d’historique bornée ; compatible avec toute technologie | implémentation temporelle originale GW GUI, MIT |
| Scintillement | modulation alternée de luminosité, intensité | déterministe par numéro de frame ; ne supprime pas la frame | implémentation temporelle originale GW GUI, MIT |
| Entrelacement simulé | champs pairs/impairs alternés, intensité | distinct du désentrelacement de restauration | implémentation temporelle originale GW GUI, MIT |
| Insertion d’images noires | une frame noire sur deux, activation | exige une séquence fiable ; appliquée en dernier | implémentation temporelle originale GW GUI, MIT |
| Restauration | dé-dithering, débruitage, réduction de bandes | avant scaler et modèle d’affichage | licences variables |
| Netteté/flou avancés | convolution, sharpen adaptatif, flou | distinct de la netteté générale | licences variables |
| Palette/colorisation | palette monochrome, température, intensité | utile aux futurs écrans monochromes | textures et licences à vérifier |
| Bordure/habillage | cadre ou décor | relève du point 8 | exclu du point 7 |
| Simulation composite GW GUI | bande chromatique, bavure horizontale et dot crawl | post-traitement explicite ; ne modifie aucune option moteur | implémentation originale GW GUI, MIT |
| Simulation S-Video GW GUI | luminance séparée, chrominance légèrement limitée | post-traitement explicite sans dot crawl ; ne modifie aucune option moteur | implémentation originale GW GUI, MIT |
| Simulation RF GW GUI | perte de netteté et bruit de transmission | post-traitement explicite ; ne modifie aucune option moteur | implémentation originale GW GUI, MIT |
| Simulation PAL GW GUI | alternance de phase et mélange chromatique vertical | post-traitement explicite ; ne change pas la norme moteur | implémentation originale GW GUI, MIT |
| Simulation NTSC GW GUI | retard chromatique horizontal et phase de teinte | post-traitement explicite ; ne change pas la norme moteur | implémentation originale GW GUI, MIT |
| Grain analogique | bruit fin monochrome modulé par la luminance, intensité | effet stylistique indépendant, distinct du bruit RF | implémentation originale GW GUI, MIT |
| VHS | instabilité, perte de bande passante, retard chromatique, tracking et commutation des têtes | effet stylistique déterministe à la résolution de sortie | implémentation originale GW GUI, MIT |
| Aberration chromatique | décalage opposé des composantes rouge et bleue, intensité | effet stylistique spatial à la résolution de sortie | implémentation originale GW GUI, MIT |
| Halo lumineux | extraction et diffusion bornée des hautes lumières, intensité | effet stylistique global, distinct des halos propres aux technologies | implémentation originale GW GUI, MIT |
| Sépia | conversion activable vers une teinte brun chaud | effet colorimétrique à la résolution de sortie | implémentation originale GW GUI, MIT |

Les familles créatives `film`, `HDR`, `waterpaint` ou `cel` montrent l’intérêt d’un catalogue
extensible, mais ne sont pas retenues pour la première proposition de fidélité d’affichage.

## Options internes des moteurs

### Amiga — PUAE

Les options `puae_video_standard`, fréquence PAL/NTSC, résolution, rapport de pixel, recadrage,
mode de ligne, saut de trames, profondeur de couleur, gamma PUAE et correction du scintillement
sont décrites par `AmigaSettingsDescriptionFunctions` et transmises au cœur. Elles restent des
options de l’émulateur.

Le futur gamma général de GW GUI est un post-traitement distinct. Son stockage ne doit pas
réutiliser `puae_gfx_gamma`, et modifier l’un ne doit pas modifier l’autre.

### Atari — Hatari et Atari800

GW GUI transmet notamment à Hatari le choix couleur/monochrome, la fréquence PAL/NTSC, l’overscan,
le saut de trames et le rapport d’aspect. Son filtre polarisé reste aussi une option interne.

Pour Atari800, la région PAL/NTSC/SECAM, l’artifacting, la palette externe, la teinte, saturation,
contraste, luminosité, gamma et délai de couleur modifient la sortie du cœur. Ils ne doivent pas
être déplacés dans le catalogue GW GUI. Les cinq réglages généraux de GW GUI s’appliqueront à la
`VideoFrame` déjà produite. Cette séparation vaut aussi pour chaque futur moteur.

## Analyse des surfaces de rendu actuelles

### Point commun d’entrée

Les pixels sont disponibles dans `MachineVideoPresenter.Present` sous forme de `VideoFrame`. Chaque
surface les convertit en BGRA32 avec `EmulationVideoPixelFunctions.ToBgra32`. Le point commun se
situe après cette conversion logique et avant l’affichage final. La chaîne traite une image commune
et ne doit pas appartenir aux bibliothèques Amiga ou Atari.

Une définition commune peut décrire l’ordre, les paramètres, les tailles source/sortie, le temps et
le numéro de trame. L’exécution reste propre au backend pour ses passes GPU et textures temporaires.

| Surface | Pixels disponibles | Partie partageable | Travail propre au backend |
|---|---|---|---|
| WPF | BGRA32 puis `WriteableBitmap` | paramètres et ajustements CPU | chaîne CPU de référence ; `ShaderEffect` seul est trop limité pour garantir les multi-passes |
| OpenGL | BGRA32 avant `glDrawPixels` | mêmes paramètres et formules | remplacer le chemin fixe par texture, quad, GLSL, framebuffers et uniformes |
| Direct3D 11 | texture BGRA32 et quad Veldrid | définition, shaders SPIR-V et chaîne communs à Vulkan | pipelines, textures temporaires et buffers de paramètres |
| Vulkan | texture BGRA32 et quad Veldrid | définition, shaders SPIR-V et chaîne communs à D3D11 | ressources et synchronisation du device Vulkan |

La faisabilité par famille est la suivante :

| Famille | WPF/CPU | OpenGL | Direct3D 11 et Vulkan |
|---|---|---|---|
| Image générale | une passe CPU commune | une passe GLSL | une passe SPIR-V Veldrid commune |
| Échantillonnage | `WriteableBitmap` rééchantillonné | sampler/fragment shader | sampler/pipeline Veldrid |
| Scalers pixel art | possible mais coût CPU élevé | programmes et FBO multi-passes | passes et textures Veldrid communes |
| Scanlines, CRT, LCD, trame | formules CPU possibles ; mesurer le coût | fragment shaders, FBO si halo | shaders portables ; textures intermédiaires pour halo |
| Désentrelacement, persistance | buffers de trames CPU | textures d’historique | textures d’historique Veldrid |
| Restauration, netteté, flou | convolutions CPU, coût selon rayon | une ou plusieurs passes | une ou plusieurs passes communes |
| Palette/colorisation | table ou formule CPU | LUT ou uniforme | LUT ou uniforme Veldrid |

Les définitions de paramètres, l’ordre, les tailles, l’espace de couleur et les règles de
compatibilité sont partagés par toutes les lignes. Seuls l’exécuteur, les shaders compilés et les
ressources intermédiaires sont propres au backend.

Direct3D 11 et Vulkan peuvent partager une exécution Veldrid. OpenGL ne doit pas être supprimé :
son contexte de compatibilité actuel doit recevoir une chaîne texturée moderne. WPF reste le repli
fonctionnel ; une version CPU est nécessaire au minimum pour les cinq réglages généraux et chaque
fonction déclarée compatible WPF.

Une fonction non réalisable partout doit déclarer ses capacités. L’interface doit la désactiver avec
une explication localisée ou utiliser un repli CPU mesuré ; elle ne doit jamais disparaître ni
changer silencieusement de renderer.

## Effets sur les comportements existants

### Snapshot

Les surfaces conservent actuellement une copie non traitée, utilisée directement par la capture
d’écran. La décision retenue définit `Snapshot` comme l’image finale après tous les traitements GW
GUI, mais avant le futur habillage du point 8, afin qu’elle soit identique à l’image visible dans la
zone d’émulation sur tout renderer. Une capture n’inclut donc ni bezel, ni cadre, ni autre décoration
externe à l’image émulée.

Une lecture GPU à chaque trame serait coûteuse. La surface doit produire une copie lisible à la
demande ou conserver une ressource de sortie réutilisable. Le contrat actuel évoluera pendant la
tâche d’implémentation correspondante.

### Rapport d’aspect et redimensionnement

`MachineVideoPresenter.FitScreen` utilise `VideoFrame.AspectRatio`. Les filtres ne changent pas ce
rapport logique. Les tailles source, intermédiaire et physique sont transmises aux passes pour
stabiliser scanlines, masques et LCD. La géométrie CRT déforme dans le rectangle ajusté sans changer
le rectangle WPF ni `VideoFrame.AspectRatio`.

Les effets sensibles à l’échelle entière utilisent un échantillonnage stable à échelle non entière
ou annoncent leur limitation ; ils ne doivent pas créer silencieusement un moiré accidentel.

### Repli vers WPF

`MachineVideoPresenter` remplace actuellement tout renderer fautif par WPF. Le repli doit conserver
la configuration. La chaîne CPU applique les équivalents déclarés et, en dernier recours, au moins
les cinq réglages généraux. Une fonction ignorée doit produire un diagnostic explicite ; une erreur
de compilation ne doit jamais modifier la configuration enregistrée.

### Application immédiate

Aujourd’hui, l’enregistrement applique immédiatement le seul `VideoRenderer` à l’onglet ouvert
correspondant. La future chaîne suivra le même ciblage `(ModuleId, ConfigurationId)` :

- enregistrer dans la seule configuration modifiée ;
- mettre à jour la seule instance ouverte correspondante, sans recréer la machine ;
- appliquer les valeurs enregistrées au prochain démarrage si aucune instance n’est ouverte ;
- mettre à jour les uniformes sans reconstruire les pipelines pour un changement numérique ;
- reconstruire atomiquement une chaîne structurellement modifiée et conserver l’ancienne si la
  nouvelle échoue.

## Décisions fonctionnelles validées

### Organisation de l’onglet Vidéo

Les cinq réglages généraux — luminosité, contraste, gamma, saturation et netteté — restent toujours
visibles. Ils ne sont jamais dupliqués dans un panneau de technologie ou de filtre.

L’échantillonnage est un sélecteur unique. Il contiendra progressivement pixels nets, bilinéaire,
sharp-bilinear, bicubique et les autres méthodes retenues. Une seule méthode est active à la fois.

La technologie d’affichage est également choisie dans un sélecteur unique. Le panneau placé sous ce
sélecteur affiche uniquement les paramètres de la technologie choisie :

| Choix principal | Paramètres et sous-choix validés |
|---|---|
| Normal | aucun modèle d’écran ; seuls l’échantillonnage, les réglages généraux et les traitements indépendants s’appliquent |
| CRT | rendu couleur ou monochrome, faisceau, masque, halo, courbure, vignettage, scanlines et trame/moiré volontaire |
| Écran à pixels fixes | technologie LCD, LCD rétroéclairé par LED ou OLED ; grille, sous-pixels, netteté et réponse temporelle partagées, avec paramètres particuliers seulement lorsqu’ils produisent une différence visible |
| Plasma | structure des cellules, diffusion, tramage temporel et rémanence |
| Écran vectoriel | lignes lumineuses, halo et persistance ; approximation depuis la `VideoFrame` raster tant qu’aucun moteur ne fournit de primitives vectorielles |

Toutes ces technologies font partie de la cible. Elles seront réalisées pas à pas et non dans une
seule modification.

### CRT

CRT reste un seul choix principal. Son sélecteur de rendu contient : couleur, monochrome vert,
ambre, blanc ou gris. Les rendus monochromes proposent des palettes prédéfinies et pourront
également recevoir une teinte personnalisée.

La référence CPU calcule la luminance Rec. 709 en lumière linéaire, puis la multiplie par la teinte
convertie de sRGB vers la lumière linéaire. Les teintes initiales sont vert `#66FF66`, ambre
`#FFB000`, blanc `#FFFFFF` et gris `#B0B0B0`. La teinte personnalisée utilise son RGB ; son alpha
est ignoré. Une teinte personnalisée absente retombe sur blanc. Le mode couleur ne modifie pas les
canaux à cette étape.

Les passes CRT CPU sont originales et s’exécutent, en lumière linéaire, dans cet ordre : courbure,
faisceau, halo, masque, vignettage. La courbure applique une déformation en tonneau bornée à `0,18` ;
le faisceau mélange les voisins verticaux jusqu’à `45 %`, ajoute une diffusion 3×3 jusqu’à `35 %`
et un gain jusqu’à `1,5` ; le halo ajoute jusqu’à `50 %` de la moyenne 3×3. Le masque atténue au
maximum de `75 %` suivant une grille d’ouverture, un shadow mask ou un slot mask et l’ordre RGB,
BGR ou monochrome. Le vignettage retire au maximum `75 %` dans les coins selon une puissance `1,5`.
Chaque valeur `0` est strictement neutre et chaque sortie est bornée à `0..1`. Ces formules ne
reprennent aucun code, texture ou table d’un shader tiers.

Veldrid et OpenGL reçoivent ces valeurs par le même encodeur d’uniformes et exécutent les mêmes
passes dans leurs shaders. Veldrid construit un nouvel ensemble texture/vue/buffers/layout/shaders/
pipeline avant de remplacer l’ensemble actif ; OpenGL détruit tout shader ou programme provisoire
si compilation ou liaison échoue. Une erreur de présentation GPU déclenche le repli existant de
`MachineVideoPresenter` vers la surface WPF et sa référence CPU, avec rejeu de la même image.

Les scanlines sont une option propre au panneau CRT et ne sont affichées que lorsque CRT est choisi.
Elles proposent au minimum orientation horizontale ou verticale, intensité, épaisseur, phase et
compensation de luminosité.

La passe CPU échantillonne au quart du pixel une onde de période deux pixels sur l’axe choisi, afin
que son épaisseur reste visible même à l’échelle 1:1. L’intensité règle son
atténuation de `0..100 %`, l’épaisseur fait varier l’exposant de l’onde de `8` à `0,5`, la phase
décale le motif de `0..2` pixels et la compensation ajoute un gain maximal de `50 %` pondéré par
l’intensité. Désactivée ou avec une intensité nulle, la passe est strictement neutre.

La trame ou le moiré volontaire appartient également au panneau CRT. Il doit rester distinct du
moiré accidentel produit par un mauvais échantillonnage. Il propose orientation horizontale ou
verticale, fréquence, phase et intensité.

La passe CPU volontaire utilise sur l’axe choisi une sinusoïde de `1..32` cycles dans la taille de
sortie, une phase de `0..2π` et une atténuation maximale de `50 %`. Elle n’inspecte ni ne corrige le
contenu et ne change pas de formule selon la méthode de redimensionnement : elle ne peut donc pas
être confondue avec une réduction du moiré accidentel. Désactivée ou d’intensité nulle, elle est
strictement neutre.

### Écrans à pixels fixes

LCD monochrome, LCD couleur, écran LCD rétroéclairé par LED et OLED ne constituent pas quatre
panneaux principaux. Ils partagent le choix **Écran à pixels fixes**, puis un sélecteur de
technologie et les réglages communs. Un paramètre conditionnel n’est ajouté que s’il représente une
différence visuelle réelle, par exemple le rétroéclairage LCD/LED ou le niveau de noir OLED. Si une
technologie ne justifie aucun réglage propre, elle reste une présélection du panneau commun.

La référence CPU et les shaders placent la matrice dans les coordonnées logiques de la frame
émulée (Processing.zw) et non dans celles des pixels physiques de l’écran. Le redimensionnement
ne change donc ni le nombre de cellules simulées ni leur phase. L’espace inter-pixels règle la
largeur géométrique de l’interstice ; l’intensité de grille règle séparément son obscurcissement,
avec des bords adoucis pour éviter une grille dure. Les sous-pixels RGB et BGR divisent chaque
cellule en trois bandes et inversent leur ordre avec une atténuation de 42 %, indépendamment de la
grille. Le mode monochrome calcule la luminance Rec. 709 linéaire et propose une palette explicite
vert, gris, ambre, bleu ou blanc ; aucune saisie ARGB n’est présentée à l’utilisateur.

Les trois technologies emploient des modèles distincts. Le LCD transmissif conserve un plancher
noir gris, un rétroéclairage diffus et un halo doux. Le LCD rétroéclairé par LED possède un plancher
plus bas, une luminance de pointe supérieure et un halo local plus marqué autour des zones
lumineuses. L’OLED est émissif : il n’expose aucun rétroéclairage, garde le plancher noir le plus
bas, applique une courbe de contraste propre et ne diffuse pas les zones lumineuses. Le curseur de
profondeur des noirs contrôle le plancher propre à chaque technologie ; les valeurs absentes des
anciens fichiers sont résolues en valeurs caractéristiques (LCD 35, LCD/LED 55, OLED 100). Le
rétroéclairage est résolu à 65 pour LCD et 80 pour LCD/LED lorsqu’un ancien fichier ne contient pas
la valeur. Le halo de rétroéclairage est une passe bornée fondée sur les quatre cellules voisines
et vaut 25 par défaut. Ces opérations existent dans la référence CPU, OpenGL et le shader portable
Vulkan/Direct3D 11.

La rémanence et le temps de réponse sont placés dans ce panneau. Le désentrelacement n’est pas une
option LCD : il reste un traitement indépendant, ou une option interne du moteur lorsque celui-ci
le réalise déjà.

La référence temporelle conserve exactement une image linéaire traitée. Pour un temps de réponse
`T` en millisecondes et un intervalle réel `Δt` tiré des timestamps, la part de la nouvelle image
vaut `1 - exp(-Δt / T)` ; `T = 0` est immédiat. La rémanence conserve en parallèle jusqu’à
`intensité / 100` de l’image précédente et prend le maximum par composante. Une première image, un
changement de taille, un timestamp qui recule ou la sortie du mode pixels fixes réinitialise cet
historique borné. Ce traitement simule uniquement la réponse de l’écran et n’est pas présenté comme
un désentrelacement.

Veldrid et OpenGL portent les mêmes paramètres spatiaux et technologiques dans leurs shaders. Chaque
surface GPU conserve une seule texture source précédente, accompagnée du timestamp ; le shader lui
applique le modèle pixels fixes courant puis les mêmes coefficients de réponse et de rémanence.
Cette texture est remplacée après chaque présentation et invalidée hors de la technologie ou lorsque
sa taille ne correspond plus. WPF conserve l’unique image linéaire précédente de la référence CPU.

### Plasma

Plasma utilise quatre intensités `0..100`, toutes strictement neutres à `0`, dans l’ordre cellules,
tramage temporel, diffusion puis rémanence. La structure de cellules divise chaque pixel source en
trois bandes RGB et atténue les deux composantes non sélectionnées jusqu’à `35 %`, tout en ajoutant
un interstice de cellule jusqu’à `20 %`. La diffusion mélange la couleur avec une moyenne 3×3
jusqu’à `50 %`. Le tramage temporel utilise une matrice de Bayer 4×4, décalée par `VideoFrame.Sequence`,
et ajoute ou retire au maximum `4 %` en lumière linéaire ; il est déterministe pour une même image et
une même séquence. La rémanence conserve jusqu’à `intensité / 100` de l’unique image linéaire
précédente, par maximum de composante. Un changement de taille, un recul de séquence ou la sortie de
Plasma réinitialise cet historique borné. Ces formules sont originales et ne reprennent aucun code,
table ou texture d’un shader tiers.

### Écran vectoriel — approximation raster

Tant que les moteurs ne fournissent que des `VideoFrame`, GW GUI ne prétend pas reconstruire les
primitives vectorielles d’origine. La référence calcule un gradient Sobel 3×3 de la luminance
Rec. 709 linéaire, divise sa magnitude par `4` puis la borne à `0..1`. Le seuil visible `0..100`
devient `0..1` ; une transition lissée de largeur `0,10` évite un bord binaire. L’intensité des lignes
`0..100` ajoute cette réponse aux composantes existantes jusqu’à leur maximum, sans effacer la
couleur raster. Le halo ajoute jusqu’à `50 %` de la moyenne 3×3 de l’image renforcée. La persistance
conserve jusqu’à `intensité / 100` de l’unique image linéaire précédente par maximum de composante.
L’ordre est détection/renforcement, halo, persistance ; intensité des lignes, halo et persistance à
zéro sont strictement neutres. Taille différente, recul de séquence ou sortie du mode vectoriel
réinitialisent l’historique. Ces formules sont originales et sans code de shader tiers.

### Traitements indépendants et catalogue ultérieur

Les scalers pixel art ne sont pas réservés aux futures machines : ils peuvent traiter toute image
basse résolution, y compris celle des Amiga et Atari actuels. Un seul scaler avancé peut être actif
à la fois.

#### xBR — niveau 1 mono-passe

La source algorithmique retenue est le shader officiel
[`xbr-lv3.glsl`](https://github.com/libretro/glsl-shaders/blob/master/xbr/shaders/xbr-lv3.glsl)
de Hyllian (copyright 2011–2015), dont l’en-tête accorde explicitement la licence MIT. GW GUI en
adapte uniquement le niveau 1 dans sa chaîne commune ; aucun fichier du catalogue Libretro n’est
embarqué. Pour chacun des quatre coins du pixel source central `E`, la référence compare les deux
distances de contours pondérées du voisinage xBR, avec le vecteur de luminance publié
`(0,299; 0,587; 0,114)` multiplié par `48`,
et n’interpole que si `E` diffère des voisins horizontal et vertical. La zone d’interpolation est la
droite de coin lissée entre `1,10` et `1,90`; la couleur candidate est le voisin horizontal ou
vertical dont la luminance est la plus proche de `E`. Si plusieurs coins répondent, la contribution
ayant le poids le plus fort est retenue. Les coordonnées et les lectures sont bornées aux bords.

Cette variante n’expose pas encore de paramètre : `xBR` est un choix unique du sélecteur
d’échantillonnage/scaler, exclusif de nearest, bilinéaire, sharp-bilinear, bicubique et des autres
scalers pixel art. Elle fonctionne à toute échelle de sortie ; à taille source identique elle est
strictement neutre. Elle s’exécute avant les réglages généraux et la technologie d’affichage, dans
la référence CPU/WPF et dans OpenGL. Le shader portable Veldrid contient la même définition, mais
son exécution xBR provoque un blocage mesuré du pilote Direct3D 11 avec le shader monolithique
actuel : Direct3D 11 et Vulkan utilisent donc explicitement la référence CPU pour xBR, puis chargent
l’image finale dans leur texture avec une copie GPU neutre. Les renderers restent sélectionnables et
tous les autres filtres continuent à employer leur chemin GPU normal. La notice MIT et l’attribution
du fichier de référence sont conservées dans `THIRD-PARTY-NOTICES.md`.

#### xBRZ — réimplémentation compatible avec la licence MIT de GW GUI

Le fichier de référence Libretro
[`6xbrz.glsl`](https://github.com/libretro/glsl-shaders/blob/master/xbrz/shaders/6xbrz.glsl)
attribue son code xBR à Hyllian sous licence MIT, mais indique explicitement que sa partie xBRZ
provient du code de Zenju sous GPL-3.0 avec une exception limitée à MAME. Cette exception ne couvre
pas GW GUI : aucun code, aucune table et aucune constante propre à cette implémentation xBRZ ne sont
donc copiés.

GW GUI fournit une réimplémentation originale de l’effet attendu : classification symétrique des
quatre coins dans un voisinage 3×3, distance de couleur en luminance et chrominance, comparaison des
énergies des deux diagonales, puis fusion faible ou forte vers le voisin horizontal ou vertical le
plus proche. La zone faible est lissée entre `1,20` et `1,90`, la zone forte entre `0,90` et `1,70` ;
un contour est fort lorsque son énergie est au moins `1,5` fois plus faible que l’alternative. Le
filtre est déterministe, borné aux bords, neutre à l’échelle 1:1 et exclusif des autres scalers.

Pour éviter de recopier la logique dans des shaders monolithiques et conserver une sortie identique,
xBRZ s’exécute dans la référence CPU commune avant le chargement de texture pour WPF, OpenGL,
Direct3D 11 et Vulkan. Les réglages généraux et la technologie d’affichage sont inclus dans cette
passe de repli ; le backend sélectionné présente ensuite l’image finale par une copie neutre.

#### HQx — noyau HQ2x mGBA

La source retenue est
[`hq2x.fs`](https://github.com/mgba-emu/mgba/blob/master/res/shaders/hq2x.shader/hq2x.fs)
de Lior Halphon, copyright 2015–2023, sous licence MIT explicite. GW GUI adapte son noyau HQ2x :
comparaison du centre à ses huit voisins dans l’espace HQ, seuils `0,018`, `0,002` et `0,005`, motif
sur huit bits, orientation vers le quadrant de sortie puis interpolations pondérées définies par les
règles du shader. Les lectures sont bornées, le résultat est déterministe et l’échelle 1:1 reste
neutre. Le choix `HQx` est exclusif des autres scalers et utilise la référence CPU commune avant la
copie neutre des quatre surfaces. La notice MIT est conservée dans `THIRD-PARTY-NOTICES.md`.

#### ScaleFX — adaptation CPU de la variante officielle

La source retenue est la chaîne officielle de Sp00kyFox publiée dans
[`libretro/glsl-shaders/scalefx`](https://github.com/libretro/glsl-shaders/tree/master/scalefx),
copyright 2016, dont chacune des cinq passes porte une licence MIT explicite. Le préréglage exécute
quatre passes d’analyse à la taille source puis une passe de reconstruction 3× ; la sortie choisit
uniquement des couleurs déjà présentes dans l’image source.

GW GUI adapte ce principe dans une passe CPU portable : mesure perceptuelle RGB de la source
officielle, classification symétrique des quatre jonctions d’un voisinage 3×3, rejet des coins
ambigus, puis choix discret du pixel source pour chacun des neuf sous-pixels d’une grille 3×3. Pour
une taille de sortie quelconque, la position normalisée est ramenée à cette grille ; les lectures
sont bornées aux bords. Cette adaptation conserve les propriétés utiles de ScaleFX — palette
préservée, diagonales raccordées et absence de mélange flou — sans embarquer le runtime multi-passe
de Libretro. Elle est déterministe, strictement neutre à l’échelle 1:1 et exclusive des autres
méthodes d’échantillonnage.

Comme xBRZ et HQx, `ScaleFX` est calculé par la référence CPU commune avant une copie neutre dans
WPF, OpenGL, Direct3D 11 et Vulkan. Cette décision évite quatre traductions divergentes de la chaîne
multi-passe et garantit la même image sur chaque renderer. L’attribution et la licence MIT sont
reproduites dans `THIRD-PARTY-NOTICES.md`.

#### ScaleNx — réimplémentation compatible avec la licence MIT de GW GUI

La référence officielle examinée est
[`scale3x.glsl`](https://github.com/libretro/glsl-shaders/blob/master/scalenx/shaders/scale3x.glsl),
qui attribue Scale3x à Andrea Mazzoleni (2001–2004) et annonce explicitement la licence GNU GPL.
Cette licence n’est pas reprise par GW GUI : aucun code, macro ou fichier de ce shader n’est copié.

GW GUI réalise indépendamment le comportement public de la famille ScaleNx : comparer exactement
les quatre voisins axiaux du pixel central, conserver le centre quand les axes opposés sont égaux,
et prolonger un voisin uniquement lorsque les deux côtés du coin se rejoignent. Une grille logique
2× est utilisée sous un agrandissement de `2,5`, puis une grille 3× au-delà ; toute taille de sortie
est ramenée à l’une de ces grilles sans mélange de couleurs. Le résultat préserve donc la palette,
reste déterministe et borné aux bords, et l’échelle 1:1 est strictement neutre.

`ScaleNx` est exclusif des autres échantillonnages et passe par la référence CPU commune avant la
copie neutre de WPF, OpenGL, Direct3D 11 et Vulkan. Comme aucun code GPL n’est incorporé, aucune
notice tierce supplémentaire n’est ajoutée au binaire MIT.

#### SABR — réimplémentation compatible avec la licence MIT de GW GUI

La source officielle examinée est
[`sabr-v3.0.cg`](https://github.com/libretro/common-shaders/blob/master/sabr/shaders/sabr-v3.0.cg)
de Joshua Street. Son en-tête indique que certaines parties proviennent de 5xBR de Hyllian et place
l’ensemble sous GNU GPL version 2 ou ultérieure. Aucun code, coefficient ni structure propre à ce
shader n’est donc copié dans GW GUI.

La variante de GW GUI est une reconstruction originale de l’objectif visuel : détecter dans un
voisinage 3×3 le coin dont les deux voisins se raccordent, comparer l’énergie de ce raccord à celle
du contour opposé, puis interpoler en sRGB vers ces voisins dans une zone diagonale bornée. Le poids
combine proximité du sous-pixel au coin, cohérence des deux voisins, contraste avec le centre et
dominance du raccord ; il est plafonné à `0,75` pour préserver les détails. À la différence de
ScaleFX et ScaleNx, SABR crée volontairement des couleurs intermédiaires afin d’anti-créneler les
diagonales.

Le choix `SABR` est déterministe, neutre en 1:1, exclusif des autres scalers et exécuté par la
référence CPU commune avant la copie neutre des quatre renderers. Cette réimplémentation ne reprend
pas le code GPL et reste sous la licence MIT du projet.

#### Dé-dithering — damiers Hyllian

Les contrats et options des moteurs Amiga et Atari du projet ne contiennent aucun dé-dithering : le
traitement GW GUI ne duplique donc pas une option moteur. La source retenue est la chaîne
[`checkerboard-dedither`](https://github.com/libretro/slang-shaders/tree/master/dithering/shaders/checkerboard-dedither)
de Hyllian, copyright 2011–2022, dont les trois passes portent explicitement la licence MIT.

La première réalisation cible les damiers alternés, cas déterministe et vérifiable. Sur l’image à
la taille source, la passe compare le centre et ses huit voisins en lumière linéaire : au moins trois
voisins diagonaux doivent appartenir au groupe du centre, au moins trois voisins axiaux à l’autre
groupe, et l’écart entre les deux groupes doit dépasser le seuil de bruit sans atteindre un contour
extrême. Le résultat mélange le centre avec la moyenne axiale, puis interpole ce résultat selon une
intensité `0..100`. À `0`, le filtre est strictement neutre ; les bords sont bornés et une zone qui ne
forme pas un damier reste intacte.

Le dé-dithering est un filtre de restauration indépendant, compatible avec tous les scalers et les
cinq technologies d’affichage. Il s’exécute avant le scaler, comme validé dans l’ordre de chaîne.
WPF utilise directement la passe CPU ; OpenGL, Direct3D 11 et Vulkan utilisent la même référence
avant une copie GPU neutre lorsqu’elle est active. L’attribution MIT est conservée dans
`THIRD-PARTY-NOTICES.md`.

#### Débruitage — passe bilatérale originale

Les contrats et options exposés par les moteurs Amiga et Atari ne proposent aucun débruitage : ce
traitement GW GUI ne masque et ne duplique donc aucun réglage moteur. Le catalogue officiel
[`slang-shaders/denoisers`](https://github.com/libretro/slang-shaders/tree/master/denoisers) confirme
les familles médiane et bilatérale. Le fichier officiel
[`bilateral.slang`](https://github.com/libretro/slang-shaders/blob/master/denoisers/shaders/bilateral.slang)
est toutefois sous GPL-2.0-or-later ; aucun code, coefficient ni enchaînement propre à ce shader
n’est repris.

GW GUI utilise une passe bilatérale 3×3 originale sur l’image linéaire à la taille source. Les
voisins proches reçoivent un poids spatial fixe, puis leur poids diminue exponentiellement selon
leur distance colorimétrique au pixel central. L’intensité `0..100` règle à la fois la tolérance aux
petites variations et le mélange avec le résultat ; `0` est strictement neutre. Une faible variation
dans un aplat est ainsi réduite, tandis qu’un contour fortement contrasté reçoit un poids
négligeable et reste net.

Le débruitage est indépendant du dé-dithering : le premier réduit les variations locales, le second
ne reconstruit que les damiers alternés reconnus. Les deux sont compatibles, dans l’ordre
dé-dithering puis débruitage, avant l’unique scaler. La référence CPU commune produit le même
résultat sur WPF, OpenGL, Direct3D 11 et Vulkan ; la réalisation originale reste sous la licence MIT
du projet et n’ajoute aucune attribution tierce.

#### Réduction des bandes — reconstruction déterministe des faibles marches

Les modules Amiga et Atari n’exposent aucune option de réduction des bandes de couleur. La source
officielle Libretro examinée est
[`misc/shaders/deband.slang`](https://github.com/libretro/slang-shaders/blob/master/misc/shaders/deband.slang),
dérivée de mpv et distribuée sous GPL-2.0-or-later ou LGPL-2.1-or-later. Aucun code, coefficient,
générateur pseudo-aléatoire ni enchaînement de cette source n’est incorporé à GW GUI.

La passe GW GUI est une réalisation originale en lumière linéaire, déterministe et sans grain. Pour
chaque pixel, elle examine les directions horizontale et verticale et ne retient qu’une direction
formant une faible marche cohérente : voisins de part et d’autre du centre, ou voisin identique au
centre suivi du niveau suivant. Un pic local est rejeté comme bruit et un écart supérieur au seuil
est rejeté comme contour. La direction valide la plus homogène est interpolée selon l’intensité
`0..100`; `0` conserve strictement les octets d’origine.

Le filtre se distingue donc du débruitage, qui traite de petites variations non structurées, et du
grain stylistique, qui ajouterait volontairement du bruit. Il s’exécute après dé-dithering et
débruitage, toujours avant le scaler. Il est compatible avec les cinq technologies d’affichage et
tous les scalers ; WPF, OpenGL, Direct3D 11 et Vulkan partagent la même référence CPU lorsque la
passe est active. La réalisation reste sous la licence MIT du projet.

#### Netteté avancée — récupération de détails à la source

Les moteurs Amiga et Atari n’exposent aucune récupération de détails. Cette fonction ne remplace
pas le réglage général `Netteté` : celui-ci reste un ajustement global `-10..+10`, peut adoucir ou
accentuer l’image et s’exécute à la résolution de sortie après le modèle d’affichage. La nouvelle
`Récupération de détails` est au contraire une restauration uniquement positive `0..100`, appliquée
à la résolution source avant le scaler ; `0` est strictement neutre.

Le catalogue officiel contient notamment
[`adaptive-sharpen.slang`](https://github.com/libretro/slang-shaders/blob/master/sharpen/shaders/adaptive-sharpen.slang),
sous licence BSD à deux clauses. Il sert uniquement à confirmer la famille « accentuation
adaptative » : GW GUI ne reprend ni son code, ni ses coefficients, ni sa structure, et conserve une
réalisation originale sous MIT.

La passe calcule une moyenne locale 3×3 et renforce le résidu de micro-détail. Sa force décroît
jusqu’à zéro lorsque le contraste local devient celui d’un contour fort ; la sortie est en plus
bornée par les extrema locaux légèrement étendus, afin de limiter les halos. Elle intervient après
dé-dithering, débruitage et réduction des bandes, mais avant l’unique scaler. Elle est compatible
avec les cinq technologies et tous les scalers, avec la même référence CPU sur WPF, OpenGL,
Direct3D 11 et Vulkan.

#### Désentrelacement GW GUI — champ explicite ou fusion verticale

Les modules Amiga et Atari n’exposent aucun désentrelacement dans leurs contrats actuels. Les frames
communes ne transportent ni indicateur entrelacé, ni dominance de champ ; une détection automatique
ou une décision silencieuse serait donc trompeuse. Le panneau propose un select explicite :
`Désactivé`, `Bob — lignes paires`, `Bob — lignes impaires` et `Fusion verticale`. La documentation
officielle de
[`crt-beans`](https://github.com/libretro/slang-shaders/blob/master/crt/shaders/crt-beans/docs/parameters.md#interlacing)
confirme notamment que le bon ordre Bob dépend du cœur et ne peut pas toujours être détecté. Cette
référence documente le vocabulaire uniquement ; aucun code de shader n’est repris.

Les modes Bob conservent le champ pair ou impair choisi et reconstruisent chaque ligne manquante par
interpolation des deux lignes conservées adjacentes, avec répétition de la ligne valide au bord. Le
mode Fusion applique des poids verticaux `25 % / 50 % / 25 %` pour réduire le peigne, au prix d’un
adoucissement assumé. Aucun mode ne change la hauteur de l’image et `Désactivé` est strictement
neutre.

Le désentrelacement travaille en lumière linéaire à la taille source, avant dé-dithering,
débruitage, réduction des bandes, récupération de détails et scaler. Il est indépendant des cinq
technologies d’affichage et incompatible seulement avec un traitement équivalent du moteur, qui
n’existe pas dans les modules actuels. La réalisation CPU originale sous MIT est utilisée à
l’identique par WPF, OpenGL, Direct3D 11 et Vulkan.

La restauration de l’image reste indépendante de la technologie : dé-dithering, débruitage,
réduction des bandes, netteté avancée et désentrelacement lorsque le moteur ne fournit pas déjà le
traitement équivalent.

Le catalogue conserve pour les étapes ultérieures, sans les perdre :

- VFD, matrices LED, matrices de points et affichages à segments ;
- papier électronique ;
- projection et ses effets optiques ;
- simulation explicitement nommée de composite, S-Video, RF, PAL ou NTSC, seulement lorsqu’elle ne
  duplique pas l’option d’un moteur ;
- rémanence générale, flou de mouvement, scintillement, entrelacement et insertion d’images noires ;
- grain, VHS, aberration chromatique, bloom, sépia et niveaux de gris.

Ces familles devront toutes être traitées après le premier socle. Les matrices et segments restent
notamment au catalogue jusqu’à ce qu’une machine prise en charge en ait réellement besoin.

#### VFD — approximation raster auto-lumineuse

La référence matérielle retenue est la documentation constructeur Noritake
[`A Guide to Fundamental VFD Operation`](https://www.noritake-elec.com/technology/general-technical-information/vfd-operation),
qui décrit des anodes couvertes de phosphore émettant de la lumière sous l’impact des électrons, et
le guide constructeur
[`Selecting Phosphor Colours`](https://www.noritake-elec.com/support/design-resources/custom-design/custom-vfd-glass#8-selecting-phosphor-colours),
qui confirme plusieurs phosphores et leurs luminosités relatives. Ces documents servent à définir
le comportement visuel ; aucun code ou actif tiers n’est incorporé et la passe reste sous MIT.

VFD est une technologie d’affichage exclusive de Normal, CRT, écran à pixels fixes, Plasma et
Vectoriel. Son panneau propose un select de phosphore `Bleu`, `Vert`, `Ambre` ou `Rouge`, puis
`Intensité du phosphore`, `Halo VFD` et `Persistance VFD`, tous bornés à `0..100`. Les valeurs
initiales lors du choix de VFD sont respectivement bleu, `70`, `25` et `20`, afin que le mode soit
immédiatement visible.

Comme les moteurs actuels fournissent uniquement une image raster et non les segments, grilles ou
anodes physiques, la passe convertit la luminance source en émission monochrome teintée, ajoute un
halo 3×3 borné et conserve une fraction maximale de la frame précédente pour la persistance. Cette
approximation est compatible avec tous les scalers, restaurations et réglages généraux. Elle utilise
la référence CPU commune avec repli déclaré sur OpenGL, Direct3D 11 et Vulkan ; les quatre
renderers produisent la même image à taille de snapshot identique.

#### Matrice LED — cellules raster auto-lumineuses

Les références matérielles retenues sont les guides Adafruit consacrés aux panneaux RGB :
[`Use an art canvas to diffuse an RGB matrix`](https://learn.adafruit.com/use-an-art-canvas-to-diffuse-rgb-matrix/overview)
documente les matrices de pas `2,5`, `3` et `4 mm`, tandis que
[`LED Matrix Diffuser`](https://learn.adafruit.com/adafruit-matrixportal-m4/led-matrix-diffuser)
explique l’emploi d’une plaque diffusante pour réduire les reflets de la grille et modifier le rendu
visuel. Ces sources servent uniquement à définir les caractéristiques ; aucun code, shader, image ou
autre actif tiers n’est incorporé. La passe originale GW GUI reste sous licence MIT.

`Matrice LED` est une technologie exclusive des autres technologies d’affichage. Son panneau
conditionnel propose un select `RGB`, `Rouge`, `Vert`, `Ambre`, `Bleu` ou `Blanc`, puis `Taille des
cellules LED`, `Espacement des cellules LED`, `Diffusion LED` et `Luminosité LED`. Les quatre
intensités sont bornées à `0..100` et valent initialement `35`, `30`, `20` et `75`.

La passe regroupe la frame redimensionnée en cellules de `2..8` pixels selon la taille choisie,
calcule la couleur moyenne de chaque cellule, applique un masque circulaire avec espace sombre et
diffusion bornée, puis conserve la couleur RGB ou transforme la luminance selon la teinte
monochrome. Elle se combine avec tous les scalers, restaurations et réglages généraux. WPF utilise
directement le pipeline CPU commun ; OpenGL, Direct3D 11 et Vulkan déclarent le même repli CPU, ce
qui garantit le même traitement déterministe sur les quatre renderers.

#### Matrice de points — LCD raster à points discrets

Les références matérielles retenues sont la fiche constructeur Crystalfontz du
[`CFAG128128ITMIVZ`](https://www.crystalfontz.com/product/cfag128128itmivz-graphic-128x128-lcd-module),
qui publie notamment une taille de point propre au module graphique, et la documentation Newhaven
du contrôleur
[`SBN1661G`](https://support.newhavendisplay.com/hc/en-us/articles/4414858548887-SBN1661G),
décrit comme pilote LCD STN à matrice de points. Elles servent uniquement à cadrer le rendu ; aucun
code, pilote, document ou actif tiers n’est incorporé. La passe originale GW GUI reste sous MIT.

`Matrice de points` est distincte de la matrice LED et exclusive des autres technologies
d’affichage. Son panneau propose les palettes `LCD vert`, `LCD gris`, `Ambre` et `Bleu`, la forme
`Rond` ou `Carré`, puis `Taille du point` et `Contraste des points` bornés à `0..100`, ainsi que
`Temps de réponse (ms)` borné à `0..1000`. Les valeurs initiales sont vert, rond, `55`, `70` et
`120 ms`.

La passe moyenne la luminance dans une trame fixe de points, applique la forme et la taille choisies,
puis interpole entre le fond et l’encre de la palette selon le contraste. Un historique séparé
reproduit la réponse exponentielle entre frames sans la confondre avec la persistance LCD générale.
Le filtre se combine avec tous les scalers, restaurations et réglages généraux. WPF utilise le
pipeline CPU commun ; OpenGL, Direct3D 11 et Vulkan déclarent le même repli CPU déterministe.

#### Affichage à segments — géométrie lumineuse raster

Les références matérielles retenues sont les fiches Broadcom du
[`HDSP-521A/523A`](https://docs.broadcom.com/doc/HDSP-521A-523A-Dual-Digit-General-Purpose-7-Segment-Display-DS),
affichage LED rouge à sept segments avec surface grise destinée au contraste, et du
[`HDSP-A27C`](https://www.broadcom.com/products/leds-and-displays/7-segment/through-hole/hdsp-a27c),
affichage alphanumérique rouge à quatorze segments uniformément éclairés. La disposition seize
segments est conservée comme extension géométrique du même modèle raster. Aucun code, schéma ou
actif tiers n’est incorporé ; la passe originale GW GUI reste sous licence MIT.

`Affichage à segments` est une technologie exclusive des autres affichages. Son panneau propose
`7 segments`, `14 segments` ou `16 segments`, les couleurs rouge, verte, ambre, bleue ou blanche,
puis `Épaisseur du segment`, `Contraste de segment` et `Halo des segments` sur `0..100`, ainsi que
`Temps de réponse des segments (ms)` sur `0..1000`. Les valeurs initiales sont sept segments, rouge,
`55`, `80`, `20` et `30 ms`.

La passe applique une géométrie normalisée répétée de segments horizontaux, verticaux et diagonaux,
module l’émission par la luminance et le contraste de la frame, puis ajoute le halo borné. Un
historique séparé applique la réponse exponentielle entre frames. Elle se combine avec tous les
scalers, restaurations et réglages généraux. WPF utilise le pipeline CPU commun ; OpenGL,
Direct3D 11 et Vulkan déclarent le même repli CPU déterministe.

#### Papier électronique — palette limitée et rafraîchissement lent

Les références constructeur retenues sont la présentation E Ink
[`Kaleido 3`](https://www.eink.com/brand/detail/Kaleido3?pubDate=20250501), qui annonce seize niveaux
de gris et 4096 couleurs, et la publication sur les plateformes ePaper récentes
[`E Ink and MediaTek Deepen Collaboration`](https://www.eink.com/news/detail/E%20Ink-and-MediaTek-Deepen-Collaboration-for-Education-and-Digital-Reading-with-AI-SoCs),
qui mentionne le tramage, l’accélération du rafraîchissement et les algorithmes réduisant le ghosting.
Ces sources définissent uniquement les caractéristiques ; aucun code ni actif tiers n’est incorporé.
La passe originale GW GUI reste sous licence MIT.

`Papier électronique` est une technologie exclusive des autres affichages. Son panneau propose
`Monochrome`, `16 niveaux de gris` ou `4096 couleurs`, puis contraste et tramage sur `0..100`, temps
de rafraîchissement sur `0..1000 ms` et image fantôme sur `0..100`. Les valeurs initiales sont
monochrome, `70`, `35`, `500 ms` et `20`.

La passe applique contraste et matrice de Bayer 4×4, quantifie en deux tons, seize gris ou seize
niveaux par canal, puis donne au blanc une légère teinte papier. Un historique distinct interpole les
frames selon le rafraîchissement et conserve une fraction bornée pour l’image fantôme. Elle se
combine avec tous les scalers, restaurations et réglages généraux. WPF utilise le pipeline CPU
commun ; OpenGL, Direct3D 11 et Vulkan déclarent le même repli CPU déterministe.

#### Projection — diffusion optique et convergence

La documentation Epson sur l’[alignement des panneaux](https://files.support.epson.com/docid/cpd6/cpd63492/source/projectors/source/adjustments/tasks/ehls650b_650w/panel_aligning_ehls650b_650w.html)
confirme que la convergence d’un projecteur se règle en déplaçant les composantes rouge et bleue
par rapport au vert, dans une plage pouvant atteindre trois pixels. La documentation Epson rappelle
également qu’une [surface de projection colorée ou texturée](https://files.support.epson.com/docid/cpd5/cpd55708/source/adjustments/tasks/panel_aligning.html)
modifie le résultat observé. Ces références servent uniquement à définir les caractéristiques ;
aucun code, shader ni actif tiers n’est incorporé. La passe originale GW GUI reste sous licence MIT.

`Projection` est une technologie exclusive des autres affichages. Son panneau propose flou optique,
diffusion lumineuse, texture de toile et convergence RGB sur `0..100`, avec les valeurs initiales
`20`, `15`, `10` et `5`. La convergence décale symétriquement le rouge et le bleu autour du vert
jusqu’à trois pixels ; la valeur nulle reste strictement neutre.

La passe raster originale moyenne localement l’image pour le flou, diffuse une fraction bornée de la
lumière, applique une trame de toile 4×4 puis décale les composantes pour la convergence. Elle ne
conserve aucun historique temporel et se combine avec tous les scalers, restaurations et réglages
généraux. WPF utilise le pipeline CPU commun ; OpenGL, Direct3D 11 et Vulkan déclarent le même repli
CPU déterministe.

#### Rémanence générale — effet temporel indépendant

La [spécification officielle des présélections slang](https://github.com/libretro/slang-shaders/blob/master/README.md)
confirme qu’un traitement peut demander l’historique des frames précédentes. Elle sert ici uniquement
à valider l’architecture temporelle : aucun shader, code, coefficient ni actif Libretro n’est repris.
L’algorithme original GW GUI reste sous licence MIT.

`Rémanence générale` est une intensité permanente sur `0..100`, neutre à `0` et initialisée à `0`.
Elle se trouve dans un panneau `Effets temporels` toujours visible, indépendamment du select de
technologie. Elle ne change ni le temps de réponse ni la rémanence propres à un LCD/OLED, Plasma,
Vectoriel, VFD, matrice de points, segments ou papier électronique.

La passe intervient après le scaler, la restauration, les réglages généraux, la technologie choisie
et ses éventuels traitements temporels. Pour chaque composante linéaire, elle conserve le maximum
entre la frame courante et la frame précédente multipliée par `intensité / 100`. Une première frame,
une valeur nulle, un changement de taille, un recul ou une répétition de séquence et la destruction du
pipeline invalident son historique séparé. WPF utilise directement le pipeline CPU commun ; OpenGL,
Direct3D 11 et Vulkan utilisent le même repli CPU déterministe lorsque l’effet est actif.

#### Flou de mouvement — mélange limité à une frame

Comme pour la rémanence générale, la [spécification officielle slang](https://github.com/libretro/slang-shaders/blob/master/README.md)
sert uniquement à confirmer l’emploi d’un historique de frames. L’algorithme est une implémentation
originale GW GUI sous licence MIT, sans reprise de shader, code, coefficient ni actif tiers.

`Flou de mouvement` est une intensité indépendante sur `0..100`, neutre et initialisée à `0`, dans
le panneau permanent `Effets temporels`. Chaque composante linéaire mélange la frame courante vers
la frame précédente avec le coefficient `intensité / 100`. L’historique mémorise la frame courante
avant mélange : l’effet porte donc exactement sur une frame et ne devient pas une seconde rémanence
cumulative.

Une première frame, une valeur nulle, un changement de taille, une séquence non croissante ou la
destruction du pipeline invalident cet historique propre. La passe intervient avant la rémanence
générale et après les traitements propres à l’affichage. Elle est compatible avec toutes les
technologies, scalers, restaurations et réglages généraux. WPF utilise le pipeline CPU commun ;
OpenGL, Direct3D 11 et Vulkan utilisent le même repli CPU déterministe lorsqu’elle est active.

#### Insertion d’images noires — passe finale du renderer

`Insertion d’images noires` est un interrupteur indépendant, désactivé par défaut, dans le panneau
permanent `Effets temporels`. Les séquences impaires sont remplacées par du noir et les séquences
paires restent affichées. WPF exécute cette passe dans son pipeline logiciel ; OpenGL, Direct3D 11 et
Vulkan l’exécutent directement dans leur shader fragment final, sans masque WPF ni changement de
visibilité de la surface native.

La passe utilise `VideoFrame.Sequence`, n’intègre aucun code ou actif tiers et reste sous licence MIT.
Elle intervient après les autres effets temporels afin qu’aucun traitement ne rééclaire la frame noire.

#### Simulation composite GW GUI — post-traitement explicite

Le [catalogue officiel common-shaders](https://github.com/libretro/common-shaders) confirme l’existence
d’une famille NTSC/composite, tandis que les documentations PUAE, Hatari et Atari800 recensées plus
haut confirment que norme vidéo, timing et artifacting restent des choix des moteurs. Ces sources
servent uniquement à délimiter l’effet : aucun code, shader, coefficient ni actif tiers n’est repris.
La passe originale GW GUI reste sous licence MIT.

`Composite` est un choix exclusif du sélecteur de liaison, contrôlé par l’unique intensité de
dégradation. Il traite seulement le `VideoFrame` déjà produit : il
ne change ni PAL/NTSC, ni fréquence, ni région, ni artifacting Atari800. Elle convertit localement les
couleurs linéaires en luminance et deux composantes chromatiques, conserve davantage de détail de
luminance que de chrominance, puis reconstruit RGB avec une faible alternance de phase déterministe
pour évoquer le dot crawl.

La passe intervient après la restauration mais avant le scaler et la technologie d’affichage. Elle
est compatible avec toutes les technologies, scalers, restaurations, réglages généraux et effets
temporels. L’utilisateur peut donc la combiner à une option moteur, mais elle ne la remplace ni ne la
modifie : l’une construit la sortie émulée, l’autre simule explicitement une dégradation après sortie.
WPF utilise la fonction CPU du fichier `SignalConnectionComposite.cs` ; OpenGL, Direct3D 11 et
Vulkan utilisent directement la fonction shader définie dans ce même fichier.

#### Simulation S-Video GW GUI — luminance séparée

Le catalogue common-shaders sert à confirmer la famille de simulations analogiques, et l’audit PUAE,
Hatari et Atari800 conserve toutes les options matérielles dans les moteurs. Aucun code, shader,
coefficient ni actif tiers n’est repris ; la passe originale GW GUI reste sous licence MIT.

`S-Video` est un choix exclusif du sélecteur de liaison. La passe conserve la luminance linéaire du pixel central et mélange
seulement les deux composantes chromatiques avec leurs voisines horizontales, au maximum à `12 %`.
Elle ne dépend pas de la séquence et n’ajoute ni alternance de phase ni dot crawl, ce qui la rend
visuellement et techniquement distincte de la simulation composite.

Comme composite, elle agit après restauration et avant scaler sans lire ni modifier les normes,
timings, régions ou artifacting des moteurs. Elle se combine avec toutes les technologies et tous les
traitements indépendants. Il ne peut plus être cumulé avec composite, RF, composante ou RGB/Péritel.
WPF utilise `SignalConnectionSVideo.cs` côté CPU ; les trois renderers natifs utilisent son shader.

#### Liaisons RGB/Péritel et composante

`RGB (Péritel/SCART)` représente la meilleure liaison analogique proposée : son intensité applique
seulement une très faible limitation horizontale, bornée à `4 %`. `Composante (YPbPr)` conserve la
luminance mais réduit davantage la bande passante chromatique, jusqu’à `18 %`. Ces deux choix sont
exclusifs des liaisons S-Video, composite et RF. Leurs implémentations CPU et shader résident
respectivement dans `SignalConnectionRgbScart.cs` et `SignalConnectionComponent.cs`.

#### Grain — bruit stylistique fin

Le catalogue slang-shaders confirme l’existence d’effets stylistiques de grain, sans qu’aucun code,
shader, coefficient, texture ou actif tiers soit repris. La passe originale GW GUI reste sous MIT.

`Grain` est une intensité `0..100`, neutre et initialisée à `0`, dans le panneau permanent
`Effets stylistiques`. Un hachage déterministe de la position et de `VideoFrame.Sequence` produit un
bruit monochrome fin ajouté aux trois composantes, borné à `±7 %` en lumière linéaire. Une même
séquence est reproductible et une nouvelle séquence renouvelle le motif.

Le grain intervient à la résolution de sortie, après scaler, affichage et netteté, mais avant les
effets temporels. Il est distinct du bruit RF, appliqué à la résolution source dans la chaîne de
simulation du signal. Il se combine avec toutes les technologies et traitements. WPF utilise le
pipeline CPU commun ; OpenGL, Direct3D 11 et Vulkan utilisent le même repli CPU déterministe.

#### VHS — instabilité et pertes de ligne

Le catalogue slang-shaders confirme la famille VHS sans qu’aucun code, shader, coefficient ou actif
tiers soit repris. La passe originale GW GUI reste sous MIT.

`VHS` est une intensité `0..100`, neutre et initialisée à `0`. Chaque ligne reçoit un décalage
horizontal déterministe borné à trois pixels ; rouge et bleu bavent vers des voisins opposés jusqu’à
`45 %`. Une ligne sur dix-sept, décalée par `VideoFrame.Sequence`, est atténuée jusqu’à `45 %`.
La même séquence est reproductible.

VHS intervient à la résolution de sortie après netteté et avant grain puis effets temporels. Il se
combine avec toutes les technologies et traitements. WPF utilise le pipeline CPU commun ; OpenGL,
Direct3D 11 et Vulkan utilisent le même repli CPU déterministe.

#### Aberration chromatique — séparation des composantes

Le catalogue slang-shaders confirme la famille des aberrations chromatiques sans qu’aucun code,
shader, coefficient, texture ou actif tiers soit repris. La passe originale GW GUI reste sous MIT.

`Aberration chromatique` est une intensité `0..100`, neutre et initialisée à `0`. À la résolution
de sortie, elle échantillonne le rouge vers la droite et le bleu vers la gauche avec un décalage
entier progressif, borné à trois pixels ; le vert reste aligné. Les bords sont pincés au dernier
pixel disponible, sans transparence ni dépendance à `VideoFrame.Sequence`.

La passe intervient après VHS et avant grain puis effets temporels. Elle se combine avec toutes les
technologies, scalers, restaurations, simulations et autres effets stylistiques. WPF utilise le
pipeline CPU commun ; OpenGL, Direct3D 11 et Vulkan utilisent le même repli CPU déterministe.

#### Bloom — diffusion des hautes lumières

Le catalogue slang-shaders confirme la famille bloom sans qu’aucun code, shader, coefficient,
texture ou actif tiers soit repris. La passe originale GW GUI reste sous MIT.

`Bloom` est une intensité `0..100`, neutre et initialisée à `0`. La passe conserve seulement la
part de chaque composante linéaire supérieure à `60 %`, la moyenne dans un rayon de deux pixels,
puis la réadditionne jusqu’à `35 %` avec saturation à `1`. Elle ne dépend ni de la séquence ni
d’un historique et n’éclaircit donc pas uniformément les zones sombres.

Le bloom intervient après l’aberration chromatique et avant grain puis effets temporels. Il reste
distinct des halos CRT, vectoriel et VFD, qui appartiennent au modèle de leur technologie. Il se
combine avec toutes les technologies et traitements ; WPF utilise le pipeline CPU commun, tandis
qu’OpenGL, Direct3D 11 et Vulkan emploient le même repli CPU déterministe.

#### Sépia — teinte chaude fondée sur la luminance

Le catalogue slang-shaders confirme la famille des effets sépia sans qu’aucun code, shader,
coefficient, texture ou actif tiers soit repris. La passe originale GW GUI reste sous MIT.

`Sépia` est une intensité `0..100`, neutre et initialisée à `0`. La luminance linéaire Rec. 709
est convertie en cible chaude avec les multiplicateurs rouge `1,07`, vert `0,93` et bleu `0,74`,
puis mélangée à la couleur d’origine selon l’intensité. Chaque composante reste bornée à `0..1` ;
la passe est déterministe et sans historique.

Le sépia intervient après bloom et avant niveaux de gris, grain puis effets temporels. Il se combine
avec toutes les technologies et tous les traitements. Si sépia et niveaux de gris sont tous deux
actifs, la passe niveaux de gris appliquée ensuite retire volontairement la teinte. Les quatre
renderers utilisent le pipeline CPU commun ou son repli CPU déterministe.

#### Niveaux de gris — luminance progressive

Le catalogue slang-shaders confirme la famille grayscale sans qu’aucun code, shader, coefficient,
texture ou actif tiers soit repris. La passe originale GW GUI reste sous MIT.

`Niveaux de gris` est une intensité `0..100`, neutre et initialisée à `0`. Pour chaque pixel,
la luminance linéaire Rec. 709 `0,2126 R + 0,7152 G + 0,0722 B` est mélangée progressivement aux
trois composantes. À `100`, rouge, vert et bleu sont donc égaux. La passe est déterministe, sans
historique, et ne modifie aucune palette ni aucun mode monochrome d’une technologie d’affichage.

Les niveaux de gris interviennent après sépia et avant grain puis effets temporels. Ils se combinent
avec toutes les technologies et tous les traitements ; à intensité maximale, ils retirent par
construction les teintes produites plus tôt dans la chaîne. Les quatre renderers utilisent le
pipeline CPU commun ou son repli CPU déterministe.

#### Simulation PAL GW GUI — phase alternée entre lignes

Les documentations PUAE, Hatari et Atari800 citées plus haut réservent la norme PAL et ses timings au
moteur ; le catalogue common-shaders confirme seulement la famille visuelle. Aucun code, shader,
coefficient ni actif tiers n’est repris. La passe originale GW GUI reste sous licence MIT.

`PAL` est un choix exclusif du sélecteur de norme. Il conserve la luminance,
mélange les composantes chromatiques de chaque ligne avec la ligne voisine à `28 %` et `36 %` au
maximum. Il n’ajoute aucun bruit ni phase colorée artificielle et ne dépend pas de la séquence.

La passe intervient après RF et avant scaler. Elle se combine avec toutes les technologies et tous
les traitements explicites. WPF utilise le pipeline CPU commun ; OpenGL, Direct3D 11 et Vulkan
utilisent directement le shader de `SignalStandardPal.cs`.

#### Simulation NTSC GW GUI — retard chromatique et phase

Les documentations PUAE, Hatari et Atari800 réservent la norme NTSC, la fréquence et la région au
moteur ; common-shaders confirme seulement la famille visuelle. Aucun code, shader, coefficient ni
actif tiers n’est repris. La passe originale GW GUI reste sous licence MIT.

`NTSC` est un choix exclusif du sélecteur de norme. Il mélange la luminance
avec le pixel horizontal précédent jusqu’à `12 %`, retarde les deux composantes chromatiques jusqu’à
`48 %` et `58 %`, puis applique une dérive de teinte stable. Il n’ajoute aucun bruit dépendant de la frame.

La passe intervient après PAL et avant scaler. Elle ne change ni standard, fréquence, région, timing
ni option moteur. WPF et les trois renderers natifs utilisent respectivement la fonction CPU et le
shader de `SignalStandardNtsc.cs`.

#### SECAM — chrominance séquentielle par ligne

`SECAM` est le troisième choix explicite de norme. Une composante de différence de couleur sur deux
est reprise de la ligne précédente, en alternance, sans bruit RF. Le traitement CPU et son shader sont
réunis dans `SignalStandardSecam.cs`. Le mode `Automatique` choisit PAL au voisinage de 50 Hz et NTSC
au voisinage de 60 Hz à partir des horodatages de frames, sans modifier la norme interne du moteur.

#### Simulation RF GW GUI — transmission bruitée

Le catalogue common-shaders confirme la famille des dégradations analogiques ; l’audit des moteurs
conserve fréquence, région, norme et tout éventuel tuner dans l’émulateur. Aucun code, shader,
coefficient ni actif tiers n’est repris. La passe originale GW GUI reste sous licence MIT.

`RF` est un choix exclusif du sélecteur de liaison, contrôlé par son unique intensité.
Elle mélange horizontalement chaque composante avec ses voisines jusqu’à `65 %`, puis ajoute un bruit
commun aux trois composantes, borné à `±8 %`. Le bruit est un hachage déterministe de la position et
de `VideoFrame.Sequence` : une même frame donne le même résultat, une nouvelle séquence change la
trame de bruit sans générateur global ni état caché.

Elle ne se cumule plus avec une autre liaison. WPF utilise la fonction CPU de
`SignalConnectionRf.cs`; OpenGL, Direct3D 11 et Vulkan utilisent directement son shader.

#### Entrelacement simulé — champs alternés

`Entrelacement` est une activation indépendante dans le panneau permanent `Effets temporels`, avec
une visibilité des trames réglable de `0..100`. La parité de `VideoFrame.Sequence` choisit le champ
temporel à conserver. Les lignes de l’autre parité proviennent de la frame précédente et sont
mélangées avec la frame courante selon la visibilité demandée, puis atténuées jusqu’à `35 %` pour que
les deux champs restent perceptibles même dans une zone statique. La première frame et toute rupture
de taille ou de séquence réinitialisent l’historique sans assombrir l’image.

Cet effet crée volontairement une apparence entrelacée après le rendu ; il est distinct du select de
désentrelacement de la restauration, qui corrige une source déjà entrelacée avant redimensionnement.
La formule originale est déterministe, avec un historique borné à une frame, sans code ni actif tiers,
et reste sous MIT. Elle est calculée à la fréquence des frames source, soit 50 champs par seconde en
PAL et 60 en NTSC lorsque le moteur produit ces cadences. Elle précède le flou de mouvement et la
rémanence générale et se combine avec toutes les technologies, scalers, restaurations et réglages.

#### Scintillement — modulation sans frame noire

Le scintillement est une intensité indépendante `0..100`, neutre et initialisée à `0`, dans le
panneau permanent `Effets temporels`. Sur les séquences impaires, la lumière linéaire est multipliée
par `1 - intensité / 200` ; les séquences paires sont inchangées. Même à `100`, la frame conserve
donc la moitié de sa lumière : l’effet reste distinct de l’insertion d’images noires.

La modulation est originale, déterministe à partir de `VideoFrame.Sequence`, sans historique, code,
shader, coefficient ni actif tiers, et reste sous licence MIT. Elle intervient après la technologie
d’affichage et avant le flou de mouvement puis la rémanence générale. Elle est compatible avec toutes
les technologies, scalers, restaurations et réglages généraux. WPF utilise le pipeline CPU commun ;
OpenGL, Direct3D 11 et Vulkan utilisent le même repli CPU déterministe lorsqu’elle est active.

### Compatibilités validées

| Activation | Compatible avec | Incompatible ou exclu par construction |
|---|---|---|
| Normal | échantillonnage, scaler, restauration, réglages généraux | tout panneau propre à une technologie d’affichage |
| CRT | scaler, restauration, réglages généraux et ses propres scanlines/trames | écran à pixels fixes, plasma, écran vectoriel |
| Écran à pixels fixes | scaler, restauration, réglages généraux et sa réponse temporelle | CRT, plasma, écran vectoriel |
| Plasma | scaler, restauration et réglages généraux | CRT, écran à pixels fixes, écran vectoriel |
| Écran vectoriel | restauration compatible, réglages généraux | CRT raster, écran à pixels fixes, plasma |
| VFD | scaler, restauration et réglages généraux | toute autre technologie d’affichage |
| Matrice LED | scaler, restauration et réglages généraux | toute autre technologie d’affichage |
| Matrice de points | scaler, restauration et réglages généraux | toute autre technologie d’affichage |
| Affichage à segments | scaler, restauration et réglages généraux | toute autre technologie d’affichage |
| Papier électronique | scaler, restauration et réglages généraux | toute autre technologie d’affichage |
| Projection | scaler, restauration et réglages généraux | toute autre technologie d’affichage |
| Rémanence générale | toutes les technologies, scalers, restaurations et réglages généraux | aucune ; elle ne remplace pas une réponse d’écran propre |
| Flou de mouvement | toutes les technologies, scalers, restaurations, réglages généraux et rémanence générale | aucune ; historique distinct limité à une frame |
| Scintillement | toutes les technologies, scalers, restaurations et autres effets temporels | aucune ; reste distinct de l’insertion d’images noires |
| Entrelacement simulé | toutes les technologies, scalers, réglages et autres effets temporels | aucun ; ne remplace pas le désentrelacement de restauration |
| Insertion d’images noires | toutes les technologies, scalers, restaurations et autres effets temporels | aucune ; appliquée en dernier et distincte du scintillement |
| Simulation composite GW GUI | toutes les technologies, scalers, restaurations, réglages et effets temporels | aucune ; ne modifie ni ne remplace les options de signal du moteur |
| Simulation S-Video GW GUI | toutes les technologies, scalers, restaurations, réglages, effets temporels et composite explicite | aucune ; luminance séparée et aucune option moteur modifiée |
| Simulation RF GW GUI | toutes les technologies, scalers, restaurations, réglages, effets temporels et autres simulations explicites | aucune ; aucune fréquence, région ou option moteur modifiée |
| Simulation PAL GW GUI | toutes les technologies, scalers, restaurations, réglages, effets temporels et autres simulations explicites | aucune ; ne change jamais la norme PAL/NTSC du moteur |
| Simulation NTSC GW GUI | toutes les technologies, scalers, restaurations, réglages, effets temporels et autres simulations explicites | aucune ; ne change jamais la norme PAL/NTSC du moteur |
| Grain | toutes les technologies, scalers, restaurations, simulations, réglages et effets temporels | aucune ; distinct du bruit RF par sa position et son amplitude |
| VHS | toutes les technologies, scalers, restaurations, simulations, réglages, grain et effets temporels | aucune ; effet stylistique à la résolution de sortie |
| Aberration chromatique | toutes les technologies, scalers, restaurations, simulations, réglages, autres effets stylistiques et effets temporels | aucune ; effet spatial déterministe à la résolution de sortie |
| Bloom | toutes les technologies, scalers, restaurations, simulations, réglages, autres effets stylistiques et effets temporels | aucune ; distinct des halos intégrés aux technologies |
| Sépia | toutes les technologies, scalers, restaurations, simulations, réglages, autres effets stylistiques et effets temporels | aucune ; les niveaux de gris appliqués ensuite retirent sa teinte |
| Niveaux de gris | toutes les technologies, scalers, restaurations, simulations, réglages, autres effets stylistiques et effets temporels | aucune ; distinct des palettes et modes monochromes des technologies |
| Scaler pixel art | une technologie d’affichage et réglages généraux | autre scaler pixel art |
| Désentrelacement GW GUI | technologie d’affichage et réglages généraux | autre méthode de désentrelacement ou traitement équivalent du moteur |

L’audit des filtres indépendants implémentés ne trouve aucune incompatibilité interne nécessitant
une confirmation. Un seul scaler peut exister dans la configuration parce que l’échantillonnage est
un select. Dé-dithering, débruitage, réduction des bandes, récupération de détails et
désentrelacement sont composables dans l’ordre de chaîne documenté, sans désactiver ni réécrire les
valeurs des autres filtres. Il n’existe actuellement aucun traitement moteur équivalent exposé par
les contrats Amiga ou Atari. Aucune nouvelle boîte Oui/Non n’est donc justifiée.

Le sélecteur principal rend les technologies mutuellement exclusives sans boîte de confirmation :
choisir une nouvelle technologie remplace naturellement l’ancienne. Une confirmation reste requise
uniquement lorsqu’une fonctionnalité indépendante nouvellement activée doit désactiver une autre
fonction indépendante déjà active.

### Tests du catalogue ultérieur

Chaque entrée possède un test CPU isolé dans
`tests/GWGUI.Tests/EmulationVideoProcessingPipelineTests.cs`, puis est incluse séparément à valeur
maximale dans `RendererSnapshotsMatchDeterministicCpuImagesAtNeutralAndBounds`. Ce second contrôle
compare la même image CPU déterministe pour WPF, OpenGL, Direct3D 11 et Vulkan, en plus de l’état
neutre. Les scénarios dédiés sont :

| Entrée | Test isolé |
|---|---|
| VFD | `VfdColorsHaloAndPersistenceAreDistinctAndBounded` |
| Matrice LED | `LedMatrixColorsCellsGapDiffusionAndBrightnessAreDistinctAndBounded` |
| Matrice de points | `DotMatrixPalettesShapesContrastAndResponseAreDistinctAndBounded` |
| Affichage à segments | `SegmentDisplayLayoutsColorsGeometryGlowAndResponseAreDistinctAndBounded` |
| Papier électronique | `EPaperModesContrastDitheringRefreshAndGhostingAreDistinctAndBounded` |
| Projection | `ProjectionBlurDiffusionTextureAndConvergenceAreDistinctAndBounded` |
| Rémanence générale | `GeneralPersistenceIsIndependentAndResetsOnSequenceOrSizeChanges` |
| Flou de mouvement | `MotionBlurBlendsOnlyThePreviousFrameAndResetsOnSequenceChanges` |
| Scintillement | `FlickerDimsOddFramesWithoutReplacingThemWithBlackFrames` |
| Entrelacement | `InterlacingWeavesAlternatingFieldsFromConsecutiveSourceFrames` |
| Insertion d’images noires | `BlackFrameInsertionBlacksOddFramesAfterOtherEffects` |
| RGB/Péritel, composante et SECAM | `AdditionalSignalChoicesProduceDistinctBoundedResults` |
| Composite | `CompositeSimulationBlursChromaAndUsesSequenceWithoutChangingSignalOptions` |
| S-Video | `SVideoSimulationPreservesLuminanceAndHasNoSequencePhase` |
| RF | `RfSimulationAddsBoundedSequenceDependentNoiseAndBlur` |
| PAL | `PalSimulationAlternatesLineChromaWithoutDependingOnFrameSequence` |
| NTSC | `NtscSimulationDelaysChromaWithoutAddingFrameNoise` |
| Grain | `GrainIsFineBoundedRepeatableAndChangesWithSequence` |
| VHS | `VhsProducesRepeatableLineJitterChromaBleedAndDropouts` |
| Aberration chromatique | `ChromaticAberrationSeparatesRedAndBlueDeterministically` |
| Bloom | `BloomSpreadsOnlyHighlightsAndIsSequenceIndependent` |
| Sépia | `SepiaWarmsLuminanceProgressivelyAndIsSequenceIndependent` |
| Niveaux de gris | `GrayscaleConvergesChannelsProgressivelyAndIsSequenceIndependent` |

La normalisation, la sérialisation, les 30 ressources et la présence permanente des contrôles sont
couvertes par `EmulationVideoConfigurationTests`, `EmulationVideoLocalizationTests` et
`EmulationVideoSettingsSectionTests`.

### Présélections

Les présélections sont des ensembles de valeurs modifiables, pas des fonctionnalités supplémentaires
ni des références persistantes vers un fichier Libretro. Elles utilisent les mêmes contrats que les
réglages manuels. Luminosité, contraste, gamma, saturation et netteté utilisent chacun `-10..+10`
avec `0` neutre. Les intensités visuelles utilisent `0..100`, où `0` désactive l’effet, tandis que les
durées temporelles utilisent `0..1000` millisecondes. Une valeur omise dans le tableau est neutre.
La rémanence, le halo ou la diffusion indiqués sans suffixe `ms` sont des intensités, pas des durées.

Les douze présélections retenues et leurs valeurs exactes sont :

| Nom visible | Échantillonnage | Technologie et valeurs non neutres |
|---|---|---|
| Normal | Nearest | Normal |
| CRT — Arcade couleur | Sharp-bilinear | CRT ; couleur ; faisceau 35 ; halo 20 ; masque grille d’ouverture 45 ; courbure 8 ; vignettage 8 ; scanlines horizontales, intensité 40, épaisseur 50, phase 0 |
| CRT — Téléviseur couleur | Bilinéaire | CRT ; couleur ; faisceau 55 ; halo 35 ; shadow mask 35 ; courbure 18 ; vignettage 15 ; scanlines horizontales, intensité 25, épaisseur 60, phase 0 |
| CRT — Monochrome vert | Sharp-bilinear | CRT ; vert ; faisceau 42 ; halo 35 ; sans masque ; courbure 12 ; vignettage 10 ; scanlines horizontales, intensité 35, épaisseur 50, phase 0 |
| CRT — Monochrome ambre | Sharp-bilinear | CRT ; ambre ; faisceau 42 ; halo 35 ; sans masque ; courbure 12 ; vignettage 10 ; scanlines horizontales, intensité 35, épaisseur 50, phase 0 |
| CRT — Monochrome blanc | Sharp-bilinear | CRT ; blanc ; faisceau 38 ; halo 25 ; sans masque ; courbure 10 ; vignettage 8 ; scanlines horizontales, intensité 30, épaisseur 50, phase 0 |
| LCD couleur | Nearest | Écran à pixels fixes ; LCD ; sous-pixels RGB ; grille 35 ; espace inter-pixels 20 ; temps de réponse 16 ms ; rémanence 10 ; rétroéclairage 70 ; profondeur du noir 8 |
| LCD monochrome | Nearest | Écran à pixels fixes ; LCD ; sous-pixels monochromes ; grille 45 ; espace inter-pixels 25 ; temps de réponse 35 ms ; rémanence 25 ; rétroéclairage 60 ; profondeur du noir 15 |
| LCD rétroéclairé LED | Nearest | Écran à pixels fixes ; LCD/LED ; sous-pixels RGB ; grille 25 ; espace inter-pixels 15 ; temps de réponse 8 ms ; rémanence 5 ; rétroéclairage 85 ; profondeur du noir 12 |
| OLED | Nearest | Écran à pixels fixes ; OLED ; sous-pixels RGB ; grille 15 ; espace inter-pixels 10 ; temps de réponse 1 ms ; rémanence 0 ; profondeur du noir 100 |
| Plasma | Bilinéaire | Plasma ; cellules 35 ; diffusion 30 ; tramage temporel 20 ; rémanence 20 |
| Écran vectoriel | Bilinéaire | Écran vectoriel ; seuil de ligne 50 ; intensité des lignes 75 ; halo 45 ; persistance 30 |

Les cinq réglages généraux valent `0` dans les douze présélections. La trame/moiré volontaire est
désactivée dans tous les presets initiaux : elle reste disponible comme réglage CRT manuel. Appliquer
une présélection remplace atomiquement toute la configuration vidéo commune, puis chaque valeur peut
être modifiée séparément. Les identifiants persistants sont en anglais et indépendants du texte
traduit : `Normal`, `CrtArcadeColor`, `CrtTelevisionColor`, `CrtGreen`, `CrtAmber`, `CrtWhite`,
`LcdColor`, `LcdMonochrome`, `LedBacklitLcd`, `Oled`, `Plasma` et `Vector`.

### Gamma retenu

Le gamma GW GUI utilise **-10 à +10**, avec **0 neutre**, comme les autres réglages généraux. La
valeur affichée est une correction perceptuelle, convertie par :

`exposant = 2^(-valeur / 10)`

Ainsi, -10 donne 2,0, 0 donne 1,0 et +10 donne 0,5 ; une valeur positive éclaircit les tons moyens.
Les opérations concernées utilisent un espace linéaire et des conversions sRGB explicites. Cette
décision est retenue de manière autonome pour ne pas bloquer l’implémentation et reste révisable dans
le journal du point 7.

### Conversions des réglages généraux

La référence CPU convertit d’abord chaque composante sRGB vers la lumière linéaire, applique dans
l’ordre luminosité, contraste, gamma et saturation, puis la netteté, avant de reconvertir vers sRGB.
Les valeurs `-10..+10` utilisent les conversions déterministes suivantes :

- luminosité : décalage linéaire `valeur / 20`, soit `-0,5..+0,5` ;
- contraste : facteur `2^(valeur / 5)` autour du gris linéaire `0,18` ;
- gamma : exposant déjà validé `2^(-valeur / 10)` ;
- saturation : facteur `1 + valeur / 10`, soit `0..2`, autour de la luminance Rec. 709 ;
- netteté positive : masque flou de force `valeur / 10` à partir d’une moyenne 3×3 ; netteté
  négative : interpolation vers cette même moyenne avec la force `-valeur / 10`.

Chaque étape borne ses composantes à `0..1`. Les pixels de bord réutilisent le pixel valide le plus
proche. La valeur 0 court-circuite l’ensemble et conserve exactement les octets source.

## Architecture préparée pour l’implémentation progressive

Le découpage proposé respecte l’architecture commune :

- **configuration commune** dans `GWGUI.Emulation` : valeurs sérialisables et identifiants stables,
  sans WPF, Veldrid, GLSL ni texte utilisateur ;
- **catalogue commun** dans `GWGUI.Emulation` : métadonnées, valeurs par défaut, groupes,
  incompatibilités, capacités et clés de ressources ;
- **composition dans `GWGUI.App`** : validation, chaîne ordonnée, ciblage de l’instance et
  changements atomiques ;
- **exécution Veldrid commune** à Direct3D 11 et Vulkan ;
- **exécution OpenGL** conforme à la même définition avec programmes et framebuffers OpenGL ;
- **exécution CPU/WPF** servant de référence et de repli déclaré ;
- **interface unique** dans l’onglet Vidéo commun, séparée visuellement des options internes, avec
  liste de fonctionnalités, panneau sélectionné et cinq réglages généraux toujours visibles.

Chaque configuration Amiga, Atari ou future porte sa propre configuration vidéo commune. Chaque
instance reçoit une copie des valeurs ; une modification cible seulement l’instance dont le module
et l’identifiant correspondent. La chaîne est ordonnée ainsi : normalisation source, restauration
éventuelle, scaler, modèle CRT/LCD/scanlines/trame, réglages généraux, sortie. Les opérations
déclarent leur espace de couleur afin d’éviter des conversions sRGB répétées.

### Ordre de réalisation validé

Tout le catalogue validé doit rester documenté, mais l’implémentation avance par étapes atomiques :

1. contrats, sérialisation, catalogue commun et chaîne sans effet ;
2. interface de l’onglet Vidéo, enregistrement et application immédiate ;
3. mode Normal, réglages généraux et échantillonnage ;
4. infrastructure multi-passe commune à Direct3D 11/Vulkan, puis équivalents OpenGL et WPF ;
5. CRT complet, y compris couleur/monochrome, scanlines et trame volontaire ;
6. écran à pixels fixes avec LCD, LCD/LED et OLED ;
7. Plasma ;
8. écran vectoriel ;
9. scalers avancés et restauration ;
10. traitements temporels, affichages spécialisés, simulations de signal et effets stylistiques.

Chaque étape doit compiler, posséder ses tests ciblés et être validée dans les quatre renderers
avant la suivante. Une fonctionnalité dont la licence, le comportement ou les paramètres exacts ne
sont pas encore validés reste bloquée à l’intérieur de son étape sans empêcher la préparation des
étapes suivantes.

### Révision des effets stylistiques

Les effets stylistiques exécutent directement leur fonction shader sur OpenGL, Direct3D 11 et
Vulkan. Chaque fichier `FilterGrain.cs`, `FilterVhs.cs`, `FilterChromaticAberration.cs`,
`FilterBloom.cs` et `FilterSepia.cs` contient son shader GPU et son repli CPU.

Le grain analogique est monochrome, limité à `4,5 %` et modulé par la luminance. VHS combine
instabilité horizontale, bande passante réduite, retard de chrominance, désaturation, pertes de
ligne et commutation des têtes. L’aberration chromatique atteint sept pixels. Le halo lumineux
diffuse seulement les hautes lumières. Sépia est un interrupteur produisant une teinte brun chaud.
Le niveau de gris générique est supprimé car il doublonnait la saturation ; les quantifications
monochromes restent propres aux technologies d’affichage spécialisées, notamment l’e-paper.

### Décisions encore ouvertes

Les points suivants n’ont pas encore été validés et ne doivent pas être transformés en valeurs
définitives dans le code :

1. algorithmes ou shaders concrets et leur licence, vérifiés fichier par fichier ;
2. paramètres propres aux prochains effets temporels, simulations de signal et effets stylistiques
   avant leur étape respective.
