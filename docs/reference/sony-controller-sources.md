# Visuels des périphériques PlayStation

PNG avec canal alpha, dimension maximale de 512 pixels. Les cadrages de face et détourages sont des adaptations générées des photographies suivantes ; les zones de commande sont définies séparément dans les profils de l'application.

| Périphérique | Référence photographique |
| --- | --- |
| DualShock | Ancien `src/GWGUI.App/Assets/Controllers/sony-dualshock-1.png`, recadré en vue de face |
| Dual Analog | https://commons.wikimedia.org/wiki/File:PlayStation_Dual_Analog.png |
| Analog Joystick | https://commons.wikimedia.org/wiki/File:PlayStation-Analog-Joystick.jpg |
| Mouse | https://commons.wikimedia.org/wiki/File:PlayStation_Mouse.jpg |
| neGcon | https://commons.wikimedia.org/wiki/File:Namco_neGcon_controller.jpg |
| GunCon | https://commons.wikimedia.org/wiki/File:Guncon_1,_2,_and_3.jpg — premier modèle uniquement |
| GunCon2 / G-Con 2 | https://retrounit.com.au/products/official-namco-playstation-2-ps2-g-con-2-light-gun-controller-npc-106-wired — photographie 185058023372-0.jpg ; détourage classique par masque ImageMagick, proportions, boutons et câble conservés, toile transparente 512×512 ; aucune génération |
| PSP-1000 | https://commons.wikimedia.org/wiki/File:Psp-front.jpg — photographie de face de la variante blanche par Asim18, domaine public ; détourage classique par masque ImageMagick, ouverture de dragonne transparente, proportions et inscriptions conservées, 512×224 ; zones recalées sur cette photo, aucune génération |
| Justifier / Hyper Blaster | https://commons.wikimedia.org/wiki/File:Konami_Justifier_01.jpg — modèle PlayStation bleu foncé uniquement |

## Tapis choisi pour PS1/PS2

Visuel générique explicitement fourni et retenu par l’utilisateur : https://m.media-amazon.com/images/I/71taHKzVsvL._AC_UF894,1000_QL80_.jpg.
Retouche demandée : retrait du câble et de la prise USB, fond extérieur transparent, blancs imprimés internes conservés. Le fabricant n’est pas identifié : ne pas présenter cette image comme un modèle officiel Sony/Konami. Fichier : `src/GWGUI.App/Assets/Controllers/sony-dance-mat.png`. Chargement bitmap commun, sans générateur vectoriel ni traitement particulier dans le catalogue.

## Catalogue physique PS1/PS2 à raccorder

Périmètre utilisateur : Sony et périphériques directement associés aux jeux PlayStation, principaux modèles par famille ; pas d’inventaire des copies de manettes standard. La présence au catalogue ne signifie pas que le cœur installé émule son protocole.

| Famille / modèles représentatifs | Connexion | Machines |
| --- | --- | --- |
| Sony PlayStation Controller, Dual Analog, DualShock, Analog Joystick | Port manette | PS1, PS2 |
| Sony DUALSHOCK 2 | Port manette | PS2 |
| Sony PlayStation Mouse | Port manette | PS1, PS2 |
| Namco neGcon, JogCon, Volume Controller | Port manette | PS1, PS2 |
| Namco Arcade Stick / HORI Real Arcade Pro | Port manette | PS1 / PS2 |
| Namco GunCon, Konami Hyper Blaster / Justifier | Port manette + signal vidéo | PS1, PS2 |
| Tapis Dance Dance Revolution / Dancing Stage | Port manette | PS1, PS2 |
| Konami Guitar Freaks / RedOctane Guitar Hero SG | Port manette | PS1 / PS2 |
| Konami beatmania / beatmania IIDX | Port manette | PS1 / PS2 |
| Konami pop’n music | Port manette | PS1, PS2 |
| Namco TaTaCon (Taiko no Tatsujin) | Port manette | PS2 |
| Taito Densha de GO! / Jet de GO! | Port manette | PS1, PS2 |
| ASCII Fishing Controller (Bass Landing) | Port manette | PS1, PS2 |
| Bandai Kids Station | Port manette | PS1, PS2 |
| G.A.E. The Maestromusic baton | Port manette | PS1, PS2 |
| Sammy Shakatto Tambourine | Port manette | PS1, PS2 |
| Sony DVD Remote avec récepteur de port manette | Port manette / IR | PS2 |
| Sony USB Keyboard SCPH-10240 / USB Mouse SCPH-10230 | USB | PS2 |
| Sony EyeToy | USB | PS2 |
| Sony SingStar microphones | USB | PS2 |
| Sony Buzz! buzzers | USB | PS2 |
| Logitech SOCOM USB Headset / Vantage microphone | USB | PS2 |
| Logitech GT Force / Driving Force / Driving Force Pro | USB | PS2 |
| HORI Flight Stick 2 (Ace Combat 5) | USB | PS2 |
| Taito Densha de GO! Type 2 USB | USB | PS2 |
| Konami KeyboardMania / DrumMania / ParaParaParadise | USB | PS2 |
| Rock Band Fender Stratocaster / batterie | USB | PS2 |
| Gametrak (Dark Wind / Real World Golf) | USB | PS2 |

Références de qualification :

- Sony, compatibilité accessoires PS2 (références clavier/souris, manettes, récepteur IR, EyeToy) : https://www.playstation.com/ja-jp/support/hardware/accessories-ps2-compatibility/
- Sony, historique PS2 (EyeToy, SingStar, Buzz!, casque SOCOM) : https://www.playstation.com/en-ca/playstation-history/2000-ps2-psp/ et https://blog.playstation.com/archive/2010/09/16/the-evolution-of-the-playstation-controller/
- PSX-SPX, recherche sur les protocoles et références SLPH/SLEH : https://psx-spx.consoledev.net/psx-spx.pdf et https://psx-spx.consoledev.net/ps1/sio/controllersandmemorycards/controllers-densha-de-go-jet-de-go-controllers/
- PCSX2, documentation et code des périphériques USB natifs : https://pcsx2.net/docs/post/controllers/ et https://github.com/PCSX2/pcsx2/tree/master/pcsx2/USB.

État : inventaire de raccordement, pas déclaration d’intégration terminée. Les photographies et capacités effectives sont à vérifier appareil par appareil. Le catalogue du PCSX2 autonome ne prouve pas la prise en charge dans LRPS2, PCEE2 ou Play.

Liste complémentaire validée explicitement par l’utilisateur le 16 octobre (pièce jointe « Texte collé.txt ») : inclure aussi ASCII Specialized Pad, ASCII Arcade Stick / V, ASCII Grip, ASCII Enhanced Control Pad, Biohazard Pad, ASCII Sphere 360, Reality Quest The Glove ; Hori Fighting Stick PS, Fighting Commander 2Way/10B ; Sony Official Dance Mat ; variantes Taito Mamecon / One Handle / Two Handle ; PS2 Logitech Cordless Controller / Cordless Action, Sega Saturn PS2 Controller, Guitar Hero Kramer Striker, variantes Arcade Style beatmania IIDX / pop’n music, Hori Onimusha 3 Soul Controller, NubyTech Resident Evil 4 Chainsaw, Fighting Stick 2, Fanatec Speedster 2/3 ; USB Logitech NetPlay, SingStar/Buzz sans fil, Sega Seamic, Densha Type 2/Shinkansen/Ryojōhen/Train Mascon/Multi Train Controller, Rock Band micro/batterie, Guitar Hero World Tour micro/batterie et platine DJ Hero. Ces dernières entrées constituent une liste de vérification : leur classement et leur compatibilité PS2 ne doivent pas être présumés. L’utilisateur accepte les modèles commerciaux de fabricants d’origine, au-delà des seuls Sony, sans variantes de couleur ni copies génériques.

## Buzz! quatre joueurs

Photographie de face du modèle PS2 : https://commons.wikimedia.org/wiki/File:Buzz_Buzzer.jpg (fichier source https://upload.wikimedia.org/wikipedia/commons/c/c8/Buzz_Buzzer.jpg).
Composition demandée : quatre copies identiques de face, sans câble, fond transparent. Résultat `sony-buzz.png`, 512 × 221 pixels, 171 794 octets. Vingt zones, une rouge et quatre réponses bleu/orange/vert/jaune par joueur. Coordonnées recalées sur les pixels colorés du PNG final ; aucune commande partagée entre joueurs. Raccordement au catalogue et définitions indépendants du support natif des cœurs. La validation graphique dans la vue App reste à faire.

## Remplacements photographiques contrôlés lors de la reprise

| Fichier final | Source photographique | Dimensions | Octets | Commandes |
| --- | --- | --- | --- | --- |
| sony-jogcon.png | https://www.psxdatacenter.com/images/pads/jogcon.jpg | 512 × 399 | 303094 | Croix directionnelle, quatre boutons, Select/Start, Mode et molette ; épaules dans les associations |
| sony-guitar-hero-sg.png | https://now.estarland.com/images/products/59/80859/PlayStation-2-Guitar-Hero-II-Gibson-SG-Red-%26-Black-Wired-Guitar-Controller-large-image.jpg | 181 × 512 | 100269 | Cinq frettes, médiator haut/bas, vibrato, Start/Select ; capteur d’inclinaison dans les associations |
| sony-hori-fighting-stick-ps.png | https://static.gamesmen.com.au/media/catalog/product/cache/57ddbad6affa8d28869fa47188b75842/p/s/ps1_arcade_stick_hori_1_.jpg | 512 × 310 | 230487 | Stick, huit boutons, Select/Start |
| sony-eyetoy.png | https://gmedia.playstation.com/is/image/SIEPDC/ps2-eyetoy-image-block-01-en-18nov24?fmt=png-alpha&wid=2048 | 512 × 383 | 348922 | Aucun bouton de jeu physique ; pas de halo sur les voyants |
| sony-singstar.png | https://gmedia.playstation.com/is/image/SIEPDC/ps2-singstarmics-image-block-01-en-18nov24?fmt=png-alpha&wid=512 | 512 × 338 | 231426 | Paire de micros filaires sans bouton de jeu ; câbles extérieurs retirés |
| sony-volume-controller.png | https://i.ebayimg.com/images/g/XJ0AAOSw8rdl8-7R/s-l1600.jpg | 272 × 512 | 224337 | A, B et rotation gauche/droite ; aucune fonction de volume audio inventée |
| sony-ascii-specialized-pad.png | https://retromania-shop.com/cdn/shop/files/IMG_3935.jpg?v=1720440009 | 512 × 418 | 305042 | Boutons numériques et SLOW ; conserver les associations spécifiques au visuel |
| sony-ascii-arcade-stick.png | https://www.lukiegames.com/assets/images/ps1_asciiware_arcade_joystick.jpg | 512 × 366 | 289993 | Stick, huit boutons, Select/Start et SLOW |
| sony-namco-arcade-stick.png | https://i.ebayimg.com/images/g/VFEAAOSw30liSJLB/s-l1200.jpg | 512 × 310 | 278049 | Stick, six grands boutons, deux petits boutons L1/L2 et Select/Start |

Les nouveaux cadrages sont des retouches des photographies, avec détourage et correction de perspective. Les anciens fichiers Sony remplacés sont supprimés après contrôle du PNG ; les photos Hori Commander et ASCII Enhanced encore sans remplacement sont conservées. Aucun nettoyage des autres marques n’est autorisé par cette demande. Les 14 PNG non Sony supprimés trop largement ont tous été restaurés à l’identique de HEAD.

État du raccordement : fichiers, profils et associations écrits pour les modèles de ce tableau et Buzz!/tapis ; contrôles statiques effectués ; aucune compilation autorisée actuellement. Le reste de la liste utilisateur ci-dessus n’est pas terminé : ne pas confondre une famille présente dans les choix avec un appareil disposant déjà d’une photographie et de ses halos.


## Hori, ASCII et modèles PS1 supplémentaires

Les Hori Fighting Commander 2Way (512 × 231, 194015 octets), 10B (512 × 327, 266398) et ASCII Enhanced Pad 8180E (512 × 332, 230889) remplacent leurs anciennes photos. Les trois profils utilisent leurs boutons photographiés ; les commandes SLOW sont propres aux visuels qui en disposent.

| Nouveau modèle raccordé dans les sources | Photographie | Export PNG |
| --- | --- | --- |
| ASCII Grip SLEH-00008 | https://img.tradera.net/large-fit/584/629708584_95b3d1d8-bfc2-45ff-8746-822571a38c3d.jpg | sony-ascii-grip.png, 258 × 512, 202303 octets |
| ASCII Biohazard / Resident Evil Pad | https://cdn.shopify.com/s/files/1/0481/3553/6805/products/185878867434-0.jpg?v=1682994821 | sony-biohazard-pad.png, 512 × 374, 198981 octets |
| ASCII Sphere 360 | https://commons.wikimedia.org/wiki/File:ASCII_Sphere_360.jpg | sony-ascii-sphere-360.png, 477 × 512, 303854 octets |
| Reality Quest The Glove | https://www.computinghistory.org.uk/userdata/images/large/60/3/product-106003.jpg | sony-reality-quest-glove.png, 512 × 251, 182850 octets |
| ASCII Fighter Stick V | https://retrounit.com.au/cdn/shop/files/186650482663-3.jpg?v=1724472670&width=1946 | sony-ascii-fighter-stick-v.png, 512 × 405, 321325 octets |

Grip : quatre boutons colorés physiquement distincts, même lorsque leurs symboles moulés sont identiques ; L1/R1/L2/R2 et Select/Start. Biohazard : ne pas inventer un bouton carré absent ; DASH et READY conservent leurs boutons photographiés, R1 est présent aux deux emplacements physiques. Sphere : six axes de translation/rotation avec deux sens par axe, conformément au manuel et à la boîte scannés sur https://game-rave.com/?p=24786 ; ne pas les réduire à deux sticks. Glove : deux axes de mouvement du poignet, boutons de jeu, RESET et bouton Reality Quest ; les inscriptions L1/R1 sont conservées sur le panneau des doigts, pas remplacées par L2/R2.

Contrôle statique : identifiants, images enregistrées et douze libellés d’axes dans les catalogues de langues vérifiés. Aucun build ni test nécessitant compilation exécuté pour ce lot. Le reste de la liste reste à réaliser.

## Périphériques musicaux — raccordement des sources

| Modèle | Photographie de référence | PNG final | Dimensions | Octets | Commandes |
| --- | --- | --- | --- | --- | --- |
| ASCII / Konami beatmania | https://www.tradergames.fr/659262-large_default/beatmania-controller-beatmania-hits-sony-playstation-1-ps1-ntsc-japan-good-condition.jpg | sony-beatmania.png | 512 × 395 | 336137 | 5 touches, disque dans les deux sens, Select/Start : 9 |
| Konami GuitarFreaks | https://retrounit.com.au/cdn/shop/products/185688262225-0.jpg?v=1670422416 | sony-guitar-freaks.png | 193 × 512 | 86045 | 3 frettes, médiator, inclinaison, Select/Start : 7 ; 6 zones visibles, capteur dans les associations |
| Konami beatmania IIDX RU029 | https://i.ebayimg.com/images/g/ZLcAAOSw8mpkmM-Z/s-l500.jpg | sony-beatmania-iidx.png | 512 × 208 | 204943 | 7 touches, disque dans les deux sens, Select/Start : 11 |
| Konami beatmania IIDX Arcade Style RU038 | https://www.beep-shop.com/wp-content/uploads/2024/11/beatmania-controller.jpg | sony-beatmania-iidx-arcade.png | 512 × 327 | 329961 | 11, même disposition fonctionnelle que le modèle domestique, zones propres |
| Konami pop’n music RU014-J2 | https://www.genkivideogames.com/imagesnew/RU014J2actual.jpg | sony-popn-music.png | 512 × 222 | 185614 | 9 boutons colorés et Select/Start : 11 |
| Konami pop’n music Arcade Style RU036 | https://i.ebayimg.com/images/g/JXEAAOSwqHJjvM2K/s-l1600.jpg | sony-popn-music-arcade.png | 512 × 200 | 253707 | 11 ; disposition et légendes du vrai panneau |
| Namco TaTaCon NPC-107AB | https://i.ebayimg.com/images/g/tb0AAOSwoHNjs2wE/s-l1200.jpg | sony-taiko-drum.png | 512 × 475 | 290201 | Peau gauche/droite, bord gauche/droite, Select/Start : 6 |
| Konami DDR Stay Cool! | https://rpgrabbit.wordpress.com/wp-content/uploads/2014/07/plastic-pad.jpg | sony-konami-ddr.png | 509 × 512 | 604632 | 4 flèches, croix/cercle et Select/Start : 8 ; pas de faux boutons sur les cases avertissement/logo |

Le tapis fourni par l’utilisateur reste le premier visuel du type DanceMat ; le Konami est un modèle supplémentaire. Les ensembles propres au visuel évitent d’ajouter deux commandes fictives au tapis Konami ou des frettes/vibrato fictifs à la GuitarFreaks. Les sept nouveaux libellés sont présents dans les trente catalogues Option, avec relecture française. Les noms commerciaux sont dans la seule base commune. Contrôle des huit enregistrements, fichiers, identifiants, noms et limites de zones effectué sans compilation ; 42 profils Sony sont désormais enregistrés dans les sources, sans prétendre que toute la liste est terminée.

Référence des commandes GuitarFreaks : https://github.com/CrazyRedMachine/BEMANI_PSX_Adapter/blob/main/README.md (3 frettes, Pick, Wailing, Select, Start). Référence TaTaCon : https://psx-spx.consoledev.net/ps1/sio/controllersandmemorycards/controllers-taiko-controllers-tatacon/ .

Prochain lot : Sony clavier SCPH-10240, souris SCPH-10230 et Logitech NetPlay. Attention : NetPlay emploie deux connexions physiques : clavier USB et manette sur port manette, décrites dans https://game.watch.impress.co.jp/docs/20020425/logi.htm ; ne pas attribuer ses boutons de manette à un port USB.

## Clavier USB, souris USB et Logitech NetPlay

| Modèle | Source | Fichier | Dimensions | Octets |
| --- | --- | --- | --- | --- |
| Sony USB Keyboard SCPH-10240 | https://stat.ameba.jp/user_images/20161001/15/0613sukesan/00/7a/j/o2048153613761982810.jpg | sony-usb-keyboard.png | 512 × 160 | 149173 |
| Sony USB Mouse SCPH-10230 | https://pc.watch.impress.co.jp/docs/article/20010620/s06.jpg | sony-usb-mouse.png | 320 × 512 | 258367 |
| Logitech / Logicool NetPlay LPKC-40000 | https://game.watch.impress.co.jp/docs/20020425/logi00.jpg | sony-netplay.png | 512 × 185 | 166870 |

Les trois images sont transparentes et sans câbles. Clavier : photo dans son onglet dédié, associations des touches dans l’éditeur Clavier, aucun faux bouton de jeu sur le port USB. Souris : trois boutons physiques, dont le bouton central rectangulaire sans molette ; les trois halos sont actifs dans l’onglet Souris uniquement. NetPlay : clavier sur USB et commandes de jeu sur port manette, 26 commandes préparées (16 boutons, huit directions des sticks, MODE et VIBRATION), 22 zones visibles ; épaules accessibles dans les associations, sans les dessiner sur une face où elles ne sont pas visibles. Le clavier reste dans son onglet, sans dupliquer ses associations dans le port manette. Les mentions d’onglet dédié et de compatibilité native peuvent se cumuler. Référence de la double connexion et des commandes : https://game.watch.impress.co.jp/docs/20020425/logi.htm .

Contrôle statique des trois profils, identifiants, noms de base et limites de zones ; 45 profils Sony enregistrés. Aucune compilation ni exécution des nouveaux tests. Les modèles suivants de la liste restent à réaliser.

## Manettes Logitech, Sega PS2 et sticks Hori PS2

| Modèle | Source photographique | Export | Dimensions | Octets | Associations / zones visibles |
| --- | --- | --- | --- | --- | --- |
| Logitech Cordless Controller | https://www.computinghistory.org.uk/userdata/images/large/85/76/product-118576.jpg | sony-logitech-cordless.png | 512 × 312 | 236044 | 26 commandes, 22 zones ; MODE et VIBRATION, épaules dans les associations |
| Logitech Cordless Action Controller | https://commondatastorage.googleapis.com/images.pricecharting.com/f9b55ae8f1a33835f8dac9c0f895cb1aa649a1d5bcc0c24074c5c05a5f4d47ea/1600.jpg | sony-logitech-cordless-action.png | 512 × 340 | 274564 | 26 commandes, 22 zones ; récepteur exclu de l’image |
| Sega Saturn Control Pad for PlayStation 2 | https://i.ebayimg.com/images/g/ZUoAAOSwi4Nl5YuX/s-l1200.jpg | sony-sega-saturn-ps2.png | 512 × 284 | 211103 | 14 commandes, 12 zones ; Select conservé, six boutons avec leurs légendes Sega et correspondances PS imprimées |
| HORI Fighting Stick 2 HP2-135 | https://i.ebayimg.com/images/g/1aMAAOSweE9lCHwe/s-l1200.jpg | sony-hori-fighting-stick-2.png | 512 × 342 | 332002 | 20 commandes et zones ; L3/R3, MODE/TURBO, SPEED et STICK |
| HORI Real Arcade Pro PS2 d’origine | https://s.pacn.ws/1/p/PA.07635.001.jpg?v=i1bp7r | sony-hori-real-arcade-pro.png | 512 × 303 | 300586 | 22 commandes et zones ; huit sélecteurs turbo identifiés de gauche à droite, aucune fausse commande sur le reflet du levier |

Le modèle Sega correspond à la version PS2 avec Select, pas à un pad Saturn d’origine. Correspondances vérifiées sur le produit et dans le compte rendu photographique https://game.watch.impress.co.jp/docs/20050331/ggl.htm : A/B/C = croix/cercle/R1, X/Y/Z = carré/triangle/L1, épaules L/R = L2/R2. Le Cordless original est également répertorié pour PS1/PSone par le musée : https://www.computinghistory.org.uk/det/56648/Logitech-Cordless-Controller/ . Les deux sticks Hori annoncent PS/PSone/PS2 sur leur documentation produit.

Attention au premier Real Arcade Pro : rangée supérieure L1, carré, triangle, R1 ; rangée inférieure L2, croix, cercle, R2. Le premier détourage utilisait les légendes d’une version ultérieure ; elles ont été corrigées avant raccordement. Référence de contrôle : https://archive.supercombo.gg/t/hori-real-arcade-pro-faq-for-all-versions/12888 . Les faux libellés OFF/ON introduits pendant le détourage du Fighting Stick 2 ont été retirés ; les vrais intitulés SPEED/STICK sont conservés.

Les dix nouveaux libellés Hori existent dans les trente catalogues Option, avec français relu. Contrôles statiques des noms de base sans doublons, identifiants, fichiers, énumérations et coordonnées des cinq profils : réussis. Cinquante profils Sony enregistrés dans les sources. Aucun build, rebuild ou test nécessitant compilation.

## Volants USB Logitech PS2

| Modèle | Source photographique | Export | Dimensions | Octets | Commandes / zones |
| --- | --- | --- | --- | --- | --- |
| Logitech / Logicool GT Force | https://game.watch.impress.co.jp/docs/20010508/logi00.jpg | sony-gt-force.png | 296 × 512 | 244304 | 10 commandes, 8 zones visibles : direction, deux pédales, quatre boutons de façade, deux boutons arrière dans les associations |
| Logitech Driving Force | https://videogamecritic.com/images/extras/driving/logitech.jpg | sony-driving-force.png | 348 × 512 | 293537 | 20 commandes et zones : croix, 12 boutons, direction et pédales |
| Logitech / Logicool Driving Force Pro | https://i.ebayimg.com/images/g/ru4AAOSwzsZluUz6/s-l1600.png | sony-driving-force-pro.png | 380 × 512 | 255811 | 22 commandes et zones, levier séquentiel latéral en plus |
| Logitech Driving Force GT | https://www.dcm-modelisme.com/document_produit/2014/46/1318576244drinving_force_GT_4.jpg | sony-driving-force-gt.png | 417 × 512 | 261458 | 29 commandes, 25 zones visibles ; palettes et boutons latéraux dans les associations ; klaxon GT, PS, +/−, rotation et appui sur molette |

Chaque image contient le vrai pédalier du modèle : frein à gauche et accélérateur à droite, aucun embrayage fictif. Le GT Force d’origine n’a pas de croix directionnelle ; ses quatre boutons de façade sont identifiés par leur position sans inventer de légende illisible. Le centre Logitech du Driving Force/Pro est un logo ; le centre GT du Driving Force GT est un vrai bouton de klaxon. Les points du pommeau sont des détails du matériel, pas des boutons.

Références : https://game.watch.impress.co.jp/docs/20010508/logi.htm et https://game.watch.impress.co.jp/docs/20010213/logicool.htm pour les quatre boutons de façade et deux boutons arrière du GT Force ; https://game.watch.impress.co.jp/docs/20070426/gtf.htm pour les 12 boutons et la croix du Pro ; https://game.watch.impress.co.jp/docs/20080611/ggl.htm pour le détail L3/R3, L2/R2, palettes, levier, klaxon, PS, +/− et molette du GT. La fiche Logitech confirme explicitement la PS2 pour ce dernier : https://www.shop.bt.com/content/uni2/documentation/5760/gt_driving_force.pdf .

Douze libellés traduits avec Argos dans les trente catalogues Option et français relu. Les deux libellés de direction auparavant uniquement en anglais ont été déplacés du catalogue invariant Controller vers Option. Accélérateur et frein réutilisent les traductions communes existantes. Le contrôle VIBRATION Logitech est désormais distinct du contrôle Guide pour préserver les autres machines Sony.

Contrôles statiques des quatre profils, zones, énumérations, images, noms et ressources : réussis ; 54 profils Sony enregistrés dans les sources. Aucun build ni test imposant compilation.


## Variantes Buzz!, SingStar, audio, SeaMic et Kramer

| Modèle | Photographie | PNG | Dimensions | Octets | Commandes / halos |
| --- | --- | --- | --- | --- | --- |
| Buzz! Wireless PS2/PS3, quatre unités | https://img.ricardostatic.ch/images/b6096c4e-66ea-4418-8c56-fc1f129af68b/t_1000x750/wireless-buzz-buzzers-fur-playstation-2-3-controller | sony-buzz-wireless.png | 512 × 279 | 238945 | 20 commandes et 20 halos, aucun halo sur les voyants bleus |
| Sony SingStar Wireless, paire | https://retrounit.com.au/cdn/shop/files/387328721716-0.jpg?v=1724603803 | sony-singstar-wireless.png | 257 × 512 | 206225 | Aucun bouton de jeu, les trappes à piles ne sont pas des boutons |
| Logitech / Sony SOCOM USB Headset | https://image.konsolenkost.de/item/images/9061573/full/Playstation-2-Headset-Kopfhoerer-Socom-US-Navy-Se.jpg | sony-socom-headset.png | 512 × 493 | 94278 | Casque complet avec perche micro ; aucun bouton de jeu |
| Logitech Vantage USB Microphone | https://www.discoazul.com/uploads/media/images/microps3logitech.jpg | sony-logitech-vantage.png | 154 × 512 | 118837 | Micro complet sans câble ni boîtier USB ; aucun bouton de jeu |
| Rock Band USB Microphone | https://i.ebayimg.com/images/g/UAwAAOSwnzFnZ7sB/s-l400.jpg | sony-rock-band-microphone.png | 126 × 512 | 103534 | Aucun bouton de jeu |
| Guitar Hero USB Microphone | https://cavegamers.com/cdn/shop/files/8731_2_f47f1db9-06ec-4f57-82e5-a0719bc167d2.jpg?v=1748125475&width=1200 | sony-guitar-hero-microphone.png | 126 × 512 | 96624 | Aucun bouton de jeu ; protection moulée du câble conservée, câble retiré |
| ASCII / Sega SeaMic ASC-1606SM | https://retrodeck.readthedocs.io/en/latest/wiki_images/controllers/sega-seamic-ps2.png | sony-seamic.png | 512 × 381 | 243167 | 19 commandes préparées, 17 zones : croix, stick non cliquable, A/B/C/X/Y/Z, Select/Start, MODE ; L/R arrière dans les associations |
| RedOctane Kramer Striker PS2 | https://i.ebayimg.com/images/g/ZlIAAOSw1CRmAMuW/s-l1600.jpg | sony-kramer-striker.png | 221 × 512 | 94967 | 11 commandes, 10 zones visibles : cinq frettes, médiator haut/bas, vibrato et Select/Start ; inclinaison dans les associations |

Les visuels filaires Buzz!/SingStar sont conservés : les versions sans fil sont des modèles distincts, pas des remplacements. Les quatre Buzz! gardent vingt associations indépendantes. Référence du matériel Sony : https://fccid.io/VZVBUZZ01/User-Manual/User-Manual-927810.pdf . La LED bleue et le sélecteur latéral d’alimentation/verrouillage ne sont pas des boutons de réponse.

Le SeaMic est une manette USB avec microphone. Les trois fenêtres étroites sous MODE sont des indicateurs, sans halos. Les dix boutons USB (six de façade, deux épaules, Select/Start), le chapeau directionnel et les axes sont décrits dans https://github.com/PCSX2/pcsx2/blob/master/pcsx2/USB/usb-pad/usb-seamic.cpp ; le code amont signale lui-même des réserves sur ses associations. La commande physique MODE reste préparable, sans prétendre que les cœurs actuels la reçoivent. Aucune compatibilité native supplémentaire n’est annoncée.

La Kramer utilise son récepteur sur port manette PS2, pas une connexion USB. Les cinq frettes ont leur centre noir et leur bordure colorée d’origine ; les autocollants ajoutés par le propriétaire ont été retirés. Manuel Activision : https://device.report/m/eb146945a54d423e76422a3560d9a3c44b96267e6de14e921ca9622c7f987dbb.pdf . Les commandes réutilisent les associations de guitare existantes.

Contrôles statiques : huit fichiers transparents, maximum 512 pixels et moins de 1 Mio ; huit profils et identifiants raccordés, zones dans les limites, noms dans la seule base commune. Indication de compatibilité corrigée dans les trente catalogues via Argos, français relu : aucune promesse de boutons fictifs sur les appareils audio. Le test existant reconnaît explicitement les visuels sans boutons, sans l’exécuter. 62 profils Sony enregistrés dans les sources. Aucun build ni test exigeant compilation. Les autres périphériques de la liste restent à réaliser.


## HORI Katana et NubyTech Chainsaw PS2

| Appareil | Photographies | Fichier | Dimensions | Octets | Commandes / zones |
| --- | --- | --- | --- | --- | --- |
| HORI Onimusha 3 Katana / Soul Controller | https://www.slashgear.com/img/gallery/this-strange-playstation-2-katana-controller-is-worth-a-ton-of-money-today/if-you-want-a-soul-controller-that-works-be-prepared-to-pay-up-1691210744.jpg et https://i.ebayimg.com/images/g/3hsAAOSwzKZgVAvI/s-l1200.jpg | sony-onimusha-katana.png | 247 × 512 | 181898 | 26 commandes, 25 zones : manette complète et geste de frappe dans les associations |
| NubyTech Resident Evil 4 Chainsaw PS2 rouge | https://www.jnlgame.com/cdn/shop/files/EE5EFE34-AD3E-4992-A56A-5CADC262A80B.jpg?v=1725382771&width=5760 | sony-re4-chainsaw.png | 315 × 512 | 171032 | 26 commandes, 21 zones visibles ; quatre boutons des poignées et capteur dans les associations |

Katana : les deux faces de la poignée jouable sont représentées pour les quatre boutons arrière R2/R1/L2/L1. La lame décorative se retire pour jouer ; ce n’est pas une partie d’entrée. Deux sticks cliquables, croix, quatre boutons de façade, Select/Start et ANALOG sont conservés. Le geste de frappe correspond au carré sur le matériel d’origine ; il reste préparable séparément dans la liste. Le sélecteur de canal radio et l’interrupteur d’alimentation ne deviennent pas de faux boutons de jeu. Manuel HORI d’origine indexé : https://fcc.report/FCC-ID/RQZUHP2-109/401874.pdf ; son téléchargement direct était indisponible, les indications textuelles indexées et les photographies ont été utilisées. La légende des huit canaux a été corrigée avant export.

Tronçonneuse : il s’agit de la vraie disposition PS2 rouge, pas du modèle GameCube jaune. Étiquette du vendeur retirée, inscriptions et empreintes décoratives conservées. Aucun bouton sur la chaîne. Boutons carré/triangle et croix/cercle, deux sticks, croix directionnelle, Select/Start et ANALOG sur la façade. Commandes des poignées accessibles dans les associations. Référence de prise en main et boutons arrière : https://www.techradar.com/gaming/consoles-pc/yes-the-ps2s-resident-evil-4-chainsaw-controller-really-is-that-bad ; présentation du capteur du modèle PS2 : https://www.gearlive.com/games/article/resident_evil_4_chainsaw_controller_for_ps2_09141129 . Aucun traitement natif du geste ajouté à ce stade.

Deux libellés de mouvement traduits avec Argos dans les trente catalogues Option, français relu. Contrôles statiques : identifiants, profils, fichiers, limites des 46 zones et ressources sans doublons ; `git diff --check` réussi. 64 profils Sony enregistrés. Aucun build ni test nécessitant compilation. Catalogue utilisateur toujours en cours, autres périphériques non annoncés terminés.


## Télécommande DVD Sony et ASCII Bass Landing

| Modèle | Photographie | Fichier | Dimensions | Octets | Commandes / zones |
| --- | --- | --- | --- | --- | --- |
| Sony SCPH-10150 | https://www.thevideogamecompany.com/cdn/shop/products/sony-playstation-2-ps2-dvd-remote-controller-scph-10150-front.jpg?v=1627697486&width=3000 | sony-dvd-remote.png | 195 × 512 | 190328 | 49 touches et 49 halos distincts |
| ASCII Tsuricon / Agetec Bass Landing SLPH-00100 / SLUH-00063 | https://i.ebayimg.com/images/g/~6gAAOSw9N1lfxeA/s-l1200.png | sony-fishing-controller.png | 274 × 512 | 162234 | 17 commandes ; 17 zones couvrant 16 commandes visibles, dont les deux poignées du même moulinet ; geste de lancer dans les associations |

La télécommande a bien ses 49 touches : 16 commandes PlayStation, dix chiffres et 23 fonctions DVD supplémentaires. Les touches DVD et leurs équivalents PlayStation restent deux boutons physiques distincts ; ENTER n’est pas effacé sous prétexte que la croix peut aussi confirmer. Ce modèle n’a pas de touches marche/éjection. Référence du manuel Sony : https://www.manualslib.com/manual/1056262/Sony-Scph-10173.html?page=16#manual . La photographie ne représente pas le récepteur IR ; le périphérique demeure préparé dans le catalogue des ports manette, sans ajouter de prise en charge IR fictive aux cœurs actuels.

La canne photographiée est l’ASCII originale : croix bleue en haut à droite et cercle rouge en bas à droite. Certains Agetec ont ces deux touches inversées ; ne pas appliquer leurs coordonnées à cette image. Dix boutons, stick central à quatre directions et appui, rotation du moulinet et geste de lancer. Pas de croix directionnelle, de second stick ou de bouton ANALOG inventé. Les deux poignées du moulinet pilotent la même commande, pas deux actions différentes. Références : manuel original Bass Landing indexé sur https://www.scribd.com/document/966316283/Bass-Landing-USA-djvu et description matérielle https://psx-spx.consoledev.net/ps1/sio/controllersandmemorycards/controllers-fishing-controllers/ . Le protocole natif du capteur n’est pas déclaré implémenté.

Dix-huit contrôles visuels DVD explicites ont été ajoutés en fin de l’énumération commune, sans renuméroter les valeurs existantes. Vingt et un libellés traduits par Argos dans les trente catalogues Option, avec sources précisées pour les termes ambigus et français relu. Lecture/pause/arrêt/recherche réutilisent les ressources communes existantes. Contrôles statiques des fichiers, identifiants, 66 zones, énumération et traductions sans doublons : réussis ; `git diff --check` réussi. 66 profils Sony enregistrés dans les sources, catalogue restant en cours. Aucun build ni test imposant compilation.


## Taito PS1 — Densha de GO! et Jet de GO!

| Modèle | Photographie | Fichier | Dimensions | Octets | Commandes / zones |
| --- | --- | --- | --- | --- | --- |
| Densha de GO! deux leviers SLPH-00051 | https://i.ebayimg.com/images/g/9kgAAeSwD-5pn-EV/s-l1200.jpg | sony-densha-two-handle.png | 512 × 320 | 280933 | 9 : A/B/C, Select/Start, traction et freinage indépendants dans les deux sens |
| Densha de GO! One Handle TCPP-20001 | https://traincontrollerdb.marcriera.cat/images/hardware/tcpp20001.jpg | sony-densha-one-handle.png | 512 × 391 | 289256 | 7 : cinq boutons et levier combiné avant/arrière |
| Taito / HORI Mamecon TCPP-20002 HPS-55 | https://retrounit.com.au/cdn/shop/files/187324428142-0.jpg?v=1750121656 | sony-densha-mamecon.png | 512 × 320 | 229663 | 9 : A/B aux épaules, C au centre de la molette, Select/Start, traction et freinage séparés |
| Jet de GO! TCPP-20005 | https://i.ebayimg.com/images/g/99UAAOSwQRFk8SJN/s-l1600.jpg | sony-jet-controller.png | 512 × 399 | 269160 | 14 : quatre boutons PS, L1/R1, Select/Start, roulis/tangage et poussée dans les deux sens |

Les trois Densha utilisent des profils et des ensembles d’associations distincts. Aucun faux bouton sur le porte-gobelet. Le TCPP-20001 ne peut pas régler traction et freinage simultanément ; le Mamecon possède au contraire un levier et une molette séparés. Les graduations bleues 8 à 1 du freinage ont été rétablies sur l’image One Handle avant son raccordement. Les directions de mouvement des leviers sont proposées, sans prétendre implémenter le protocole des crans du matériel. Références techniques du chercheur : https://traincontrollerdb.marcriera.cat/hardware/slph00051/ et https://traincontrollerdb.marcriera.cat/hardware/tcpp20001/ .

Jet de GO! : manche en U, rotation et mouvement avant/arrière, ensemble de poussée à poignée double solidaire. Pas de second axe de poussée inventé à partir de ses deux tiges. Pas de D-pad ni de sticks miniatures sur l’image. Les huit boutons photographiés et les trois axes sont préparés ; aucun traitement natif ajouté. Référence de l’étude matérielle : https://psx-spx.consoledev.net/ps1/sio/controllersandmemorycards/controllers-densha-de-go-jet-de-go-controllers/ .

Douze libellés traduits avec Argos dans les trente catalogues Option, français relu. Six contrôles de leviers ajoutés en fin de l’énumération commune. Contrôles statiques : quatre profils, 39 zones, fichiers, identifiants, noms et ressources sans doublons ; `git diff --check` réussi. 70 profils Sony enregistrés dans les sources. Aucun build ni test nécessitant compilation ; les autres périphériques approuvés restent en cours.


## Cinq commandes de train USB PS2

| Modèle | Photographie | PNG | Dimensions | Octets | Commandes / zones |
| --- | --- | --- | --- | --- | --- |
| Taito Type 2 TCPP-20009 | https://commons.wikimedia.org/wiki/File:Densya_de_GO!_controller_type2_JPN.jpg | sony-densha-usb.png | 512 × 412 | 269111 | 15 commandes et zones, pédale incluse |
| Taito Shinkansen TCPP-20011 | https://i.ebayimg.com/images/g/tw0AAOSwETdmr4Vu/s-l1200.png | sony-densha-shinkansen.png | 462 × 512 | 373699 | 15 commandes et zones, pédale incluse ; aucun halo sur écran/voyants |
| Taito Ryojōhen TCPP-20014 | https://traincontrollerdb.marcriera.cat/images/hardware/tcpp20014.jpg | sony-densha-ryojohen.png | 512 × 356 | 298783 | 16 commandes, 15 zones ; pédale optionnelle dans les associations |
| Collection Train Mascon COTM-02001 | https://traincontrollerdb.marcriera.cat/images/hardware/cotm02001.jpg | sony-train-mascon.png | 512 × 375 | 312340 | 17 commandes, 16 zones ; second niveau de pression A dans les associations |
| Multi Train Controller SOTP-031201 | https://traincontrollerdb.marcriera.cat/images/hardware/sotp031201.jpg | sony-multi-train.png | 512 × 451 | 335898 | 17 commandes, 16 zones ; second niveau de pression A dans les associations |

Type 2 / Shinkansen : croix, A/B/C/D, Select/Start, traction/freinage séparés et pédale d’avertisseur. Ryojōhen : portes gauche/droite, caméra, annonce et avertisseur sont des entrées distinctes ; les crans et zones de freinage ne deviennent pas des boutons inventés. Les trois appareils acceptent une pédale par jack, mais seule une pédale réellement photographiée est représentée.

Train Mascon / Multi Train : levier combiné, inverseur avant/neutre/arrière, croix, commandes de façade et ATS. Le premier a une touche de fermeture des portes, le second une touche D. A possède deux niveaux de pression réellement distincts, conservés séparément dans les associations ; un seul halo est placé sur son bouton physique. Aucun halo sur les quatre voyants. Les graduations des deux retouches ont été corrigées avant export, le Multi Train photographié présente une cartouche P5/B8. Aucune gestion native des cartouches, lampes ou afficheurs n’est ajoutée à cette étape.

Références matérielles du chercheur : https://traincontrollerdb.marcriera.cat/hardware/tcpp20009/ , https://traincontrollerdb.marcriera.cat/hardware/tcpp20011/ , https://traincontrollerdb.marcriera.cat/hardware/tcpp20014/ , https://traincontrollerdb.marcriera.cat/hardware/cotm02001/ et https://traincontrollerdb.marcriera.cat/hardware/sotp031201/ . Les rapports USB y distinguent bien les entrées et les sorties ; le catalogue préparé ne signifie pas que les émulateurs les prennent déjà en charge.

Crédit du Type 2 : photographie originale par ignis, Wikimedia Commons, https://commons.wikimedia.org/wiki/File:Densya_de_GO!_controller_type2_JPN.jpg ; licence choisie CC BY-SA 3.0, https://creativecommons.org/licenses/by-sa/3.0/ . Retouches : perspective, détourage, câbles retirés, pédale repositionnée et réduction. Le PNG dérivé est proposé sous la même licence.

Douze nouveaux libellés traduits avec Argos dans les trente catalogues Option, français relu. Appui léger et ferme sont textuellement distincts dans chacune des langues ; ATS et les lettres imprimées restent invariants. Contrôles statiques des cinq profils, 77 zones, fichiers, énumérations et ressources : réussis ; `git diff --check` réussi. 75 profils Sony enregistrés. Aucun build ni test nécessitant compilation. Les autres périphériques de la liste restent à réaliser.


## Kids Station, Maestromusic, Tambourine et Flight Stick 2

| Modèle | Photo de référence | PNG | Dimensions | Octets | Commandes / halos |
| --- | --- | --- | --- | --- | --- |
| Bandai Kids Station BANC-0002 | https://i.ebayimg.com/00/s/MTYwMFgxNjAw/z/Ur8AAOSwTB1oLB9n/%24_57.JPG?set_id=880000500F | sony-kids-station.png | 512 × 505 | 432073 | quatre grandes touches PS, quatre halos |
| The Maestromusic GAE001 | https://s.pacn.ws/1/p/59/pa.94853.1.png?v=ji63n1 | sony-maestro-baton.png | 512 × 42 | 17085 | deux boutons, geste de battue ; deux halos |
| ASCII Tambourine ASC-0528T | https://static.mercdn.net/item/detail/orig/photos/m33599151050_1.jpg?1743170241 | sony-tambourine.png | 512 × 355 | 201231 | deux boutons latéraux, frappe jaune, secousse et déplacement horizontal/vertical ; trois halos |
| HORI Flight Stick 2 PS2 USB | https://game.watch.impress.co.jp/docs/20040218/fs200.jpg ; vues rapprochées fs201.jpg et fs208.jpg | sony-hori-flight-stick-2.png | 512 × 353 | 240580 | 39 affectations préparées, 32 zones visibles |

Détourages et redressements photographiques, sans câbles. Le bâton est extrait de la photographie du produit sur son emballage original. La transparence a été vérifiée sur les exports ; les pixels de contour très faibles (alpha 1/255) ne sont pas des boutons ni un fond opaque. Seules les six photos de travail de ce lot ont été supprimées ; les originaux des retouches sont conservés.

Kids Station ne reçoit ni croix directionnelle ni Start/Select inventés. Le bâton possède deux touches et un mouvement de battue, sans ajout de directions de mouvement non attestées. Le tambourin conserve sa barre de détection ultrasonique ; aucun halo sur les transducteurs noirs ou les extrémités de la barre. Les directions représentent le déplacement du tambourin devant sa barre, sans prétendre implémenter son protocole matériel. Présentation du fabricant via son emballage HSN-0001 et photo du propriétaire : https://jp.mercari.com/item/m33599151050 . Le rapport de test d’adaptateur distingue les boutons et la frappe : https://www.raphnet-tech.com/products/psx_to_usb/index_fr.php .

Flight Stick 2 : commandes vérifiées dans le reportage photographique https://game.watch.impress.co.jp/docs/20040218/fs2.htm et le pilote matériel original https://www.tamanegi.org/prog/hfsd/README.txt (sources : https://www.tamanegi.org/prog/hfsd/p/hfsd_src-0.4.2.tar.gz), recoupées avec https://github.com/danieloneill/mhn_hotas . Manche, HAT analogique avec appui, palonnier et gaz ; D-PAD 1/2 à quatre directions, D-PAD 3 à trois positions, TRIGGER/FIRE C/LAUNCH/A/B/SW1/D/ST et sélecteur M1/M2/M3. Les deux molettes physiques ont des associations préparées, sans prise en charge native affirmée (le pilote Linux ne les lit pas). Les boutons A/B sont préparés comme appuis ; le transport continu de leur pression n’est pas ajouté. Les commandes cachées restent dans les lignes d’affectation ; aucun halo sur les voyants ou le réglage mécanique de tension sous la base.

Dix-huit libellés Argos dans les trente catalogues Option, français relu et corrigé (baguette, secouer, palonnier, molettes). Quatre noms commerciaux dans la base commune. Contrôles statiques : 41 zones dans les limites des images, contrôles d’énumération valides, identifiants/constantes/registre présents, XML sans doublons, ressources complètes, `git diff --check` réussi. Aucun build. Les sources enregistrent désormais 79 profils Sony ; les périphériques approuvés restants ne sont pas déclarés terminés.


## Instruments PS2 et GameTrak

| Modèle | Photo retenue | PNG | Dimensions | Octets | Affectations / zones |
| --- | --- | --- | --- | --- | --- |
| KeyboardMania RU025 | https://raw.githubusercontent.com/has207/keyboardmania-midi-bridge/master/keyboardmania.jpg | sony-keyboardmania.png | 512 × 183 | 169389 | 28 / 30 |
| DrumMania RU021 | https://cdn.consolevariations.com/38919/aNbXRjDnDKiw9FyBUF2jJOemtK7ZTt-metaUGxheXN0YXRpb24tMi1QUzItRHJ1bS1NYW5pYS1EcnVtcy1Db250cm9sbGVyLWJ5LUtvbmFtaS5qcGc%3D-.jpg | sony-drummania.png | 512 × 345 | 268204 | 8 / 8 |
| ParaParaParadise RU034 | https://i.ebayimg.com/00/s/MTYwMFgxNjAw/z/2YcAAeSwG4ppz1Zz/%24_57.JPG?set_id=880000500F | sony-parapara.png | 512 × 178 | 128058 | 9 / 19 |
| Rock Band Fender Stratocaster | https://i.ebayimg.com/images/g/MksAAOSwkFpj2tAE/s-l1200.jpg | sony-rock-band-guitar.png | 196 × 512 | 104074 | 26 / 25 |
| Rock Band Drum Kit | https://i.ebayimg.com/images/g/l2cAAOSw7k5g~t-o/s-l960.jpg | sony-rock-band-drums.png | 512 × 494 | 231526 | 16 / 16 |
| Guitar Hero World Tour Drum Kit | https://www.e-shop.gr/images/PS2/BIG/PS2.00670.jpg | sony-guitar-hero-drums.png | 351 × 512 | 118768 | 17 / 17 |
| DJ Hero Turntable | https://i.ebayimg.com/images/g/OawAAOSw6PJfIhDM/s-l1200.jpg | sony-dj-hero.png | 512 × 326 | 296586 | 20 / 21 |
| In2Games GameTrak | https://image.konsolenkost.de/item/images/1061983/full/1061983-0_1.jpg | sony-gametrak.png | 512 × 324 | 172514 | 13 / 13 |

Photographies détourées/redressées, sans câbles de connexion, boîtes, baguettes de batterie ni décor. Les pédales sont conservées. Les deux gants GameTrak reprennent le modèle photographié ; les deux sorties de fils sont des capteurs, pas des boutons. Aucun halo sur le logo central de sa base ou les gants. Les pixels extérieurs et coins des PNG ont été contrôlés transparents ; le gris que certains aperçus montrent sous l’alpha n’est pas un fond opaque.

KeyboardMania : 24 notes C3–B4 (14 touches blanches, 10 noires), Select/Start et deux sens de molette. Pas de C5 ajouté. Les deux molettes visibles partagent les deux commandes attestées, sans inventer deux nouveaux axes indépendants ; aucun halo sur les segments décoratifs du boîtier. Référence matérielle de l’auteur du pilote : https://github.com/has207/keyboardmania-midi-bridge ; confirmation du nombre de touches par le projet PCSX2 : https://github.com/PCSX2/pcsx2/issues/7861 .

DrumMania : prise port manette visible sur la photo originale, corrigée dans le catalogue PS2 ; ce modèle n’est plus proposé comme appareil USB. Cinq surfaces (charleston, caisse claire, deux toms, cymbale), pédale, Select/Start. La possibilité d’utiliser des batteries MIDI via USB ne transforme pas le RU021 en appareil USB. Le test matériel de l’adaptateur confirme sa connexion PS1/PS2 : https://www.raphnet-tech.com/products/psx_to_usb/index.php .

ParaParaParadise : les cinq modules sont tous présents et distincts, trois fenêtres optiques par module, quatre touches de menu uniquement sur le module central. Les trois fenêtres d’un même module commandent sa même zone de détection ; elles ne sont pas quinze boutons indépendants. Rock Band : cinq grandes frettes et cinq frettes solo distinctes, strum, vibrato, inclinaison, croix/menu, PS et sélecteur d’effets à cinq positions. Ce sélecteur est une seule pièce physique : ses cinq associations éclairent le même élément. L’inclinaison reste dans les affectations. Les deux batteries conservent les modèles et pédales d’origine, sans cymbale supplémentaire inventée.

DJ Hero : configuration photographiée avec platine à droite et volet ouvert ; trois touches colorées, scratch, crossfader et effets dans les deux sens, touches PS et menu. Euphoria et Triangle partagent leur entrée matérielle et leurs deux halos. Référence du projet : https://github.com/PCSX2/pcsx2/blob/master/pcsx2/USB/usb-pad/usb-turntable.cpp . La platine unique reste unique ; le format de rapport permettant une platine à gauche ne justifie pas d’en dessiner deux.

GameTrak : six axes physiques (orientation X/Y et longueur de chaque fil), préparés dans les deux sens, et une pédale. Pas de boutons HID fictifs ajoutés à partir des bits réservés de son descripteur. Références des études matérielles : https://github.com/CreativeInquiry/GameTrak-Liberation et https://github.com/PCSX2/pcsx2/blob/master/pcsx2/USB/usb-pad/usb-gametrak.cpp . Ce raccordement prépare le catalogue et les associations ; il n’ajoute aucun protocole natif, transport continu de pression ou nouvelle compatibilité d’émulateur.

Contrôles statiques : huit profils, 149 zones dans les limites des images, 55 contrôles ajoutés en fin de l’énumération sans doublon, noms/constantes/registre présents, cinquante nouveaux libellés Argos dans les trente catalogues Option, français relu et corrigé (charleston, caisse claire, toms, crossfader, capteurs). XML et `git diff --check` vérifiés. Seules les huit photos de travail de ce lot ont été supprimées. Aucun build ni test compilé. 87 profils Sony sont enregistrés dans les sources.


## Quatre volants à port manette

| Modèle | Photo retenue | PNG | Dimensions | Octets | Affectations / zones |
| --- | --- | --- | --- | --- | --- |
| Mad Catz Analog / Digital Steering Wheel | https://pixel-retro.com/cdn/shop/products/IMG_8795_6922fbc1-78aa-4f2b-98df-f2467007ccae.jpg?v=1674091039&width=1445 ; pédales sur la boîte d’origine https://i.etsystatic.com/22369196/r/il/41cabb/5520389752/il_fullxfull.5520389752_acvq.jpg | sony-mad-catz-wheel.png | 509 × 512 | 433188 | 21 / 19 |
| InterAct V3 SV-1118 | https://retromtl.com/cdn/shop/files/PS1-InterActV3RacingWheelLooseWear1.jpg?v=1779809285&width=1500 | sony-interact-v3.png | 318 × 512 | 236242 | 19 / 19 |
| Fanatec Speedster 2 | https://img.ricardostatic.ch/images/b5003676-1fee-4c0d-b8fb-33dd89f68de8/t_1000x750/lenkrad-play-station-speedster-2-fanatec ; façade intacte https://image.konsolenkost.de/item/images/1064266/full/1064266-0_1.jpg | sony-fanatec-speedster-2.png | 512 × 291 | 193145 | 21 / 21 |
| Fanatec Speedster 3 | https://img.ricardostatic.ch/images/a6cd723a-aea6-44d4-9a4a-4abb99d9505d/t_1000x750/fanatec-speedster-3-fur-sony-ps-2 ; pédales https://image.konsolenkost.de/item/images/1106339/full/1106339-1655473917-0.png | sony-fanatec-speedster-3.png | 368 × 512 | 293223 | 18 / 17 |

Détourages photographiques avec les pédaliers propres aux modèles ; aucun pédalier moderne ou troisième pédale substitué. Le Speedster 2 reprend sa façade intacte, pas la photo dont le cache central manque. La retouche V3 a été corrigée pour rétablir L2/L1 sur le côté gauche. Sept photos de travail supprimées après contrôle ; originaux des retouches conservés.

Mad Catz : inscriptions I/II/A/B/L/R conservées avec leurs alias PS, croix, Select/Start, commutateur et levier. Les deux touches arrière restent dans les affectations, sans halo inventé sur une partie cachée. Étude matérielle : https://psx-spx.consoledev.net/controllersandmemorycards/#mad-catz-steering-wheel-sleh-0006 . V3 : onze boutons, croix et deux pédales, SET préparé comme commande de configuration physique. Aucun halo sur les molettes mécaniques de hauteur/inclinaison ou les voyants. Notice originale du fabricant archivée : https://www.game-rave.com/?p=26595 .

Speedster 2 : huit touches de façade, croix sur la base, MODE/Start/Select, levier et deux pédales. Pas de faux L3/R3. Speedster 3 : huit touches bleues, Start/Select/MODE/TUNING, palettes arrière L3/R3 et deux pédales ; le klaxon central partage l’entrée L2 et éclaire le même réglage que la touche L2. Pas de croix inventée ni de halo sur les six vis de la couronne. Notice d’origine : https://www.manualslib.com/manual/2299035/Fanatec-Speedster-3.html?page=5 . Le manuel couvre également PSone/PlayStation ; les quatre visuels restent disponibles sur les ports manette PS1/PS2. Ce catalogue prépare les commandes physiques, sans modifier le protocole des émulateurs.

Quatre profils avec définitions propres au visuel, 76 zones contrôlées. Axes/leviers réutilisent leurs traductions déjà présentes dans les trente catalogues ; MODE/SET/TUNING et les noms commerciaux sont invariants. Constantes, identifiants et registre raccordés, XML et `git diff --check` valides. Aucun build. Le registre Sony contient 91 profils dans les sources.


## Clôture du catalogue demandé — 17 octobre 2026

La liste approuvée a été recoupée avec les identifiants du registre : les 77 profils spécialisés contrôlés et les six familles standard Sony PS1/PS2 sont présents ; le registre complet compte 91 profils Sony, y compris les autres machines Sony déjà existantes. Les modèles demandés restants sont maintenant raccordés (images, commandes et halos), indépendamment de leur prise en charge native. Aucun modèle générique ni variante de couleur supplémentaire n’a été ajouté pour gonfler ce nombre.

Le manuel Sphere 360 est désormais accessible : https://www.game-rave.com/wp-content/uploads/2024/10/PSX-Demo-360-Controller-Demo-Manual-1-web.jpg ; provenance et description des six axes : https://www.game-rave.com/?p=24786 . Le dessin original confirme douze touches/directions et un sélecteur à quatre positions 0/1/2/3. Quatre affectations MODE et quatre petites zones ont été ajoutées au sélecteur ; le 0 qui manquait à la retouche a été rétabli. Les six axes restent proposés séparément dans les associations, sans faux boutons directionnels sur la sphère. Export corrigé : 477 × 512, 289800 octets.

Contrôles finaux sans compilation : toutes les méthodes du registre ont une définition unique et toutes leurs images existent ; 1 210 zones littérales Sony ont été vérifiées pour leurs coordonnées et références d’énumération (les Buzz! construisent leurs vingt zones chacun par boucle). XML sans doublons, identifiants d’énumération uniques et `git diff --check` valides. Tous les fichiers PNG `sony-*` présents respectent 512 pixels maximum et moins de 1 Mio. Aucun ancien JPEG Sony/Buzz/JogCon/SingStar/EyeToy ne subsiste parmi les assets ; aucun faux fichier `sony-dance-mat.vector`. Les quatorze images non Sony précédemment rétablies sont présentes et identiques à HEAD.

Les contrôles de ce lot sont statiques : aucun build, aucun test nécessitant compilation, aucune exécution de l’application modifiée. La compatibilité native des périphériques reste une étape distincte conformément à la demande ; leur présence et la préparation des associations ne sont pas présentées comme une prise en charge par les émulateurs. Le scan temporaire du Sphere 360 et l’exporteur temporaire ont été supprimés après leur dernier usage. Le dossier des photos de travail de cette tâche est vide ; les PNG finaux et les originaux des retouches sont conservés.


## Capacités déclarées par les adaptateurs et transmission native — 17 octobre 2026

Les périphériques non pris en charge ne sont plus proposés. Le catalogue photographique reste conservé. Les choix USB viennent désormais de `IEmulatorControllerAdapter.GetUsbControllerTypes`, implémenté par chaque adaptateur concerné ; la sélection et sa validation utilisent cette même déclaration. Il ne s’agit pas de rendre un cœur compatible avec un protocole qu’il ne possède pas.

- LRPS2 (`pcsx2`) : clavier et souris USB. La sélection des USB synthétiques 8/9 est convertie vers les emplacements natifs 0/1 par `NativeUsbType`, puis l’identifiant annoncé par le cœur est transmis à `retro_set_controller_port_device`. Source amont : https://github.com/libretro/ps2/blob/master/pcsx2/PAD/PAD.cpp (clavier/souris USB coexistent avec la DualShock 2).
- PCEE2 : GunCon 2 sur USB 1 et/ou USB 2. `ControllerConfigurationFunctions.Configure` convertit les choix en option native `pcsx2_lightgun` ; `CreatePorts` distingue les lectures lightgun et joypad sur leurs ports respectifs. Le changement de GunCon reste soumis au redémarrage déjà imposé. Le dépôt amont indique explicitement que les autres USB (volants, microphones, EyeToy) ne sont pas raccordés : https://github.com/WizzardSK/pcee2-libretro .
- Play! : pas de périphérique USB déclaré par l’adaptateur actuel ; seuls les périphériques de port manette qu’il expose sont proposés. Référence de l’interface libretro : https://docs.libretro.com/library/play/ . Cela ne préjuge pas des fonctionnalités d’autres distributions ou forks.
- PS1 : les listes existantes des adaptateurs SwanStation, Beetle PSX/HW et PCSX-ReARMed passent par `ControllerFunctions.Supported(configuration, port)`. `ControllerPortFunctions.Resolve` retrouve l’identifiant natif dans les périphériques annoncés par le cœur et le transmet au setter. Les limites par port de SwanStation restent conservées. Référence : https://github.com/libretro/swanstation/blob/master/src/libretro/libretro_host_interface.cpp .

Les tapis et sticks arcade utilisent leur protocole JOYPAD déjà raccordé ; les sticks arcade sont désormais initialisés aussi avant le chargement lorsque le cœur n’a pas encore publié ses descriptions. Aucun protocole spécifique supplémentaire n’est annoncé compatible par simple ressemblance du visuel.

Scénarios de tests sources adaptés/ajoutés pour les choix USB propres aux trois cœurs PS2, l’option GunCon 2 PCEE2, les identifiants natifs clavier/souris LRPS2 et la résolution JOYPAD des tapis/sticks. Contrôle statique des appels et `git diff --check` ; aucun build, aucun test exécuté ni validation en application.


## Audit multitap PS2 et cartes mémoire — 17 octobre 2026

Play! libretro : `Source/ui_libretro/PH_Libretro_Input.cpp` interroge exclusivement le port libretro 0 et transmet les entrées à `SetButtonState(0, ...)` / `SetAxisState(0, ...)`. Un seul joueur est raccordé dans cette interface, indépendamment du matériel PS2. https://github.com/jpd002/Play-/blob/master/Source/ui_libretro/PH_Libretro_Input.cpp .

LRPS2 : `PAD::LoadConfig()` dans `pcsx2/PAD/PAD.cpp` force `MultitapPort0_Enabled` et `MultitapPort1_Enabled` à false ; aucune option multitap dans `libretro/libretro_core_options.h`. Le nombre théorique de structures de pad (8) ne constitue pas une prise en charge activable. https://github.com/libretro/ps2/blob/master/pcsx2/PAD/PAD.cpp .

PCEE2 : le multitap natif expose les modes disabled/port1/port2/both. La couche charge aussi la section `[MemoryCards]` de `pcsx2/inis/PCSX2.ini` avant les options libretro. Ces dernières écrasent uniquement Slot1/Slot2. Le moteur lit les six autres emplacements via `Multitap1_Slot2_Enable/Filename` jusqu’à `Multitap2_Slot4_Enable/Filename`. Cela permet de raccorder les cartes supplémentaires via l’INI déjà géré par GW GUI. Sources : https://github.com/WizzardSK/pcee2-libretro/blob/libretro/pcee2-libretro/Libretro.cpp et https://github.com/WizzardSK/pcee2-libretro/blob/libretro/pcsx2/Pcsx2Config.cpp . Le catalogue GW GUI était limité à deux cartes, raccordement complémentaire nécessaire.


Raccordement réalisé : les cartes PCEE2 conservent les indices physiques 0/1, 2–4 (multitap 1) et 5–7 (multitap 2). Le catalogue expose 2, 5, 5 ou 8 emplacements selon le mode, identifiés MC 1/2 puis MC 1B–1D et MC 2B–2D. Les fichiers natifs reprennent les noms du moteur `Mcd-Multitap1-Slot02.ps2`, etc. Les six clés Enable/Filename sont écrites dans l’INI, les emplacements désactivés sont explicitement désactivés sans effacer les autres sections. Préparation et persistance utilisent les chemins de cartes choisis avec leurs indices physiques, y compris lorsque seul le second multitap est actif. Aucun multitap fictif ajouté à LRPS2 ou Play!.

Correction USB de la même reprise : les affectations identiques des touches d’un clavier émulé ne sont plus traitées comme des remappages réservés, ce qui bloquait l’application des sélections USB après l’apparition de l’onglet Clavier. Les remappages vers un raccourci réservé et les conflits non identiques restent contrôlés. Scénario de régression ajouté : clavier → souris → aucun, visibilité des onglets et retour après changement de machine. Le dernier cadre Général LRPS2 signalé (langues, base GameIndex externe, améliorations propres aux jeux) est masqué ; les valeurs internes existantes sont conservées.

Vérifications : analyse syntaxique Roslyn sans compilation sur les treize fichiers C# modifiés/créés ; `git diff --check` réussi. Scénarios de tests sources ajoutés/adaptés, non exécutés. Aucun nouveau build, commit ou lancement de l’application pour cette reprise.
