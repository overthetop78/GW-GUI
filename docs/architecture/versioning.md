# Version, compilation et révision

GW GUI, chaque module et le SDK possèdent des versions distinctes parce qu’ils sont distribués
indépendamment.

## Application

`GWGUI.App.csproj` définit `0.3.0` comme version locale par défaut. `scripts/build.ps1` lit cette
valeur lorsqu’aucun `-Version X.Y.Z` n’est fourni et la transmet aux publications de l’application,
du lanceur et de l’updater. `scripts/package.ps1` exige une version et l’emploie aussi dans les noms
du ZIP portable et de l’installateur.

Lors d’une publication GitHub, le numéro `X.Y.Z` du fichier
`.github/release-notes/vX.Y.Z.md` est repris dans les notes, le titre, le tag et les paquets. Les
modifications doivent être commitées et poussées sur `main` avant l’exécution de
`scripts/publish-release.cmd`.

## Modules

Chaque `module.json` porte sa propre `moduleVersion`. Le build d’un module utilise cette valeur et
sa publication emploie le tag `module-<id>-vX.Y.Z`. Une nouvelle version d’un module ne change pas la
version de GW GUI et ne republie pas l’application.

## SDK

`src/GWGUI.Emulation/GWGUI.Emulation.csproj` porte la version du paquet public
`GWGUI.Emulation.SDK`; elle est actuellement `1.0.1`. Le même numéro doit apparaître dans ses notes,
son tag `sdk-vX.Y.Z` et son paquet NuGet. Sa compatibilité publique est détaillée dans
[`emulation-sdk-versioning.md`](emulation-sdk-versioning.md).

## Révision

La version fonctionnelle sert à comparer les mises à jour. Le commit Git identifie la révision exacte
du code. Une compilation locale peut contenir des modifications non commitées et ne constitue donc
pas, à elle seule, un paquet officiel. La procédure complète est décrite dans
[`../project/release.md`](../project/release.md).
