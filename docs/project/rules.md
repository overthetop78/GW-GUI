# Règles permanentes du projet

Ce document contient des règles de travail. Ce ne sont pas des tâches et elles ne doivent pas apparaître sous forme de cases à cocher.

`.codex/config.toml` reste la source d’autorité pour l’exécution par Codex. Le présent document en
donne une version lisible avec les règles propres au projet. Une règle trouvée dans un autre document
ne devient applicable que si l’utilisateur demande explicitement de l’adopter. Les consignes
explicites de l’utilisateur restent prioritaires.

## Autorité sur les décisions

- L’utilisateur décide du périmètre, de l’ordre des travaux et du comportement du produit.
- Une proposition de l’assistant reste une proposition tant que l’utilisateur ne l’a pas validée.
- Ne pas extrapoler une demande, inventer une limitation, un format, une extension ou un comportement.
- En cas d’ambiguïté susceptible de modifier le résultat, demander avant de coder.
- Lorsqu’une fonction est demandée, la réaliser complètement dans le périmètre décidé, sans version volontairement réduite.

## Préparation et exécution des tâches

- Avant une réalisation, préparer une checklist hiérarchique : point principal, groupes, tâches puis
  actions concrètes. Le nombre de niveaux dépend du travail.
- Le dernier niveau décrit toujours une modification concrète de fichier : créer, modifier, déplacer,
  renommer ou supprimer, avec le chemin et le résultat attendu. Lire un fichier seul n’est pas une
  action terminale.
- Exécuter les actions une par une et dans l’ordre. Cocher une action seulement après sa réalisation,
  puis cocher ses parents uniquement lorsque tous leurs enfants sont terminés.
- Si une action nécessaire manque, l’ajouter avant de l’exécuter et réorganiser uniquement le travail
  restant. Les nouvelles actions suivent la dernière action déjà cochée.
- Lire les fichiers concernés et comprendre le fonctionnement existant avant toute modification.
  L’application étant fonctionnelle, ne pas inventer un défaut antérieur.
- Lorsqu’un problème apparaît après une modification, chercher d’abord dans cette modification et
  comparer avec le fonctionnement ou les fichiers antérieurs.
- Ne pas extrapoler hors de la demande. Si aucune solution ne respecte le périmètre après relecture
  des fichiers et de leurs versions, arrêter le travail concerné et demander la décision manquante.
- Poser dès le départ toute question indispensable lorsqu’une consigne est réellement ambiguë.

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

## Tests

- Ne pas ajouter de tests inutiles ou qui répètent seulement l’implémentation.
- Placer les tests nécessaires à la fin du groupe de tâches concerné ou du point complet.
- Conserver les nouveaux tests utiles et autonomes.
- Un test ajouté pour l’occasion qui crée des fichiers ou dépend de fichiers, applications ou DLL
  externes est supprimé après usage avec ses artefacts temporaires. Cette règle ne demande pas de
  supprimer les tests ou fichiers qui existaient avant le travail.

## Traductions

- Traiter toute nouvelle chaîne visible dans toutes les langues distribuées en utilisant les
  ressources communes existantes.
- Conserver dans la base commune les valeurs invariantes telles que CPU, GPU, RAM, ROM, noms de
  machines et formats, sans les recopier dans chaque culture.
- Utiliser Argos, déjà installé et fonctionnel, pour les traductions. Ne pas revérifier son
  installation et ne pas utiliser Google Traduction.

## Outils et build Debug

- Lorsqu’une commande échoue, vérifier et corriger d’abord son invocation. Les outils déclarés comme
  fonctionnels sont considérés disponibles.
- Si un outil nécessaire manque réellement, demander son installation au lieu de multiplier les
  contournements.
- Quand un build Debug est demandé, lancer `scripts/build.ps1 -Configuration Debug`. Si la stratégie
  PowerShell l’exige, employer
  `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Debug`.
- Vérifier la réussite du script et la présence de `build/Debug/GW GUI/gwgui.exe`, puis communiquer
  ce chemin pour le test utilisateur.

## Formats multiples et détection

- Une disquette ou une image peut contenir plusieurs systèmes ou formats reconnus.
- La détection ne doit pas écraser un résultat valide uniquement parce qu’un autre résultat a été trouvé en premier.
- Un choix manuel de machine ou de format ne doit pas être interprété automatiquement comme la preuve que les autres systèmes présents doivent être ignorés.
- Le comportement exact du choix manuel dépend de l’opération concernée et doit être conservé ou défini explicitement, pas déduit silencieusement.
- Si la détection automatique ne trouve rien de fiable, l’interface affiche un état vide ou `Aucun`, sans conserver le choix d’une image précédente.

## Documentation

- Un document encore globalement correct est corrigé directement.
- Un document devenu largement faux est réécrit ou remplacé par un document actuel propre. Son
  historique reste dans Git.
- Les documents sont découpés par sujet afin de rester lisibles et faciles à maintenir.
- Les règles, l’état réalisé, les décisions produit et les tâches restantes restent dans des documents distincts.
- Une feuille de tâches terminée est supprimée après transfert de ses résultats durables.

## Images de test

- `image_test/validated_images` contient uniquement les images dont le parcours demandé a été validé.
- Une image validée est déplacée, jamais copiée, vers `validated_images/<marque>/<modèle>/<type de disquette>/`.
- Une image déjà validée n’est pas retraitée.
- Les images générées sont elles aussi testées puis classées dans leur famille finale.
- Les fichiers parasites et dossiers sources devenus vides sont supprimés après déplacement des images validées.
- Le résultat d’une image est communiqué avant de passer à l’image suivante.
- Les essais physiques ne couvrent ni le nettoyage des têtes ni la mise à jour du firmware. Les fonctions restent présentes dans le logiciel.

## Git

- Aucun commit, push, tag ou autre écriture Git distante n’est effectué sans demande explicite de
  l’utilisateur.
- Une feuille de tâches peut décrire un futur commit, push ou tag sans l’autoriser. La case reste
  inactive jusqu’à la demande explicite correspondante.
- Une demande explicite de publication GitHub autorise les opérations Git nécessaires prévues par
  la procédure de cette publication.
- Lorsqu’un commit est demandé, son périmètre suit exactement la demande. `Commit tout` signifie
  commiter tout l’état courant du dépôt.

## Publications GitHub

### Application GW GUI

- Cette procédure commence uniquement après une demande explicite de publication.
- Préparer `.github/release-notes/vX.Y.Z.md` avec les nouveautés, améliorations, corrections,
  changements techniques utiles et le lien de comparaison avec la version précédente. Supprimer les
  rubriques vides.
- Employer le même `X.Y.Z`, pris dans le nom du fichier, dans les notes, le titre, le tag et les
  paquets.
- Commiter le code et les notes puis les pousser sur `main` avant de lancer
  `scripts/publish-release.cmd`.
- Répondre aux invites avec X pour Major, Y pour Minor et Z pour Revision.
- Choisir le type demandé : Latest pour une stable, Pre-release pour une snapshot ou aucun label. Si
  le type n’est pas donné, le demander avant la publication.

### Module d’émulation indépendant

- Cette procédure commence uniquement après une demande explicite de publication du module.
- Préparer `.github/release-notes/modules/<id>/vX.Y.Z.md` avec les changements propres au module.
- Prendre `X.Y.Z` dans `moduleVersion` du `module.json` concerné et employer le même identifiant et la
  même version dans les notes, le titre, le tag `module-<id>-vX.Y.Z` et les paquets.
- Commiter et pousser le code du module, son manifeste et ses notes sur `main`, puis créer et pousser
  le tag sur ce commit.
- Le tag déclenche `.github/workflows/module-release.yml`. Ne pas lancer avant cela son mode manuel,
  sauf demande explicite.

### SDK d’émulation

- Cette procédure commence uniquement après une demande explicite de publication du SDK.
- Vérifier sur NuGet.org la stratégie Trusted Publishing liée à `overthetop78/GW-GUI` et
  `sdk-release.yml`, sans environnement si le workflow n’en utilise pas. Vérifier que `NUGET_USER`
  contient le nom public NuGet.org. Ne créer ni stocker de clé API permanente.
- Préparer `.github/release-notes/sdk/vX.Y.Z.md` avec les changements de contrats et de compatibilité.
- Prendre `X.Y.Z` dans `Version` de `src/GWGUI.Emulation/GWGUI.Emulation.csproj` et l’employer dans
  les notes, le tag `sdk-vX.Y.Z` et le paquet `GWGUI.Emulation.SDK.X.Y.Z.nupkg`.
- Commiter et pousser le SDK, sa documentation et ses notes sur `main`, puis créer et pousser le tag
  pour déclencher `.github/workflows/sdk-release.yml`.
