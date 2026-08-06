# Architecture technique

## Composants

- **Application WPF/MVVM** : navigation, formulaires, validation et localisation.
- **Placement de fenêtre** : la restauration valide d’abord les coordonnées dans le bureau virtuel, puis interroge avec `MonitorFromWindow` la zone de travail du moniteur réellement choisi. Les coordonnées natives sont converties par WPF selon le DPI de ce moniteur avant le confinement final.
- **Gestionnaire de commandes Greaseweazle** : construction typée des arguments, exécution asynchrone, capture de sortie, annulation et codes de retour.
- **Gestionnaire Host Tools** : `IGwInstallationManager` couvre détection, recherche de version, installation contrôlée, sélection et retour arrière. Une instance est injectée depuis le point de composition dans `MainWindow`, puis partagée avec Options; les fenêtres ne portent plus les règles d’historique des exécutables.
- **Construction des commandes** : `IGwCommandBuilder` est le contrat unique consommé par les fenêtres et le registre matériel. Son implémentation délègue aux validateurs spécialisés Lecture, Écriture, Conversion, Maintenance et Diagnostics afin que la commande affichée soit exactement celle exécutée.
- **Catalogue Greaseweazle** : commandes, options, profils de formats, extensions et compatibilités correspondant à la version détectée de `gw`.
- **Configuration persistante** : options générales, matériel, profils par onglet et préférences de session.
- **Journal d’erreurs** : les exceptions UI, AppDomain, tâches non observées et échecs de sauvegarde interceptés sont consignés dans `Data/Logs/errors-AAAAMMJJ.log` avec contexte, version, environnement et pile complète. Une erreur de sauvegarde des Options est signalée mais n’empêche jamais leur fermeture.
- **Profils typés** : trois instances de `IProfileStore<OperationProfile>` sont chacune liées à Lecture, Écriture ou Conversion. Elles possèdent leur propre profil système immuable, refusent les profils d’un autre onglet et partagent uniquement la sérialisation JSON au chargement et à la sauvegarde.
- **Registre matériel** : `IHardwareRegistry` orchestre découverte série, interrogation `gw info`, identité USB stable et conservation des contrôleurs absents; l’infrastructure Windows fournit son implémentation.
- **Moteur SCP** : lecture du conteneur, analyse des pistes/révolutions et décodeurs extensibles.
- **Rendu SkiaSharp** : `IScpRenderer` et `SkiaScpRenderer` dessinent faces, pistes et structures sans dépendre du contrôle WPF; `ScpDiskView` gère zoom, sélection, panoramique et survol.

## Principes

- Aucun lancement via une console visible.
- Les arguments sont transmis comme une liste structurée afin de préserver correctement espaces, accents et guillemets.
- L’interface ne se bloque jamais pendant une commande.
- Une seule commande `gw` peut être active dans l’application : le runner journalisé créé au point de composition est partagé par Lecture, Écriture, Conversion, Maintenance, Diagnostics/Matériel et le scan des contrôleurs. L’ouverture d’un outil est refusée avec un message localisé pendant une commande active; le runner constitue le dernier verrou pour les autres chemins.
- La commande affichée correspond exactement aux arguments exécutés.
- Les options non activées ne sont pas émises.
