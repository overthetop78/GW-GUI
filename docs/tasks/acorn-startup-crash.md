# Correction du plantage signalé après le build audio Acorn

- [x] Reproduire le défaut avec les fichiers effectivement utilisés.
  - [x] Créer `artifacts/acorn-startup-check/check.csproj` et `Program.cs` : charger en lecture seule les options de la configuration BBC enregistrée, démarrer le cœur livré avec ses ROM externes, avancer plusieurs images puis le libérer en finally ; ne modifier aucun réglage utilisateur.
  - [x] Modifier `artifacts/acorn-startup-check/Program.cs` et `check.csproj` : reproduire le démarrage et l’arrêt de la session complète avec la sortie audio livrée, annuler et attendre sa libération en finally ; conserver les réglages utilisateur en lecture seule.
  - [x] Modifier ce fichier avec le résultat et la cause vérifiée ; désigner les fichiers de correction avant leur modification.
- [x] Corriger et livrer après reproduction.
  - [x] Modifier ce fichier pour préciser les actions nécessaires dans les fichiers concernés, les contrôles de non-régression et le build Debug Acorn.
- [x] Nettoyer la validation temporaire.
  - [x] Supprimer `artifacts/acorn-startup-check/` et ses sorties après utilisation et consigner les résultats vérifiés dans ce document.

Éléments établis : GW GUI PID 93072 a été arrêté sur demande et sa terminaison vérifiée. Les trois erreurs du journal du 27 octobre à 07:31–07:32 sont des refus de démarrage causés par `acornemu.dfs` vide. Le fichier machine a ensuite été enregistré à 07:33:26 avec tous les lecteurs disquette/HDD désactivés. Aucun événement Windows de crash GW GUI n’a été trouvé dans les trois dernières heures. Ces éléments ne prouvent pas encore la cause du blocage signalé.


Premier contrôle : le cœur AcornEmu réellement livré initialise les ROM externes de la configuration actuelle puis exécute 50 images avec vidéo et audio, sans erreur. Il est libéré en finally. Le défaut signalé ne se reproduit donc pas dans ce parcours direct ; cela ne vérifie pas encore le démarrage de la session complète ni son utilisation dans App. La précision utilisateur sur le moment du blocage a été demandée.

Second contrôle : la session complète issue de la fabrique livrée démarre avec les options enregistrées et la vraie sortie WasapiAudioOutput ; état Running après 1,2 seconde ; StopAsync atteint Stopped ; DisposeAsync termine avec attente du thread. Pas de blocage dans ce parcours. Aucune correction de code ni modification de la configuration utilisateur : la reproduction du défaut dans App reste en attente de la précision utilisateur.


- [x] Reproduire l’allumage dans le contrôle WPF réel.
  - [x] Créer `artifacts/acorn-ui-startup-check/check.csproj` et `Program.cs` : utiliser les DLL livrées, le contrôle MachineController, sa commande Power et l’affichage Direct3D ; conserver la configuration utilisateur en lecture seule, journaliser les étapes et libérer contrôle, session, worker GPU, fenêtre et Dispatcher en finally.
  - [x] Modifier `artifacts/acorn-ui-startup-check/Program.cs` : employer le chargement de module et CreateRuntimeAsync de App, la configuration complète et le profil vidéo enregistrés ; vérifier que ce chemin ne réactive aucun lecteur déconnecté.
  - [x] Modifier ce suivi avec la reproduction et les fichiers du correctif avant modification.
  - [x] Supprimer `artifacts/acorn-ui-startup-check/` et ses sorties après validation.

Précision reçue : le blocage survient à l’allumage du BBC Model A. Les contrôles directs n’incluaient pas le contrôle WPF ni le rendu de la machine.

Le premier cycle Power du contrôle WPF réel avec Direct3D 11 a réussi : allumage, rendu, arrêt, libération du contrôle et fermeture de la fenêtre hors écran. Le parcours ne passait pas encore par le chargement de module et CreateRuntimeAsync de App.

Le second test WPF utilise EmulationModuleLoadContext, la fabrique du module, LoadConfigurationsAsync, CreateRuntimeAsync et le profil vidéo enregistrés, puis le vrai MachineController. Le runtime annonce un seul périphérique attaché (cassette), sans réactiver les disques désactivés. Power ON, rendu Direct3D 11, UI répondante, Power OFF et destruction complète ont tous réussi. Le comportement précis signalé (UI figée, dialogue, fermeture ou écran noir) a été demandé pour poursuivre la reproduction. Aucune correction logicielle n’est revendiquée.


- [x] Corriger le gel de l’interface pendant la préparation graphique et préciser le firmware manquant.
  - [x] Créer `artifacts/shader-startup-check/check.csproj` et `Program.cs` : charger aussi les DLL natives SPIR-V livrées, imposer que Direct3D reste actif sans repli WPF, redimensionner pendant le premier shader et vérifier les battements du Dispatcher ; libérer les ressources dans finally.
  - [x] Modifier `artifacts/shader-startup-check/Program.cs` : exercer la présentation DXGI dans une fenêtre visible sans activation et plusieurs dimensions pendant le premier shader, puis fermer cette fenêtre et attendre les workers.
  - [x] Modifier `artifacts/shader-startup-check/Program.cs` : mesurer la réponse du Dispatcher pendant qu’une opération graphique occupe le périphérique ; forcer un redimensionnement et laisser le worker libérer la porte même en cas d’échec.
  - [x] Modifier ce suivi avec la reproduction graphique et les fichiers précis du correctif avant modification.
  - [x] Modifier `src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs` : supprimer le redimensionnement GPU synchrone du callback UI OnRenderSizeChanged ; conserver ResizeSwapchainToClient dans PresentCore sur le worker graphique.
  - [x] Modifier `artifacts/shader-startup-check/Program.cs` : exiger un retour du Dispatcher inférieur au temps de contention GPU et vérifier première compilation, présentation Direct3D et arrêt sans repli WPF.
  - [x] Modifier ce suivi pour détailler les fichiers des messages de firmware manquant et leurs traductions existantes avant modification.
  - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Constants/FirmwareRomNames.cs`, modifier `Functions/FirmwareFunctions.cs` et `Common/Constants/CoreErrorResourceKeys.cs` : nommer la ROM manquante et utiliser l’exception firmware localisée existante au lieu de FileNotFoundException générique.
  - [x] Modifier les fichiers `src/GWGUI.Emulation.Acorn/Resources/*/Error.resx` : ajouter le diagnostic avec nom de ROM et instruction vers l’onglet ROM, en utilisant Argos pour les langues et les invariants comme arguments.
  - [x] Modifier les fichiers de livraison sous `build/Debug` avec `scripts/local-building.cmd --building=debug --modules=acorn` et contrôler le rendu graphique corrigé et les erreurs de ROM avec les DLL effectivement livrées.
  - [x] Modifier `artifacts/shader-startup-check/Program.cs` et `check.csproj` : vérifier le message français réellement présenté après échec de démarrage sans DFS, l’absence de demande DFS sans contrôleur, reconnaître les ROM disque utilisateur et démarrer avec DFS 0.9H sans modifier la configuration enregistrée.
  - [x] Supprimer `artifacts/shader-startup-check/` et ses artefacts après contrôle.

Symptôme confirmé : toute la fenêtre se fige à l’allumage et l’indicateur de préparation du shader reste bloqué. Le contrôle précédent ne vérifiait pas l’absence de repli WPF après erreur native. Dans VeldridVideoSurface, le callback UI OnRenderSizeChanged attend _deviceGate tandis que le worker tient cette même porte pendant création du périphérique, compilation et présentation ; la création DXGI peut communiquer avec le thread de fenêtre. Cette piste doit être reproduite avec les DLL natives réellement livrées.

Second défaut confirmé dans le journal : la ROM disque non renseignée est levée comme FileNotFoundException(acornemu.dfs), puis traduite en MachineStartFailed. L’utilisateur demande un diagnostic expliquant le besoin de DFS/ADFS au lieu de ce message générique.

Avec les DLL natives ajoutées au chemin comme dans le lanceur, le test reste effectivement en Direct3D 11, termine le shader et présente des images pendant les redimensionnements. Le test hors écran ne reproduit pas le blocage.

Reproduction de la contention : le callback de redimensionnement UI reste bloqué 1 928 ms quand une opération graphique possède _deviceGate pendant 1 800 ms. Ce verrou est détenu pendant EnsureDevice, compilation des shaders, présentation et redimensionnement. Le redimensionnement est déjà réalisé dans PresentCore ; son doublon synchrone sur le thread WPF n’est pas nécessaire et fige ce thread pendant le travail GPU. Le message DFS est corrigé par le mécanisme existant TranslateLocalized, sans modifier le contrat des messages ni leur présentation App.

Après correction et build, le même contrôle de contention GPU revient au Dispatcher en 116 ms au lieu de 1 928 ms. La compilation native du shader se termine, le rendu reste en Direct3D 11 sans repli WPF, des images sont présentées et les ressources sont libérées. Le build Debug Acorn s’est terminé avec succès.


## Résultat vérifié des corrections

- Le callback WPF de changement de taille n’attend plus la porte du périphérique graphique. Le worker conserve la mise à jour de la swapchain avant chaque présentation, y compris Direct3D 11 et Vulkan ; aucune logique Acorn n’est ajoutée au rendu commun.
- Mesure reproduite avec le même travail graphique occupant sa porte pendant 1 800 ms : avant correction le redimensionnement et le Dispatcher attendent 1 928 ms ; après correction 116 ms puis 107 ms. Les shaders natifs se compilent, le moteur reste Direct3D 11 (sans repli WPF), les images sont présentées et le cycle Power s’arrête normalement. Contrôle, worker, surface, fenêtre et Dispatcher sont libérés après le test.
- Le champ ROM absent ou pointant vers un fichier absent produit une exception firmware localisée portant le nom de ROM, conservée par le relais de messages existant. Le texte français réellement choisi par ControlErrorPresenter a été vérifié après échec complet de StartAsync : « La ROM DFS / ADFS est manquante. Sélectionnez son fichier dans l’onglet ROM des réglages de la machine. » Aucun nouveau contrat SDK ni affichage d’erreur App n’a été créé.
- Le diagnostic existe dans les 30 catalogues de ressources, avec {0} conservé ; Argos a produit les traductions, le français a été corrigé grammaticalement. Les noms MOS, BASIC, SAA5050, DFS / ADFS et Econet restent invariants dans le fichier de constantes de l’adaptateur.
- Le catalogue identifie les fichiers utilisateur dfs0.9h.rom, dfs10.rom, dnfs120-201666.rom et hope_dfs.rom comme systèmes disque partiellement compatibles avec le Model A équipé de l’extension nécessaire. Le démarrage a été testé avec le fichier réel DFS 0.9H et l’Intel 8271, sans modification du fichier de configuration utilisateur. Sans interface disque activée, aucune ROM optionnelle disque n’est demandée.
- Le script `scripts/local-building.cmd --building=debug --modules=acorn` a terminé avec succès. Les contrôles ont chargé les DLL de la livraison, pas un assemblage substitué. La livraison est `build/Debug/GW GUI/gwgui.exe`, avec `Modules/acorn/gwgui.emulation.acorn.dll`.

La contention UI est reproduite et corrigée ; le gel indéfini exact du premier essai utilisateur n’a pas été reproduit comme tel. Le build corrigé est fourni pour le nouvel essai. Les erreurs DFS du journal et leur diagnostic vague sont corrigés séparément de l’affichage.

- [x] Enregistrer le correctif vérifié.
  - [x] Modifier ce suivi après nettoyage et créer le commit des sources, des ressources et du diagnostic dans les objets Git du dépôt.

Le dossier de validation shader, ses DLL, copies audio et sorties a été supprimé après réussite.
