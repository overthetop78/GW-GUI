# Ordre obligatoire des travaux

Les règles permanentes sont dans [rules.md](../rules.md). Le présent dossier ne contient que des tâches.

L’ordre ci-dessous reprend l’ordre demandé. Il ne doit pas être réorganisé sans décision explicite de l’utilisateur.

1. [Auditer tout le code](01-full-code-audit.md).
2. [Refactoriser et découper tout le code](02-full-refactoring.md).
3. [Centraliser constantes et textes techniques](03-constants-and-text.md).
4. [Structurer enums, modèles de données et contrats](04-models-and-contracts.md).
5. [Séparer les fonctions et services](05-functions-and-services.md).
6. [Réorganiser toutes les traductions](06-localization.md).
7. [Contrôler l’interface, la robustesse et la maintenance](07-ui-robustness-maintenance.md).
8. [Créer le workflow GitHub de build](08-github-build.md).
9. [Valider toutes les images puis le matériel réel](09-final-validation.md), toujours en dernier.

Chaque document contient ses propres tâches et sous-tâches. La fréquence des commits et des pushes suit exactement la section Git de [rules.md](../rules.md).
