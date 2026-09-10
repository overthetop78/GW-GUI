# Présentation des machines masquées

## Transitions retenues

Le présentateur suit `MachineView.VideoHost` (`IsVisibleChanged`, `Loaded`, `Unloaded`)
sur le dispatcher UI. Ce conteneur se déplace avec Screen en plein écran ; le contrôle
vidéo seul est masqué pendant la compilation des shaders et ne doit pas bloquer celle-ci.
La fenêtre contenant VideoHost est suivie pour sa minimisation (`StateChanged`). Le focus
n'intervient pas. L'arrêt masque VideoHost via le mécanisme existant ; la fermeture
détruit le présentateur. La pause conserve la dernière image et autorise sa reprise.

Le worker ne lit aucun contrôle WPF. Un état publié et une génération de présentation
invalident les images en attente. Un appel GPU déjà engagé peut finir ; masquer ne doit
pas attendre le GPU sur le thread UI. Aucun nouveau rendu masqué n'est engagé après
observation du nouvel état. Les notifications de statut/médias restent limitées par
le mécanisme existant, sans passage par les shaders.

Au retour, la dernière frame du module est envoyée, même sans nouvel événement en pause.
Les transitions de surface et les rappels asynchrones doivent ignorer une ancienne génération.

## Ressources et historique

La suspension ne dispose pas les surfaces, programmes, pipelines ou textures. Une remise
à zéro temporelle précède la première frame après suspension. OpenGL réutilise ResetHistory ;
Veldrid invalide ses indicateurs. Les pipelines de capture invalident aussi leurs historiques.
Le pipeline logiciel reçoit un contrat ResetHistory ; son worker réalise cette opération
sur son propre thread avant le prochain traitement et peut abandonner sa frame en attente.
Une frame logicielle déjà en calcul peut terminer, mais son résultat masqué est ignoré.

Les captures explicites d'une machine masquée utilisent la dernière frame et les réglages
courants par le traitement de capture existant, sans réactiver le rendu continu. Les changements
de shader restent appliqués ; une compilation réellement requise est différée jusqu'au retour.
Le code des moteurs et leurs boucles d'émulation ne sont pas modifiés.

## État réalisé et limites

Le présentateur applique maintenant cette suspension aux voies WPF et GPU. Le suivi
de VideoHost et de sa fenêtre évite le blocage par le masque de chargement des shaders.
Les historiques logiciel, entrelacé et GPU sont invalidés à la reprise, sans conserver
une image ancienne dans les effets. Les événements FramePresented restent utilisés pour
les statuts et médias des machines masquées, avec la limitation de fréquence existante ;
leur nom historique ne signifie donc pas qu'un dessin GPU a eu lieu.

Une opération déjà engagée peut terminer après le masquage ; les nouvelles images en
attente sont abandonnées. Le présentateur ne suspend ni l'audio, ni le cœur, ni la production
des frames du module. Le coût CPU correspondant et celui des notifications restent présents.
Cette visibilité WPF ne détecte pas une fenêtre simplement recouverte par une autre application.

Les tests vérifient la reprise sans nouvel événement vidéo, la pause simulée, la minimisation,
la concurrence du worker et le repli WPF. Les essais réalisés ont aussi contrôlé les trois rendus
natifs, leurs changements d'onglet et de fenêtre, et l'identité des pipelines conservés. Les mesures
utilisateur avec zéro à quatre machines confirment que la charge GPU de présentation disparaît quand
les surfaces sont masquées. L'analyse et la réduction éventuelle du CPU restant sont différées.
