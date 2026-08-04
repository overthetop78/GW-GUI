# Décisions produit et interface

## Produit

- Application Windows 10/11 x64 destinée d’abord à un usage personnel, partageable ensuite.
- Remplacement fonctionnel de GreaseweazleGUI v2.129 avec une interface plus claire et intuitive.
- Couverture complète des commandes de Greaseweazle Host Tools 1.23.
- Exécution de `gw.exe` sans fenêtre console externe.
- Commande générée et journaux visibles dans l’application.
- Interface française et anglaise.
- Ressources de traduction `.resx` : `Strings.resx`, `Strings.fr.resx` et `Strings.en.resx`.
- Tous les textes visibles passent par des clés de ressources; aucune chaîne française ou anglaise ne doit être écrite directement dans une vue.
- Le projet doit être complet : les capacités annoncées ne sont pas découpées en une « première version » volontairement incomplète.

## Socle technique

- C# et .NET 10.
- WPF avec architecture MVVM.
- SkiaSharp pour le visualiseur SCP et les rendus graphiques intensifs.
- Windows 10/11 x64 comme cible officielle.

## Navigation

- Onglets principaux pour les opérations fréquentes, notamment Lecture, Écriture, Conversion et Visualisation.
- Les opérations ne seront pas choisies dans une liste de boutons radio comme dans l’ancien GUI.
- Les diagnostics et réglages matériels sont accessibles par le menu Options et des boîtes de dialogue, pas dans la fenêtre principale.
- Les fonctions rarement utilisées ne doivent pas encombrer les opérations principales.

## Comportements communs

- Les paramètres techniques sont accessibles dans un panneau avancé dépliable dans l’onglet concerné.
- Une option décochée n’ajoute aucun argument à la commande `gw`.
- Les libellés sont compréhensibles et les infobulles indiquent l’argument `gw` correspondant.
- Le bouton Exécuter devient Arrêter pendant une commande; l’arrêt demande confirmation.
- Une barre d’état globale affiche le port COM, le lecteur actif si nécessaire, le profil actif et une diode d’état.
- Les barres de progression par face/piste sont masquées hors opération.
- Le dossier courant est partagé pendant la session. Après redémarrage, l'application revient au dossier d'images défini dans les Options.
- L’état utile des onglets est conservé lors d’un changement d’onglet.
- Les champs et listes doivent utiliser des noms compréhensibles (`Image brute (SCP)`, `Atari ST — 720 Kio`) plutôt que de simples extensions ou identifiants techniques.
- Les listes longues placent les choix courants en premier et rendent les choix rares accessibles sans recherche interminable.

## Profils

- Les profils sont propres à leur onglet et ne sont jamais partagés entre opérations.
- Chaque onglet utilisant des profils possède un profil système permanent `Par défaut` / `Default`.
- Le profil Par défaut ne peut être ni renommé, ni supprimé, ni remplacé.
- Le profil Par défaut désactive toutes les options facultatives et n’ajoute aucun argument optionnel à `gw`.
- L’enregistrement demande uniquement le nom du profil.
- Enregistrer sous un nouveau nom crée naturellement une copie.
- Les Options permettent de renommer et supprimer les profils utilisateur, classés par onglet.
- Il n’existe pas de bouton Dupliquer : charger un profil puis l’enregistrer sous un nouveau nom crée sa copie.
- Si le nom saisi existe déjà, une confirmation demande s’il faut remplacer le profil.
- Le bouton Réinitialiser recharge le profil actif. Avec `Par défaut`, il restaure complètement l’état sans options facultatives.

## Matériel

- Plusieurs contrôleurs Greaseweazle et plusieurs lecteurs peuvent être configurés et mémorisés.
- L’identifiant USB stable est mémorisé lorsqu’il est disponible, ainsi que le dernier port COM connu.
- Les lecteurs débranchés restent enregistrés et visibles dans la configuration.
- Avec un seul lecteur configuré, aucun sélecteur inutile n’est affiché dans les onglets.
- Les caractéristiques physiques servent au libellé et n'ajoutent pas automatiquement d'options `gw`.
- Les caractéristiques sont choisies dans des listes dans les Options; l’utilisateur ne saisit pas un nom convivial arbitraire.
- Si plusieurs lecteurs sont configurés, la sélection d’un lecteur devient disponible. Un lecteur débranché reste affiché comme indisponible.
- Si un seul lecteur est configuré, `gw` utilise son lecteur implicite et aucun `--drive` inutile n’est ajouté.
