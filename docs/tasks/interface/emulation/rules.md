# Règles communes de rédaction et de suivi

[Sommaire](../emulation-improvements.md)

## Règles de rédaction et de suivi des tâches

- Les groupes et les tâches sont toujours écrits et réalisés exactement dans leur ordre réel d’exécution.
- Une sous-tâche d’action est cochée uniquement après l’écriture, la création, la modification, la copie ou le déplacement demandé et sa vérification.
- Une tâche finalisée est cochée lorsque toutes ses sous-tâches sont cochées. Le même principe s’applique ensuite en remontant jusqu’au groupe général.
- Une lecture, une recherche ou une réflexion n’est jamais une tâche isolée : elle fait partie d’une action qui produit ou modifie dans la même sous-tâche un fichier identifié.
- Lorsqu’un fichier doit être créé, sa création précède toujours l’ajout de son contenu.
- Toute modification indique le fichier concerné avant de décrire les changements à y effectuer.
- Un déplacement de code commence par le déplacement ou la copie du code existant en conservant exactement son fonctionnement. La suppression de l’ancien emplacement intervient seulement après vérification du déplacement. Toute modification fonctionnelle éventuelle constitue une tâche ultérieure séparée.
- Aucun comportement n’est modifié, corrigé ou remplacé par préférence personnelle. Une correction non prévue n’est effectuée que si une erreur réelle est constatée.
- Ne jamais inventer et ne jamais extrapoler un comportement, une donnée, une dépendance, une solution ou une tâche.
- Ne jamais sauter une étape : chaque tâche et chaque sous-tâche est réalisée dans l’ordre écrit, uniquement lorsque toutes les étapes précédentes nécessaires sont réellement terminées, vérifiées et cochées.
- Ne passer à la tâche suivante qu’après avoir coché la tâche précédente réellement terminée. Si une tâche nécessaire a été oubliée pendant l’exécution, l’inscrire d’abord au bon endroit puis la réaliser avant de reprendre la suite.
- Lorsqu’une action potentiellement nécessaire n’est pas inscrite, lire d’abord les fichiers et le fonctionnement directement concernés afin de déterminer si elle est réellement indispensable et entièrement justifiée. Si elle l’est, ajouter la tâche correspondante au bon endroit dans l’ordre d’exécution, puis seulement effectuer cette action.
- Si cette vérification ne permet pas de trancher sans inventer, extrapoler ou choisir un comportement non validé, arrêter le travail et demander une décision avant toute modification.
- Lorsque plusieurs informations ou décisions sont nécessaires pour poursuivre, identifier toutes les questions réellement bloquantes et les poser ensemble afin de pouvoir compléter les tâches puis les exécuter sans interruptions évitables.
- Ne jamais casser le code : préserver le fonctionnement existant qui n’est pas explicitement concerné, vérifier chaque modification et corriger uniquement les régressions qu’elle provoque avant de poursuivre.
- Lorsqu’un changement nécessaire touche un système existant, l’améliorer sans le remplacer ni retirer son fonctionnement. Écrire auparavant toutes les tâches nécessaires après avoir relu les fichiers et le fonctionnement concernés ; tout remplacement explicitement nécessaire doit être décrit et validé avant son exécution.
- Toujours respecter l’ensemble des règles de rédaction, d’ordre, d’exécution, de vérification et de suivi des tâches, sans exception implicite.
- Avant toute modification, lire les fichiers directement concernés et uniquement les contrats, appels, dépendances, présentateurs ou contrôleurs pertinents pour la tâche, dans l’étendue nécessaire pour comprendre le fonctionnement réel et l’architecture utilisée, sans relire inutilement des fichiers inchangés déjà compris.
- Lire les tests existants lorsqu’une tâche demande de créer, modifier ou exécuter des tests, notamment pour vérifier qu’un fichier ou un scénario équivalent n’existe pas déjà ; ne pas parcourir des tests sans rapport avec l’action à réaliser.
- Ne jamais écrire, modifier ou extrapoler du code sans savoir comment la partie concernée de l’application fonctionne réellement. Si le fonctionnement ou l’architecture ne peut pas être établi avec certitude depuis le projet, arrêter le travail et demander une décision.
- Toujours respecter l’architecture existante du projet : placer les énumérations dans des fichiers d’énumérations sous le dossier Enums approprié, les constantes dans des fichiers de constantes sous le dossier Constants approprié et les fonctions dans des fichiers de fonctions sous le dossier Functions approprié.
- Lorsqu’une énumération, une constante ou une fonction peut être commune, l’écrire une seule fois pour l’usage commun et la placer dans la couche et le dossier communs correspondant à sa portée réelle, sans duplication locale.
- Ne laisser aucun nombre, texte ou autre valeur brute inexpliquée dans le code : toute valeur utilisée par le fonctionnement doit être portée par une constante nommée dans le fichier de constantes approprié.
- Ne laisser aucun texte visible directement dans le code. Tout texte affiché doit utiliser une ressource de localisation placée dans le fichier approprié, même lorsque sa valeur est identique dans toutes les langues ou qu’aucune variation de traduction n’est attendue.
- Lorsqu’un texte visible est ajouté ou modifié, créer ou modifier sa ressource dans la base appropriée puis dans tous les fichiers de langues pris en charge avant d’utiliser cette ressource dans l’interface.
- Les tests intermédiaires doivent être ciblés et rapides. Un petit test créé uniquement pour vérifier ponctuellement une action peut être retiré après cette vérification lorsqu’aucune tâche ne demande de le conserver.
- La création d’un test durable, plus large ou regroupant plusieurs vérifications doit toujours être prévue par une tâche écrite avant la création ou la modification de son fichier.
