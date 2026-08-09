# Audit du versioning

Ce document vérifie l’état réel des actions décrites dans [`docs/versioning.md`](../versioning.md). Il ne remplace pas la spécification de versioning.

## État constaté le 9 août 2026

| Élément attendu | État | Preuve actuelle |
|---|---|---|
| Version produit de l’application | Partiel | `GWGUI.App.csproj` fixe `0.1.0` par défaut. |
| Version commune aux quatre projets | Non réalisé | Aucun `Directory.Build.props`; Domain, Infrastructure et Scp ne partagent pas la propriété de l’application. |
| Révision numérique et hash calculés une seule fois | Non réalisé | `build.ps1` ne calcule aucune identité Git; `package.ps1` reçoit seulement `Version`. |
| Build distinct de la révision Git | Non réalisé | Aucun numéro de build séparé n’est produit ou transmis. |
| Absence de double hash informationnel | Non garanti | `IncludeSourceRevisionInInformationalVersion` n’est pas configuré et le script accepte une version arbitraire. |
| Noms ZIP/installateur issus de la même identité | Partiel | Les deux utilisent la valeur `Version`, mais sans build ni révision commune vérifiée. |
| Affichage version + build + révision dans À propos | Non réalisé | `AboutWindow` supprime tout ce qui suit `+`. |
| Copie des informations de diagnostic de version | Non réalisé | Aucun bouton ni service correspondant. |
| Contrôle des métadonnées EXE/DLL après publication | Non réalisé | Aucun contrôle dans `package.ps1` ni dans le workflow. |
| Échec si les binaires n’ont pas la même identité | Non réalisé | Aucun garde-fou de cohérence. |
| Marquage d’un arbre de travail `dirty` | Non réalisé | Les scripts n’interrogent pas l’état Git. |
| Test d’une compilation isolée par projet | Non réalisé | Le workflow restaure/teste la solution et package l’application seulement. |
| Comparaison des mises à jour par version produit | À confirmer lors de la phase concernée | La règle est documentée, mais aucun test dédié de l’identité complète n’existe. |
| Workflow de publication | Réalisé partiellement | `.github/workflows/release.yml` teste, package et publie sur tag. Ce n’est pas encore le workflow complet de build demandé en phase 8. |

## Tâches à conserver

Les actions non réalisées doivent rester dans la liste des tâches. Elles sont ajoutées à la phase 08, avec les contrôles de build GitHub, car elles forment un même bloc cohérent de fabrication et d’identification des binaires.

Le workflow `release.yml` existant ne doit pas être supprimé : il doit être audité puis adapté. Le fait qu’il existe ne valide pas les exigences de versioning manquantes.

