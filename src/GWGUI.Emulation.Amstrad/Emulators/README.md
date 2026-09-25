# Adaptateurs de cœurs Amstrad

Chaque cœur possède un dossier `Emulators/<nom-du-cœur>/`. Ce dossier contient son adaptateur, ses URL,
son installation et la traduction de ses options, médias, entrées et sorties natives.

Pour ajouter un cœur :

1. créer son dossier ;
2. implémenter `Common/Interfaces/IEmulatorAdapter.cs`, jamais directement les contrats de `GWGUI.Emulation` ;
3. traduire les commandes définies par la machine vers l’API native du cœur ;
4. enregistrer sa `EmulationEmulatorDefinition` dans le catalogue Amstrad.

Chaque dossier d’émulateur contient directement ses propres catégories
`Constants`, `Contracts`, `Dictionaries`, `Enums`, `Exceptions`, `Factories`,
`Functions`, `Interfaces` et `Services` lorsqu’elles sont nécessaires. Ses
espaces de noms suivent ces dossiers.

L’émulateur dépend de `Common` et implémente ses prises. `Common`, `Modules`,
`GWGUI.Emulation` et `GWGUI.App` ne référencent jamais l’espace de noms ou les
types de l’émulateur concret.
