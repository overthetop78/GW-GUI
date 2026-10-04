# Traduction française des écrans du BIOS PC-FX

Le menu principal de `pcfx-fr.rom` est déjà partiellement traduit. Les captures fournies par l'utilisateur montrent encore du japonais dans la maintenance des fichiers, le lecteur CD et l'invite de changement de disque. La ROM française sélectionnée est bien celle à compléter ; ne pas remettre ce constat en cause pour expliquer ces captures.

L'original est `C:\Users\overt\AppData\Roaming\GW GUI\Emulation\Machines\Nec\Firmware\pcfx.rom`. Conserver cet original intact et écrire la version française `pcfx-fr.rom` dans le même dossier. Les fichiers de recherche et les captures de vérification vont dans `artifacts/pce-translation/`, hors Git, jamais dans `docs/`.

Argos japonais → anglais (`ja_en` 1.1) et anglais → français (`en_fr` 1.9) sont déjà installés. Aucun modèle japonais → français direct n'est présent dans l'index officiel consulté le 4 octobre 2026. Employer en priorité un modèle direct s'il devient disponible ; sinon utiliser les deux modèles installés et réviser chaque traduction, car un essai de « ディスクを交換してください » a produit une réponse incorrecte. Hiragana, katakana et kanji relèvent du même modèle japonais une fois décodés en Unicode ; la recherche dans la ROM doit aussi considérer CP932/Shift-JIS, EUC-JP, ISO-2022-JP, UTF-8, UTF-16, JIS brut et les éventuelles représentations propres au BIOS, sans conclure qu'une absence de chaîne dans ces encodages signifie une image.

- [ ] Compléter les écrans encore en japonais.
  - [ ] Identifier leurs ressources dans le BIOS.
    - [ ] Créer `artifacts/pce-translation/pcfx-bios-screens.json` avec, pour chaque libellé visible de la maintenance des fichiers, du lecteur CD et de l'invite de changement de disque, sa source exacte dans `pcfx.rom`, son format et les octets ou ressources à modifier, après examen des encodages et représentations indiqués ci-dessus.
  - [ ] Traduire et reconstruire la ROM française.
    - [ ] Modifier `C:\Users\overt\AppData\Roaming\GW GUI\Emulation\Machines\Nec\Firmware\pcfx-fr.rom` pour remplacer les ressources identifiées par leurs équivalents français, en conservant les traductions déjà fonctionnelles du menu principal et sans modifier `pcfx.rom`.
  - [ ] Vérifier les écrans dans l'émulateur.
    - [ ] Créer `artifacts/pce-translation/pcfx-bios-screen-validation.md` avec les captures et le résultat de la vérification de ces trois écrans dans la ROM française chargée, ainsi que la taille et les empreintes des deux ROM.
