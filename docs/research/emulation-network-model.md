# Registre réseau commun aux émulateurs

## Demande et limites de responsabilité

Le registre appartient à `GWGUI.Emulation`. Les modules y décrivent leurs interfaces configurées, puis publient leur état réel pendant l'exécution. `GWGUI.App` présente un tableau en lecture seule, mis à jour automatiquement, sans effectuer les échanges réseau à la place des modules.

Les configurations des machines restent la source persistante. Le registre est en mémoire et se reconstruit au lancement. Il ne nécessite pas un second fichier contenant un état de marche susceptible de devenir périmé.

L'état demandé est celui de la machine : allumée ou arrêtée. L'extinction, l'échec du démarrage, la fermeture de l'écran, la décharge du cœur et la fin inattendue d'un processus doivent tous supprimer son raccordement actif. La configuration peut rester visible avec un état arrêté.

Le format demandé est un tableau récapitulatif ; les intitulés initiaux sont des indications des informations recherchées, pas une raison de supprimer IP ou port, de les fusionner dans le contrat, ou d'imposer une autre présentation. Aucun champ « segment » ou groupe virtuel supplémentaire n'est retenu.

Le mécanisme doit pouvoir décrire un ordinateur, une console, un routeur ou un serveur indépendamment de sa marque, de son processeur et de son époque. La compatibilité provient des interfaces et des protocoles réellement émulés, jamais du nom de la machine.

## Distinctions nécessaires

La logique des couches sert à classer les données correctement. Elle n'impose pas sept services, sept contrats ou sept ensembles d'options dans l'application.

| Information | Signification | Conséquence pour les données |
| --- | --- | --- |
| Interface | Port ou adaptateur installé et réellement émulé | Type connu en enum ; identité propre à l'interface pour les machines qui en ont plusieurs |
| Réseau | Technologie à laquelle cette interface participe | Enum commun ; aucune déduction à partir de la marque ou du CPU |
| Encapsulation ou format natif | Organisation des octets, trames ou paquets échangés | Enum distinct du moyen utilisé par le PC pour les transporter |
| Protocole réseau | Par exemple IPv4, IPv6, IPX ou un protocole natif non IP | Valeurs typées ; plusieurs protocoles peuvent coexister sur une interface |
| Protocole de transport émulé | Par exemple TCP, UDP ou SPX, lorsque le module dispose réellement de cette information | Ne pas le confondre avec le transport de l'émulateur sur le PC |
| Adressage émulé | Adresse matérielle, numéro de station, adresse réseau/nœud, adresse IP ou circuit | Types connus en enum, valeurs structurées et validées selon leur type |
| Point de connexion de l'émulateur | Adresse utilisée pour atteindre l'instance et son transport hôte | IP et port distincts lorsqu'ils s'appliquent ; aucune obligation d'IP pour les autres mécanismes |
| Relais | Équipement ou mécanisme explicitement utilisé pour acheminer les échanges | Nature typée et référence séparées ; aucun convertisseur implicite |

Deux distinctions sont indispensables :

- Un paquet IP peut passer par Ethernet, ARCNET, une ligne série ou X.25. « IP » ne suffit pas à déclarer deux interfaces matériellement compatibles.
- Deux émulateurs peuvent tous deux utiliser UDP sur le PC et transporter des formats natifs incompatibles. Le registre ne doit pas les raccorder pour cette seule raison.

## Inventaire documenté des familles

Ce tableau recense les familles qui imposent des formes différentes au contrat. Il ne prétend pas que les cœurs actuels les implémentent. L'énumération d'une technologie ne constitue pas une option fonctionnelle à proposer dans un module qui ne l'émule pas.

| Famille | Interfaces et formats à distinguer | Adressage ou paramètres pertinents | Sources |
| --- | --- | --- | --- |
| Ethernet et IEEE 802 | Ethernet II, IEEE 802.3, LLC, SNAP ; une même interface peut accepter plusieurs encapsulations | Adresse matérielle distincte de l'adresse de protocole ; MTU, type d'encapsulation | [RFC 826](https://www.rfc-editor.org/info/rfc826/), [RFC 1042](https://www.rfc-editor.org/rfc/rfc1042.html) |
| Token Ring et Token Bus | Réseaux IEEE 802 distincts d'Ethernet ; paramètres d'accès et de trame propres | Adresses de stations ; informations de routage de trame selon l'interface | [RFC 1042](https://www.rfc-editor.org/rfc/rfc1042.html) |
| ARCNET | Réseau à adresses matérielles courtes ; fragmentation spécifique | Adresse ARCNET distincte de l'adresse IP ; MTU et fragmentation | [RFC 1201](https://www.rfc-editor.org/rfc/rfc1201.html) |
| LocalTalk / AppleTalk | Support LocalTalk et protocoles AppleTalk à distinguer ; AppleTalk n'est pas réservé à un seul support | Numéro de réseau, nœud et informations de zone selon le protocole | [IANA ARP](https://www.iana.org/assignments/arp-parameters), [RFC 1378](https://www.rfc-editor.org/rfc/rfc1378.html) |
| Econet / AUN | Interface Econet native et encapsulation AUN distinctes ; AUN est transporté sur IP | Réseau et station Econet ; coordonnées hôtes pour l'encapsulation | [Manuel Econet](https://bjh21.me.uk/acorn/manuals/EconetTech1/circuit.html), [PRM Econet](https://www.riscos.com/support/developers/prm/econet.html), [PRM AUN](https://www.riscos.com/support/developers/prm/aun.html) |
| Série et modem | Interface série, flux brut, SLIP et PPP ne sont pas des synonymes | Paramètres de série, état du modem, protocole négocié ; pas nécessairement d'adresse IP | [RFC 1055](https://www.rfc-editor.org/info/rfc1055/), [RFC 1661](https://www.rfc-editor.org/info/rfc1661/) |
| IPX | Protocole réseau, distinct de TCP et UDP ; peut notamment être négocié sur PPP | Numéro de réseau et de nœud ; socket selon l'échange | [RFC 1552](https://www.rfc-editor.org/info/rfc1552/) |
| X.25 / LAPB | Interface, circuit et encapsulation des protocoles à distinguer | Adresse X.121, circuit virtuel et paramètres de paquet ; adresse IP éventuelle séparée | [RFC 1356](https://www.rfc-editor.org/info/rfc1356/), [Cisco X.25/LAPB](https://www.cisco.com/c/en/us/td/docs/ios-xml/ios/wan_smxl/configuration/xe-16-7/wan-smxl-16-7-book/wan-cfg-x25-lapb.html) |
| Frame Relay | Circuit et encapsulation propres ; un équipement peut acheminer d'autres protocoles après configuration | DLCI et paramètres de circuit ; ne pas inventer un port TCP/UDP pour les remplacer | [Cisco interfaces série](https://www.cisco.com/c/en/us/td/docs/ios-xml/ios/interface/configuration/15-sy/ir-15-sy-book/ir-cfg-ser-if.html) |
| ATM | Cellules, adaptation et encapsulation multiprotocole | Identifiants de circuit et format d'adaptation ; protocole porté distinct | [Cisco ATM](https://www.cisco.com/c/en/us/td/docs/ios-xml/ios/atm/command/atm-cr-book.pdf) |
| ARPANET | Interface hôte/IMP 1822 et 1822L ; ne pas réduire le réseau historique à une interface Ethernet/IP | Identifiant d'hôte, identifiant d'IMP et, selon le mode, noms/adresses logiques | [RFC 802](https://www.rfc-editor.org/rfc/rfc802.html) |
| Omninet | Réseau propriétaire documenté, avec interfaces et adresses de périphériques | Adresse de périphérique configurée matériellement ; format natif spécifique | [Guide Corvus Omninet](https://www.bitsavers.org/pdf/corvus/7100-03108_Omninet_Network_Installation_Guide_Dec82.pdf) |
| Réseau Sinclair | Interface 1 et réseau Sinclair documentés ; ne pas l'assimiler à un réseau IP | Numéro de station, protocole et compatibilité de la version réellement émulée | [Manuel de service Interface 1](https://spectrumforeveryone.com/wp-content/uploads/2017/08/ZX-Interface-1-2-Microdrive-Service-Manual.pdf), [Guide QL](https://sinclairql.speccy.org/archivo/docs/sys/QL_UsersGuide.pdf) |
| Sans-fil IEEE 802.11 | Interface radio et protocoles de réseau distincts | Adresse matérielle, mode et paramètres radio réellement émulés ; IP éventuelle séparée | [Manuel PS5](https://www.playstation.com/content/dam/global_pdc/en-gb/corporate/support/manuals/ps5-docs/2100ab/CFI-21XX_PS5_Instruction_Manual_Web%24en-gb.pdf), [IANA interfaces](https://www.iana.org/assignments/smi-numbers) |
| Autres supports normalisés | FDDI, Fibre Channel, InfiniBand, IEEE 1394, interfaces infrarouges, interfaces ISDN et autres catégories enregistrées | Adresses, unités et paramètres propres au support ; pas d'hypothèse universelle MAC/IP/port | [IANA ARP](https://www.iana.org/assignments/arp-parameters), [IANA interfaces](https://www.iana.org/assignments/smi-numbers) |
| Connexions de consoles | Certains connecteurs échangent des bits ou octets synchronisés, sans pile IP | État et horloge du port, rôle des participants et format du protocole réellement implémenté | [Pan Docs : connecteur Link](https://github.com/gbdev/pandocs/blob/master/src/External_Connectors.md) |

Les registres IANA fournissent des types normalisés ; ils ne constituent pas à eux seuls une liste de compatibilité entre toutes les interfaces. Les réseaux propriétaires doivent aussi être référencés dans leurs manuels. Il ne faut ni inventer une entrée ni attribuer une compatibilité parce que deux technologies figurent dans le même inventaire.

## Application aux exemples demandés

| Machine ou famille | Ce que le contrat commun doit permettre |
| --- | --- |
| Amstrad CPC | Décrire une interface de communication effectivement installée. L'interface RS232C officielle communique avec modems et autres ordinateurs ; cela ne lui attribue pas automatiquement Ethernet ou TCP/IP. [Manuel Amstrad](https://www.cpcwiki.eu/index.php/Original_Amstrad_RS232_interface) |
| Acorn BBC et autres machines Econet | Décrire l'interface, la station et le mode natif/AUN effectivement choisi. Econet existe aussi sur des interfaces pour PC, Nascom et Research Machines. [PRM Econet](https://www.riscos.com/support/developers/prm/econet.html) |
| PC, notamment une configuration équipée d'un Ryzen | Décrire chaque adaptateur installé ; le nom du processeur ne détermine pas les interfaces ou les protocoles. Les identifiants de carte et de machine restent ceux du module. |
| Atari ST | Le logiciel STinG fournit une pile TCP/IP ; les interfaces et pilotes doivent être décrits séparément. Les pilotes EtherNEC/NETUS-Bee documentent notamment l'accès Ethernet. Aucune compatibilité n'est attribuée au ST sur son seul nom. [Sources STinG](https://github.com/th-otto/STinG/blob/master/README.md), [documentation des pilotes par leur distributeur](https://hardware.atari.org/software/soft.htm) |
| Commodore PET | Le MUPET documenté par son fabricant partage les disques entre plusieurs postes. Il ne faut pas le transformer en interface Ethernet ou supposer un dialogue IP entre ces postes. Le contrat doit permettre un équipement de partage et ses rôles réels. [Fiche CMD/BMB](https://fragglet.github.io/sdl-sopwith/articles/mupet-brochure.pdf) |
| Dreamcast | Distinguer le modem et le Broadband Adapter. Leur présence ne garantit pas que le logiciel chargé prenne en charge les deux. [Manuel Sega](https://www.dreamcast.es/descargas/manuales/Broadband_Manual.pdf) |
| PS2 | Le Network Adaptor SCPH-10281 documente Ethernet et modem ; la variante et le logiciel compatible doivent être identifiés. Ces deux interfaces ne doivent pas être fusionnées en un profil unique supposé. [Manuel Sony SCPH-10281](https://psx-core.ru/_ld/4/477_SCPH-10281.pdf) |
| PS5 et autres consoles modernes | Décrire le matériel de la variante émulée, ses interfaces installées et les protocoles réellement disponibles. Le manuel PS5 documente notamment Ethernet et IEEE 802.11. [Manuel Sony](https://www.playstation.com/content/dam/global_pdc/en-gb/corporate/support/manuals/ps5-docs/2100ab/CFI-21XX_PS5_Instruction_Manual_Web%24en-gb.pdf) |
| Routeur Cisco | Décrire plusieurs interfaces indépendantes, leurs encapsulations et leurs adresses. Le routage ou l'interfonctionnement configuré est exécuté par la machine émulée, pas ajouté automatiquement par le registre. [Guide Cisco](https://www.cisco.com/c/en/us/td/docs/ios-xml/ios/interface/configuration/15-sy/ir-15-sy-book/ir-cfg-ser-if.html) |
| Serveur ARPANET | Permettre une interface hôte/IMP et son adressage propre, sans exiger un numéro de station Econet ni remplacer les identifiants par une IP moderne. [RFC 802](https://www.rfc-editor.org/rfc/rfc802.html) |

## Conséquences pour les enums et les ressources

- Les catégories connues sont des enums : types d'interface, réseaux, protocoles, transports, formats échangés, types d'adresses, types de relais et raisons d'incompatibilité.
- Les valeurs d'adresse, les identifiants de machine et les identifiants d'interface sont des données ; leurs valeurs ne deviennent pas des enums arbitraires.
- Les labels affichés ne proviennent pas de `ToString()` sur les enums ni de chaînes inscrites dans le tableau. Une association enum → clé de ressource fournit l'affichage.
- Les noms techniques invariants restent dans la base commune. Les intitulés traduisibles sont traités dans toutes les langues avec Argos puis vérifiés.
- Le registre n'implique pas que tous les transports ou tous les réseaux sont déjà implémentés. Chaque module déclare seulement ses capacités réelles ; les enums n'activent pas de matériel inexistant.
- Le champ IP ne remplace pas une adresse matérielle, une station ou un circuit. IP et port restent séparés dans les données et leur origine, hôte ou machine émulée, doit être explicite.
- Le port de connexion de l'émulateur n'est pas la liste des ports applicatifs ouverts par le système émulé.

Les registres des [protocoles IP](https://www.iana.org/assignments/protocol-numbers) et des [types de matériel](https://www.iana.org/assignments/arp-parameters) servent à distinguer les catégories connues. Les champs des différents protocoles ne partagent pas tous le même espace de numérotation : une valeur numérique ARP, EtherType ou IP ne doit pas être réutilisée comme identifiant universel sans sa catégorie.

## Données à retenir pour le registre

Le choix ci-dessous conserve le tableau de suivi demandé. Il ne crée ni écran de configuration du réseau commun ni vue alternative de remplacement.

| Donnée du registre | Typage attendu | Affichage ou emploi |
| --- | --- | --- |
| État machine | Booléen, alimenté par la durée de vie réelle | Colonne d'état en lecture seule |
| Machine | Identité du module, de la configuration et de la machine | MachineId/nom ; aucune règle spéciale BBC dans le registre |
| Interface | Identité de l'interface et type connu en enum | Permet plusieurs cartes/ports d'une machine, notamment un routeur |
| Réseau | Enum de technologie réseau | Intitulé obtenu par les ressources communes |
| Protocole(s) | Enums des protocoles réellement déclarés, classés selon leur rôle | Une interface peut porter plusieurs protocoles ; ne pas déduire un logiciel actif de la seule carte |
| Format de transfert | Enum de l'unité/encapsulation effectivement échangée par le module | Trame, paquet ou flux exact ; intervient dans le contrôle de compatibilité |
| Transport de l'instance | Enum du moyen effectivement employé par l'émulateur | Distinguer UDP/TCP du PC des protocoles du système émulé |
| IP de connexion | Adresse IP structurée, facultative, IPv4 ou IPv6 | Colonne IP lorsqu'elle s'applique ; reste distincte du port dans le contrat |
| Port de connexion | Valeur numérique facultative, distincte de l'adresse | Colonne Port ; absent lorsque le mécanisme n'a pas de port réseau |
| Adresse(s) native(s) | Type d'adresse en enum et valeur validée selon ce type | Station Econet, adresse matérielle, réseau/nœud IPX ou AppleTalk, hôte/IMP, circuit… |
| Relais | Type connu en enum et référence de l'équipement ou du point réellement utilisé | Aucune conversion accordée par le seul libellé « relais » |
| Paramètres natifs utiles | Données typées fournies par le module | Seulement les paramètres intervenant réellement dans le raccordement du matériel concerné |

Le moteur ne peut pas produire une liste fiable de tous les ports applicatifs ou protocoles actifs d'un système invité à partir de son seul fichier de configuration matériel. Les informations découvertes ou négociées pendant l'exécution doivent être publiées par le module lorsqu'il peut réellement les connaître ; les autres restent non renseignées. Le registre ne doit pas les inventer.

## Règles de rapprochement

1. Consulter les interfaces déclarées et effectivement actives des autres machines ; exclure les références périmées et les interfaces de la machine elle-même d'un raccordement automatique à une autre machine.
2. Vérifier la compatibilité du matériel et des formats natifs réellement échangés. Le même nom de transport hôte, notamment UDP, ne suffit pas.
3. Distinguer la possibilité d'être raccordé à un même réseau de la possibilité de dialoguer avec un protocole donné. Deux interfaces Ethernet peuvent partager un support tout en exécutant des protocoles incompatibles.
4. Respecter les contraintes déclarées du réseau : rôles, paramètres de ligne, adressage, conflits et protocole disponible. Ne pas compléter les informations absentes par une compatibilité supposée.
5. Ne jamais convertir implicitement Econet en AUN, Ethernet en LocalTalk, IPX en IP, ou deux autres formats incompatibles.
6. Un relais n'accorde aucune compatibilité par son simple nom. Seul un équipement ou module qui exécute réellement la fonction déclarée peut acheminer ou convertir les échanges concernés.
7. Maintenir l'état à jour jusqu'à la libération effective de la session, y compris après une erreur et lors de la fermeture de son écran.

## Réalisation

- [x] Registre réseau générique et consultation dans App.
  - [x] Recherche et modèle de données.
    - [x] Créer `docs/research/emulation-network-model.md` : sources primaires, familles de réseaux, distinction des couches, responsabilités et règles sans conversion implicite.
  - [x] Contrats et registre communs.
    - [x] Créer les enums dans `src/GWGUI.Emulation/Enums/` : `EmulationNetworkInterfaceType.cs`, `EmulationNetworkType.cs`, `EmulationNetworkProtocol.cs`, `EmulationNetworkTransport.cs`, `EmulationNetworkTransferFormat.cs`, `EmulationNetworkAddressType.cs`, `EmulationNetworkRelayType.cs`, `EmulationNetworkCompatibility.cs` ; catégories connues et valeurs internes distinctes des codes transmis.
    - [x] Créer `src/GWGUI.Emulation/Contracts/EmulationNetworkEndpoint.cs`, `EmulationNetworkAddress.cs` et `EmulationNetworkRelay.cs` : identités, état, catégories typées, IP structurée, port séparé, adresses natives et références de relais.
    - [x] Créer `src/GWGUI.Emulation/Constants/EmulationNetworkConstants.cs` et `Functions/EmulationNetworkValueFunctions.cs` : constantes et correspondances vers les ressources, sans utiliser les noms d’enums comme textes affichés.
    - [x] Créer `src/GWGUI.Emulation/Interfaces/IEmulationNetworkManager.cs`, `IEmulationNetworks.cs`, `IEmulationNetworkRegistration.cs` et `Services/EmulationNetworkService.cs` : configurations, publication, propriétaires de session, mise à jour, retrait, compatibilité contrôlée par les deux modules.
    - [x] Modifier `src/GWGUI.Emulation/EmulationSdk.cs` : exposer le registre commun en mémoire.
  - [x] Raccordement du module Acorn existant.
    - [x] Modifier `src/GWGUI.Emulation.Acorn/Modules/AcornEmulationModule.cs` : description des interfaces Econet configurées.
    - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Constants/NetworkConstants.cs` : constantes une par ligne, paramètres d’adressage et version du registre.
    - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Functions/NetworkFunctions.cs` : description typée, extraction des coordonnées et contrôle natif de station.
    - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Services/EmulatorCore.Network.cs` et `EconetTransport.cs` : déclaration de présence, correspondants actifs automatiques, maintien des correspondants externes explicites, libération dans les blocs `finally` des propriétaires existants.
  - [x] Tableau de consultation.
    - [x] Créer `src/GWGUI.App/Constants/Emulation/EmulationNetworkLayoutConstants.cs`, `Contracts/Emulation/Network/EmulationNetworkRow.cs` et `Views/Controls/Emulation/Network/EmulationNetworkSection.cs` : tableau compact en lecture seule, état, machine, interface, réseau, protocoles, format, transport, IP, port, adresses natives, relais et point non IP.
    - [x] Modifier `EmulationSection.cs`, `EmulationSectionLayoutFunctions.cs`, `EmulationSectionConfigurationFunctions.cs` et `EmulationSectionTabDragFunctions.cs` dans `src/GWGUI.App/Views/Controls/Emulation/Machine/` : onglet fixe, configurations opaques transmises au registre et préservation des onglets de machines.
    - [x] Modifier les 30 fichiers `src/GWGUI.App/Resources/*/Emulation/EmulationWelcome.resx` : invariants dans la base, traductions Argos puis correction des erreurs de vocabulaire et de sens dans chaque langue.
  - [x] Vérification et corrections constatées.
    - [x] Créer `tests/GWGUI.LocalEmulationTests/Emulation/EmulationNetworkRegistryTests.cs` : configurations hors tension, états actifs, incompatibilités, paramètres natifs, identités de machines indépendantes du matériel, plusieurs interfaces, IP et port distincts, retrait et erreurs de notification.
    - [x] Modifier `tests/GWGUI.Tests/Interface/EmulationViews/MachineTabsScenarios.cs` : compter l’onglet fixe et conserver les contrôles de sélection et fermeture des machines.
    - [x] Créer `tests/GWGUI.LocalEmulationTests/Emulation/TemporaryNetworkTransportTests.cs` et `Interface/SettingsViews/TemporaryNetworkViewTests.cs` : échange UDP réel sans liste manuelle, refus Econet/AUN, état, affichage WPF et capture.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Network/EmulationNetworkSection.cs` : imposer `BindingMode.OneWay`, après détection par le contrôle WPF d’une tentative de liaison bidirectionnelle sur l’état en lecture seule.
    - [x] Supprimer les deux sources de tests temporaires précédentes et leurs captures/journaux propres dans `artifacts/` après validation.
  - [x] Livraison Debug.
    - [x] Modifier le présent document : consigner les résultats du script officiel et la vérification des fichiers livrés ci-dessous.

## Validation du 29 octobre 2026

- 64 contrôles ciblés réussis : registre, ADLC Econet, AUN, échange UDP réel, fermeture/sélection d’onglets et affichage WPF.
- Le test réseau relie deux stations sans correspondants manuels, vérifie les octets reçus puis retire leurs propriétaires. Il utilise deux ports UDP distincts alloués par le système. Les ports configurés en utilisation normale doivent également permettre l’ouverture de sockets distinctes ; le registre publie les coordonnées réellement ouvertes, sans modifier silencieusement la configuration.
- Le refus d’Econet brut vers AUN est vérifié malgré leur transport UDP commun.
- Le rendu français a été inspecté à 1080 pixels de largeur : tableau compact, IP et port séparés, consultation uniquement et défilement horizontal dans la fenêtre.
- Les 30 catalogues ont été vérifiés : XML, absence de doublons, couverture des enums et des nouvelles clés, invariants dans la base commune. Les erreurs Argos observées ont été corrigées, dont les répétitions, les négations perdues et les sens non informatiques.
- Les sources temporaires, captures et journaux de ces contrôles sont supprimés ; aucun processus `testhost` ne reste actif après validation.
- `scripts/local-building.cmd --building=debug --modules=A` : code de sortie 0.
- `build/Debug/GW GUI/gwgui.exe` et `lib/gwgui.app.dll` présents ; 24 manifestes et 24 DLL d’entrée de modules vérifiés.

Le contrat permet de décrire les différentes familles documentées ; il ne prétend pas émuler leurs cartes réseau. Le raccordement effectif ajouté concerne Econet dans AcornEmu. Les autres modules pourront déclarer leurs interfaces et fournir leurs contrôles natifs avec les mêmes contrats. La découverte automatique porte sur les sessions du registre de la même application ; les émulateurs externes conservent une destination explicite et doivent parler le même format.

## Correction de l’emplacement et de la présentation demandée

- [x] Déplacer et présenter le tableau dans les préférences d’émulation.
  - [x] Déplacer l’onglet.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSection.cs` et `EmulationSectionLayoutFunctions.cs` : retirer l’onglet Réseau des écrans de machines et son rafraîchissement local.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationPreferencesSection.cs` : ajouter Réseau après Configuration et rafraîchir ses traductions.
    - [x] Modifier `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationPreferencesSectionConfigurationFunctions.cs` : publier également les configurations chargées par les préférences dans le registre existant.
  - [x] Refaire la présentation sans sélection.
    - [x] Créer `src/GWGUI.App/Views/Styles/EmulationNetworkStyles.xaml` : styles locaux du tableau, en-têtes, lignes et cellules, alignement vertical central et absence de sélection utilisateur.
    - [x] Modifier `src/GWGUI.App/Constants/Emulation/EmulationNetworkLayoutConstants.cs` et `Views/Controls/Emulation/Network/EmulationNetworkSection.cs` : appliquer les styles, les espacements et l’état centré en lecture seule.
  - [x] Valider et livrer.
    - [x] Modifier `tests/GWGUI.Tests/Interface/EmulationViews/MachineTabsScenarios.cs` : rétablir le nombre d’onglets des machines.
    - [x] Créer `tests/GWGUI.LocalEmulationTests/Interface/SettingsViews/TemporaryNetworkPreferencesTests.cs` : vérifier l’emplacement, le centrage et l’absence de sélection ; produire une capture dans `artifacts/network-preferences.png`.
    - [x] Supprimer la source temporaire, sa capture et son journal après contrôle.
    - [x] Modifier le présent document : résultats des vérifications et du build Debug complet exigé par le contrôle automatique, sans modification supplémentaire des modules.

- [x] Modifier `src/GWGUI.App/Views/Styles/EmulationNetworkStyles.xaml` : placer les styles implicites des cellules et en-têtes dans les ressources locales ; le contrôle WPF a montré que les styles globaux prenaient priorité sur les setters du tableau.

- [x] Modifier `src/GWGUI.App/Constants/Emulation/EmulationNetworkLayoutConstants.cs`, `Views/Styles/EmulationNetworkStyles.xaml` et `Views/Controls/Emulation/Network/EmulationNetworkSection.cs` : élargir la colonne d’état pour garder « Allumée » sur une ligne et annuler le gras hérité de l’onglet dans les données et la description.

### Résultat de la correction

Réseau se trouve dans les préférences d’émulation, après Configuration, et ne figure plus parmi les onglets des machines. Les préférences publient aussi les configurations qu’elles chargent vers le registre existant. Le changement d’emplacement n’altère pas la durée de vie des interfaces actives.

Les styles sont limités au contrôle réseau : texte normal pour les données, en-têtes distincts, marges régulières, centrage vertical mesuré dans une cellule réelle et cases d’état centrées. Les lignes ne reçoivent ni clic ni focus clavier ; le défilement et le redimensionnement des colonnes restent disponibles. La colonne d’état conserve son titre sur une ligne.

Trois contrôles ciblés réussis : emplacement et rendu WPF, sélection/fermeture des onglets des machines et libellé du sélecteur. Le contrôle WPF a été relancé avec succès après les derniers ajustements ; la capture finale a été inspectée. Source temporaire, capture et journal supprimés ; aucun `testhost` restant.

Le build application seule (`--modules=0`) a été refusé par le contrôle automatique, qui exigeait `--modules=A` pour ce chantier. Le build complet a donc été exécuté : code de sortie 0, exécutable et bibliothèque App présents, 24 manifestes et DLL vérifiés. Aucun code de module supplémentaire n’a été modifié pour cette correction de présentation.

## Identification de la ROM Econet

- [x] Clarifier le champ réseau et distinguer absence de validation et incompatibilité.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Resources/00-Base/Emulation.resx` : nommer le champ invariant « Econet — NFS ROM », commun à toutes les langues ; il s’agit uniquement de noms et sigles.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Common/Machines/BbcMicro/Dictionaries/FirmwareCompatibilityCatalog.cs` : pour une ROM réseau reconnue mais non évaluée dans le profil, retourner Unknown plutôt que déduire Incompatible de son absence ; conserver les résultats explicitement connus.
  - [x] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuEconetTests.cs` : vérifier cette distinction dans le contrôle existant de ROM réseau.
  - [x] Modifier le présent document : consigner l’origine du rouge, les contrôles et le build Debug avec modules.

Le rouge de Torch NET provenait du résultat par défaut Incompatible appliqué aux ROM réseau reconnues mais absentes du profil du Model A. Ce résultat est remplacé par Unknown : l’identité du fichier est reconnue, sa compatibilité n’est pas établie. Les deux identités Acorn NFS 3.34 et 3.35K déjà évaluées conservent leur résultat partiellement compatible. Aucun fonctionnement de Torch NET n’est déclaré validé.

Le champ, son titre de sélection et sa destination dans la liste reprennent la même ressource commune « Econet — NFS ROM ». Les sigles et noms sont invariants : aucune duplication dans les autres langues. Le message de ROM manquante reste justifié lorsque l’interface Econet est activée sans fichier sélectionné ; aucune ROM n’est choisie ou intégrée automatiquement.

47 contrôles ciblés réussis, XML et absence de surcharge locale du libellé vérifiés. Build officiel `scripts/local-building.cmd --building=debug --modules=A` terminé avec code 0 ; exécutable, bibliothèque App et 24 DLL de modules vérifiés.
