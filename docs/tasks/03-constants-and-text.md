# 3 — Constantes et textes techniques

## 3.1 Constantes

- [x] Inventorier toutes les valeurs fixes trouvées pendant l’audit.
- [ ] Créer les fichiers de constantes nécessaires pour chaque domaine identifié par l’audit.
- [ ] Séparer notamment identifiants de machines, formats, codecs, protections, systèmes de fichiers, conteneurs, extensions, commandes `gw`, géométries et contrôles d’intégrité.
- [ ] Remplacer les chaînes et nombres recopiés par ces définitions.
- [ ] Documenter la source des valeurs techniques non évidentes.
- [x] Intégrer dans les données embarquées toute définition spéciale de disquette nécessaire au produit.

## 3.2 Aucun texte brut visible

- [x] Retirer tout libellé, message, infobulle, titre ou erreur visible écrit directement dans C# ou XAML.
- [x] Envoyer chaque texte utilisateur vers la ressource de traduction de son domaine.
- [ ] Garder les noms techniques non traduisibles dans un catalogue neutre commun.
- [ ] Vérifier que le même nom technique n’est pas recopié dans trente langues s’il doit rester identique.
- [x] Distinguer messages de journaux techniques et messages destinés à l’utilisateur.

## 3.3 Contrôles

- [ ] Ajouter un contrôle des identifiants utilisés mais absents des catalogues.
- [ ] Ajouter un contrôle des constantes dupliquées.
- [ ] Ajouter un contrôle des textes visibles codés en dur avec une liste blanche technique documentée.
