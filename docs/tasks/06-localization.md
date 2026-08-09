# 6 — Réorganisation des traductions

## 6.1 Arborescence

- [ ] Créer une arborescence `Resources/Languages`.
- [ ] Créer un dossier par domaine fonctionnel afin d’éviter les fichiers de plus de mille lignes.
- [ ] Prévoir au minimum : `Common`, `Errors`, `Menus`, `Read`, `Write`, `Conversion`, `Visualizer`, `Explorer`, `Options`, `Hardware`, `Tools`, `Profiles`, `Logs`, `Formats` et `About`.
- [ ] Créer pour chaque domaine une ressource neutre et une ressource par langue distribuée.
- [ ] Garder dans `Common` uniquement ce qui est réellement commun et basique.
- [ ] Placer une erreur spécialisée dans le domaine qui la produit ou dans `Errors` si elle est réellement partagée.

## 6.2 Chargeur de ressources

- [ ] Adapter le chargeur à la nouvelle arborescence sans changer les clés inutilement.
- [ ] Conserver le repli culture exacte, culture parente, puis anglais/neutre.
- [ ] Refuser les clés dupliquées entre domaines.
- [ ] Tester le changement de langue immédiat dans les fenêtres déjà ouvertes.
- [ ] Conserver les noms natifs des langues dans le sélecteur.

## 6.3 Vérification

- [ ] Vérifier la parité des clés de toutes les langues.
- [ ] Vérifier les placeholders, retours à la ligne et valeurs vides.
- [ ] Détecter les corruptions d’encodage et le mojibake.
- [ ] Vérifier les langues de droite à gauche.
- [ ] Vérifier séparément les traductions de l’application et celles de l’installateur.
- [ ] Vérifier que les listes de formats des cinq fonctions utilisent les mêmes noms du catalogue central.
