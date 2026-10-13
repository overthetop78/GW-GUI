# Wenquxing : validations restantes

Implémentation intégrée et commitée : NC1020, PC1000, CC800, NC2000, NC3000, WQXEmu, registre, build, images et traductions. Livraison Debug avec 23 modules réussie ; les cinq PNG embarqués et la navigation réelle dans les paramètres ont été vérifiés, avec destruction effective de la fenêtre. Les erreurs de DLL absente et d’exports absents ont aussi été exécutées via le lanceur livré : erreur retournée, processus terminé et transport libéré.

Contrôles acquis sur la DLL officielle 20261011-041014 : cinq descriptions, dix réaffectations et effacements par modèle, persistance, clavier natif sans anciens alias, firmware manquant signalé, LCD 160×80, états natifs sauvegardés/restaurés, cinq sauvegardes .wqxs à l’arrêt, sources intactes, hôtes terminés et perte d’hôte signalée en Faulted. Les sources, images originales et fixtures temporaires ont été supprimées.

Les douze dumps sont conservés dans F:/Retro/ROMS-BOOT/Wenquxing et ont été configurés directement. Après reconstruction Debug réussie des 23 modules, la DLL officielle 20261013-044006 produit un LCD 160×80, sauvegarde et restaure les états sur les cinq modèles. NC1020, PC1000 et CC800 affichent leur calculateur ; NC2000 reste sur la vérification système après 17 secondes ; NC3000 reste sur le dialogue de conservation des données flash. Seul CC800 a produit du PCM non nul ; aucune écoute physique ne permet encore de qualifier le son des cinq modèles. Tous les hôtes sont terminés ; Stop puis Dispose conservent Stopped et libèrent la sortie audio.

- [x] Nettoyer la qualification terminée.
  - [x] Supprimer `artifacts/temp/wenquxing-firmware-validation` : retirer uniquement la fixture temporaire et ses artefacts après confirmation de la sortie des processus détenus ; conserver les dumps de F:/Retro.

