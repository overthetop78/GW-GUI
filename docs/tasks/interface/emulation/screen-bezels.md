# Habillages d’écran — idée future

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

### Idée future : habillages d’écran

Cette partie est une idée à conserver pour plus tard. Elle ne doit pas être réalisée maintenant.

Il faudra étudier la possibilité d’afficher un habillage en plein écran :

- une télévision ou un écran d’ordinateur pour les ordinateurs et consoles de salon ;
- le corps de la console pour une console portable.

Sans habillage, le plein écran classique actuel reste disponible.

Le choix de l’habillage est enregistré dans la configuration de la machine. Plusieurs habillages peuvent être proposés pour une même machine, notamment lorsque le matériel a connu plusieurs modèles ou plusieurs couleurs, par exemple Lynx et Lynx II, ou différentes couleurs de Game Boy Color.

Pour une console portable, les habillages représentent uniquement cette console et ses variantes. Il n’est pas obligatoire que tout le boîtier reste visible : l’habillage peut ne montrer que le contour de l’écran d’une Game Boy, d’une Lynx, d’une Game Gear ou d’une autre console portable. Une partie extérieure du boîtier peut donc être coupée, mais l’écran de la console doit rester entièrement visible, correctement placé et sans déformation.

Au départ, les habillages sont décoratifs. Une évolution ultérieure pourra éventuellement afficher les boutons pressés, les voyants allumés ou d’autres éléments animés.

L’utilisation des habillages en mode fenêtré reste à étudier. Les images nécessaires pourront être recherchées et rendues transparentes au besoin lorsque cette fonction sera effectivement abordée.

## Checklist détaillée — Point 8 : habillages d’écran en plein écran

La section Idée future : habillages d’écran indique explicitement que cette fonction ne doit pas être réalisée maintenant. Aucune tâche de code, d’image, de configuration, de test ou de traduction n’est donc autorisée dans l’état actuel du document.

- [ ] Autoriser explicitement le démarrage de cette idée future avant toute autre action
  - [ ] Modifier docs/tasks/interface/emulation/screen-bezels.md, dans la section Idée future : habillages d’écran, uniquement après une décision explicite de réalisation, pour inscrire que le point 8 peut commencer et conserver la date de cette décision.

- [ ] Compléter les décisions encore ouvertes avant d’écrire une checklist d’implémentation
  - [ ] Modifier docs/tasks/interface/emulation/screen-bezels.md après l’autorisation pour inscrire les décisions validées concernant le mode fenêtré, les variantes initiales, les images à produire ou rechercher, leur redistribution, le recadrage autorisé et le comportement lorsqu’un habillage manque.
  - [ ] Modifier docs/tasks/interface/emulation/screen-bezels.md après ces décisions pour remplacer le présent bloc par une checklist d’implémentation fondée sur les fichiers alors réellement présents, sans anticiper maintenant une architecture, des actifs ou des comportements non validés.
