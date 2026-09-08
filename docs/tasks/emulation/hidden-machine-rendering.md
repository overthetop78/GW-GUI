# Suspendre le rendu des machines masquées

## Périmètre

Conserver les machines démarrées en fonctionnement, avec leurs entrées, leur audio et leur
production d'images. Suspendre seulement leur traitement de présentation lorsqu'aucune
surface de la machine n'est visible. Conserver les shaders compilés et les ressources GPU
réutilisables ; reprendre sur l'image courante sans rejouer les images accumulées.

Chaque instance conserve ses propres réglages vidéo. Le plein écran reste un affichage
visible même si l'onglet d'origine est masqué. Ne pas confondre visibilité et focus.
Ne pas ajouter d'option utilisateur ni changer les moteurs d'émulation.

Cette feuille prépare le travail : aucune tâche d'implémentation ci-dessous n'est réalisée.
Suivre `.codex/config.toml` : une action terminale à la fois ; cocher après réalisation,
puis les parents après leurs enfants. Inscrire toute action supplémentaire avant de
l'exécuter, immédiatement après la dernière action terminée.

## Liste ordonnée

- [ ] 1. Définir la suspension à partir du fonctionnement existant
  - [ ] 1.1. Formaliser les transitions et les vérifications
    - [ ] Créer `docs/architecture/hidden-machine-rendering.md` à partir de `MachineVideoPresenter.cs`, `MachineController.cs`, `MachineView.cs` et `EmulationSectionMachineFunctions.cs` : décrire les événements de visibilité de la surface réelle, les changements d'onglet, le passage sur Lecture, le plein écran, la pause, l'arrêt et la fermeture ; préciser quels événements WPF utiliser sans accéder aux contrôles depuis le worker GPU.
    - [ ] Compléter ce document après examen de `IEmulationVideoSurface.cs`, `OpenGlVideoSurface.cs`, `VeldridVideoSurface.cs`, `WpfVideoSurface.cs` et de leurs traitements : définir la conservation des ressources, le traitement de la dernière image et la remise à zéro des historiques temporels à la reprise ; préserver le fonctionnement des captures et des changements de réglages pendant le masquage. Inscrire dans cette feuille les fichiers supplémentaires nécessaires avant leur modification.
    - [ ] Créer `docs/tasks/emulation/hidden-machine-rendering-validation.md` avec le protocole de comparaison avant/après : mêmes machines, mêmes shaders par instance, mêmes dimensions de fenêtre et même durée de mesure ; séparer CPU App/moteurs, GPU App et mémoire. Reprendre les onze étapes utilisateur comme référence historique, sans transformer les captures ponctuelles en moyennes mesurées.
- [ ] 2. Suspendre et reprendre la présentation sans arrêter les machines
  - [ ] 2.1. Relier le rendu à la visibilité effective
    - [ ] Modifier `src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs` pour suivre sur le thread UI la visibilité de la surface réellement affichée, publier cet état au worker et gérer les abonnements lors du remplacement de surface et de la destruction ; tenir compte du transfert de l'écran vers la fenêtre plein écran dans `SetDisplayHost`.
    - [ ] Modifier `src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs` pour ne plus programmer de nouvelles présentations masquées, vider la frame GPU en attente et revérifier l'état avant les appels `Present`, y compris dans la file WPF ; coordonner le masquage avec un rendu déjà engagé sans course ni blocage du thread UI. Préserver la collecte utile à l'état de la machine et les notifications non vidéo nécessaires.
    - [ ] Modifier `src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs` pour présenter la dernière image disponible au retour, même si la machine est en pause, sans accumulation de frames ; conserver les ressources et l'état de compilation des shaders. Vérifier les interactions avec `SetMachine`, `SetRenderer`, `SetVideoProcessing`, le repli WPF et `Dispose`.
  - [ ] 2.2. Reprendre les effets temporels sans ancienne image parasite
    - [ ] Modifier `src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoSurface.cs` pour exposer la remise à zéro de l'historique temporel sans détruire les shaders ni les ressources de rendu, suivant le contrat défini au point 1.
    - [ ] Modifier `src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs` pour appliquer ce contrat avec la réinitialisation d'historique existante, sans recréer le programme de shaders.
    - [ ] Modifier `src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs` pour invalider les indicateurs, séquences et horodatages de l'historique sous la synchronisation existante, en conservant textures, shaders et pipeline Vulkan/Direct3D.
    - [ ] Modifier `src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs` pour appliquer le même contrat au traitement logiciel selon l'inventaire du point 1 ; inscrire au préalable chaque fichier de traitement supplémentaire à adapter si nécessaire.
    - [ ] Modifier `src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs` pour appliquer cette remise à zéro avant la première image de reprise, sous la synchronisation du rendu ; éviter une réinitialisation à chaque frame et préserver les chargements légitimes dus à un changement de shader.
- [ ] 3. Vérifier le comportement et mesurer le résultat
  - [ ] 3.1. Ajouter seulement les tests autonomes utiles
    - [ ] Créer `tests/GWGUI.Tests/Interface/EmulationViews/HiddenMachineRenderingTests.cs` avec l'infrastructure WPF existante et des machines/surfaces factices en mémoire : aucune présentation masquée, reprise sur la dernière image, reprise en pause, conservation de la surface, historique réinitialisé à la reprise, frame en attente lors du masquage et fermeture sans rappel tardif. Ne pas utiliser de DLL native ni de fichiers de données ; conserver ces tests.
    - [ ] Compléter `docs/tasks/emulation/hidden-machine-rendering-validation.md` avec les résultats réels des tests ciblés et des contrôles existants pertinents pour la vidéo et les onglets ; inscrire toute correction nécessaire dans cette feuille avant de la réaliser.
  - [ ] 3.2. Valider dans l'application et consigner les limites
    - [ ] Produire `build/Debug/GW GUI/gwgui.exe` avec `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Debug` et inscrire le résultat du build dans `docs/tasks/emulation/hidden-machine-rendering-validation.md`.
    - [ ] Compléter `docs/tasks/emulation/hidden-machine-rendering-validation.md` après essais du paquet : une puis quatre machines, changements rapides d'onglet, Lecture sans vidéo visible, plein écran aller/retour, pause/reprise, arrêt, fermeture et réglages par instance. Contrôler la continuité de l'émulation et du son, la fraîcheur de l'image au retour, les captures et l'absence de rechargement de shaders provoqué par le seul changement d'onglet ; distinguer validations réalisées et validations utilisateur attendues.
    - [ ] Compléter `docs/tasks/emulation/hidden-machine-rendering-validation.md` avec les mesures CPU/GPU/mémoire comparables avant/après, les conditions et les limites ; ne pas promettre de gain chiffré avant mesure. Si des scripts ou fichiers d'essai sont nécessaires, inscrire leurs chemins et leur suppression dans cette liste avant de les créer.
    - [ ] Supprimer les seuls scripts, fixtures, captures et fichiers temporaires ajoutés pour cette validation, après vérification de leurs chemins, puis consigner leur nettoyage dans `docs/tasks/emulation/hidden-machine-rendering-validation.md` ; préserver les tests permanents et les données utilisateur.
    - [ ] Mettre à jour `docs/architecture/hidden-machine-rendering.md` avec le comportement final et les limites vérifiées, puis cocher les groupes uniquement après réalisation de toutes leurs actions.

Aucune nouvelle chaîne d'interface n'est prévue. Si une chaîne devient nécessaire dans
le périmètre autorisé, ajouter d'abord les tâches de traduction de toutes les langues
avec Argos et la base commune. Ne pas traduire les noms propres ni dupliquer les invariants.
