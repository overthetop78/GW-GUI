# Règles permanentes du projet

Ce document contient des règles de travail. Ce ne sont pas des tâches et elles ne doivent pas apparaître sous forme de cases à cocher.

## Autorité sur les décisions

- L’utilisateur décide du périmètre, de l’ordre des travaux et du comportement du produit.
- Une proposition de l’assistant reste une proposition tant que l’utilisateur ne l’a pas validée.
- Ne pas extrapoler une demande, inventer une limitation, un format, une extension ou un comportement.
- En cas d’ambiguïté susceptible de modifier le résultat, demander avant de coder.
- Lorsqu’une fonction est demandée, la réaliser complètement dans le périmètre décidé, sans version volontairement réduite.

## Modifications du code

- Préserver le comportement lors d’un refactor structurel.
- Ne jamais corriger un cas en ciblant le nom d’une image particulière. La correction doit concerner le format, le conteneur, la machine, le système de fichiers, l’encodage ou la protection.
- Ne pas masquer un problème en ajoutant une exception locale lorsque la responsabilité est mal placée.
- Éviter les duplications : ce qui est réellement commun doit être partagé et ce qui diffère réellement doit rester spécialisé.
- Ne pas remplacer mécaniquement un bloc de `if` par un bloc de `switch` équivalent. Le mécanisme retenu doit correspondre à la nature réellement fermée ou extensible du choix.
- Ne pas mutualiser des algorithmes dont le CRC, l’ordre des bits, la géométrie ou la structure diffèrent réellement.
- Ne pas utiliser des fichiers `partial` uniquement pour masquer la taille d’une classe sans séparer ses responsabilités.
- Aucun texte visible ne doit être écrit directement dans le code ou le XAML.
- Les identifiants techniques stables, extensions, géométries et valeurs fixes proviennent de catalogues ou constantes dédiés.
- Chaque nouvelle chaîne traduisible doit être ajoutée aux ressources de toutes les langues distribuées.
- Toute opération longue doit rester annulable et ne doit pas bloquer l’interface.
- Les fichiers temporaires ou partiels sont nettoyés après erreur ou annulation selon le comportement décidé.

## Formats multiples et détection

- Une disquette ou une image peut contenir plusieurs systèmes ou formats reconnus.
- La détection ne doit pas écraser un résultat valide uniquement parce qu’un autre résultat a été trouvé en premier.
- Un choix manuel de machine ou de format ne doit pas être interprété automatiquement comme la preuve que les autres systèmes présents doivent être ignorés.
- Le comportement exact du choix manuel dépend de l’opération concernée et doit être conservé ou défini explicitement, pas déduit silencieusement.
- Si la détection automatique ne trouve rien de fiable, l’interface affiche un état vide ou `Aucun`, sans conserver le choix d’une image précédente.

## Documentation

- Un document encore globalement correct est corrigé directement.
- Un document devenu largement faux est déplacé dans `docs/old`, puis remplacé par un document actuel propre.
- Les documents sont découpés par sujet afin de rester lisibles et faciles à maintenir.
- Les documents archivés servent uniquement d’historique et ne définissent plus l’état actuel.
- Les règles, l’état réalisé, les décisions produit et les tâches restantes restent dans des documents distincts.

## Images de test

- `image_test/validated_images` contient uniquement les images dont le parcours demandé a été validé.
- Une image validée est déplacée, jamais copiée, vers `validated_images/<marque>/<modèle>/<type de disquette>/`.
- Une image déjà validée n’est pas retraitée.
- Les images générées sont elles aussi testées puis classées dans leur famille finale.
- Les fichiers parasites et dossiers sources devenus vides sont supprimés après déplacement des images validées.
- Le résultat d’une image est communiqué avant de passer à l’image suivante.
- Les essais physiques ne couvrent ni le nettoyage des têtes ni la mise à jour du firmware. Les fonctions restent présentes dans le logiciel.

## Git

- Chaque tâche terminée reçoit toujours un commit, y compris une tâche documentaire, structurelle, de classement ou de validation.
- Un push est effectué lorsqu’une ou plusieurs tâches terminées forment un bloc complet et cohérent.
- Un bloc incomplet ne doit pas être poussé ni présenté comme terminé.
- Les commits de refactor pur et les changements fonctionnels sont séparés lorsqu’ils ne constituent pas la même tâche.
