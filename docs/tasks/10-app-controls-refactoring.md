# 10 — Refactorisation de `GWGUI.App/Controls`

Cette feuille suit uniquement le nettoyage structurel des contrôles de l’application. Le comportement et la présentation doivent rester identiques. Les dimensions, marges et autres valeurs purement visuelles restent dans les vues.

Une case n’est cochée qu’après modification et validation de la tâche correspondante.

## 10.1 Ressources et constantes communes

- [x] Remplacer les textes d’accessibilité écrits en dur dans les contrôles Amiga par des ressources traduites.
- [x] Centraliser le nom de la police d’icônes et les symboles techniques d’interface réellement partagés.
- [x] Centraliser les valeurs par défaut des supports d’émulation et des opérations Greaseweazle.
- [x] Centraliser les intervalles techniques et tolérances utilisés par les contrôles.

## 10.2 Fonctions communes

- [x] Créer un formateur commun des capacités et supprimer les implémentations dupliquées.
- [x] Centraliser la sélection et la lecture des valeurs de `ComboBox` lorsqu’elles suivent la même règle.
- [x] Centraliser l’exécution asynchrone des boutons avec restauration garantie de leur état.
- [x] Centraliser l’affichage et la journalisation des erreurs des contrôles.

## 10.3 Entrées d’émulation

- [x] Sortir la syntaxe et l’analyse des raccourcis clavier de `AmigaMachineView`.
- [x] Sortir la correspondance des touches Amiga de `AmigaMachineView`.
- [x] Sortir la correspondance des boutons de manette et l’analyse des ports XInput.
- [x] Faire utiliser les mêmes définitions d’entrées par la vue de la machine et l’éditeur d’associations.

## 10.4 Stockage et rendu

- [ ] Sortir les fonctions communes des dialogues de stockage dans des fichiers dédiés.
- [ ] Sortir la conversion des images vidéo de `AmigaMachineView`.
- [ ] Sortir la gestion de capture relative de la souris de `AmigaMachineView`.

## 10.5 Découpage des grands contrôles

- [ ] Séparer de `OptionsEmulationSection` le catalogue des choix techniques et des modèles.
- [ ] Séparer de `OptionsEmulationSection` la lecture et l’écriture des configurations Amiga.
- [ ] Mutualiser les constructeurs de cartes, champs, chemins et boutons réellement identiques.
- [ ] Vérifier que `OptionsEmulationSection` ne conserve que la composition et les interactions propres à l’écran.
- [ ] Vérifier que `AmigaMachineView` ne conserve que la composition et le cycle de vie propres à la vue.

## 10.6 Contrôles finaux

- [ ] Rechercher de nouveau les textes visibles écrits en dur dans tout `GWGUI.App/Controls`.
- [ ] Rechercher de nouveau les constantes métier ou techniques dupliquées, hors mise en page.
- [ ] Ajouter ou adapter les tests unitaires des composants extraits.
- [ ] Exécuter tous les tests du projet et vérifier le build de l’application.
