# 1 — Audit complet du code

Cette phase concerne tout le dépôt. `AtariScpSectorImageReader.cs` est un exemple confirmé du problème, pas la limite de l’audit.

## 1.1 Inventaire exhaustif

- [ ] Inventorier tous les fichiers C#, XAML, scripts, ressources, projets et workflows.
- [ ] Décrire la responsabilité réelle de chaque fichier, indépendamment de son nom actuel.
- [ ] Repérer toutes les classes dont le nom désigne une machine alors qu’elles traitent plusieurs familles.
- [ ] Repérer tous les fichiers qui mélangent plusieurs niveaux : conteneur, flux, secteur, système de fichiers, conversion, UI ou persistance.
- [ ] Repérer les fichiers courts qui regroupent malgré tout plusieurs responsabilités susceptibles de grossir.
- [ ] Repérer les fichiers volumineux qui doivent être découpés et ceux qui peuvent rester tels quels parce qu’ils ont une responsabilité unique.
- [ ] Produire une carte des dépendances entre `GWGUI.App`, `GWGUI.Domain`, `GWGUI.Infrastructure`, `GWGUI.Scp` et les tests.

## 1.2 Flux et images de disquettes

- [ ] Cartographier tous les lecteurs et écrivains de conteneurs.
- [ ] Cartographier tous les décodeurs et encodeurs de pistes.
- [ ] Cartographier toutes les reconstructions sectorielles.
- [ ] Cartographier tous les lecteurs de systèmes de fichiers.
- [ ] Cartographier la détection de machine, format, géométrie, système, protection et image multiformat.
- [ ] Cartographier les parcours Lecture, Écriture, Conversion, Visualisateur et Explorateur jusqu’aux services techniques.
- [ ] Vérifier quels résultats sont recalculés plusieurs fois entre Explorateur et Visualisateur.

## 1.3 Problèmes de structure

- [ ] Relever toutes les longues chaînes de `if`, `else if`, comparaisons de chaînes et préfixes de format.
- [ ] Relever les `switch` déjà trop grands ou susceptibles de devenir un second monolithe.
- [ ] Relever les chemins qui essaient successivement de nombreux formats sans utiliser les informations déjà disponibles.
- [ ] Relever les chemins qui arrêtent trop tôt la détection et peuvent manquer une disquette multiformat.
- [ ] Relever toutes les duplications exactes et toutes les duplications ne différant que par une définition de machine ou de géométrie.
- [ ] Relever les dépendances UI vers des implémentations techniques concrètes.
- [ ] Relever les états globaux ou partagés qui devraient être propres à un onglet ou une opération.

## 1.4 Données et textes

- [ ] Relever les nombres magiques, tailles, géométries, CRC, marques, extensions et identifiants écrits dans les algorithmes.
- [ ] Relever les noms de machines, formats, codecs, protections et systèmes de fichiers recopiés dans plusieurs fichiers.
- [ ] Relever tout texte visible codé en dur dans C# ou XAML.
- [ ] Relever les messages techniques pouvant rester non traduits et les distinguer des messages destinés à l’utilisateur.
- [ ] Relever les modèles de données mélangés à leur logique de lecture ou d’affichage.

## 1.5 Livrable de l’audit

- [ ] Produire un tableau fichier par fichier : responsabilité actuelle, problèmes, destination proposée et risque du déplacement.
- [ ] Indiquer les dépendances à préserver et les tests nécessaires avant chaque découpage.
- [ ] Soumettre toute nouvelle décision de comportement découverte pendant l’audit avant de l’intégrer au plan d’implémentation.
- [ ] Faire valider la cartographie avant d’ouvrir la phase 2.
