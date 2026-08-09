# 2 — Refactorisation et découpage de tout le code

Cette phase applique l’audit à tous les fichiers concernés. Elle ne se limite ni à `FluxDecoding.cs`, ni à `MainWindow`, ni à `AtariScpSectorImageReader.cs`.

## 2.1 Séparer les responsabilités

- [ ] Créer un fichier par algorithme spécialisé de décodage ou d’encodage.
- [ ] Séparer lecteurs de conteneurs, décodeurs de flux, reconstruction sectorielle, systèmes de fichiers et écrivains de conteneurs.
- [ ] Séparer les politiques propres aux machines et formats des primitives réellement communes.
- [ ] Séparer la détection de format de l’exécution du lecteur choisi.
- [ ] Séparer la détection de protection du système de fichiers et du conteneur.
- [ ] Séparer planification de Conversion, validation de compatibilité et exécution.
- [ ] Séparer logique des onglets, composants visuels réutilisables et services globaux.
- [ ] Découper les fichiers d’Options, de réglages et de profils lorsque l’audit confirme plusieurs responsabilités.

## 2.2 Exemple confirmé à généraliser

- [ ] Retirer d’`AtariScpSectorImageReader.cs` les comportements Amstrad, IBM PC, BBC, Epson, UCSD et toute autre famille non Atari.
- [ ] Déplacer chaque règle spécifique dans le module de sa machine ou de son format.
- [ ] Extraire les opérations ISO FM/MFM vraiment communes dans un composant sans nom de machine.
- [ ] Conserver séparément Atari ST, Atari 8 bits, Amstrad, IBM PC, BBC/Acorn, Epson et UCSD lorsqu’ils ont des règles distinctes.
- [ ] Rechercher et corriger le même défaut de nommage ou de regroupement dans tous les autres fichiers.

## 2.3 Conditions, sélection et détection

- [ ] Remplacer les enchaînements de conditions extensibles par des catalogues, registres ou stratégies adaptés.
- [ ] Utiliser les identifiants de machine, format, conteneur et protection déjà connus pour limiter le travail inutile.
- [ ] Définir et tester séparément le rôle du choix manuel dans Lecture, Écriture, Conversion, Explorateur et Visualisateur.
- [ ] Faire produire à la détection l’ensemble des résultats compatibles nécessaires à une disquette multiformat.
- [ ] Définir, documenter et tester le classement des résultats automatiques.

## 2.4 Supprimer les duplications

- [ ] Extraire le code identique de lecture de bits, CRC, MFM, FM, GCR, parcours circulaire et sélection de révolution.
- [ ] Paramétrer les différences simples par des définitions de format plutôt que recopier l’algorithme.
- [ ] Centraliser la construction des objets secteur, piste, géométrie et résultat de détection.
- [ ] Centraliser les listes de capacités utilisées par les cinq onglets concernés.

## 2.5 Application et interface

- [ ] Réduire les fenêtres monolithiques en contrôles, ViewModels et services ayant une responsabilité claire.
- [ ] Conserver les onglets visibles et leur comportement décidé.
- [ ] Garder chaque bloc Profil indépendant même si son composant visuel est réutilisé.
- [ ] Extraire menu, console, barre d’état, progression par faces et blocs opérationnels si l’audit le confirme.

## 2.6 Sécurité du refactor

- [ ] Établir les tests ciblés avant chaque déplacement.
- [ ] Déplacer une responsabilité à la fois.
- [ ] Compiler et exécuter les tests concernés après chaque déplacement.
- [ ] Vérifier que le code déplacé n’est pas enregistré ou appelé deux fois.
- [ ] Supprimer l’ancien chemin seulement lorsque tous ses consommateurs utilisent le nouveau.
