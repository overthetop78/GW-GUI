# Vérification des associations de manettes

- Définitions contrôlées dans les 14 modules présents : Acorn, Amstrad, Apple, Atari, Bandai, Coleco, Commodore, Epoch, Fairchild, Microsoft, NEC, Nintendo, Sega et Sony.
- Affectations initiales supprimées dans Coleco, Epoch et Fairchild. Les autres définitions de manettes utilisent déjà des valeurs vides.
- Transmission implicite supprimée pour les configurations sans associations dans Acorn, Amstrad, Apple, Atari, Bandai, Commodore, Microsoft, NEC, Nintendo, Sega et Sony. Coleco, Epoch et Fairchild résolvent désormais une source vide en absence de choix utilisateur.
- Les identifiants et indices natifs restent présents : ils désignent les commandes émulées et ne constituent pas des associations physiques. Les choix explicitement enregistrés restent appliqués.
- 29 contrôles ciblés Fairchild, Epoch, Coleco et NEC réussis. Le contrôle Fairchild active tous les boutons physiques et vérifie qu'aucune commande n'est produite sans association.
- Une série élargie a produit 70 réussites et un échec du test préexistant `InputMappingsResolveSelectedDeviceKeysAndAxisThresholds` : il attend le premier appareil lorsqu'un identifiant est introuvable, tandis que le résolveur SDK renvoie déjà un état vide dans HEAD. Ce comportement et ce test n'ont pas été modifiés pour cette demande.
- Build complet Debug A réussi ; exécutable final et 14 modules présents. Rendu réel Fairchild vérifié : associations vides, commandes assignables. Les essais natifs, états, remplacement de cartouche, deux rôles BIOS et fermeture des processus réussissent ; projet et fixtures temporaires supprimés.
