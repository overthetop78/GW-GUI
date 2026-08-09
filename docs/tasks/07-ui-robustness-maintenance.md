# 7 — Interface, robustesse et maintenance

Ces tâches viennent après le refactor principal et avant la validation finale des images.

## 7.1 Interface

- [ ] Vérifier chaque fenêtre à taille normale, réduite, maximisée et avec plusieurs DPI.
- [ ] Vérifier qu’aucun contrôle ne dépasse de son cadre et qu’aucun texte n’est tronqué.
- [ ] Vérifier les défilements : visibles uniquement lorsque nécessaires et fonctionnement correct à la molette.
- [ ] Vérifier l’indépendance des composants réutilisés entre onglets.
- [ ] Vérifier les onglets, focus, survols, icônes, cadres, alignements et états désactivés.
- [ ] Reprendre le thème sombre plus tard, sans le mélanger au refactor fonctionnel.
- [ ] Vérifier la restauration de position et taille avant l’affichage de la fenêtre.
- [ ] Vérifier les silhouettes de disquettes et leur correspondance avec le média détecté.
- [ ] Vérifier les sélecteurs automatiques et manuels Machine, Format et Protection.

## 7.2 Performance

- [ ] Mesurer séparément conteneur, décodage, reconstruction, système de fichiers et rendu.
- [ ] Supprimer les analyses répétées inutiles.
- [ ] Mettre en cache uniquement les résultats immuables réutilisables.
- [ ] Afficher progressivement les données lorsque le traitement est long.
- [ ] Vérifier changement rapide d’image, annulation et fermeture pendant une analyse.
- [ ] Vérifier spécialement la détection multiformat sur les images de flux sans ralentir les images sectorielles ciblées.

## 7.3 Erreurs, journaux et persistance

- [ ] Localiser tous les messages utilisateur et conserver le détail technique dans les journaux.
- [ ] Remplacer la boîte d’erreur d’un format simplement non reconnu par l’état d’interface prévu.
- [ ] Vérifier un journal par action, rotation, archivage et ouverture du dossier Logs.
- [ ] Vérifier nettoyage des fichiers temporaires et partiels.
- [ ] Vérifier les réglages absents, anciens, partiels ou corrompus.
- [ ] Ajouter et vérifier les migrations qui conservent les réglages existants.

## 7.4 Documentation et crédits

- [ ] Maintenir les documents actuels après chaque bloc terminé.
- [ ] Mettre à jour l’état du projet et la liste des formats réellement pris en charge.
- [ ] Maintenir les crédits, licences et liens des projets réellement utilisés ou étudiés.
