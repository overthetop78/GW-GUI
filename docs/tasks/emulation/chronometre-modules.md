# Chronomètre des quinze modules

Objectif : terminer les quinze modules du point 2, avec validation et commit de chacun. Référence : `sony-new-modules.md` et `liste-emulateur-libretro.md`.

Départ (heure locale du PC) : **2026-10-09 23:04:54**.
Budget demandé : **huit heures de travail**. Échéance initiale : **2026-10-10 07:04:54**.
Interruption exclue à la demande de l'utilisateur : **2026-10-10 02:01:19 à 09:52:09**, soit **07:50:50**. Reprise avec **05:03:34** restantes ; échéance recalculée : **2026-10-10 14:55:43**.

À chaque reprise de conversation pour cet objectif : récupérer l'heure locale réelle du PC, ajouter une ligne à la suite du journal et calculer le temps de travail écoulé et le temps restant en excluant les interruptions identifiées et autorisées par l'utilisateur. Conserver les timestamps réels ; ne pas remettre le chronomètre à zéro. Depuis la reprise à 09:52:09, ajouter la durée écoulée à 02:56:25 et la retrancher des 05:03:34 restantes. Le délai demandé ne constitue pas une preuve de réalisation ; cocher seulement le travail effectivement réalisé et vérifié.

| Heure locale du PC | Temps écoulé | Temps restant | État et prochaine action |
|---|---|---|---|
| 2026-10-09 23:04:54 | 00:00:00 | 08:00:00 | Départ. Sony commité ; Acorn en cours. Affectations spéciales BBC décrites ; poursuivre persistance, module, ressources, images et intégration avant validation et commit. Les quatorze autres modules restent à réaliser dans l'ordre du plan. |
| 2026-10-09 23:06:21 | 00:01:27 | 07:58:33 | Reprise. Persistance Acorn et affectations créées ; compléter les gestionnaires des paramètres et le module. |
| 2026-10-09 23:11:49 | 00:06:55 | 07:53:05 | Reprise. Paramètres matériels/audio et lecteur ssd/dsd décrits ; raccorder le module, compléter les options natives, puis ressources et intégration. |
| 2026-10-09 23:17:13 | 00:12:19 | 07:47:41 | Reprise. Module et factory créés, 24 affectations b2 couvertes. Préparer les ressources dans le découpage Atari, puis images et build. |
| 2026-10-09 23:28:02 | 00:23:08 | 07:36:52 | Reprise. Corriger le doublon d'affectations et retirer les ressources numérotées du clavier, puis poursuivre les images et l'intégration. |
| 2026-10-09 23:30:34 | 00:25:40 | 07:34:20 | Reprise. Doublons retirés et audit des ressources réussi ; sourcer les PNG des machines puis intégrer le projet aux builds. |
| 2026-10-09 23:33:12 | 00:28:18 | 07:31:42 | Reprise. Comparer les affectations Atari, Commodore et Amstrad ; vérifier que les choix par défaut Acorn restent modifiables et persistés. |
| 2026-10-09 23:34:44 | 00:29:50 | 07:30:10 | Reprise. Affectations personnalisées raccordées aux contrats communs ; reprendre les images et l'intégration Acorn. |
| 2026-10-09 23:41:14 | 00:36:20 | 07:23:40 | Reprise. Acorn compile sans erreur ; compléter les images puis vérifier les affectations et le runtime avant le build complet et le commit. |
| 2026-10-09 23:50:51 | 00:45:57 | 07:14:03 | Reprise. Dix tests Acorn réussis ; défauts de réaffectation corrigés, logo ajouté ; compléter les photos fidèles et valider le runtime. |
| 2026-10-09 23:56:52 | 00:51:58 | 07:08:02 | Reprise. Build complet réussi ; vérifier le runtime natif et sa fermeture, puis terminer les images et le commit Acorn. |
| 2026-10-10 00:02:28 | 00:57:34 | 07:02:26 | Reprise. Quinze essais Acorn réussis ; image BBC B ajoutée, finaliser Model A et audit avant nettoyage, build et commit. |
| 2026-10-10 00:10:13 | 01:05:19 | 06:54:40 | Reprise. Une seule ressource BBC commune ; onze tests autonomes et cinq essais réels validés. Terminer le build puis commiter Acorn avant Apple. |
| 2026-10-10 00:16:33 | 01:11:39 | 06:48:20 | Reprise. Acorn commité ; terminer le relevé natif AppleWin/Mini vMac puis créer le projet et les catalogues Apple. |
| 2026-10-10 00:21:10 | 01:16:16 | 06:43:43 | Reprise. Apple : options binaires et firmware avant création du module. |
| 2026-10-10 00:41:11 | 01:36:17 | 06:23:42 | Reprise. Apple : adaptateurs natifs, firmware, supports et état natif avant paramètres et validation. |
| 2026-10-10 00:46:31 | 01:41:37 | 06:18:22 | Reprise. Apple : exports, options et buffers natifs, puis callbacks propres aux deux cœurs. |
| 2026-10-10 00:49:04 | 01:44:10 | 06:15:49 | Reprise. Apple : callbacks et entrées natives. |
| 2026-10-10 00:52:22 | 01:47:28 | 06:12:31 | Reprise. Apple : supports de session et sauvegardes natives. |
| 2026-10-10 00:57:24 | 01:52:30 | 06:07:29 | Reprise. Apple : machines, engine et module SDK. |
| 2026-10-10 01:01:53 | 01:56:59 | 06:03:00 | Reprise. Apple : paramètres et stockage SDK. |
| 2026-10-10 01:08:47 | 02:03:53 | 05:56:06 | Reprise. Apple : affectations spécifiques et entrées SDK. |
| 2026-10-10 01:12:53 | 02:07:59 | 05:52:00 | Reprise. Apple : affectations clavier et contrôleurs. |
| 2026-10-10 01:21:39 | 02:16:45 | 05:43:14 | Reprise. Apple : images photographiques et intégration. |
| 2026-10-10 01:46:19 | 02:41:25 | 05:18:34 | Reprise. Apple : exécution native et arrêt vérifié. |
| 2026-10-10 01:48:59 | 02:44:05 | 05:15:54 | Reprise. Apple : validation native et fenêtre de paramètres. |
| 2026-10-10 01:52:49 | 02:47:55 | 05:12:04 | Reprise. Apple : photographies complètes et crédits. |
| 2026-10-10 01:56:44 | 02:51:50 | 05:08:09 | Reprise. Apple : icône et ressources incorporées. |
| 2026-10-10 01:59:36 | 02:54:42 | 05:05:17 | Reprise. Apple : source photographique complète du J-Plus. |
| 2026-10-10 02:01:19 | 02:56:25 | 05:03:34 | Reprise. Huit PNG Apple absents ; choix de détourage déterministe toujours en attente. |
| 2026-10-10 09:52:09 | 02:56:25 | 05:03:34 | Reprise. Détourage classique pris en charge ; PNG provisoires à revoir si nécessaire. |
| 2026-10-10 10:01:57 | 03:06:13 | 04:53:46 | Reprise. Bandai : contrats des trois cœurs natifs. |
| 2026-10-10 10:16:40 | 03:20:56 | 04:39:03 | Bandai : projet, modèles, contrats et persistance portable créés ; poursuite des adaptateurs natifs. |
| 2026-10-10 10:23:36 | 03:27:52 | 04:32:07 | Reprise. Bandai : hôte natif, supports, sauvegardes et arrêt. |
| 2026-10-10 10:36:59 | 03:41:15 | 04:18:44 | Reprise. Bandai : machines, affectations et états. |
| 2026-10-10 10:51:37 | 03:55:53 | 04:04:06 | Reprise. Bandai : raccordement du module ; PNG provisoires sans attente. |
| 2026-10-10 10:58:30 | 04:02:46 | 03:57:13 | Reprise. Bandai : informations machine et ressources. |
| 2026-10-10 11:07:40 | 04:11:56 | 03:48:03 | Reprise. Bandai : fin des traductions, informations machine et PNG. |
| 2026-10-10 11:22:41 | 04:26:57 | 03:33:01 | Reprise. Blocage de 02:01:19 à 09:52:09 exclu ; décompte repris avec 05:03:34 restantes. |
| 2026-10-10 11:24:01 | 04:28:17 | 03:31:41 | Reprise. Bandai : audit Argos réussi ; retrait des invariants dupliqués avant informations matérielles. |
| 2026-10-10 11:49:50 | 04:54:06 | 03:05:52 | Bandai : build complet, 13 tests et essais natifs réussis ; fermeture des DLL, processus et cartes vidéo vérifiée ; préparation du commit. |
| 2026-10-10 11:53:51 | 04:58:07 | 03:01:51 | Bandai commité (1499a40f7). Coleco : trois DLL et métadonnées officielles récupérées ; vérifier les contrats natifs et ADAM. |
| 2026-10-10 11:56:40 | 05:00:56 | 02:59:02 | Reprise. Décompte conservé depuis 09:52:09 avec 05:03:34 restantes ; période de blocage exclue. Coleco : contrats natifs à poursuivre. |
| 2026-10-10 12:07:35 | 05:11:51 | 02:48:07 | Bandai corrigé et vérifié dans la fenêtre réelle ; commit 4a0a5c185. Acorn : présentation corrigée, build en cours. |
| 2026-10-10 12:12:24 | 05:16:40 | 02:43:18 | Bandai et Acorn corrigés et commités ; fenêtres réelles vérifiées. Reprendre Coleco au projet et aux contrats. |
| 2026-10-10 12:13:13 | 05:17:29 | 02:42:29 | Reprise. Coleco : projet et manifeste créés ; contrats ColecoVision et ADAM en cours. |
| 2026-10-10 12:34:36 | 05:38:52 | 02:21:06 | Reprise. Révision des captures Bandai ; suppression globale des dossiers de Général et des mentions de redémarrage. |
| 2026-10-10 13:02:45 | 06:07:01 | 01:52:57 | Reprise. Suppression des choix de dossiers dans tous les modules, y compris Atari et Commodore ; validation finale. |
