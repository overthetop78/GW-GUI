# Architecture technique

## Composants

- **Application WPF/MVVM** : navigation, formulaires, validation et localisation.
- **Gestionnaire de commandes Greaseweazle** : construction typée des arguments, exécution asynchrone, capture de sortie, annulation et codes de retour.
- **Catalogue Greaseweazle** : commandes, options, profils de formats, extensions et compatibilités correspondant à la version détectée de `gw`.
- **Configuration persistante** : options générales, matériel, profils par onglet et préférences de session.
- **Moteur SCP** : lecture du conteneur, analyse des pistes/révolutions et décodeurs extensibles.
- **Rendu SkiaSharp** : deux faces circulaires, zoom, sélection et survol.

## Principes

- Aucun lancement via une console visible.
- Les arguments sont transmis comme une liste structurée afin de préserver correctement espaces, accents et guillemets.
- L’interface ne se bloque jamais pendant une commande.
- Une seule opération matérielle incompatible peut utiliser un contrôleur donné à la fois.
- La commande affichée correspond exactement aux arguments exécutés.
- Les options non activées ne sont pas émises.
