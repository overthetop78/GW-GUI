# Journal des décisions de conception

Ce journal conserve les questions posées, la réponse retenue et les précisions importantes. Il complète les spécifications par écran.

## Produit et technologie

| Question | Décision/réponse validée |
|---|---|
| Plateforme cible | Windows 10/11 x64. |
| Public | Débutants et experts; usage personnel principal, partage possible. |
| Référence fonctionnelle | GreaseweazleGUI v2.129 avec Host Tools 1.23, mais sans reprendre son interface confuse. |
| Étendue | Remplacer les fonctions du GUI existant et couvrir complètement `gw`; ajouter des fonctions utiles comme le visualiseur SCP. |
| Technologie | C#/.NET 10, WPF, MVVM et SkiaSharp après étude des alternatives. |
| Priorité technique | Fiabilité et réactivité avant performance théorique maximale ou multiplateforme. |
| Style | Moderne Windows 10/11, clair et aéré; intuitivité avant décoration. |
| Langues | Français et anglais via ressources `.resx` comme références initiales. Extension demandée à plusieurs langues pour le logiciel et l’installateur; langues supplémentaires à confirmer. |

## Fenêtre et exécution

| Question | Décision/réponse validée |
|---|---|
| Navigation principale | Onglets par fonctions utiles; pas de menu radio comme l’ancien GUI. Le regroupement exact découle de l’étude de chaque opération. |
| Commande générée | Visible, copiable, en lecture seule; champ séparé d’arguments experts. |
| Journaux | Intégrés à la fenêtre, jamais dans une console DOS externe; panneau inférieur redimensionnable et masquable. |
| Bouton principal | Exécuter devient Arrêter pendant la commande; confirmation avant interruption. |
| Barre d’état | Port COM, lecteur actif si nécessaire, profil actif, diode d’état avec infobulle, progression par face/piste. |
| Progression au repos | Masquée hors opération. |

## Matériel

| Question | Décision/réponse validée |
|---|---|
| Port COM dans Lecture | Refusé. Le matériel est configuré durablement dans Options. |
| Plusieurs matériels | Plusieurs Greaseweazle et lecteurs peuvent être enregistrés. |
| Lecteurs débranchés | Restent configurés et visibles comme indisponibles; pas de rescan obligatoire à chaque utilisation. |
| Identification | Mémoriser l’identifiant USB stable si disponible et le dernier port COM. |
| Nom libre du lecteur | Refusé. Le libellé est construit depuis des listes décrivant taille/type, contrôleur et sélection A/B ou 0/1. |
| Un seul lecteur | Pas de sélecteur inutile et pas de `--drive` explicite si le défaut `gw` suffit. |

## Profils

| Question | Décision/réponse validée |
|---|---|
| Portée | Chaque profil appartient exclusivement à son onglet/action. |
| Profil système | `Par défaut` / `Default`, toujours présent, non renommable, non supprimable, sans option facultative. |
| Enregistrement | Bouton/icône Sauvegarder puis dialogue demandant seulement le nom. |
| Nom déjà utilisé | Demander si le profil existant doit être remplacé. |
| Duplication | Aucun bouton dédié : enregistrer sous un autre nom crée la copie. |
| Gestion | Renommer et supprimer depuis une liste dans Options, classée par onglet. |
| Réinitialiser | Recharge le profil actif; avec Par défaut, remise à zéro complète. |

## Lecture

| Question | Décision/réponse validée |
|---|---|
| Choix du résultat | Deux parcours : capture brute SCP ou disquette au format connu. |
| Relation format/extension | Le format connu réduit automatiquement les types d’image compatibles; ne pas afficher une liste globale inutile. |
| Nature du SCP | Toujours une capture brute. Une vérification de format éventuelle ne change pas cette nature. |
| Contrôles toujours visibles | Profil, résultat, nom et dossier. |
| Options techniques | Panneau avancé déplié dans l’onglet. |
| Aide | Libellé humain et infobulle contenant l’option `gw`, son rôle et un exemple. |
| Option décochée | Argument retiré; valeur temporairement conservée. |
| Persistance | Restaurer la configuration au redémarrage mais vider le nom du fichier. |
| Dossier temporaire | Reste pendant la session; au redémarrage, retour au dossier par défaut défini dans Options. |
| Nom | Sans extension, modifiable et copiable. |
| Numérotation | Chiffres ou lettres, masques `0/00/000` et `A/AA/AAA`; après Z viennent AA, AB, etc. |
| Incrément | Seulement après lecture réussie. |
| Conflit de nom | Écraser, prendre le numéro suivant ou revenir modifier le nom. |

## Écriture

| Question | Décision/réponse validée |
|---|---|
| Organisation | Même logique que Lecture, mais choix d’un fichier source. |
| Détection | Déduire type et format du fichier; l’utilisateur peut modifier. Bloquer si le format est ambigu. |
| Confirmation | Résumé obligatoire avant chaque écriture. |
| Vérification | Active par défaut. `--no-verify` reste avancé et est signalé dans le réglage et le résumé. |
| Dossier | Pas de dossier Écriture séparé; utiliser le dossier courant partagé puis le défaut général au prochain lancement. |
| Profils | Mémorisent format imposé et options, jamais le fichier, le dossier ou le lecteur. |

## Conversion

| Question | Décision/réponse validée |
|---|---|
| Sources | Toutes les sources prises en charge par `gw convert`; SCP reste le cas d’usage central mais les autres ne sont pas supprimées. |
| Compatibilité | Une source n’affiche comme exécutables que ses conversions compatibles; les autres restent visibles mais désactivées avec explication. |
| Simple/multiple | Un seul panneau à cases; une case donne une conversion simple, plusieurs donnent une multiconversion. Aucun sélecteur global supplémentaire. |
| Formats courants/rares | Courants en haut; bouton pour étendre les rares. Toute ligne cochée est épinglée en haut. |
| Extensions | Chaque format montre ses extensions compatibles. Aucune extension cochée = extension implicite par défaut. |
| Extension explicite | Une extension cochée remplace le défaut; plusieurs cochées produisent plusieurs fichiers. Le défaut est indiqué en infobulle uniquement. |
| Nom | Reprend le nom de la source sans extension à chaque chargement; reste modifiable. |
| Tags | Option générale mémorisée et case dans Conversion. Le tag précède toujours le nom (`[PC-720] Disquette.ima`). Les Options proposent des modèles tels que `[{FAMILY}] `, `[{FORMAT}] ` et `[{FAMILY}-{FORMAT}] `, un champ libre utilisant les variables expliquées dans une légende à droite, un aperçu immédiat noir/vert parcourant plusieurs exemples et les cinq derniers modèles personnalisés en ordre MRU. Un modèle choisi remplace entièrement le champ; toute modification est sauvegardée automatiquement. Aucun tag n’est ajouté à Lecture pour le moment. |
| Échec dans une série | Continuer les autres conversions puis afficher un bilan. |
| Fichiers existants | Résumé avant lancement avec Écraser, Ignorer ou Numéroter, individuellement ou pour tous. |

## Visualiseur SCP

| Question | Décision/réponse validée |
|---|---|
| Référence | Vue Visual Floppy Disk de HxC montrant les deux faces circulaires et les pistes colorées. |
| Dépendances | Aucun autre GUI externe; lecteur, analyse et rendu intégrés. |
| Accès | Onglet dédié et proposition d’ouverture après une lecture SCP. |
| Apparence | Même principe fonctionnel que HxC mais interface modernisée, pas une reproduction fidèle de sa disposition. |
| Couverture | Architecture permettant de couvrir tous les analyseurs HxC, sans présenter le produit comme une version à moitié terminée. |

## Outils, diagnostics et matériel

| Question | Décision/réponse validée |
|---|---|
| Onglet Outils | Liste latérale; Effacer et Nettoyer les têtes y sont regroupés. |
| Profils Effacer/Nettoyer | Aucun profil. Valeurs de session puis réglages sûrs au redémarrage. |
| Confirmations | Effacer confirme la destruction; Nettoyer confirme la présence d’une disquette de nettoyage. |
| `info`, `bandwidth`, `rpm`, `seek` | `Options → Diagnostics`, dans des dialogues, pas dans la fenêtre principale. |
| `pin`, `reset`, `delays` | `Options → Matériel`, dans des dialogues. |
| `update` | `Options → Matériel → Firmware`. |
