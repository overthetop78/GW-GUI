# Visualisateur et Explorateur

## Portée commune

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

## Visualisateur

### Affichage

- Afficher une grande représentation par face.
- Représenter le support physique correspondant : 3 pouces, 3,5 pouces DD/HD, 5,25 pouces ou 8 pouces.
- Conserver le disque visualisé presque aussi grand que le support et synchroniser son zoom.
- Afficher les pistes, secteurs, structures, données, absences et anomalies avec la légende définie.
- Afficher deux barres de synthèse Face 0 et Face 1 avec un bloc par piste.
- Les couleurs de ces blocs proviennent du résultat réel du décodage ; une anomalie légère ne doit pas condamner arbitrairement toute la piste.
- Réinitialiser l’ancien affichage avant chaque nouvelle lecture, écriture, conversion ou analyse.
- Mettre en forme progressivement pendant le chargement sans bloquer l’application.

### Interactions

- Zoom et déplacement restent fluides.
- La sélection d’une piste se fait par clic.
- Aucun calcul, sélection ou panneau d’information ne doit être déclenché au simple survol.
- L’inspecteur flottant décrit la piste sélectionnée avec ses onglets Résumé, Révolutions, Structures et Secteurs.
- L’inspecteur peut être déplacé sans redimensionner les faces et sans dépasser de manière inutilisable.
- La légende et les barres Face 0/Face 1 restent visibles.

## Explorateur

### Ouverture et détection

- Ouvrir une image existante ou lire une disquette physique après confirmation.
- La lecture directe produit une capture temporaire avec Greaseweazle, l’analyse puis la supprime.
- Un conteneur valide mais non reconnu reste ouvert avec un état `Aucun` ou `Non reconnu`, sans boîte d’erreur injustifiée.
- Le choix manuel d’une machine ou d’un format permet de tenter l’interprétation demandée sans inventer un catalogue.
- Pour une image multiformat, conserver et présenter les systèmes réellement reconnus selon l’interface qui sera validée.

### Disposition

- Afficher l’arborescence des dossiers à gauche.
- Afficher le contenu du dossier au centre avec nom, date, type et taille.
- Afficher à droite les informations du disque lorsqu’aucun élément central n’est sélectionné.
- Afficher à droite les informations du fichier ou dossier sélectionné dans la liste centrale.
- Une sélection dans l’arborescence de gauche ne remplace pas cette règle du panneau de détails.
- Utiliser des icônes et types de fichiers adaptés au système reconnu, pas une classification globale par extension.
- Afficher volume, machine, format, protection, système de fichiers, capacité, espace libre, éléments et avertissements disponibles.

### Images protégées ou sans catalogue standard

- Chercher à décoder les structures physiques de toute image, protégée ou non.
- Afficher les dossiers et fichiers réels lorsqu’un catalogue standard ou propriétaire pris en charge existe.
- Ne jamais inventer de noms de fichiers lorsqu’aucun catalogue n’existe.
- Dans ce cas, exposer pistes, secteurs, état, taille et contenu brut extractible.
- Afficher `Protection : —` lorsqu’aucune protection n’est reconnue, sinon son nom technique.
- Une protection est distincte du conteneur, du format logique et du système de fichiers.

## Traductions

Chaque nouveau libellé, état, erreur, avertissement et texte accessible est ajouté aux ressources de toutes les langues distribuées. Les noms techniques identiques dans toutes les langues restent dans la ressource neutre appropriée.

