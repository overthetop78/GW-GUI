# Décisions produit et interface

## Produit

- Application Windows 10/11 x64 destinée d’abord à un usage personnel, partageable ensuite.
- Remplacement fonctionnel de GreaseweazleGUI v2.129 avec une interface plus claire et intuitive.
- Couverture complète des commandes de Greaseweazle Host Tools 1.23.
- Exécution de `gw.exe` sans fenêtre console externe.
- Commande générée et journaux visibles dans l’application.
- Interface multilingue. Le français et l’anglais restent les langues de référence; plusieurs langues supplémentaires doivent couvrir à la fois le logiciel et l’installateur.
- Ressources de traduction `.resx` séparées par fonction (`Common`, `Actions`, `Errors`, `Read`, `Write`, `Conversion`, `Visualizer`, `Explorer`, etc.). Chaque fonction possède une ressource neutre et une déclinaison par culture distribuée.
- Au premier lancement, l’application utilise la langue d’interface de Windows si elle est prise en charge. Si elle ne l’est pas ou si sa détection échoue, l’application utilise l’anglais, langue de base et de repli.
- Une langue déjà choisie et enregistrée dans les Options est conservée. La langue de l’installateur est indépendante et ne force pas celle de l’application.
- La langue s’applique dès sa sélection et est mémorisée sans utiliser le bouton Enregistrer. Tous les textes liés aux ressources sont actualisés sur place dans toutes les fenêtres déjà ouvertes : aucune fenêtre n’est fermée, recréée, déplacée ou rouverte.
- Chaque langue est toujours affichée sous son nom natif; seules les traductions réellement livrées sont proposées.
- Langues supplémentaires demandées : allemand, italien, espagnol, polonais, russe, japonais, chinois simplifié, chinois traditionnel, portugais, portugais brésilien, grec, coréen, néerlandais, tchèque, hongrois, turc, suédois, danois, norvégien, finnois, roumain, ukrainien, arabe, hébreu, thaï, indonésien et vietnamien.
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
- SkiaSharp pour le Visualisateur d’images de disquette et les rendus graphiques intensifs.
- Windows 10/11 x64 comme cible officielle.

## Navigation

- Onglets principaux pour Lecture, Écriture, Conversion, Visualisation, Explorateur et Outils.
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
- Les Options affichent trois listes de profils utilisateur côte à côte, sans le profil système Par défaut. Un second clic lent ou F2 renomme; le menu contextuel permet Renommer ou Supprimer, avec confirmation de la suppression.
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

## Explications et réponses confirmées

### Choix confirmés

#### Pourquoi ne pas reprendre l’écran d’accueil de GreaseweazleGUI ?

Il est jugé trop fouillis : toutes les actions et les ports série sont mélangés. Les opérations fréquentes deviennent des onglets; le matériel se configure dans Options et reste mémorisé.

#### Pourquoi C#/.NET, WPF et SkiaSharp ?

Ce socle offre une intégration Windows mature, une exécution fiable de processus sans console, une architecture testable et un rendu accéléré adapté au Visualisateur d’images de disquette.

#### Les traductions utilisent-elles des fichiers `.lng` ?

Non. Les ressources natives `.resx` de .NET sont retenues. Elles sont séparées par domaine fonctionnel et par culture. Les écrans utilisent des clés et aucun texte d’une langue distribuée n’est codé directement dans la vue. Le français et l’anglais servent de références aux langues distribuées dans l’application et dans l’installateur.

#### Que fait le profil Par défaut ?

Il revient aux réglages natifs de `gw` sans option supplémentaire. Il est toujours présent.

#### Les profils sont-ils globaux ?

Non. Un profil de Lecture ne peut pas être utilisé dans Écriture ou Conversion.

#### Comment sont gérés les lecteurs multiples ?

Ils sont définis dans les Options et restent mémorisés. Un sélecteur n’apparaît dans une opération que si plusieurs lecteurs configurés rendent un choix nécessaire.

#### Où se trouvent les diagnostics et commandes matérielles ?

Dans le menu Options, au sein de boîtes de dialogue dédiées. Ils ne prennent pas de place dans la fenêtre principale.

#### Pourquoi le port COM n’est-il pas dans Lecture ?

Le contrôleur et ses lecteurs sont configurés durablement dans Options. L’opération utilise le lecteur actif; une liste n’est utile que si plusieurs lecteurs configurés exigent un choix.

#### Pourquoi séparer SCP et formats connus ?

SCP est une capture brute du flux. ADF, ST, IMG/IMA et les autres formats sectoriels décrivent une représentation connue. Dans une opération donnée, choisir AmigaDOS doit présenter les sorties Amiga compatibles au lieu d’une liste globale d’extensions inutiles. Ce filtrage d’interface ne signifie pas qu’une image multiformat ne contient qu’un système et ne doit pas supprimer les autres systèmes détectés.

#### Que fait la numérotation automatique ?

Elle permet d’enchaîner des lectures `Disquette_01`, `Disquette_02`, etc., ou avec des lettres. Le compteur ne progresse qu’après succès et gère explicitement les conflits.

#### Comment fonctionne la multiconversion ?

Il n’existe pas de mode distinct. Une sortie cochée effectue une conversion simple; plusieurs sorties cochées créent une file de conversions. Les sorties incompatibles avec la source sont désactivées.

#### Pourquoi l’extension par défaut de Conversion n’est-elle pas cochée automatiquement ?

Une ligne cochée sans extension explicite utilise son extension par défaut. Cocher une extension signifie volontairement remplacer ce défaut ou demander plusieurs conteneurs. Cela évite de décocher systématiquement un choix imposé par l’interface.

#### Pourquoi les diagnostics ne sont-ils pas dans Outils ?

Ils sont rarement nécessaires et n’ont pas besoin d’occuper la fenêtre principale. Ils s’ouvrent comme dialogues depuis Options → Diagnostics.

### Décisions désormais appliquées

- La commande et les journaux partagent un panneau inférieur réductible, intégré à la fenêtre principale.
- Les tags de conversion utilisent des identifiants stables (`PC-720`, `ST-720`, `AMIGA-DD`, etc.) et un modèle personnalisable contenant obligatoirement `{tag}`. Le modèle initial est ` [{tag}]`.
- Le menu Options contient les dialogues Diagnostics, Matériel, Mise à jour du firmware, historique des journaux et préférences générales.
- La fenêtre principale utilise les onglets Lecture, Écriture, Conversion, Visualisation, Explorateur et Outils. Effacement et nettoyage sont regroupés dans Outils; les diagnostics rares restent des dialogues.
- La matrice format ↔ extensions est portée par le catalogue de formats. La source détectée filtre les sorties réellement compatibles; une ligne cochée sans extension explicite utilise son extension implicite, tandis que les coches d’extensions la remplacent ou demandent plusieurs conteneurs.
- Les paramètres rarement utilisés sont placés dans des panneaux Avancé propres à chaque opération. Ils sont mémorisés, inclus dans les profils de l’onglet et réinitialisés en choisissant le profil système permanent Par défaut.

### Vérifications nécessitant encore des données réelles

- Valider les commandes avec plusieurs contrôleurs et lecteurs physiques.
- Vérifier les formats rares sur un corpus de captures libre et représentatif.
- Ajuster, si nécessaire, l’ordre des formats à partir de retours d’usage réels sans changer le fonctionnement retenu.
