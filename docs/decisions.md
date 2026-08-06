# Décisions produit et interface

## Produit

- Application Windows 10/11 x64 destinée d’abord à un usage personnel, partageable ensuite.
- Remplacement fonctionnel de GreaseweazleGUI v2.129 avec une interface plus claire et intuitive.
- Couverture complète des commandes de Greaseweazle Host Tools 1.23.
- Exécution de `gw.exe` sans fenêtre console externe.
- Commande générée et journaux visibles dans l’application.
- Interface multilingue. Le français et l’anglais restent les langues de référence; plusieurs langues supplémentaires doivent couvrir à la fois le logiciel et l’installateur.
- Ressources de traduction `.resx` : `Strings.resx`, `Strings.fr.resx`, `Strings.en.resx`, puis un catalogue complet par culture ajoutée.
- Au premier lancement, l’application utilise la langue d’interface de Windows si elle est prise en charge. Si elle ne l’est pas ou si sa détection échoue, l’application utilise l’anglais, langue de base et de repli.
- Une langue déjà choisie et enregistrée dans les Options est conservée. La langue de l’installateur est indépendante et ne force pas celle de l’application.
- Enregistrer une autre langue dans les Options l’applique immédiatement : la fenêtre principale est recréée automatiquement dans la culture choisie, sans demander à l’utilisateur de quitter ou relancer le programme. Les réglages courants sont sauvegardés avant ce rafraîchissement.
- Langues supplémentaires demandées : allemand, espagnol, italien, russe, chinois, japonais, portugais brésilien, néerlandais et polonais.
- Traduction et relecture réalisées avec ChatGPT/Codex, complétées par les tests de parité et la vérification dans l’interface; aucun relecteur externe n’est requis.
- L’interface fonctionnelle actuelle sera reprise écran par écran avec l’utilisateur et ne constitue pas une validation visuelle définitive.
- Tous les textes visibles passent par des clés de ressources; aucune chaîne française ou anglaise ne doit être écrite directement dans une vue.
- Le projet doit être complet : les capacités annoncées ne sont pas découpées en une « première version » volontairement incomplète.
- L’utilisateur décide du périmètre fonctionnel. Une fonction qu’il valide doit être réalisée complètement, pas réduite à un minimum provisoire.
- Les idées supplémentaires découvertes pendant l’étude sont présentées comme propositions et ne deviennent pas des décisions ou du travail autorisé sans validation explicite de l’utilisateur.
- L’architecture peut réserver une extension future, mais cette anticipation ne doit ni ajouter silencieusement une fonction ni modifier le comportement demandé.
- Aide → À propos contient des crédits et références transparents pour les dépendances, outils et sources techniques réellement utilisés ou étudiés, avec liens cliquables.

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

## Conflits de fichier en Lecture

- Aucun bouton générique `Oui`, `Non` ou `Annuler` n’est utilisé : le dialogue nomme directement les trois décisions `Écraser`, `Prendre le numéro suivant` et `Me laisser modifier le nom`.
- `Prendre le numéro suivant` active la numérotation si nécessaire et recherche le premier nom libre selon le mode chiffre ou lettre choisi.
- `Me laisser modifier le nom`, ainsi que la fermeture du dialogue, ramène le clavier dans le champ du nom sans lancer de commande.
- Le compteur n’avance définitivement qu’après une lecture réussie; une annulation ou un échec ne le consomme pas.

## Matériel

- Plusieurs contrôleurs Greaseweazle et plusieurs lecteurs peuvent être configurés et mémorisés.
- L’identifiant USB stable est mémorisé lorsqu’il est disponible, ainsi que le dernier port COM connu.
- Les lecteurs débranchés restent enregistrés et visibles dans la configuration.
- Avec un seul lecteur configuré, aucun sélecteur inutile n’est affiché dans les onglets.
- Les caractéristiques physiques servent au libellé et n'ajoutent pas automatiquement d'options `gw`.
- Les caractéristiques sont choisies dans des listes dans les Options; l’utilisateur ne saisit pas un nom convivial arbitraire.
- Si plusieurs lecteurs sont configurés, la sélection d’un lecteur devient disponible. Un lecteur débranché reste affiché comme indisponible.
- Si un seul lecteur est configuré, `gw` utilise son lecteur implicite et aucun `--drive` inutile n’est ajouté.
- La page Matériel affiche toujours les lecteurs configurés. Chaque lecteur occupe une ligne : numéro automatique non modifiable, taille, densité, RPM, enregistrement et oubli avec confirmation.
- Oublier le dernier lecteur d’un Greaseweazle oublie également ce contrôleur. Les autres lecteurs d’un même contrôleur sont conservés.
- Le sélecteur des onglets reste masqué lorsqu’un seul lecteur est utilisable. Les libellés visibles utilisent `Lecteur 1`, `Lecteur 2`, taille, densité et COM; A/B reste interne.
- `--device` n’est émis que si plusieurs Greaseweazle configurés et disponibles doivent être distingués. `--drive` n’est émis que si plusieurs lecteurs partagent le contrôleur sélectionné.
- Le démarrage vérifie automatiquement la présence des contrôleurs déjà configurés sans supprimer, remplacer ni reconfigurer leurs lecteurs.
- Si le même contrôleur est retrouvé sur un autre COM, seul son port courant est actualisé. Un contrôleur absent reste mémorisé comme déconnecté.
- En cas d’absence, le dialogue propose une nouvelle recherche, l’ouverture des paramètres du matériel ou la poursuite sans le lecteur. Les opérations nécessitant ce lecteur sont désactivées jusqu’à sa reconnexion.
- Un nouveau Greaseweazle détecté n’est jamais configuré automatiquement. L’application demande une seule fois si son lecteur doit être défini.
- En cas de refus, le contrôleur est conservé dans la liste « détecté, non configuré » et la question n’est pas reposée aux démarrages suivants. Il peut être configuré ultérieurement depuis les Options.
- La reconnexion d’un contrôleur configuré ne déclenche aucune nouvelle configuration : son lecteur et ses réglages existants sont réutilisés.
