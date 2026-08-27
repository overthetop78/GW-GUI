# GameInput — validations restantes

## État actuel vérifié

La migration de XInput vers Microsoft GameInput est réalisée dans le code de production.

- GW GUI référence `Microsoft.GameInput` 3.5.268.
- L’interopérabilité native, la durée de vie COM et les callbacks sont isolés dans `Services/Input/GameInput`.
- Les contrôleurs, claviers et souris sont énumérés par des callbacks séparés puis fusionnés par identifiant GameInput.
- Les manettes, contrôleurs génériques, sticks d’arcade, joysticks de vol, volants, claviers et souris utilisent le même lecteur physique.
- Les boutons, axes, gâchettes, Guide, Share et palettes sont représentés séparément lorsqu’ils sont annoncés.
- L’installateur embarque `GameInputRedist.msi`, vérifie la version 3.5.268.0 minimale et ne réinstalle pas le runtime lorsqu’une version suffisante ou plus récente est trouvée.
- La lecture des anciens identifiants `xinput:*` reste présente uniquement pour la compatibilité des associations enregistrées ; aucun lecteur XInput natif parallèle ne subsiste.

Les constats techniques et matériels déjà vérifiés se trouvent dans [la référence GameInput](../../reference/gameinput.md).

## Compatibilité des configurations enregistrées

- [ ] Vérifier sur un fichier de configuration antérieur que chaque association `xinput:*` certaine est conservée ou convertie sans changement de commande.
- [ ] Vérifier qu’une ancienne manette absente reste identifiable sans être réaffectée silencieusement à une autre manette.
- [ ] Vérifier qu’une reconnexion retrouve les associations du même périphérique indépendamment de son ordre de connexion.

## Initialisation et publication

- [ ] Produire un message utilisateur traduit et exploitable lorsque le runtime GameInput ne peut pas être initialisé.
- [ ] Vérifier séparément la version installée et le ZIP portable sur un ordinateur ne possédant ni SDK .NET ni outils de développement.
- [ ] Vérifier que l’installateur ignore une version GameInput égale ou plus récente et installe le MSI embarqué lorsqu’aucun runtime suffisant n’est présent.

## Validation matérielle

- [ ] Vérifier tous les boutons, les deux sticks et les deux gâchettes d’une manette Xbox standard.
- [ ] Vérifier simultanément les deux gâchettes afin de confirmer leur indépendance.
- [ ] Vérifier Guide et Share comme deux commandes distinctes.
- [ ] Vérifier les quatre palettes d’une manette Xbox Elite comme quatre commandes distinctes.
- [ ] Vérifier qu’une manette sans Share ni palettes n’expose aucune commande inexistante.
- [ ] Vérifier plusieurs manettes simultanées, leur déconnexion, leur reconnexion et un changement d’ordre.
- [ ] Vérifier plus de quatre manettes si le matériel disponible le permet.
- [ ] Vérifier clavier et souris dans une machine émulée sans modifier les interactions WPF.
- [ ] Vérifier au moins un volant, pédalier ou joystick de simulation si le matériel est disponible.
- [ ] Vérifier les associations dans l’éditeur puis dans une instance Amiga et une instance Atari.
- [ ] Vérifier qu’un même périphérique n’apparaît jamais deux fois après la fusion des callbacks.
- [ ] Compiler en Debug et effectuer les validations ciblées disponibles avant de clôturer ce plan.

## Condition de fin

Le plan est terminé lorsque les configurations antérieures restent utilisables, que les publications installée et portable fonctionnent sans environnement de développement et que les périphériques disponibles ont passé les validations ci-dessus sans double détection.
