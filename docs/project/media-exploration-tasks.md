# Exploration et visualisation des HDD et supports optiques

Travail différé : commencer après le chantier en cours de création, formatage et gestion des images HDD décrit dans [le catalogue des formats](hard-disk-format-catalog.md).

Toutes les cases restent ouvertes jusqu’à réalisation et vérification. La hiérarchie est : groupe de groupes de tâches (1), groupe de tâches (1.1), tâche (1.1.1), sous-tâche (1.1.1.1). Les capacités restent indépendantes des machines et émulateurs consommateurs.

## 1. Accès commun aux images

### 1.1. Modèle de lecture

- [ ] **1.1.1. Séparer conteneur, organisation du support et système de fichiers.**
  - [ ] 1.1.1.1. Inventorier les lecteurs existants et leurs capacités effectives, distinctes de la création et du formatage.
  - [ ] 1.1.1.2. Définir des accès communs aux blocs, partitions, sessions, pistes et volumes, sans imposer une géométrie de disquette.
  - [ ] 1.1.1.3. Conserver les chemins de lecture des disquettes derrière des adaptateurs compatibles.
- [ ] **1.1.2. Permettre l’exploration de grandes images en lecture seule.**
  - [ ] 1.1.2.1. Lire les blocs et répertoires à la demande avec annulation et mémoire bornée.
  - [ ] 1.1.2.2. Gérer les offsets et tailles de grande capacité sans troncature.
  - [ ] 1.1.2.3. Signaler formats inconnus, données corrompues et dépendances manquantes sans modifier les sources.

## 2. Exploration des fichiers

### 2.1. Images HDD

- [ ] **2.1.1. Explorer les volumes contenus dans une image HDD.**
  - [ ] 2.1.1.1. Ajouter les lecteurs de conteneurs et de partitionnements nécessaires en réutilisant les composants disponibles.
  - [ ] 2.1.1.2. Détecter les volumes directs et les différentes partitions, puis proposer leur sélection.
  - [ ] 2.1.1.3. Brancher les lecteurs de systèmes de fichiers et afficher dossiers, fichiers, capacité et espace libre lorsque connus.

### 2.2. Images optiques

- [ ] **2.2.1. Lire les organisations des images CD, DVD et autres supports optiques.**
  - [ ] 2.2.1.1. Inventorier les formats et variantes, notamment ISO, BIN/CUE et MDF/MDS, et leurs fichiers associés.
  - [ ] 2.2.1.2. Lire les sessions et pistes de données ou audio selon les informations conservées par chaque format.
  - [ ] 2.2.1.3. Traiter les descripteurs absents ou incohérents sans inventer une organisation du support.
- [ ] **2.2.2. Explorer les systèmes de fichiers optiques.**
  - [ ] 2.2.2.1. Ajouter ou réutiliser les lecteurs ISO 9660, Joliet, Rock Ridge et UDF selon leurs variantes prises en charge.
  - [ ] 2.2.2.2. Permettre le choix du volume ou de la session à parcourir.
  - [ ] 2.2.2.3. Présenter les pistes audio séparément des fichiers d’un volume de données.

### 2.3. Explorateur existant

- [ ] **2.3.1. Réutiliser l’interface d’exploration.**
  - [ ] 2.3.1.1. Adapter le document affiché pour conserver l’arborescence, les listes et les informations de fichiers existantes.
  - [ ] 2.3.1.2. Ajouter les sélecteurs de partitions, sessions et volumes pertinents pour le support ouvert.
  - [ ] 2.3.1.3. Charger progressivement les répertoires et conserver la navigation actuelle des disquettes.

## 3. Visualisation des supports

### 3.1. HDD

- [ ] **3.1.1. Ajouter un rendu des blocs et de l’organisation du HDD.**
  - [ ] 3.1.1.1. Afficher secteurs logiques, partitions et zones réservées.
  - [ ] 3.1.1.2. Afficher les zones allouées et libres lorsque le lecteur du système de fichiers les expose.
  - [ ] 3.1.1.3. Permettre la sélection d’une zone et la consultation de ses informations.
- [ ] **3.1.2. Représenter les surfaces et plateaux lorsque pertinent.**
  - [ ] 3.1.2.1. Distinguer géométrie déclarée, géométrie virtuelle et information physique réellement disponible.
  - [ ] 3.1.2.2. Identifier explicitement toute représentation schématique ; ne pas présenter des défauts physiques ou une disposition réelle non décrits par l’image.

### 3.2. Supports optiques

- [ ] **3.2.1. Représenter les pistes et sessions.**
  - [ ] 3.2.1.1. Ajouter un rendu distinguant pistes audio, données et sessions documentées par l’image.
  - [ ] 3.2.1.2. Relier la sélection visuelle aux secteurs et volumes correspondants.
- [ ] **3.2.2. Prendre en compte les couches et faces des supports optiques.**
  - [ ] 3.2.2.1. Vérifier, format par format, la disponibilité du nombre de couches, de leurs limites, du sens de parcours et des faces éventuelles.
  - [ ] 3.2.2.2. Afficher et sélectionner les couches connues avec leurs plages de secteurs et transitions lorsque décrites.
  - [ ] 3.2.2.3. Distinguer couches, faces, pistes et sessions dans le modèle et l’affichage.
  - [ ] 3.2.2.4. Afficher « information indisponible » lorsque la structure physique n’est pas conservée ; ne pas déduire une limite de couche de la seule taille de l’image.

### 3.3. Visualiseur existant

- [ ] **3.3.1. Étendre les rendus sans imposer le modèle SCP aux autres médias.**
  - [ ] 3.3.1.1. Réutiliser les commandes communes de navigation, zoom et sélection lorsque adaptées.
  - [ ] 3.3.1.2. Conserver le rendu des pistes et du flux des disquettes et ajouter des rendus HDD et optiques indépendants.

## 4. Validation et préservation de l’existant

### 4.1. Tests automatisés

- [ ] **4.1.1. Vérifier les lecteurs et modèles sur des données simulées en mémoire.**
  - [ ] 4.1.1.1. Couvrir conteneurs, partitions multiples, volumes, sessions, pistes, couches et informations absentes.
  - [ ] 4.1.1.2. Couvrir les lectures aux limites, images tronquées, descripteurs invalides et dépendances manquantes.
  - [ ] 4.1.1.3. Vérifier l’absence d’écriture sur les sources et le chargement à la demande sans créer de fichiers images réels.
- [ ] **4.1.2. Vérifier l’intégration dans l’interface et le workflow de release.**
  - [ ] 4.1.2.1. Tester sélection et navigation à partir de modèles simulés sans dépendre d’un bureau interactif.
  - [ ] 4.1.2.2. Vérifier les comportements existants de l’explorateur et du visualiseur de disquettes.
  - [ ] 4.1.2.3. Intégrer ces tests rapides à la suite de release, sans lancer d’émulateurs ni de programmes externes.
