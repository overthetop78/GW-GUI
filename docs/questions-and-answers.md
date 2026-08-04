# Questions et réponses

## Choix confirmés

### Pourquoi ne pas reprendre l’écran d’accueil de GreaseweazleGUI ?

Il est jugé trop fouillis : toutes les actions et les ports série sont mélangés. Les opérations fréquentes deviennent des onglets; le matériel se configure dans Options et reste mémorisé.

### Pourquoi C#/.NET, WPF et SkiaSharp ?

Ce socle offre une intégration Windows mature, une exécution fiable de processus sans console, une architecture testable et un rendu accéléré adapté au visualiseur SCP.

### Les traductions utilisent-elles des fichiers `.lng` ?

Non. Les ressources natives `.resx` de .NET sont retenues. Les écrans utilisent des clés et aucun texte français ou anglais n’est codé directement dans la vue.

### Que fait le profil Par défaut ?

Il revient aux réglages natifs de `gw` sans option supplémentaire. Il est toujours présent.

### Les profils sont-ils globaux ?

Non. Un profil de Lecture ne peut pas être utilisé dans Écriture ou Conversion.

### Comment sont gérés les lecteurs multiples ?

Ils sont définis dans les Options et restent mémorisés. Un sélecteur n’apparaît dans une opération que si plusieurs lecteurs configurés rendent un choix nécessaire.

### Où se trouvent les diagnostics et commandes matérielles ?

Dans le menu Options, au sein de boîtes de dialogue dédiées. Ils ne prennent pas de place dans la fenêtre principale.

### Pourquoi le port COM n’est-il pas dans Lecture ?

Le contrôleur et ses lecteurs sont configurés durablement dans Options. L’opération utilise le lecteur actif; une liste n’est utile que si plusieurs lecteurs configurés exigent un choix.

### Pourquoi séparer SCP et formats connus ?

SCP est une capture brute du flux. ADF, ST, IMG/IMA et les autres formats sectoriels décrivent une représentation connue. Choisir AmigaDOS doit mener directement aux sorties Amiga compatibles, sans parcourir une liste globale d’extensions inutiles.

### Que fait la numérotation automatique ?

Elle permet d’enchaîner des lectures `Disquette_01`, `Disquette_02`, etc., ou avec des lettres. Le compteur ne progresse qu’après succès et gère explicitement les conflits.

### Comment fonctionne la multiconversion ?

Il n’existe pas de mode distinct. Une sortie cochée effectue une conversion simple; plusieurs sorties cochées créent une file de conversions. Les sorties incompatibles avec la source sont désactivées.

### Pourquoi l’extension par défaut de Conversion n’est-elle pas cochée automatiquement ?

Une ligne cochée sans extension explicite utilise son extension par défaut. Cocher une extension signifie volontairement remplacer ce défaut ou demander plusieurs conteneurs. Cela évite de décocher systématiquement un choix imposé par l’interface.

### Pourquoi les diagnostics ne sont-ils pas dans Outils ?

Ils sont rarement nécessaires et n’ont pas besoin d’occuper la fenêtre principale. Ils s’ouvrent comme dialogues depuis Options → Diagnostics.

## Points à décider

- Disposition exacte du panneau commande/journaux.
- Modèles et personnalisation des tags de conversion.
- Organisation finale du menu Options et emplacement exact de la mise à jour du firmware.
- Maquettes visuelles détaillées de chaque onglet.
- Libellés définitifs et modèles proposés pour les tags.
- Matrice vérifiée format de disquette ↔ extensions d’entrée/sortie.
- Organisation exacte des catégories dans les panneaux avancés.
