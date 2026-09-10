# Publications restantes

La procédure actuelle est décrite dans [`../project/release.md`](../project/release.md). L’ancien
système de snapshot promue sans recompilation n’est plus prévu. Cette feuille contient uniquement
les prochaines publications à faire après validation des changements concernés.

Les cases ci-dessous décrivent la procédure et ne déclenchent rien seules. Une publication, son
commit, son push et son tag ne sont exécutés qu’après une demande explicite de l’utilisateur.

- [ ] 1. Publier la prochaine version de GW GUI
  - [ ] 1.1 Préparer une version supérieure à 0.3.0
    - [ ] Créer `.github/release-notes/vX.Y.Z.md` avec les changements de l’application, les améliorations de l’interface et le lien de comparaison depuis la version précédente, en utilisant le même `X.Y.Z` pour toute la publication.
  - [ ] 1.2 Publier depuis `main`
    - [ ] Commiter et pousser le code et les notes sur `main`, puis exécuter `scripts/publish-release.cmd` avec le type de publication demandé et inscrire la version effectivement publiée dans `docs/project/release.md`.

- [ ] 2. Publier une nouvelle version du module Atari lorsque ses changements sont validés
  - [ ] 2.1 Préparer la version du module
    - [ ] Modifier `src/GWGUI.Emulation.Atari/module.json` et créer `.github/release-notes/modules/atari/vX.Y.Z.md` avec une version supérieure à 1.0.0 et les seules modifications propres au module.
  - [ ] 2.2 Déclencher la publication indépendante
    - [ ] Commiter et pousser les changements, créer puis pousser le tag `module-atari-vX.Y.Z`, et inscrire dans `docs/project/release.md` la version confirmée par le workflow et son catalogue `module-atari-catalog`.

- [ ] 3. Publier le SDK seulement après évolution de son contrat public
  - [ ] 3.1 Préparer la prochaine version compatible
    - [ ] Modifier la version dans `src/GWGUI.Emulation/GWGUI.Emulation.csproj`, mettre à jour `src/GWGUI.Emulation/README.md` et créer `.github/release-notes/sdk/vX.Y.Z.md` selon la compatibilité décrite dans `docs/architecture/emulation-sdk-versioning.md`.
  - [ ] 3.2 Déclencher Trusted Publishing
    - [ ] Commiter et pousser les changements, créer puis pousser le tag `sdk-vX.Y.Z`, et inscrire dans `docs/project/release.md` la version réellement publiée sur NuGet.org.
