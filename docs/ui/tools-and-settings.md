# Interface — exploration, visualisation et paramètres

## Visualisateur et Explorateur

### Portée commune

- Les deux onglets acceptent les images de disquettes prises en charge, pas uniquement les fichiers SCP.
- Une image ouverte dans l’un est chargée dans l’autre sans imposer un changement d’onglet.
- Le dernier dossier d’ouverture est partagé et mémorisé.
- Une nouvelle image annule et remplace le traitement précédent.
- Les résultats techniques réutilisables sont partagés afin d’éviter un second décodage.
- Les listes Machine, Format et Protection proviennent du même catalogue que Lecture, Écriture et Conversion.
- Lorsque Détection automatique est active, chaque nouvelle image recalcule ces choix.
- Si rien n’est reconnu, les choix deviennent vides ou `Aucun` ; ils ne conservent pas la valeur de l’image précédente.
- Lorsque la détection automatique est désactivée, les choix manuels ne sont pas changés automatiquement.
- Une image peut contenir plusieurs systèmes reconnus ; l’interface et le modèle ne doivent pas réduire silencieusement ce résultat à une seule famille.

### Visualisateur

#### Affichage

- Afficher une grande représentation par face.
- Représenter le support physique correspondant : 3 pouces, 3,5 pouces DD/HD, 5,25 pouces ou 8 pouces.
- Conserver le disque visualisé presque aussi grand que le support et synchroniser son zoom.
- Afficher les pistes, secteurs, structures, données, absences et anomalies avec la légende définie.
- Afficher deux barres de synthèse Face 0 et Face 1 avec un bloc par piste.
- Les couleurs de ces blocs proviennent du résultat réel du décodage ; une anomalie légère ne doit pas condamner arbitrairement toute la piste.
- Réinitialiser l’ancien affichage avant chaque nouvelle lecture, écriture, conversion ou analyse.
- Mettre en forme progressivement pendant le chargement sans bloquer l’application.

#### Interactions

- Zoom et déplacement restent fluides.
- La sélection d’une piste se fait par clic.
- Aucun calcul, sélection ou panneau d’information ne doit être déclenché au simple survol.
- L’inspecteur flottant décrit la piste sélectionnée avec ses onglets Résumé, Révolutions, Structures et Secteurs.
- L’inspecteur peut être déplacé sans redimensionner les faces et sans dépasser de manière inutilisable.
- La légende et les barres Face 0/Face 1 restent visibles.

### Explorateur

#### Ouverture et détection

- Ouvrir une image existante ou lire une disquette physique après confirmation.
- La lecture directe produit une capture temporaire avec Greaseweazle, l’analyse puis la supprime.
- Un conteneur valide mais non reconnu reste ouvert avec un état `Aucun` ou `Non reconnu`, sans boîte d’erreur injustifiée.
- Le choix manuel d’une machine ou d’un format permet de tenter l’interprétation demandée sans inventer un catalogue.
- Pour une image multiformat, conserver et présenter les systèmes réellement reconnus selon l’interface qui sera validée.

#### Disposition

- Afficher l’arborescence des dossiers à gauche.
- Afficher le contenu du dossier au centre avec nom, date, type et taille.
- Afficher à droite les informations du disque lorsqu’aucun élément central n’est sélectionné.
- Afficher à droite les informations du fichier ou dossier sélectionné dans la liste centrale.
- Une sélection dans l’arborescence de gauche ne remplace pas cette règle du panneau de détails.
- Utiliser des icônes et types de fichiers adaptés au système reconnu, pas une classification globale par extension.
- Afficher volume, machine, format, protection, système de fichiers, capacité, espace libre, éléments et avertissements disponibles.

#### Images protégées ou sans catalogue standard

- Chercher à décoder les structures physiques de toute image, protégée ou non.
- Afficher les dossiers et fichiers réels lorsqu’un catalogue standard ou propriétaire pris en charge existe.
- Ne jamais inventer de noms de fichiers lorsqu’aucun catalogue n’existe.
- Dans ce cas, exposer pistes, secteurs, état, taille et contenu brut extractible.
- Afficher `Protection : —` lorsqu’aucune protection n’est reconnue, sinon son nom technique.
- Une protection est distincte du conteneur, du format logique et du système de fichiers.

### Traductions

Chaque nouveau libellé, état, erreur, avertissement et texte accessible est ajouté aux ressources de toutes les langues distribuées. Les noms techniques identiques dans toutes les langues restent dans la ressource neutre appropriée.

## Options, matériel et diagnostics

### Options générales

- La fenêtre conserve sa taille, reste modale et utilise des onglets horizontaux. Un bouton Fermer global hors des onglets a le même effet que la croix.
- Il n’existe aucun bouton général Enregistrer/Annuler : les listes sont appliquées immédiatement, les champs texte à la perte du focus et la fermeture effectue une dernière sauvegarde de sécurité.
- Dossier d’images par défaut.
- Langue française ou anglaise.
- La langue et le thème utilisent des listes déroulantes compactes côte à côte et s’appliquent immédiatement sans fermer ni recréer de fenêtre.
- Les hauteurs natives des champs et listes déroulantes sont conservées; leur texte est centré verticalement. Le dossier occupe le cadre gauche de la première ligne et le cadre plus court Langue/Thème se trouve à sa droite.
- Configuration des tags de conversion : activation générale, modèles prédéfinis, champ modifiable, légende des variables, aperçu et cinq modèles personnalisés récents.
- Gestion des profils utilisateur par onglet : renommer et supprimer.
- La valeur initiale du modèle est `[{FAMILY}-{FORMAT}] ` et le texte produit est placé avant le nom. Les variables disponibles couvrent nom, famille, format, extension, date et heure dans plusieurs écritures compatibles avec les noms de fichiers Windows; chacune est expliquée dans la légende à droite.
- L’aperçu est présenté dans un cadre sombre à texte vert; le bouton immédiatement voisin parcourt les exemples dans un ordre déterministe.
- Les cinq derniers modèles personnalisés sont conservés en ordre MRU : un doublon remonte en tête et le sixième chasse le plus ancien. Les modèles prédéfinis ne modifient pas cet historique.
- L’historique affiche toujours cinq emplacements numérotés de 1 à 5; les emplacements encore vides restent visibles.
- La légende présente chaque variable dans un cartouche monospace et son explication traduite à côté.
- À la taille normale de la fenêtre, le bloc complet des tags — réglages, cinq modèles récents, légende et aperçu — tient sans barre de défilement et sans élément coupé. Les défilements de la page et de la liste restent automatiques : ils apparaissent uniquement si l’utilisateur réduit suffisamment l’espace disponible.
- Un changement temporaire dans Conversion n’écrase pas ce réglage général.

### Matériel

- Plusieurs contrôleurs Greaseweazle et lecteurs mémorisés.
- Host Tools, contrôleurs et lecteurs sont regroupés dans le même onglet Matériel; Scanner et Ajouter un lecteur sont placés au-dessus de la liste compacte.
- Chaque lecteur occupe une ligne compacte sans tableau ni en-têtes : port COM, repère du lecteur, taille, densité, vitesse, disponibilité, configuration et actions Enregistrer/Oublier. La liste grandit avec les lecteurs et ne défile que lorsque leur nombre l’exige.
- Le bouton de recherche des Host Tools s’appelle explicitement Rechercher gw.exe; il ne doit pas être confondu avec Scanner, qui recherche les contrôleurs physiques.
- Dans une distribution portable, les Host Tools sont rangés sous `Data\Greaseweazle\<version>\gw.exe`. L’ancien dossier `Data\host-tools` et les archives contenant un dossier racine redondant sont migrés automatiquement.
- Le scan détecte les contrôleurs USB/série, mais Windows et `gw info` ne peuvent pas énumérer les lecteurs physiques placés derrière la nappe. Le premier lecteur est donc configuré sur le contrôleur détecté et le bouton Ajouter un lecteur permet d’en déclarer un autre manuellement.
- Identification USB et dernier port COM.
- Type physique du lecteur : 3 pouces, 3,5 pouces, 5,25 pouces ou 8 pouces.
- Sélection Greaseweazle A/B ou 0/1 lorsque nécessaire.
- Les lecteurs absents restent dans la configuration et sont signalés comme indisponibles.
- Le matériel est décrit par des listes : contrôleur associé, valeur de sélection A/B ou 0/1, taille 3, 3,5, 5,25 ou 8 pouces, densité/capacité connue et éventuellement vitesse habituelle.
- Ces propriétés servent d’abord à construire un libellé rapide; elles ne doivent pas modifier silencieusement les commandes.
- L’application peut mémoriser l’identifiant USB afin de retrouver le contrôleur même si Windows change son port COM.

### Menu Options

#### Diagnostics — boîtes de dialogue

- Informations du contrôleur (`gw info`).
- Mesure de bande passante USB (`gw bandwidth`).
- Mesure de vitesse du lecteur (`gw rpm`).
- Déplacement vers un cylindre (`gw seek`).

#### Matériel — boîtes de dialogue

- Lecture/modification des broches (`gw pin`).
- Réinitialisation du contrôleur (`gw reset`).
- Consultation/modification des délais (`gw delays`).

#### Firmware

- Mise à jour du firmware du contrôleur (`gw update`).
- La mise à jour du firmware se trouve dans `Options > Matériel > Firmware`.
- Mise à jour du bootloader séparée et protégée par un avertissement renforcé.
- Distinguer clairement : mise à jour du GUI, mise à jour des outils hôte `gw`, mise à jour du firmware matériel.

### Maintenance dans l'onglet Outils

- Effacer une disquette (`gw erase`).
- Nettoyer les têtes (`gw clean`).
- Aucun profil pour ces actions.
- Valeurs conservées pendant la session puis retour aux réglages sûrs au redémarrage.
- Confirmations adaptées à la destruction des données et à l'insertion d'une disquette de nettoyage.

### Rôle des boîtes de dialogue

- `info` : modèle, version de firmware, connexion et informations de bootloader pour les matériels concernés.
- `bandwidth` : mesure ponctuelle du débit USB en lecture/écriture.
- `rpm` : mesure de la vitesse réelle du lecteur avec nombre d’itérations.
- `seek` : déplacement de la tête vers un cylindre pour test ou maintenance; options force et moteur actif.
- `pin` : lecture ou modification de broches d’interface pour les usages matériels experts.
- `reset` : retour du contrôleur à son état de démarrage en cas de problème.
- `delays` : lecture/modification des temporisations de sélection, pas, stabilisation, moteur, watchdog, pré/post-écriture et masque d’index.
- `update` : mise à jour normale ou avancée du firmware matériel.
