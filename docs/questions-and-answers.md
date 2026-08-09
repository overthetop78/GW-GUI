# Questions et réponses

## Choix confirmés

### Pourquoi ne pas reprendre l’écran d’accueil de GreaseweazleGUI ?

Il est jugé trop fouillis : toutes les actions et les ports série sont mélangés. Les opérations fréquentes deviennent des onglets; le matériel se configure dans Options et reste mémorisé.

### Pourquoi C#/.NET, WPF et SkiaSharp ?

Ce socle offre une intégration Windows mature, une exécution fiable de processus sans console, une architecture testable et un rendu accéléré adapté au Visualisateur d’images de disquette.

### Les traductions utilisent-elles des fichiers `.lng` ?

Non. Les ressources natives `.resx` de .NET sont retenues. Elles sont séparées par domaine fonctionnel et par culture. Les écrans utilisent des clés et aucun texte d’une langue distribuée n’est codé directement dans la vue. Le français et l’anglais servent de références aux langues distribuées dans l’application et dans l’installateur.

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

SCP est une capture brute du flux. ADF, ST, IMG/IMA et les autres formats sectoriels décrivent une représentation connue. Dans une opération donnée, choisir AmigaDOS doit présenter les sorties Amiga compatibles au lieu d’une liste globale d’extensions inutiles. Ce filtrage d’interface ne signifie pas qu’une image multiformat ne contient qu’un système et ne doit pas supprimer les autres systèmes détectés.

### Que fait la numérotation automatique ?

Elle permet d’enchaîner des lectures `Disquette_01`, `Disquette_02`, etc., ou avec des lettres. Le compteur ne progresse qu’après succès et gère explicitement les conflits.

### Comment fonctionne la multiconversion ?

Il n’existe pas de mode distinct. Une sortie cochée effectue une conversion simple; plusieurs sorties cochées créent une file de conversions. Les sorties incompatibles avec la source sont désactivées.

### Pourquoi l’extension par défaut de Conversion n’est-elle pas cochée automatiquement ?

Une ligne cochée sans extension explicite utilise son extension par défaut. Cocher une extension signifie volontairement remplacer ce défaut ou demander plusieurs conteneurs. Cela évite de décocher systématiquement un choix imposé par l’interface.

### Pourquoi les diagnostics ne sont-ils pas dans Outils ?

Ils sont rarement nécessaires et n’ont pas besoin d’occuper la fenêtre principale. Ils s’ouvrent comme dialogues depuis Options → Diagnostics.

## Décisions désormais appliquées

- La commande et les journaux partagent un panneau inférieur réductible, intégré à la fenêtre principale.
- Les tags de conversion utilisent des identifiants stables (`PC-720`, `ST-720`, `AMIGA-DD`, etc.) et un modèle personnalisable contenant obligatoirement `{tag}`. Le modèle initial est ` [{tag}]`.
- Le menu Options contient les dialogues Diagnostics, Matériel, Mise à jour du firmware, historique des journaux et préférences générales.
- La fenêtre principale utilise les onglets Lecture, Écriture, Conversion, Visualisation, Explorateur et Outils. Effacement et nettoyage sont regroupés dans Outils; les diagnostics rares restent des dialogues.
- La matrice format ↔ extensions est portée par le catalogue de formats. La source détectée filtre les sorties réellement compatibles; une ligne cochée sans extension explicite utilise son extension implicite, tandis que les coches d’extensions la remplacent ou demandent plusieurs conteneurs.
- Les paramètres rarement utilisés sont placés dans des panneaux Avancé propres à chaque opération. Ils sont mémorisés, inclus dans les profils de l’onglet et réinitialisés en choisissant le profil système permanent Par défaut.

## Vérifications nécessitant encore des données réelles

- Valider les commandes avec plusieurs contrôleurs et lecteurs physiques.
- Vérifier les formats rares sur un corpus de captures libre et représentatif.
- Ajuster, si nécessaire, l’ordre des formats à partir de retours d’usage réels sans changer le fonctionnement retenu.
