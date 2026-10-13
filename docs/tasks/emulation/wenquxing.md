# Wenquxing : validations restantes

Implémentation intégrée et commitée : NC1020, PC1000, CC800, NC2000, NC3000, WQXEmu, registre, build, images et traductions. Livraison Debug avec 23 modules réussie ; les cinq PNG embarqués et la navigation réelle dans les paramètres ont été vérifiés, avec destruction effective de la fenêtre.

Contrôles acquis sur la DLL officielle 20261011-041014 : cinq descriptions, dix réaffectations et effacements par modèle, persistance, clavier natif sans anciens alias, firmware manquant signalé, LCD 160×80, états natifs sauvegardés/restaurés, cinq sauvegardes .wqxs à l’arrêt, sources intactes, hôtes terminés et perte d’hôte signalée en Faulted. Les sources, images originales et fixtures temporaires ont été supprimées.

- [ ] Qualifier le firmware d’origine et le son audible des cinq modèles.
  - [ ] Exécuter une session réelle sur des copies de dumps disponibles.
    - [ ] Créer `artifacts/temp/wenquxing-firmware-validation/Validation.csproj` et `Program.cs` : utiliser les DLL livrées, les dumps d’origine localisés et les affectations persistées ; contrôler démarrage, réponse visible aux touches, son, état, reprise de session et arrêt effectif ; libérer tous les objets détenus en finally.
    - [ ] Créer `artifacts/temp/wenquxing-firmware-validation/result.txt` : résultat observé par modèle, sans assimiler le diagnostic rempli de zéros à un firmware fonctionnel.
  - [ ] Nettoyer les fixtures après contrôle.
    - [ ] Supprimer `artifacts/temp/wenquxing-firmware-validation` après sortie des hôtes et libération des fichiers détenus ; conserver les dumps d’origine.

Aucun dump de ces cinq modèles trouvé lors de la recherche ciblée dans F:/Retro/ROMS-BOOT. Le son audible et le démarrage du firmware d’origine restent non qualifiés ; le diagnostic rempli de zéros prouve seulement l’intégration native.
