# Étude — Acquisition adaptative de flux, récupération de pistes, consolidation SCP et écriture 48 TPI avec un lecteur 96 TPI

## 1. Objectif

GW-GUI doit pouvoir exploiter au maximum les capacités du Greaseweazle pour :

* capturer le flux magnétique brut d'une disquette ;
* analyser chaque piste indépendamment ;
* détecter les pistes correctement lues, douteuses ou mauvaises ;
* relire automatiquement uniquement les pistes problématiques ;
* augmenter progressivement le nombre de révolutions capturées ;
* conserver temporairement toutes les données utiles ;
* sélectionner les meilleures captures lorsqu'une sélection est pertinente ;
* préserver les variations volontaires comme les weak bits et certaines protections ;
* reconstruire un fichier SCP final cohérent ;
* permettre de reprendre une récupération interrompue après plantage ;
* afficher l'état courant de la disquette dans la visualisation de surface sans devoir générer continuellement un fichier SCP ;
* améliorer l'écriture de disquettes 48 TPI avec un lecteur 96 TPI ;
* vérifier automatiquement les écritures et éventuellement les recommencer.

Le principe général doit être :

**capture → analyse → récupération → consolidation → export**

et non :

**capture → fichier définitif immédiatement → bricolage du fichier → réécriture répétée**.

---

# 2. Ce que GW-GUI possède déjà

L'architecture actuelle est déjà très proche de ce qui est nécessaire.

## 2.1. Acquisition physique piste par piste

`PhysicalDiskFluxAcquisitionService` :

* reçoit une liste précise de pistes ;
* positionne physiquement le lecteur avec `DriveCylinder` et `DriveHead` ;
* capture directement le flux Greaseweazle ;
* accepte un nombre de révolutions configurable ;
* possède déjà des retries pour les erreurs de seek ;
* possède déjà des retries pour les dépassements de flux ;
* gère l'index physique ;
* gère un faux index ;
* gère les disquettes hard-sectored ;
* construit un `ScpTrack` au fur et à mesure ;
* construit finalement un `ScpImage`.

Il n'est donc pas nécessaire de créer une nouvelle couche matérielle.

Il faut surtout ajouter une couche d'orchestration de récupération au-dessus de l'acquisition existante.

---

## 2.2. Modèle SCP en mémoire

`ScpImage` contient :

* le header ;
* les pistes ;
* les révolutions de chaque piste ;
* l'état du checksum source ;
* la taille du fichier source.

Les collections sont actuellement exposées en lecture seule.

C'est une bonne chose pour représenter un **snapshot SCP cohérent**, mais ce n'est pas forcément le meilleur objet pour représenter une **session de récupération mutable**.

Il est préférable de conserver :

* un modèle de travail mutable spécifique ;
* puis de générer un nouveau `ScpImage` quand une représentation SCP cohérente est nécessaire.

---

## 2.3. Écriture SCP

Le `ScpWriter` actuel fait déjà exactement ce qu'il faut pour la génération finale :

* écriture vers un fichier temporaire ;
* génération de toutes les pistes ;
* reconstruction de la table des offsets ;
* reconstruction du header ;
* calcul du checksum global ;
* réécriture du checksum ;
* remplacement atomique du fichier destination.

Il ne faut donc **pas patcher manuellement un SCP existant**.

La stratégie doit rester :

**modifier le modèle en mémoire → régénérer le SCP entièrement avec `ScpWriter`**.

C'est plus fiable et beaucoup moins casse-couilles.

---

## 2.4. Contrainte importante du format SCP

Le nombre de révolutions est enregistré dans le header SCP.

Ton `ScpWriter` impose déjà correctement que chaque piste présente possède exactement le nombre de révolutions indiqué dans le header :

`track.Revolutions.Count == header.Revolutions`

Sinon l'écriture est refusée.

Cela implique qu'une session de récupération peut parfaitement avoir :

* piste 0 : 3 captures ;
* piste 1 : 3 captures ;
* piste 2 : 17 captures ;
* piste 3 : 8 captures ;

mais **un SCP final standard ne peut pas directement représenter cette asymétrie**.

Le format de travail interne doit donc être plus riche que le SCP.

---

# 3. Modèle de travail recommandé

Il faut distinguer trois niveaux.

## 3.1. Session de récupération

Objet de travail principal.

Il contient **tout ce qui a été capturé**, sans se limiter aux contraintes SCP.

Exemple conceptuel :

* Session

  * informations lecteur ;
  * informations Greaseweazle ;
  * paramètres de capture ;
  * géométrie ;
  * pistes ;
  * historique des opérations ;
  * état global.

Chaque piste possède :

* adresse logique ;
* adresse physique ;
* toutes les révolutions capturées ;
* résultats d'analyse ;
* statut ;
* nombre de tentatives ;
* révolutions retenues ;
* éventuelles anomalies ;
* éventuelle détection de protection.

---

## 3.2. Snapshot SCP

À tout moment, GW-GUI peut fabriquer un `ScpImage` temporaire à partir de la session.

Ce snapshot sert notamment à :

* afficher la surface ;
* utiliser les outils SCP existants ;
* prévisualiser ;
* exporter.

Il ne représente pas forcément toutes les données accumulées dans la session.

---

## 3.3. SCP final

Le SCP final est une **projection standardisée de la session**.

Il contient uniquement le nombre de révolutions retenu pour l'export.

Le SCP reste donc :

* portable ;
* compatible ;
* standard ;
* utilisable par les autres logiciels.

La session GW-GUI reste, elle, beaucoup plus riche.

---

# 4. Acquisition initiale

La première passe ne doit pas chercher immédiatement à obtenir une lecture parfaite de chaque piste.

Elle doit être rapide et suffisamment représentative.

Paramètres typiques :

* `InitialRevolutions = 2` ou `3`
* capture de toutes les pistes ;
* analyse immédiate après chaque piste.

Après cette passe, chaque piste reçoit un statut.

Par exemple :

| Statut     | Signification                                   |
| ---------- | ----------------------------------------------- |
| Good       | lecture satisfaisante                           |
| Suspect    | anomalies mais contenu probablement récupérable |
| Bad        | erreurs importantes                             |
| Missing    | aucune donnée exploitable                       |
| Unstable   | variations importantes entre révolutions        |
| Protected  | variations probablement volontaires             |
| Recovering | récupération supplémentaire en cours            |
| Abandoned  | limite de récupération atteinte                 |

Les noms exacts peuvent évidemment suivre les conventions du projet.

---

# 5. Relecture adaptative

C'est le cœur du système.

Une piste bonne ne doit pas être relue inutilement.

Une piste douteuse reçoit des révolutions supplémentaires.

Exemple :

* lecture initiale : 3 révolutions ;
* mauvaise piste : +2 ;
* nouvelle analyse ;
* toujours mauvaise : +2 ;
* etc.

Avec par exemple :

* `InitialRevolutions = 3`
* `AdditionalRevolutions = 2`
* `MaximumRevolutions = 15`

Le nombre maximal doit être une option utilisateur.

---

# 6. Pourquoi relire plusieurs révolutions

Une nouvelle révolution ne recrée évidemment pas des données qui n'existent plus.

Elle permet cependant de :

* obtenir une lecture correcte d'un secteur qui échoue occasionnellement ;
* contourner temporairement du bruit magnétique ;
* récupérer différentes parties valides sur différentes révolutions ;
* détecter une instabilité réelle ;
* distinguer une piste physiquement mauvaise d'une simple mauvaise lecture ;
* reconnaître certains comportements de protection.

Le Greaseweazle travaille justement sur les transitions de flux brutes, ce qui permet d'aller beaucoup plus loin qu'un contrôleur PC classique. Le projet Greaseweazle décrit SCP comme un format raw flux supportant plusieurs révolutions par piste. ([GitHub][1])

---

# 7. Arrêt anticipé

Il ne faut pas forcément atteindre `MaximumRevolutions`.

Exemple :

* 3 révolutions → mauvaise ;
* 5 → mauvaise ;
* 7 → toutes les données nécessaires deviennent cohérentes.

La récupération s'arrête à 7.

Le maximum représente seulement :

**la limite à partir de laquelle GW-GUI considère qu'insister n'apportera probablement plus suffisamment d'information.**

---

# 8. Analyse de qualité

Il faut éviter un simple booléen « bonne / mauvaise ».

Une piste peut avoir plusieurs indicateurs.

## 8.1. Analyse logique

Si le format est connu :

* nombre de secteurs attendus ;
* nombre de secteurs retrouvés ;
* CRC ;
* headers ;
* IDs de secteur ;
* secteurs dupliqués ;
* secteurs manquants ;
* incohérences de géométrie.

---

## 8.2. Analyse brute du flux

Indépendamment du format :

* régularité des timings ;
* transitions aberrantes ;
* intervalles exceptionnellement courts ou longs ;
* cohérence des périodes de révolution ;
* stabilité entre plusieurs lectures ;
* régions fortement variables ;
* densité de transitions.

---

## 8.3. Score de qualité

Il peut être pratique de fournir un score général, par exemple de 0 à 100.

Mais ce score ne doit pas devenir la seule source de décision.

Une piste protégée pourrait obtenir un mauvais score de stabilité tout en étant parfaitement correcte.

Il faut donc conserver plusieurs indicateurs séparés.

---

# 9. Révolutions « meilleures »

Pour une piste normale, les révolutions les plus propres vont généralement converger vers la même donnée.

Par exemple sur 10 captures :

* R1 : erreur CRC ;
* R2 : correcte ;
* R3 : correcte ;
* R4 : erreur ;
* R5 : correcte ;
* etc.

Il est alors raisonnable de sélectionner les meilleures.

Mais il ne faut **jamais fabriquer de nouvelles données par vote majoritaire aveugle**.

GW-GUI doit conserver une séparation claire entre :

* information réellement observée ;
* information reconstruite ou choisie.

---

# 10. Weak bits et protections

C'est une exception extrêmement importante.

Certaines protections reposent précisément sur :

* zones instables ;
* weak bits ;
* timings inhabituels ;
* variations entre révolutions ;
* données volontairement invalides.

Dans ce cas :

**les révolutions différentes ne sont pas nécessairement mauvaises.**

Au contraire, leur différence peut constituer l'information à préserver.

Il faut donc éviter une logique du genre :

« les trois révolutions les plus identiques sont forcément les meilleures ».

Un détecteur de variabilité volontaire doit pouvoir empêcher cette optimisation.

---

# 11. Sélection des révolutions pour le SCP final

Supposons :

* SCP final prévu : 3 révolutions ;
* piste 12 relue 15 fois.

GW-GUI possède alors 15 captures dans sa session.

Pour le SCP final, il peut retenir 3 révolutions.

Pour une piste normale :

* choisir les meilleures captures ;
* ou choisir les captures donnant la représentation la plus fiable.

Pour une piste protégée :

* conserver des révolutions représentatives de la variabilité ;
* ne pas éliminer artificiellement les weak bits.

---

# 12. Alternative : augmenter le nombre global de révolutions SCP

Il serait techniquement possible de produire un SCP avec davantage de révolutions globales.

Par exemple 10.

Mais toutes les pistes devraient alors posséder 10 révolutions.

Cela :

* augmente inutilement la taille du fichier ;
* force à relire ou dupliquer les données des pistes déjà correctes ;
* ne présente généralement aucun intérêt.

La session interne est donc une meilleure solution.

---

# 13. Fusion d'une piste relue

GW-GUI peut parfaitement :

* charger ou posséder un `ScpImage` initial ;
* relire seulement une piste physique ;
* analyser les nouvelles captures ;
* produire un nouvel état de cette piste ;
* reconstruire un `ScpImage` ;
* réexporter le fichier.

Il ne faut pas modifier directement les octets du SCP précédent.

Le writer actuel reconstruit déjà automatiquement :

* offsets ;
* descripteurs de révolution ;
* données de flux ;
* table des pistes ;
* checksum.

---

# 14. Checksum SCP

Le checksum n'est donc pas un problème fonctionnel pour la nouvelle fonctionnalité.

Il devient simplement invalide dès que le contenu change.

Mais `ScpWriter` le recalcule déjà.

Aucune logique spéciale n'est nécessaire dans la récupération.

La récupération ne travaille jamais sur le checksum.

Elle travaille sur les objets.

Le writer s'occupe de la sérialisation finale.

---

# 15. Affichage de la surface pendant la récupération

La visualisation actuelle est particulièrement adaptée à ce système.

`ScpDiskView` reçoit directement un `ScpImage` avec `SetImage()` puis demande au renderer de le préparer.

Il n'est donc pas nécessaire d'écrire un SCP sur disque pour mettre à jour la surface.

Le workflow peut être :

* capture piste ;
* mise à jour de la session ;
* création/actualisation du snapshot SCP ;
* mise à jour du renderer ;
* rafraîchissement du graphique.

---

# 16. Amélioration possible du rendu incrémental

Actuellement `SetImage()` :

* remplace l'image ;
* efface le cache du renderer ;
* réinitialise la vue.

Cela peut être gênant si une piste est remplacée toutes les quelques secondes pendant une récupération.

Une amélioration possible serait d'ajouter un mécanisme du genre :

* `UpdateTrack(track)`
* invalider uniquement le cache de cette piste ;
* conserver :

  * zoom ;
  * position ;
  * sélection ;
  * côté affiché.

Ainsi la surface pourrait évoluer en direct sans clignoter ni revenir constamment à la vue initiale.

---

# 17. Sauvegarde temporaire

Tout conserver uniquement en RAM serait dangereux.

Une récupération longue peut être perdue à cause de :

* plantage ;
* coupure ;
* déconnexion USB ;
* fermeture accidentelle ;
* reboot ;
* panne du lecteur.

Il faut donc ajouter une **session persistante récupérable**.

---

# 18. Format de session GW-GUI

Le format de session ne doit pas être limité par SCP.

Il peut conserver :

* toutes les révolutions capturées ;
* révolutions rejetées ;
* révolutions retenues ;
* scores ;
* analyses ;
* tentatives ;
* erreurs ;
* métadonnées matériel ;
* paramètres ;
* historique de récupération.

Exemple conceptuel :

* version du format de session ;
* date de création ;
* date dernière sauvegarde ;
* modèle Greaseweazle ;
* firmware ;
* fréquence d'échantillonnage ;
* lecteur sélectionné ;
* géométrie ;
* format supposé/détecté ;
* pistes ;
* captures par piste ;
* qualité ;
* statut ;
* sélection d'export.

---

# 19. Autosave

La session doit être sauvegardée régulièrement.

Par exemple :

* après chaque piste ;
* après chaque récupération supplémentaire ;
* après chaque modification importante.

Pas besoin d'attendre 30 secondes arbitrairement.

**Une piste terminée = un point de sauvegarde naturel.**

---

# 20. Écriture atomique de la session

Comme pour le SCP :

* écrire `.tmp` ;
* flush ;
* remplacer l'ancien fichier.

Ainsi un plantage pendant l'autosave ne détruit pas la session précédente.

---

# 21. Compression de la session

Les flux bruts peuvent prendre de la place.

La session peut donc éventuellement être compressée.

Mais la priorité doit être :

1. fiabilité ;
2. vitesse ;
3. récupération ;
4. compression.

Pas l'inverse.

---

# 22. Reprise après plantage

Au démarrage, GW-GUI peut détecter une session inachevée.

L'utilisateur peut choisir :

* reprendre ;
* ouvrir sans continuer ;
* abandonner ;
* supprimer la session.

Lors de la reprise, GW-GUI sait :

* quelles pistes sont terminées ;
* lesquelles sont mauvaises ;
* combien de révolutions ont déjà été capturées ;
* où reprendre.

---

# 23. Export intermédiaire

Même pendant une récupération non terminée, l'utilisateur peut vouloir sauvegarder un SCP.

Il doit pouvoir le faire.

Le fichier représente alors :

**l'état actuel de la récupération**.

La session reste ouverte et peut continuer ensuite.

C'est différent du concept de « SCP final ».

---

# 24. Écriture physique existante

GW-GUI possède déjà :

* écriture piste par piste ;
* seek ;
* émission directe des intervalles de flux ;
* précompensation ;
* vérification optionnelle après écriture.

Il y a donc déjà quasiment toute l'infrastructure requise pour une écriture plus intelligente.

---

# 25. Limite actuelle de la vérification d'écriture

Actuellement, si `Verify` échoue sur une piste :

* une erreur de vérification est ajoutée ;
* l'écriture s'arrête.

Une future stratégie peut améliorer ça.

Par exemple :

* écrire ;
* vérifier ;
* retry ;
* revérifier ;
* seulement ensuite abandonner ;
* éventuellement continuer sur la piste suivante selon les options.

---

# 26. Options d'écriture adaptative

Exemples d'options :

* `VerifyAfterWrite`
* `MaximumWriteRetries`
* `ContinueAfterWriteFailure`
* `VerificationRevolutions`
* `MinimumVerificationScore`

Les noms restent évidemment à adapter aux conventions GW-GUI.

---

# 27. Écriture 48 TPI avec un lecteur 96 TPI

Un lecteur 1,2 Mo 5¼" utilise généralement :

* 80 pistes ;
* 96 TPI ;
* une tête relativement étroite.

Un lecteur 360 Ko classique utilise :

* 40 pistes ;
* 48 TPI ;
* une tête plus large.

Pour lire une disquette 40 pistes dans un lecteur 80 pistes :

* piste logique 0 → cylindre physique 0 ;
* piste logique 1 → cylindre physique 2 ;
* piste logique 2 → cylindre physique 4 ;
* etc.

Cela fonctionne bien pour la lecture.

---

# 28. Problème lors de l'écriture 48 TPI avec une tête 96 TPI

La tête 96 TPI écrit une bande plus étroite.

Une ancienne piste écrite par une tête 48 TPI peut donc conserver des résidus magnétiques autour de la nouvelle piste.

Quand la disquette est ensuite lue par une vraie tête 48 TPI, cette tête plus large peut récupérer :

* la nouvelle piste ;
* mais également les anciennes informations restantes autour.

Cela peut provoquer :

* erreurs ;
* jitter ;
* CRC incorrects ;
* données parasites.

---

# 29. Nettoyage des pistes intermédiaires

Une méthode connue consiste à utiliser les positions intermédiaires du lecteur 96 TPI.

Disposition :

| Cylindre physique 96 TPI | Utilisation 48 TPI      |
| -----------------------: | ----------------------- |
|                        0 | données piste logique 0 |
|                        1 | zone nettoyée           |
|                        2 | données piste logique 1 |
|                        3 | zone nettoyée           |
|                        4 | données piste logique 2 |
|                        5 | zone nettoyée           |

L'objectif est de réduire les anciennes informations parasites entre les pistes utiles.

---

# 30. Attention : DC erase

Le meilleur nettoyage serait un **DC erase** :

* write gate actif ;
* aucune transition de flux envoyée.

Cela neutralise la zone sans y créer un nouveau motif exploitable.

Il existe précisément une demande de fonctionnalité Greaseweazle décrivant cette utilisation : effacer une piste sur deux lorsqu'un lecteur 96 TPI écrit une disquette destinée à un lecteur 48 TPI. Cette demande est toujours marquée ouverte dans le dépôt Greaseweazle. ([GitHub][2])

Donc il ne faut **pas supposer actuellement que le firmware Greaseweazle fournit directement la primitive DC erase exacte dont on aurait besoin**.

---

# 31. Différence entre « effacer » et DC erase

Greaseweazle dispose bien d'opérations d'effacement dans ses outils modernes, mais cela ne garantit pas que cette opération soit identique au DC erase spécifique décrit ci-dessus. Des utilisations récentes montrent bien une commande d'effacement piste par piste. ([GitHub][3])

Il faudra donc vérifier :

* ce que fait exactement la commande actuelle ;
* quel motif magnétique elle produit ;
* si elle suffit pour cet usage ;
* ou si une primitive spécifique doit être ajoutée.

---

# 32. Ne pas écrire simplement des zéros

Écrire un secteur rempli de `00` n'est **pas** un effacement magnétique.

Le codage FM/MFM produit toujours des transitions.

Il faut donc distinguer :

* données logiques égales à zéro ;
* absence de flux ;
* motif d'effacement.

---

# 33. Algorithme 48 TPI proposé

Pour chaque piste logique :

1. aller sur le cylindre physique `2 × piste` ;
2. écrire la piste ;
3. aller sur le cylindre physique voisin ;
4. nettoyer cette zone ;
5. relire la piste utile ;
6. analyser son flux ;
7. relire la piste nettoyée ;
8. mesurer les résidus ;
9. recommencer si les critères ne sont pas atteints ;
10. passer à la piste logique suivante.

---

# 34. Ordre écriture / nettoyage

Il faudra tester deux stratégies :

### Stratégie A

* écrire piste utile ;
* nettoyer piste voisine ;
* vérifier piste utile.

### Stratégie B

* nettoyer piste voisine ;
* écrire piste utile ;
* vérifier piste utile.

Le comportement réel dépendra de :

* largeur effective de la tête ;
* champ magnétique ;
* alignement mécanique ;
* lecteur.

Il faut donc rendre la stratégie testable plutôt que la figer immédiatement.

---

# 35. Vérification de la piste nettoyée

GW-GUI peut relire la piste intermédiaire après nettoyage.

Mais il faut être précis sur ce qu'il mesure.

Greaseweazle fournit les **timings des transitions de flux**.

Il ne fournit pas directement l'amplitude analogique du signal de la tête.

GW-GUI ne peut donc pas dire :

« le signal fait 14 % de puissance ».

Il peut en revanche mesurer :

* nombre de transitions résiduelles ;
* densité de transitions ;
* stabilité ;
* motifs répétitifs ;
* structures ressemblant à du FM/MFM ;
* quantité de flux interprétable.

---

# 36. Residual Flux Ratio

On peut définir un indicateur interne du type :

**Residual Flux Ratio**

Il ne représente pas l'amplitude physique.

Il représente la quantité de structure magnétique encore détectable par rapport à un seuil ou une référence.

Exemple utilisateur :

* `MaximumResidualFluxPercent = 2 %`

Si le score dépasse 2 % :

* refaire le nettoyage ;
* relire ;
* recalculer.

---

# 37. Attention au terme « bruit »

Le terme utilisateur « bruit maximum » est compréhensible.

Mais techniquement il serait préférable de distinguer :

* `ResidualFlux`
* `Instability`
* `UnexpectedTransitions`
* `DecodeNoise`

Cela évite de mélanger plusieurs phénomènes.

---

# 38. Retry du nettoyage

Options possibles :

* `MaximumEraseRetries`
* `MaximumResidualFluxPercent`
* `ResidualFluxVerificationRevolutions`

Exemple :

* nettoyer ;
* relire 2 révolutions ;
* score 8 % → retry ;
* score 4 % → retry ;
* score 1,7 % → accepté.

---

# 39. Écriture de deux pistes identiques côte à côte

Il serait techniquement possible d'écrire le même contenu sur :

* `2n`
* `2n+1`

Mais ce n'est pas la solution recommandée.

Une tête 48 TPI pourrait recevoir simultanément deux bandes fines dont :

* la phase n'est pas exactement identique ;
* les timings diffèrent légèrement ;
* l'alignement n'est pas parfait.

Le résultat peut être pire.

La stratégie à privilégier reste :

**une piste utile + une zone voisine nettoyée.**

---

# 40. Vérification après écriture

Une piste écrite doit pouvoir être immédiatement relue.

GW-GUI peut comparer :

* flux écrit ;
* flux relu ;
* données décodées ;
* CRC ;
* stabilité ;
* timings.

Puis appliquer :

* accepté ;
* retry ;
* échec définitif.

---

# 41. Nombre maximal de retries

Il faut absolument une limite.

Sinon une disquette physiquement morte peut provoquer une boucle infinie.

Exemples :

* `MaximumReadRetries = 5`
* `MaximumWriteRetries = 3`
* `MaximumEraseRetries = 3`

---

# 42. Détection de stagnation

Encore mieux qu'un simple compteur :

GW-GUI peut voir si les retries n'améliorent plus rien.

Exemple :

* tentative 1 : score 42 ;
* tentative 2 : 61 ;
* tentative 3 : 62 ;
* tentative 4 : 61.

On peut considérer que la récupération stagne.

Une option pourrait arrêter plus tôt.

---

# 43. Conservation des mauvaises captures

Les mauvaises captures ne doivent pas forcément être jetées immédiatement.

Une capture mauvaise peut contenir :

* un secteur absent des autres ;
* un weak bit intéressant ;
* une région utile ;
* une preuve d'instabilité.

La session doit donc pouvoir les conserver.

Le filtrage définitif intervient seulement lors de l'export ou lorsque l'utilisateur purge explicitement les données.

---

# 44. Format-aware et raw

Il faut séparer deux modes d'analyse.

## Raw

GW-GUI ne suppose rien concernant le format.

Il préserve intégralement le flux.

## Format-aware

GW-GUI connaît ou détecte :

* géométrie ;
* encoding ;
* secteurs ;
* CRC.

Cette connaissance peut aider à scorer les captures.

Mais elle ne doit **jamais modifier silencieusement le flux brut**.

Greaseweazle lui-même avertit qu'une lecture avec format sans mode raw peut décoder puis régénérer un flux « parfait », ce qui détruirait justement certaines informations de préservation. ([GitHub][1])

Pour GW-GUI, la règle devrait donc rester :

**capture brute d'abord, analyse ensuite.**

---

# 45. Informations à conserver par révolution

Chaque révolution capturée pourrait conserver :

* ID ;
* timestamp ;
* numéro de tentative ;
* index time ;
* intervalles de flux ;
* source ;
* score global ;
* score logique ;
* score de stabilité ;
* erreurs ;
* secteurs trouvés ;
* CRC ;
* indication sélectionnée/non sélectionnée ;
* raison du rejet ;
* éventuel flag `PotentialWeakBits`.

---

# 46. Informations à conserver par piste

Chaque piste pourrait conserver :

* cylindre logique ;
* tête logique ;
* cylindre physique ;
* tête physique ;
* ensemble des captures ;
* meilleures captures ;
* nombre de retries ;
* statut ;
* score ;
* anomalies ;
* protection potentielle ;
* commentaire technique ;
* date dernière modification.

---

# 47. État global de la session

Exemples :

* New
* Capturing
* Analysing
* Recovering
* Paused
* Completed
* PartiallyRecovered
* Cancelled
* Failed

---

# 48. Annulation

Une annulation utilisateur ne doit pas rendre la session inutilisable.

Après annulation :

* terminer proprement l'opération matérielle courante ;
* sauvegarder la session ;
* conserver toutes les pistes déjà récupérées ;
* permettre la reprise.

---

# 49. Déconnexion matérielle

En cas de perte du Greaseweazle :

* ne pas jeter la RAM ;
* sauvegarder immédiatement la session ;
* passer en état interrompu ;
* permettre une reconnexion.

---

# 50. Fichier SCP intermédiaire

Optionnellement, GW-GUI peut générer périodiquement un SCP de secours.

Mais ce n'est pas indispensable si le format de session est fiable.

Le SCP intermédiaire est surtout utile pour :

* ouvrir immédiatement la capture dans un autre logiciel ;
* obtenir une sauvegarde portable ;
* diagnostiquer GW-GUI.

---

# 51. Session vs SCP

| Fonction                              | Session GW-GUI |      SCP |
| ------------------------------------- | -------------: | -------: |
| Nombre variable de captures par piste |              ✅ |        ❌ |
| Captures rejetées                     |              ✅ |        ❌ |
| Scores                                |              ✅ |        ❌ |
| Historique                            |              ✅ |        ❌ |
| Reprise de récupération               |              ✅ |        ❌ |
| Standard externe                      |              ❌ |        ✅ |
| Compatibilité autres outils           |              ❌ |        ✅ |
| Flux brut                             |              ✅ |        ✅ |
| Export final                          |         source | résultat |

---

# 52. Interface utilisateur

Dans l'onglet lecture, il pourrait y avoir une section :

## Récupération avancée

Options :

* Activer la récupération adaptative
* Révolutions initiales
* Révolutions supplémentaires par tentative
* Maximum de révolutions
* Maximum de retries
* Arrêter automatiquement si aucune amélioration
* Conserver toutes les captures
* Autosave de session
* Export SCP automatique en fin de capture

---

# 53. Affichage par piste

Le graphique de surface pourrait utiliser des états visuels :

* bonne ;
* douteuse ;
* mauvaise ;
* récupération ;
* abandonnée ;
* protection probable.

En cliquant sur une piste :

* nombre de captures ;
* nombre de révolutions ;
* scores ;
* anomalies ;
* historique ;
* possibilité de relire manuellement.

---

# 54. Relecture manuelle

L'utilisateur doit pouvoir sélectionner une piste et demander :

* +1 révolution ;
* +3 ;
* +5 ;
* jusqu'à X ;
* relire complètement ;
* réanalyse.

Cela permettra aussi de tester les algorithmes sans relancer toute la disquette.

---

# 55. Export manuel d'une nouvelle version SCP

Une session ouverte peut générer plusieurs SCP :

* `disk-v1.scp`
* récupération supplémentaire ;
* `disk-v2.scp`

La session reste la même.

---

# 56. Qualité et vérité des données

Principe important :

**GW-GUI ne doit jamais inventer une donnée silencieusement.**

Il peut :

* choisir ;
* classer ;
* comparer ;
* fusionner des observations si l'opération est explicitement définie ;
* signaler une reconstruction.

Il ne doit pas présenter une valeur reconstruite comme directement capturée.

---

# 57. Protection de l'original

Lors d'une opération de préservation :

la lecture adaptative ne modifie évidemment jamais la disquette.

Les fonctionnalités d'effacement / écriture 48 TPI doivent être totalement séparées du workflow de lecture.

Aucune ambiguïté d'interface ne doit permettre de lancer un write depuis une opération de récupération.

---

# 58. Refactor conseillé de l'acquisition

Actuellement `AcquireAsync()` :

* ouvre le périphérique ;
* sélectionne le lecteur ;
* démarre le moteur ;
* lit toutes les pistes ;
* ferme le périphérique.

Pour une récupération adaptative, il serait plus propre de pouvoir conserver une session matérielle ouverte.

Par exemple conceptuellement :

* Open
* Select
* MotorOn
* CaptureTrack
* Analyse
* CaptureTrackAgain
* CaptureNextTrack
* MotorOff
* Close

Sinon relire individuellement une piste en rappelant tout le service provoquerait :

* ouverture USB ;
* sélection ;
* spin-up ;
* lecture ;
* fermeture ;

à chaque fois.

Ce serait inutilement lent.

---

# 59. Refactor possible

Séparer :

* gestion de session matérielle ;
* capture d'une piste ;
* orchestration d'une disquette.

Par exemple conceptuellement :

* `PhysicalDiskSession`
* `PhysicalTrackCaptureService`
* `PhysicalDiskRecoveryService`

Sans obligation de reprendre ces noms.

---

# 60. BuildTrack actuel

L'acquisition construit déjà un `ScpTrack` immédiatement après la capture et le transmet dans le progress report.

C'est très intéressant pour la récupération dynamique.

Le pipeline peut donc devenir :

**CaptureTrack → BuildTrack → Analyse → décision → capture supplémentaire éventuelle**

avant de passer à la suivante.

---

# 61. Stratégie de récupération recommandée

Pour une première implémentation :

### Phase 1

* 3 révolutions par piste ;
* analyse simple ;
* retry automatique uniquement si secteur/CRC incorrect ;
* maximum 10 révolutions ;
* conservation de toutes les captures en session ;
* export 3 meilleures révolutions.

### Phase 2

Ajouter :

* analyse brute ;
* détection de stagnation ;
* weak bits ;
* protection.

### Phase 3

Ajouter :

* écriture 48 TPI assistée ;
* nettoyage pistes voisines ;
* contrôle du flux résiduel.

Cela évite de transformer une fonctionnalité simple en usine nucléaire dès la première PR. 😏

---

# 62. Tests nécessaires

## Lecture

* piste parfaite ;
* une mauvaise révolution ;
* secteur lisible une fois sur cinq ;
* piste complètement morte ;
* index instable ;
* fake index ;
* hard sectors ;
* weak bits ;
* protection connue.

## SCP

* reconstruction d'une seule piste ;
* checksum ;
* offsets ;
* piste manquante ;
* pistes non contiguës ;
* nombre de révolutions incorrect ;
* annulation pendant écriture.

## Session

* save ;
* reload ;
* crash simulé ;
* session tronquée ;
* reprise ;
* version de format inconnue.

## Écriture

* verification OK ;
* retry ;
* échec permanent ;
* write protected ;
* déconnexion ;
* précompensation.

## 48 TPI

Tester avec :

* vraie disquette 360K ;
* lecteur 96 TPI ;
* vraie lecture ultérieure dans un lecteur 48 TPI.

C'est ce dernier test qui dira si l'algorithme apporte réellement quelque chose.

---

# 63. Métriques intéressantes pour les tests 48 TPI

Comparer :

### Écriture 96 TPI normale

avec

### Écriture 96 TPI + nettoyage intermédiaire

Puis lire les deux dans :

* lecteur 96 TPI ;
* lecteur 48 TPI.

Mesurer :

* secteurs corrects ;
* CRC ;
* retries ;
* jitter ;
* résidus sur pistes intermédiaires.

---

# 64. Lecteurs matériels concernés

Pour le 3 pouces, l'objectif discuté est un lecteur polyvalent de type :

* EME-231 ;
* EME-232 ;

car ils permettent notamment :

* 80 pistes ;
* double face ;
* DD ;

et peuvent donc couvrir davantage de médias qu'un EME-155/156 limité.

Pour le 5¼", l'objectif est plutôt :

* lecteur 80 pistes ;
* 96 TPI ;
* double face ;
* DD/HD ;
* suffisamment configurable ;
* compatible Greaseweazle.

La fonctionnalité 48 TPI assistée vise précisément à améliorer l'utilisation d'un tel lecteur comme lecteur unique.

Elle ne supprimera cependant jamais totalement la différence physique entre :

* une vraie tête 48 TPI ;
* une tête 96 TPI.

---

# 65. Limites physiques impossibles à corriger logiciellement

Même avec une excellente analyse de flux, GW-GUI ne peut pas :

* agrandir physiquement une tête ;
* mesurer directement l'amplitude analogique du champ magnétique via Greaseweazle ;
* restaurer une information magnétique réellement détruite ;
* garantir qu'une piste 96 TPI écrite sera identique à une piste créée avec une tête 48 TPI ;
* supprimer toutes les différences mécaniques entre lecteurs.

Le logiciel peut en revanche :

* mieux exploiter le matériel ;
* détecter les problèmes ;
* recommencer ;
* sélectionner les meilleures observations ;
* nettoyer des zones parasites ;
* vérifier les résultats.

---

# 66. Décisions d'architecture recommandées

À retenir :

1. **Le SCP n'est pas le modèle de travail.**
2. **La session de récupération est le modèle de travail.**
3. **Toutes les captures intéressantes restent disponibles dans la session.**
4. **Le `ScpImage` est un snapshot cohérent.**
5. **Le `ScpWriter` reste l'unique responsable de la sérialisation SCP.**
6. **Le SCP final est généré à la demande.**
7. **La surface travaille directement depuis le snapshot mémoire.**
8. **Les mauvaises pistes sont relues indépendamment.**
9. **Le nombre de révolutions de récupération peut varier par piste.**
10. **Le nombre de révolutions exportées en SCP reste global.**
11. **Les weak bits ne doivent pas être supprimés comme du bruit.**
12. **Aucune donnée ne doit être inventée silencieusement.**
13. **L'autosave doit permettre une reprise après crash.**
14. **La vérification d'écriture doit pouvoir retry plutôt que s'arrêter immédiatement.**
15. **Le mode 48 TPI doit être une stratégie d'écriture optionnelle.**
16. **Le nettoyage intermédiaire doit être vérifié expérimentalement.**
17. **Le flux résiduel est mesurable ; l'amplitude analogique ne l'est pas directement.**
18. **Le vrai DC erase doit être distingué d'un simple formatage ou d'une écriture de zéros.**

---

# 67. Priorité d'implémentation

Je ferais les travaux dans cet ordre :

### Étape 1 — Session de récupération

Créer le modèle de session mutable.

### Étape 2 — Acquisition adaptative

Relire automatiquement une piste selon sa qualité.

### Étape 3 — Autosave / reprise

Rendre la récupération résistante aux crashs.

### Étape 4 — Snapshot SCP

Générer un `ScpImage` depuis la session.

### Étape 5 — Surface dynamique

Actualiser les pistes sans reset complet du graphique.

### Étape 6 — Sélection des révolutions

Score + choix des meilleures captures.

### Étape 7 — Détection de variabilité volontaire

Weak bits / protections.

### Étape 8 — Retry d'écriture

Étendre la vérification déjà existante.

### Étape 9 — Mode 48 TPI sur 96 TPI

Ajouter :

* step ×2 ;
* nettoyage pistes intermédiaires ;
* vérification ;
* retries ;
* mesure du flux résiduel.

---

# 68. Résultat recherché

À terme, GW-GUI ne serait plus simplement une GUI permettant de lire ou écrire une image avec Greaseweazle.

Il deviendrait un véritable **outil de récupération et de préservation assistée au niveau flux** :

* acquisition adaptative ;
* analyse ;
* récupération ;
* comparaison multi-révolutions ;
* préservation des protections ;
* reprise après incident ;
* validation d'écriture ;
* exploitation intelligente de lecteurs non parfaitement adaptés au média.

L'architecture actuelle permet déjà une bonne partie de cette évolution sans remise à plat majeure : l'acquisition est déjà piste par piste, `ScpImage` est déjà le modèle utilisé par la visualisation, le writer SCP reconstruit proprement le conteneur et l'écriture physique dispose déjà d'une couche de vérification.

**En gros : il ne manque pas une nouvelle architecture, il manque surtout une couche intelligente d'orchestration autour de ce que GW-GUI sait déjà faire.**

[1]: https://github.com/keirf/greaseweazle/wiki/Supported-Image-Types/a8b5dc0199b156ef95bd9039277b3e9d4cde28d4?utm_source=chatgpt.com "Supported Image Types · keirf/greaseweazle Wiki · GitHub"
[2]: https://github.com/keirf/greaseweazle/issues/78?utm_source=chatgpt.com "Feature request: DC Erase, Write Test Tracks · Issue #78 · keirf/greaseweazle · GitHub"
[3]: https://github.com/keirf/greaseweazle/issues/542?utm_source=chatgpt.com "GW 4.1 with GUI: Failed to verify Track 0.1 · Issue #542 · keirf/greaseweazle · GitHub"
