# 2 — Refactorisation et découpage de tout le code

Cette phase applique l’audit à tous les fichiers concernés. Elle ne se limite ni à `FluxDecoding.cs`, ni à `MainWindow`, ni à `AtariScpSectorImageReader.cs`.

## 2.1 Séparer les responsabilités

- [x] Créer un fichier par algorithme spécialisé de décodage ou d’encodage.
- [x] Séparer lecteurs de conteneurs, décodeurs de flux, reconstruction sectorielle, systèmes de fichiers et écrivains de conteneurs.
- [x] Séparer les politiques propres aux machines et formats des primitives réellement communes.
- [x] Séparer la détection de format de l’exécution du lecteur choisi.
- [x] Séparer la détection de protection du système de fichiers et du conteneur.
- [x] Séparer planification de Conversion, validation de compatibilité et exécution.
- [x] Séparer logique des onglets, composants visuels réutilisables et services globaux.
- [x] Découper les fichiers d’Options, de réglages et de profils lorsque l’audit confirme plusieurs responsabilités.

## 2.2 Exemple confirmé à généraliser

- [x] Retirer d’`AtariScpSectorImageReader.cs` les comportements Amstrad, IBM PC, BBC, Epson, UCSD et toute autre famille non Atari.
- [x] Déplacer chaque règle spécifique dans le module de sa machine ou de son format.
- [x] Extraire les opérations ISO FM/MFM vraiment communes dans un composant sans nom de machine.
- [x] Conserver séparément Atari ST, Atari 8 bits, Amstrad, IBM PC, BBC/Acorn, Epson et UCSD lorsqu’ils ont des règles distinctes.
- [x] Rechercher et corriger le même défaut de nommage ou de regroupement dans tous les autres fichiers.

## 2.3 Conditions, sélection et détection

- [x] Remplacer les enchaînements de conditions extensibles par des catalogues, registres ou stratégies adaptés.
- [x] Utiliser les identifiants de machine, format, conteneur et protection déjà connus pour limiter le travail inutile.
- [x] Définir et tester séparément le rôle du choix manuel dans Lecture, Écriture, Conversion, Explorateur et Visualisateur.
- [x] Faire produire à la détection l’ensemble des résultats compatibles nécessaires à une disquette multiformat.
- [x] Définir, documenter et tester le classement des résultats automatiques.

## 2.4 Supprimer les duplications

- [x] Extraire le code identique de lecture de bits, CRC, MFM, FM, GCR, parcours circulaire et sélection de révolution.
- [x] Paramétrer les différences simples par des définitions de format plutôt que recopier l’algorithme.
- [x] Centraliser la construction des objets secteur, piste, géométrie et résultat de détection.
- [x] Centraliser les listes de capacités utilisées par les cinq onglets concernés.

## 2.5 Application et interface

- [x] Réduire les fenêtres monolithiques en contrôles, ViewModels et services ayant une responsabilité claire.
- [x] Conserver les onglets visibles et leur comportement décidé.
- [x] Garder chaque bloc Profil indépendant même si son composant visuel est réutilisé.
- [x] Extraire menu, console, barre d’état, progression par faces et blocs opérationnels si l’audit le confirme.

## 2.6 Sécurité du refactor

- [x] Établir les tests ciblés avant chaque déplacement.
- [x] Déplacer une responsabilité à la fois.
- [x] Compiler et exécuter les tests concernés après chaque déplacement.
- [x] Vérifier que le code déplacé n’est pas enregistré ou appelé deux fois.
- [x] Supprimer l’ancien chemin seulement lorsque tous ses consommateurs utilisent le nouveau.

## Résultat

La phase 2 est terminée. L’architecture obtenue et les règles exactes de détection, de choix manuel, de multiformat et de composition de l’interface sont décrites dans [`docs/architecture.md`](../architecture.md).

Les déplacements ont été enregistrés par responsabilité dans des commits distincts. Les contrôles ciblés couvrent notamment :

- les commandes Lecture, Écriture et Conversion et leurs formats sélectionnés ;
- la compatibilité et la multiconversion depuis un flux brut ;
- le choix manuel de l’Explorateur ;
- le classement automatique des systèmes de fichiers ;
- une image SCP multiformat Atari ST/IBM PC/Amiga ;
- le choix automatique ou manuel du Visualisateur ;
- la séparation des profils par onglet ;
- la parité et l’aller-retour des décodeurs et encodeurs enregistrés.

Validation de clôture :

- compilation complète de `GWGUI.sln` : réussie, 0 erreur ;
- tests ciblés des frontières sensibles : 39/39 réussis ;
- suite complète `GWGUI.Tests` : 501/501 réussis.
