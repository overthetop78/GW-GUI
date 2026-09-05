# Aides contextuelles

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

## 4. Aides sur les champs

Une petite icône **(i)** doit être placée immédiatement après le nom de chaque champ dont le nom seul ne permet pas réellement de comprendre son rôle, ses choix ou leurs conséquences.

Cette aide concerne les champs, pas les boutons ni les titres de groupes.

L’icône doit toujours être visible. Sa taille normale constitue sa zone cliquable ; aucune zone invisible plus grande n’est demandée. L’infobulle au survol confirme que le pointeur se trouve bien sur l’icône.

### Aide rapide au survol

Lorsque la souris survole l’icône, une explication rapide apparaît.

Cette explication :

- tient sur une seule ligne ;
- indique simplement à quoi sert le champ ;
- disparaît lorsque le survol se termine ;
- ne contient pas de texte long ni de défilement.

### Aide détaillée au clic

Un clic sur l’icône ouvre une aide plus détaillée avec une présentation de type post-it.

Cette aide explique simplement ce que fait le champ, les choix disponibles et leurs différences utiles. Le texte reste court, clair et concis, sans longs paragraphes ni mise en forme de documentation. Un défilement n’est utilisé que si le contenu concis ne tient réellement pas dans le post-it.

Une fois le post-it ouvert, n’importe quelle touche du clavier ou un nouveau clic le ferme.

### Présentation validée du post-it

- largeur maximale : 380 px ;
- hauteur maximale : 240 px ;
- espacement entre l’icône et le post-it : 8 px ;
- marge intérieure du post-it : 12 px ;
- placement normal : à droite de l’icône, centré verticalement sur celle-ci ;
- repli : à gauche de l’icône lorsque l’espace disponible à droite est insuffisant ;
- arrière-plan : ressource de thème CardBrush ;
- bordure : ressource de thème BorderBrush ;
- texte : ressource de thème TextBrush.

### Périmètre et traductions

Le système doit être utilisé pour les champs concernés dans les différents onglets Amiga et Atari.

Pour chaque champ portant une icône **(i)**, deux textes distincts — l’aide courte au survol et l’aide concise au clic — doivent être ajoutés aux ressources et traduits dans toutes les langues prises en charge par GW GUI. Aucun de ces textes ne doit être écrit directement dans le code.

### Inventaire des champs visibles

Les boutons, les titres de groupes, les tableaux d’associations et les valeurs de résumé ne sont pas des champs de réglage et sont exclus. Les champs communs aux deux modules sont regroupés lorsqu’ils utilisent le même libellé et la même ressource. Le sélecteur de périphérique physique reste recensé mais ne recevra pas d’aide, car sa suppression est demandée au point 6.

| Origine | Onglet | Champ visible | Identifiant(s) | Ressource du libellé | Aide contextuelle | Clé d’aide courte | Texte court | Clé d’aide concise | Texte concis |
|---|---|---|---|---|---|---|---|---|---|
| Amiga, Atari | Audio | Activer le son | `AmigaSettingsConstants.AudioEnabled, AtariSettingsConstants.AudioEnabled` | `Emulation.Audio.Enabled` | Non | — | — | — | — |
| Amiga, Atari | Audio | Bruit des lecteurs | `AmigaSettingsDescriptionFunctionsConstants.OptionFloppySound, AtariVideoAudioSettingsConstants.FloppySoundVolumeOption` | `Emulation.Audio.Floppy.Sound` | Non | — | — | — | — |
| Atari | Audio | Bruit des lecteurs de disquettes | `AtariVideoAudioSettingsConstants.FloppySoundOption` | `Emulation.Audio.Floppy.Enabled` | Non | — | — | — | — |
| Amiga | Audio | Couper le son des lecteurs vides | `AmigaSettingsDescriptionFunctionsConstants.OptionFloppySoundEmptyMute` | `Emulation.Audio.Floppy.MuteEmpty` | Oui | `Emulation.Help.Audio.Floppy.MuteEmpty.Short` | Silence empty floppy drives | `Emulation.Help.Audio.Floppy.MuteEmpty.Detailed` | Silences the emulated drive sound while no disk is inserted. Disable this option to keep the idle mechanical noise. |
| Amiga | Audio | Filtre Amiga | `AmigaSettingsDescriptionFunctionsConstants.OptionSoundFilter` | `Emulation.Audio.Filter` | Oui | `Emulation.Help.Audio.Filter.Short` | Select analog audio filtering | `Emulation.Help.Audio.Filter.Detailed` | Selects how the original analog audio filter is reproduced. Emulated follows the hardware behavior. The other choices force the filter on or off. |
| Atari | Audio | Filtre audio polarisé | `AtariVideoAudioSettingsConstants.PolarizedFilterOption` | `Emulation.Audio.PolarizedFilter` | Oui | `Emulation.Help.Audio.PolarizedFilter.Short` | Enable the polarized audio filter | `Emulation.Help.Audio.PolarizedFilter.Detailed` | Applies the polarized filter to the generated audio signal. Enable it to reproduce the filtered sound. Disable it to keep the unfiltered signal. |
| Amiga | Audio | Interpolation | `AmigaSettingsDescriptionFunctionsConstants.OptionSoundInterpol` | `Emulation.Audio.Interpolation` | Oui | `Emulation.Help.Audio.Interpolation.Short` | Select audio interpolation | `Emulation.Help.Audio.Interpolation.Detailed` | Selects how audio samples are calculated between generated sample points. Higher-quality methods produce smoother sound but require more processing. |
| Amiga | Audio | Latence | `AmigaSettingsConstants.AudioLatency` | `Emulation.Audio.LatencyLabel` | Oui | `Emulation.Help.Audio.Latency.Short` | Set audio latency | `Emulation.Help.Audio.Latency.Detailed` | Sets the duration of the audio buffer. A shorter buffer reduces delay but can cause crackling. A longer buffer improves stability but increases delay. |
| Atari | Audio | Latence | `AtariVideoAudioSettingsConstants.AudioLatencyOption` | `Emulation.Audio.Latency` | Oui | `Emulation.Help.Audio.Latency.Short` | Set the audio buffer delay | `Emulation.Help.Audio.Latency.Detailed` | Chooses the audio buffer duration. Lower values reduce delay but may crackle; higher values improve stability with more latency. |
| Amiga | Audio | Périphérique | `AmigaSettingsConstants.AudioOutput` | `Emulation.Audio.Device` | Non | — | — | — | — |
| Atari | Audio | POKEY stéréo | `AtariEightBitSettingsConstants.PokeyStereoOptionKey` | `Emulation.Atari.Audio.PokeyStereo` | Oui | `Emulation.Help.Audio.PokeyStereo.Short` | Enable dual POKEY stereo | `Emulation.Help.Audio.PokeyStereo.Detailed` | Adds a second emulated POKEY chip to produce stereo sound. Enable this option only for software that supports dual POKEY audio. |
| Amiga | Audio | Séparation stéréo | `AmigaSettingsConstants.AudioStereoSeparation` | `Emulation.Audio.StereoSeparation` | Oui | `Emulation.Help.Audio.StereoSeparation.Short` | Set stereo separation | `Emulation.Help.Audio.StereoSeparation.Detailed` | Sets the separation between the left and right channels. A lower value mixes the channels together. A higher value keeps them farther apart. |
| Atari | Audio | Sortie audio | `AtariVideoAudioSettingsConstants.AudioOutputOption` | `Emulation.Audio.Output` | Non | — | — | — | — |
| Amiga | Audio | Type de bruit des lecteurs | `AmigaSettingsDescriptionFunctionsConstants.OptionFloppySoundType` | `Emulation.Audio.Floppy.SoundType` | Oui | `Emulation.Help.Audio.Floppy.SoundType.Short` | Select the floppy drive sound set | `Emulation.Help.Audio.Floppy.SoundType.Detailed` | Selects the sample set used for the mechanical sounds of emulated floppy drives. It does not change drive behavior or disk data. |
| Amiga | Audio | Type de filtre | `AmigaSettingsDescriptionFunctionsConstants.OptionSoundFilterType` | `Emulation.Audio.FilterType` | Oui | `Emulation.Help.Audio.FilterType.Short` | Select the audio filter model | `Emulation.Help.Audio.FilterType.Detailed` | Selects the hardware filter response to emulate. Auto follows the selected machine model. A specific choice always uses that filter response. |
| Atari | Audio | Volume | `AtariVideoAudioSettingsConstants.AudioVolumeOption` | `Explorer.Volume` | Non | — | — | — | — |
| Amiga | Audio | Volume audio du CD | `AmigaSettingsDescriptionFunctionsConstants.OptionSoundVolumeCd` | `Emulation.Audio.Cd.Volume` | Non | — | — | — | — |
| Amiga | Manettes | Adaptateur parallèle pour quatre joysticks | `AmigaSettingsConstants.ParallelJoystickAdapter` | `Emulation.Amiga.Controller.ParallelAdapter` | Oui | `Emulation.Help.Controller.ParallelAdapter.Short` | Enable two additional joystick ports | `Emulation.Help.Controller.ParallelAdapter.Detailed` | Emulates a parallel-port adapter that adds two joystick ports. Enable it for software that supports four simultaneous players. |
| Amiga | Manettes | Cadence du turbo | `AmigaSettingsDescriptionFunctionsConstants.OptionTurboPulse` | `Emulation.Controller.Turbo.Pulse` | Oui | `Emulation.Help.Controller.TurboPulse.Short` | Set the turbo pulse interval | `Emulation.Help.Controller.TurboPulse.Detailed` | Sets the time between automatic button presses produced by turbo. A shorter interval produces faster repeated firing. |
| Atari | Manettes | Compatibilité des manettes | `AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey` | `Emulation.Atari.Controller.Compatibility` | Oui | `Emulation.Help.Controller.Compatibility.Short` | Select a controller compatibility mode | `Emulation.Help.Controller.Compatibility.Detailed` | Selects a special controller mapping, such as dual-stick control, swapped ports, or Joy2B+. Choose None to keep the standard port mapping. |
| Application | Manettes | Périphérique physique | `PhysicalDevice` | `Emulation.Controller.Device` | Non | — | — | — | — |
| Atari | Manettes | Sensibilité analogique | `AtariEightBitSettingsConstants.AnalogSensitivityOptionKey` | `Emulation.Atari.Controller.AnalogSensitivity` | Oui | `Emulation.Help.Controller.AnalogSensitivity.Short` | Set analog input sensitivity | `Emulation.Help.Controller.AnalogSensitivity.Detailed` | Sets how strongly an analog input responds to movement. A higher value produces a larger response from the same physical movement. |
| Atari | Manettes | Sensibilité numérique | `AtariEightBitSettingsConstants.DigitalSensitivityOptionKey` | `Emulation.Atari.Controller.DigitalSensitivity` | Oui | `Emulation.Help.Controller.DigitalSensitivity.Short` | Set digital input sensitivity | `Emulation.Help.Controller.DigitalSensitivity.Detailed` | Sets how quickly a digital direction reaches full movement. A higher value makes direction changes reach their maximum more quickly. |
| Atari | Manettes | Tir automatique | `AtariEightBitSettingsConstants.AutofireOptionKey` | `Emulation.Atari.Controller.Autofire` | Oui | `Emulation.Help.Controller.Autofire.Short` | Select the autofire mode | `Emulation.Help.Controller.Autofire.Detailed` | Disabled turns autofire off. On button repeats fire while the assigned button is held. Always repeats fire continuously. |
| Application | Manettes | Type de manette | `ControllerType` | `Emulation.Controller.Type` | Non | — | — | — | — |
| Atari | Manettes | Vitesse des paddles | `AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey` | `Emulation.Atari.Controller.PaddleSpeed` | Oui | `Emulation.Help.Controller.PaddleSpeed.Short` | Set paddle movement speed | `Emulation.Help.Controller.PaddleSpeed.Detailed` | Sets how quickly digital input moves an emulated paddle. A lower value moves it more slowly. A higher value moves it more quickly. |
| Amiga, Atari | CPU | FPU | `AmigaSettingsDescriptionFunctionsConstants.OptionFpuModel, AtariSettingsConstants.Fpu` | `Emulation.Fpu.Model` | Oui | `Emulation.Help.Cpu.FpuModel.Short` | Select the FPU | `Emulation.Help.Cpu.FpuModel.Detailed` | Selects the floating-point unit used for floating-point instructions. Choose None to disable the FPU. Available models depend on the selected CPU. |
| Amiga, Atari | CPU | Modèle de CPU | `AmigaSettingsDescriptionFunctionsConstants.OptionCpuModel, AtariSettingsConstants.Cpu` | `Emulation.Cpu.Model` | Non | — | — | — | — |
| Amiga, Atari | CPU | Précision | `AmigaSettingsDescriptionFunctionsConstants.OptionCpuCompatibility, AtariSettingsConstants.CpuPrecision` | `Emulation.Cpu.Precision` | Oui | `Emulation.Help.Cpu.Precision.Short` | Select CPU emulation accuracy | `Emulation.Help.Cpu.Precision.Detailed` | Selects how closely CPU timing follows the original hardware. More accurate modes can improve compatibility but may require more processing. |
| Amiga, Atari | CPU | Vitesse d’origine | `AmigaSettingsConstants.CpuOriginalSpeed, AtariSettingsConstants.CpuOriginalFrequency` | `Emulation.Cpu.SpeedOriginal` | Non | — | — | — | — |
| Amiga, Atari | CPU | Vitesse du CPU | `AmigaSettingsConstants.CpuSpeed, AtariSettingsConstants.CpuFrequency` | `Emulation.Cpu.Speed` | Oui | `Emulation.Help.Cpu.Speed.Short` | Set the emulated CPU speed | `Emulation.Help.Cpu.Speed.Detailed` | Sets the processor frequency used by the emulated machine. The original speed preserves hardware timing. Higher values accelerate CPU-limited software. |
| Atari | Général | Disques durs | `AtariSettingsConstants.HardDiskFolder` | `Emulation.Storage.HardDisk.List` | Non | — | — | — | — |
| Application | Général | Modèle | `ModelSelector` | `Emulation.Model` | Non | — | — | — | — |
| Amiga | Souris | Joysticks analogiques contrôlant la souris | `AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouse` | `Emulation.Mouse.Analog` | Oui | `Emulation.Help.Mouse.Analog.Short` | Use analog sticks to control the mouse | `Emulation.Help.Mouse.Analog.Detailed` | Allows analog joystick axes to move the emulated mouse pointer. Enable it when a physical analog stick should control the pointer. |
| Amiga, Atari | Souris | Vitesse de la souris | `AmigaSettingsDescriptionFunctionsConstants.OptionMouseSpeed, AtariMouseSettingsConstants.SpeedOptionKey` | `Emulation.Mouse.Speed` | Non | — | — | — | — |
| Amiga | Souris | Vitesse de la souris analogique | `AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeed, AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeedRight` | `Emulation.Mouse.AnalogSpeed` | Oui | `Emulation.Help.Mouse.AnalogSpeed.Short` | Set analog mouse speed | `Emulation.Help.Mouse.AnalogSpeed.Detailed` | Sets pointer speed when an analog stick controls the emulated mouse. A higher value moves the pointer farther for the same stick movement. |
| Amiga | Souris | Zone morte des joysticks analogiques | `AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseDeadzone` | `Emulation.Mouse.AnalogDeadzone` | Oui | `Emulation.Help.Mouse.AnalogDeadzone.Short` | Set the analog mouse dead zone | `Emulation.Help.Mouse.AnalogDeadzone.Detailed` | Ignores small analog-stick movements around the center while controlling the mouse. Increase the value if the pointer moves while the stick is released. |
| Atari | RAM | Banque fantôme Axlon $0F | `AtariEightBitSettingsConstants.AxlonShadowOptionKey` | `Emulation.Atari.Memory.AxlonShadow` | Oui | `Emulation.Help.Memory.AxlonShadow.Short` | Enable the Axlon $0F shadow bank | `Emulation.Help.Memory.AxlonShadow.Detailed` | Mirrors the Axlon control bank at address $0F. Enable it only for software or memory expansions that use this additional address. |
| Atari | RAM | Extension mémoire Axlon | `AtariEightBitSettingsConstants.AxlonMemoryOptionKey` | `Emulation.Atari.Memory.Axlon` | Oui | `Emulation.Help.Memory.Axlon.Short` | Set Axlon expansion memory | `Emulation.Help.Memory.Axlon.Detailed` | Sets the amount of bank-switched Axlon memory. Disabled removes the expansion. The other choices provide the selected memory capacity. |
| Atari | RAM | Extension mémoire Mosaic | `AtariEightBitSettingsConstants.MosaicMemoryOptionKey` | `Emulation.Atari.Memory.Mosaic` | Oui | `Emulation.Help.Memory.Mosaic.Short` | Set Mosaic expansion memory | `Emulation.Help.Memory.Mosaic.Detailed` | Sets the amount of bank-switched Mosaic memory. Disabled removes the expansion. The other choices provide the selected memory capacity. |
| Atari | RAM | Extensions mémoire | `AtariSettingsConstants.AlternateMemory` | `Emulation.Memory.Extensions` | Oui | `Emulation.Help.Memory.Extensions.Short` | Set additional expansion memory | `Emulation.Help.Memory.Extensions.Detailed` | Sets the amount of additional expansion memory supported by the selected machine. Choose None to use only the configured main memory. |
| Amiga | RAM | Fast RAM | `AmigaSettingsDescriptionFunctionsConstants.OptionFastmemSize` | `Emulation.Memory.Fast` | Oui | `Emulation.Help.Memory.Fast.Short` | Set Fast RAM size | `Emulation.Help.Memory.Fast.Detailed` | Sets the amount of Fast RAM available directly to the CPU. Additional Fast RAM can help compatible software but changes the emulated hardware configuration. |
| Atari | RAM | MapRAM | `AtariEightBitSettingsConstants.MapRamOptionKey` | `Emulation.Atari.Memory.MapRam` | Oui | `Emulation.Help.Memory.MapRam.Short` | Enable MapRAM | `Emulation.Help.Memory.MapRam.Detailed` | Allows compatible software to map writable RAM into the system ROM address area. This option is available only on machine models that support MapRAM. |
| Amiga, Atari | RAM | Mémoire principale | `AmigaSettingsDescriptionFunctionsConstants.OptionChipmemSize, AtariConfigurationOptionConstants.MainMemory` | `Emulation.Memory.Main` | Non | — | — | — | — |
| Amiga | RAM | RAM Zorro III | `AmigaSettingsDescriptionFunctionsConstants.OptionZ3memSize` | `Emulation.Memory.Z3` | Oui | `Emulation.Help.Memory.Z3.Short` | Set Zorro III RAM size | `Emulation.Help.Memory.Z3.Detailed` | Sets the amount of 32-bit Fast RAM connected through the Zorro III bus. Use it only with compatible 32-bit machines and software. |
| Amiga | RAM | Slow RAM | `AmigaSettingsDescriptionFunctionsConstants.OptionBogomemSize` | `Emulation.Memory.Slow` | Oui | `Emulation.Help.Memory.Slow.Short` | Set Slow RAM size | `Emulation.Help.Memory.Slow.Detailed` | Sets the amount of Slow RAM in the trapdoor expansion area. This memory is slower than Fast RAM and uses a different hardware address range. |
| Amiga | ROM | Clé ROM | `AmigaSettingsConstants.RomKeyPath` | `Emulation.Firmware.Rom.Key` | Oui | `Emulation.Help.Firmware.RomKey.Short` | Select the ROM decryption key | `Emulation.Help.Firmware.RomKey.Detailed` | Selects the key file used to decrypt a licensed encrypted ROM image. Leave this field empty when the selected ROM is not encrypted. |
| Atari | ROM | Démarrage rapide | `AtariSettingsDescriptionFunctionsConstants.HatariFastboot` | `Emulation.Atari.FastBoot` | Oui | `Emulation.Help.Firmware.FastBoot.Short` | Enable fast startup | `Emulation.Help.Firmware.FastBoot.Detailed` | Skips selected hardware initialization delays to shorten startup. Disable this option when software requires the original startup sequence. |
| Amiga | ROM | Kickstart | `AmigaSettingsConstants.KickstartPath` | `Emulation.Firmware.Rom.Kickstart` | Non | — | — | — | — |
| Amiga | ROM | ROM étendue | `AmigaSettingsConstants.ExtendedRomPath` | `Emulation.Firmware.Rom.Extended` | Oui | `Emulation.Help.Firmware.ExtendedRom.Short` | Select an extended ROM | `Emulation.Help.Firmware.ExtendedRom.Detailed` | Selects the secondary firmware ROM required by some machine models. Leave this field empty when the selected model does not use an extended ROM. |
| Atari | ROM | ROM système | `AtariSettingsConstants.SystemFirmware` | `Emulation.Firmware.Rom.System` | Non | — | — | — | — |
| Atari | Stockage | Accélération SIO | `AtariEightBitSettingsConstants.SioAccelerationOptionKey` | `Emulation.Atari.Storage.SioAcceleration` | Oui | `Emulation.Help.Storage.SioAcceleration.Short` | Enable accelerated SIO transfers | `Emulation.Help.Storage.SioAcceleration.Detailed` | Speeds up compatible transfers through the serial input/output bus. Disable this option when software depends on the original transfer timing. |
| Atari | Stockage | Afficher l’activité des lecteurs sur l’écran de l’émulateur | `AtariMachineOptionConstants.DriveActivity, AtariEightBitSettingsConstants.ShowActivityOptionKey` | `Emulation.Storage.ActivityOsd` | Non | — | — | — | — |
| Atari | Stockage | Afficher la vitesse d’émulation à l’écran | `AtariEightBitSettingsConstants.ShowSpeedOptionKey` | `Emulation.Atari.Storage.SpeedOsd` | Non | — | — | — | — |
| Atari | Stockage | Afficher le compteur secteur/bloc | `AtariEightBitSettingsConstants.ShowSectorOptionKey` | `Emulation.Atari.Storage.SectorOsd` | Non | — | — | — | — |
| Atari | Stockage | Démarrer depuis la cassette | `AtariEightBitSettingsConstants.CassetteBootOptionKey` | `Emulation.Atari.Storage.CassetteBoot` | Oui | `Emulation.Help.Storage.CassetteBoot.Short` | Enable cassette startup | `Emulation.Help.Storage.CassetteBoot.Detailed` | Makes the emulated machine try to start from the attached cassette image. Disable this option when starting from another device. |
| Atari | Stockage | Horloge temps réel R-Time 8 | `AtariEightBitSettingsConstants.RealTimeClockOptionKey` | `Emulation.Atari.Storage.RealTimeClock` | Oui | `Emulation.Help.Storage.RealTimeClock.Short` | Enable the R-Time 8 clock | `Emulation.Help.Storage.RealTimeClock.Detailed` | Emulates an R-Time 8 real-time clock so compatible software can read the current date and time. |
| Atari | Stockage | Périphérique d’impression P: | `AtariEightBitSettingsConstants.PrinterDeviceOptionKey` | `Emulation.Atari.Storage.PrinterDevice` | Oui | `Emulation.Help.Storage.PrinterDevice.Short` | Enable the P: printer device | `Emulation.Help.Storage.PrinterDevice.Detailed` | Makes the emulated P: printer device available to software. Disable this option when printer-device emulation is not needed. |
| Atari | Stockage | Périphérique série R: | `AtariEightBitSettingsConstants.SerialDeviceOptionKey` | `Emulation.Atari.Storage.SerialDevice` | Oui | `Emulation.Help.Storage.SerialDevice.Short` | Enable the R: serial device | `Emulation.Help.Storage.SerialDevice.Detailed` | Makes the emulated R: serial device available to software. Disable this option when serial-device emulation is not needed. |
| Atari | Vidéo | Artéfacts haute résolution | `AtariEightBitSettingsConstants.ArtifactingModeOptionKey` | `Emulation.Atari.Video.Artifacting` | Oui | `Emulation.Help.Video.Artifacting.Short` | Select high-resolution color artifacting | `Emulation.Help.Video.Artifacting.Detailed` | Selects how high-resolution patterns produce composite-video colors. None disables artifact colors. The other modes reproduce different palettes or chip behavior. |
| Amiga | Vidéo | Blitter | `AmigaSettingsDescriptionFunctionsConstants.OptionImmediateBlits` | `Emulation.State.ImmediateBlits` | Oui | `Emulation.Help.Video.ImmediateBlits.Short` | Select blitter timing | `Emulation.Help.Video.ImmediateBlits.Detailed` | Selects whether blitter operations finish immediately or follow emulated hardware timing. Immediate mode is faster but less timing-accurate. |
| Amiga | Vidéo | Changement de fréquence | `AmigaSettingsDescriptionFunctionsConstants.OptionVideoAllowHzChange` | `Emulation.Video.HzChange` | Oui | `Emulation.Help.Video.HzChange.Short` | Allow output refresh-rate changes | `Emulation.Help.Video.HzChange.Detailed` | Allows the output refresh rate to follow changes in the emulated video mode. Locked keeps the current output refresh rate. |
| Amiga | Vidéo | Collisions | `AmigaSettingsDescriptionFunctionsConstants.OptionCollisionLevel` | `Emulation.Video.Collision.Level` | Oui | `Emulation.Help.Video.CollisionLevel.Short` | Select collision detection detail | `Emulation.Help.Video.CollisionLevel.Detailed` | Selects which sprite and playfield collisions are calculated. More complete detection improves compatibility but requires more processing. |
| Atari | Vidéo | Contraste | `AtariEightBitSettingsConstants.ColorContrastOptionKey` | `Emulation.Atari.Video.Contrast` | Non | — | — | — | — |
| Amiga | Vidéo | Corriger le scintillement | `AmigaSettingsDescriptionFunctionsConstants.OptionGfxFlickerfixer` | `Emulation.Video.FlickerFixer` | Oui | `Emulation.Help.Video.FlickerFixer.Short` | Reduce interlaced display flicker | `Emulation.Help.Video.FlickerFixer.Detailed` | Reduces flicker in interlaced video output. Enable it for a steadier image. Disable it to preserve the original interlaced display behavior. |
| Amiga, Atari | Vidéo | Format d’image | `AmigaSettingsDescriptionFunctionsConstants.OptionVideoAspect, AtariVideoAudioSettingsConstants.AspectRatioOption` | `Emulation.Video.AspectRatio` | Oui | `Emulation.Help.Video.AspectRatio.Short` | Select the displayed aspect ratio | `Emulation.Help.Video.AspectRatio.Detailed` | Selects how the emulated image is scaled horizontally and vertically. Auto follows the emulated output. A fixed choice forces that display shape. |
| Amiga, Atari | Vidéo | Gamma | `AmigaSettingsDescriptionFunctionsConstants.OptionGfxGamma, AtariEightBitSettingsConstants.ColorGammaOptionKey` | `Emulation.Video.Gamma` | Oui | `Emulation.Help.Video.Gamma.Short` | Adjust image gamma | `Emulation.Help.Video.Gamma.Detailed` | Changes the brightness of midtones without directly changing the black and white levels. Lower values darken midtones. Higher values brighten them. |
| Atari | Vidéo | Luminosité | `AtariEightBitSettingsConstants.ColorBrightnessOptionKey` | `Emulation.Atari.Video.Brightness` | Non | — | — | — | — |
| Amiga | Vidéo | Mode de lignes | `AmigaSettingsDescriptionFunctionsConstants.OptionVideoVresolution` | `Emulation.Video.LineMode` | Oui | `Emulation.Help.Video.LineMode.Short` | Select the video line mode | `Emulation.Help.Video.LineMode.Detailed` | Selects how vertical video lines are displayed. Auto follows the emulated mode. Other choices force single lines, doubled lines, or scanlines when available. |
| Amiga, Atari | Vidéo | Norme | `AmigaSettingsDescriptionFunctionsConstants.OptionVideoStandard, AtariVideoAudioSettingsConstants.StandardOption, AtariConfigurationOptionConstants.VideoStandard` | `Emulation.Video.Standard` | Oui | `Emulation.Help.Video.Standard.Short` | Select the video standard | `Emulation.Help.Video.Standard.Detailed` | Selects the emulated television timing, such as PAL or NTSC. This affects refresh rate, hardware timing, and software compatibility. |
| Atari | Vidéo | Palette externe | `AtariEightBitSettingsConstants.ExternalPaletteOptionKey` | `Emulation.Atari.Video.ExternalPalette` | Oui | `Emulation.Help.Video.ExternalPalette.Short` | Select an external color palette | `Emulation.Help.Video.ExternalPalette.Detailed` | Selects a predefined color palette instead of colors generated from the current video settings. Choose None to use the generated colors. |
| Amiga | Vidéo | Profondeur des couleurs | `AmigaSettingsDescriptionFunctionsConstants.OptionGfxColors` | `Emulation.Video.Colors` | Oui | `Emulation.Help.Video.Colors.Short` | Select output color depth | `Emulation.Help.Video.Colors.Detailed` | Selects the color depth used to render the image. 24-bit keeps more color detail. 16-bit uses a smaller range of colors. |
| Atari | Vidéo | Région | `AtariSettingsConstants.Region` | `Emulation.Atari.Video.Region` | Oui | `Emulation.Help.Video.Region.Short` | Select the hardware region | `Emulation.Help.Video.Region.Detailed` | Selects the regional timing used by the emulated machine. The choice can affect video frequency, CPU timing, and compatible firmware. |
| Amiga, Atari | Vidéo | Rendu | `AmigaSettingsConstants.VideoRenderer, AtariSettingsConstants.VideoRenderer` | `Emulation.Video.Settings.Rendering` | Oui | `Emulation.Help.Video.Rendering.Short` | Select the video renderer | `Emulation.Help.Video.Rendering.Detailed` | Selects the graphics backend used to draw the emulated display. Available renderers can differ in performance and compatibility with the host system. |
| Amiga, Atari | Vidéo | Résolution | `AmigaSettingsDescriptionFunctionsConstants.OptionVideoResolution, AtariVideoAudioSettingsConstants.ResolutionOption` | `Emulation.Video.Resolution` | Non | — | — | — | — |
| Atari | Vidéo | Retard de couleur GTIA | `AtariEightBitSettingsConstants.ColorDelayOptionKey` | `Emulation.Atari.Video.ColorDelay` | Oui | `Emulation.Help.Video.ColorDelay.Short` | Adjust GTIA color delay | `Emulation.Help.Video.ColorDelay.Detailed` | Sets the GTIA color phase delay used to reproduce colors. Default uses the standard value. Numeric choices shift the resulting hues. |
| Amiga, Atari | Vidéo | Rogner les bordures | `AmigaSettingsDescriptionFunctionsConstants.OptionCrop, AtariVideoAudioSettingsConstants.CropOption` | `Emulation.Video.Crop` | Non | — | — | — | — |
| Atari | Vidéo | Saturation | `AtariEightBitSettingsConstants.ColorSaturationOptionKey` | `Emulation.Atari.Video.Saturation` | Non | — | — | — | — |
| Amiga, Atari | Vidéo | Saut d’images | `AmigaSettingsDescriptionFunctionsConstants.OptionGfxFramerate, AtariVideoAudioSettingsConstants.FrameSkipOption` | `Emulation.Video.FrameSkip` | Oui | `Emulation.Help.Video.FrameSkip.Short` | Set frame skipping | `Emulation.Help.Video.FrameSkip.Detailed` | Sets how many display frames are omitted. Disabled draws every frame. Higher values reduce rendering work but make motion less smooth. |
| Atari | Vidéo | Teinte | `AtariEightBitSettingsConstants.ColorHueOptionKey` | `Emulation.Atari.Video.Hue` | Non | — | — | — | — |

## Checklist détaillée — Point 5 : aides contextuelles sur les champs

Cette checklist réalise la demande fonctionnelle décrite dans la section 4. Les aides concernent uniquement les champs explicitement validés dans les éditeurs Amiga et Atari. ExplanationResourceKey devient la clé de l’aide courte ; une seconde clé distincte transporte l’aide concise au clic.

- [x] Fixer le périmètre et le contenu avant de créer l’interface
  - [x] Modifier docs/tasks/interface/emulation/contextual-help.md, dans la section 4, pour ajouter un tableau des champs visibles provenant de src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs, src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs et des champs fixes construits par l’application, en excluant les boutons et titres.
  - [x] Modifier le tableau de la section 4 dans docs/tasks/interface/emulation/contextual-help.md pour marquer uniquement les champs dont le libellé ne suffit pas, après validation de leur présence ou de leur absence d’aide ; ne pas prévoir d’aide pour le sélecteur de périphérique physique dont la suppression est demandée au point 6.
  - [x] Modifier le tableau de la section 4 dans docs/tasks/interface/emulation/contextual-help.md pour inscrire, pour chaque champ retenu, la clé d’aide courte, son texte d’une ligne, la clé d’aide concise et son texte expliquant uniquement le rôle, les choix et leurs différences utiles.
  - [x] Modifier la section 4 dans docs/tasks/interface/emulation/contextual-help.md pour inscrire la présentation validée du post-it, notamment ses dimensions maximales, son placement et ses couleurs, afin qu’aucune valeur visuelle ne soit choisie pendant l’implémentation.

- [x] Étendre les contrats communs avant de modifier les mises en page
  - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationSettingsField.cs pour conserver ExplanationResourceKey comme clé optionnelle de l’aide courte et ajouter DetailedExplanationResourceKey comme clé optionnelle de l’aide concise.
  - [x] Modifier src/GWGUI.App/Contracts/Emulation/Settings/EmulationSettingsControlField.cs pour transporter le libellé, le contrôle, l’aide courte localisée et l’aide concise localisée, tout en autorisant l’absence des deux aides.
  - [x] Modifier src/GWGUI.App/Contracts/Views/Emulation/Settings/EmulationCpuSettingsContent.cs pour transporter des EmulationSettingsControlField pour les champs CPU actuellement séparés, sans intégrer le résumé du processeur à un champ d’aide.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par l’extension de ces contrats.

- [x] Créer le libellé réutilisable avant de remplacer les libellés actuels
  - [x] Créer le fichier vide src/GWGUI.App/Constants/Emulation/EmulationSettingsFieldHelpConstants.cs.
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationSettingsFieldHelpConstants.cs pour définir uniquement les dimensions, espacements et couleurs validés du post-it.
  - [x] Créer le fichier vide src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour reproduire le TextBlock actuel lorsque les deux aides sont absentes et ne créer aucune icône dans ce cas.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour afficher immédiatement après le libellé une icône permanente utilisant ControlVisualConstants.InformationGlyph lorsque les deux aides sont présentes, avec uniquement sa taille visible comme zone cliquable.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour affecter l’aide courte à une infobulle sans retour à la ligne ni défilement, visible seulement pendant le survol.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour ouvrir au clic un Popup de type post-it contenant le libellé et l’aide concise, selon les valeurs validées, et activer le défilement uniquement lorsque le contenu dépasse ses dimensions maximales.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour fermer ce Popup sur toute touche ou sur le clic suivant, sans le fermer pendant le clic d’ouverture, puis détacher tous ses gestionnaires lors de la fermeture et de Unloaded.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par ce contrôle.

- [x] Faire passer les champs décrits par les modules par un seul chemin
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter CreateControlField, qui crée le contrôle existant, localise LabelResourceKey et les deux clés d’aide lorsqu’elles existent, puis retourne EmulationSettingsControlField.
  - [x] Modifier AddBlocks dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour utiliser CreateControlField sans modifier l’ordre, les colonnes, la visibilité ou les contrôles des blocs.
  - [x] Modifier BuildCpuSettingsTab et BuildMemorySettingsTab dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour utiliser CreateControlField sans modifier les choix, les règles, les résumés ni le calcul de RAM totale.
  - [x] Modifier BuildInputSettingsTab dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleInputSettingsSection.cs pour utiliser CreateControlField sans modifier les associations ni leur enregistrement.

- [x] Remplacer les libellés des mises en page par le contrôle commun
  - [x] Modifier CompactForm dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsLayout.cs pour recevoir des EmulationSettingsControlField et construire leurs libellés avec EmulationSettingsFieldLabel, puis conserver une surcharge sans aide pour les appelants hors des éditeurs de machine.
  - [x] Modifier SettingsFields et SettingsFieldGrid dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationHardwareSettingsLayout.cs pour recevoir des EmulationSettingsControlField, utiliser EmulationSettingsFieldLabel et lier sa visibilité à celle du contrôle correspondant.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationCpuSettingsLayout.cs pour consommer les EmulationSettingsControlField de EmulationCpuSettingsContent sans modifier les cartes Processeur, Compatibilité et Accélération.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMemorySettingsLayout.cs pour transmettre les EmulationSettingsControlField sans perdre les aides ni modifier les cadres de mémoire.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationInputSettingsLayout.cs pour transmettre les EmulationSettingsControlField de la souris et des options analogiques sans modifier les tableaux d’associations.
  - [x] Modifier docs/tasks/interface/emulation/contextual-help.md après validation du tableau pour ajouter à cet emplacement une sous-tâche distincte, nommant son fichier, pour chaque champ fixe approuvé qui ne passe pas encore par EmulationSettingsControlField ; n’effectuer aucune modification de ce champ avant l’ajout de sa sous-tâche.
    - Résultat du tableau validé : aucun champ fixe construit par l’application n’est approuvé pour recevoir une aide ; aucune sous-tâche de modification d’un champ fixe n’est donc ajoutée.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par le remplacement des libellés.

- [x] Ajouter les paires de textes validées dans toutes les ressources avant de les utiliser
  - [x] Examiner tous les catalogues de src/GWGUI.App/Resources/00-Base et retirer de chaque catalogue localisé les clés dont la valeur est un nom propre, un modèle ou un identifiant technique invariant, notamment les modèles de machines, sans supprimer ces clés de 00-Base.
    - Résultat : 201 clés invariantes réparties dans 8 catalogues restent uniquement dans 00-Base ; 5829 entrées localisées ont été retirées des 29 langues.
  - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour ajouter exactement les clés et textes validés dans le tableau de la section 4.
      - [x] Modifier src/GWGUI.App/Resources/ar-SA/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/cs-CZ/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/da-DK/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/de-DE/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/el-GR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/en-US/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/es-ES/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/fi-FI/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/fr-FR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/he-IL/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/hu-HU/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/id-ID/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/it-IT/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/ja-JP/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/ko-KR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/nb-NO/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/nl-NL/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/pl-PL/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/pt-BR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/pt-PT/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/ro-RO/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/ru-RU/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/sv-SE/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/th-TH/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/tr-TR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/uk-UA/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/vi-VN/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/zh-Hans/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/zh-Hant/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs pour affecter les deux clés uniquement aux champs Amiga approuvés dans le tableau.
  - [x] Modifier src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs pour affecter les deux clés uniquement aux champs Atari approuvés dans le tableau, sans réutiliser les explications de compatibilité propres à Atari.
  - [x] Réaliser dans l’ordre chaque sous-tâche de champ fixe ajoutée à docs/tasks/interface/emulation/contextual-help.md afin de transporter exactement les deux clés approuvées, sans étendre l’aide à un autre élément.

- [x] Vérifier les ressources et le comportement avant de terminer le point
  - [x] Exécuter un contrôle de parité des clés d’aide entre src/GWGUI.App/Resources/00-Base/Emulation.resx et les 29 fichiers de langue, puis corriger uniquement les clés absentes ou supplémentaires créées par ce point.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par les ressources et les clés d’aide.
  - [x] Corriger le post-it observé pendant la vérification : empêcher le contenu de reprendre la police d’icônes afin que le texte reste lisible, l’ouvrir sous le libellé sans masquer le champ de saisie ou le sélecteur, et ne le replier au-dessus que si l’espace inférieur est insuffisant.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore après cette correction et corriger uniquement les erreurs qu’elle introduit.
  - [x] Fermer l’instance de GW GUI ayant révélé les défauts du post-it avant de modifier de nouveau son implémentation.
  - [x] Remplacer le Popup séparé par un post-it intégré à la fenêtre Options, visuellement jaune et sans bande noire, placé sous le champ associé sans le masquer, maintenu entièrement dans les limites de la fenêtre et replié au-dessus uniquement si nécessaire.
  - [x] Rendre l’aide courte visible immédiatement au survol et rendre chaque clic sur l’icône fiable pour ouvrir ou fermer le post-it.
  - [x] Relire le comportement réel de chaque champ approuvé et réécrire entièrement ses aides courte et détaillée dans 00-Base avec un texte exact, neutre et réutilisable, sans nom de machine ou d’émulateur.
  - [x] Reporter exactement les aides courte et détaillée réécrites dans le tableau de la section 4 de docs/tasks/interface/emulation/contextual-help.md.
  - [x] Répercuter les textes d’aide corrigés dans les 29 langues avec le traducteur IA du dépôt, sans traduire les noms propres, modèles et identifiants techniques conservés uniquement dans 00-Base.
  - [x] Contrôler les 106 aides dans les 29 langues, retirer tout caractère de remplacement ou reste de protection technique, puis corriger manuellement les formulations françaises inexactes avant la compilation.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore après ces corrections et corriger uniquement les erreurs qu’elles introduisent.
  - [x] Fermer l’instance de GW GUI utilisée pour constater que le post-it intégré recouvre encore le champ de la ligne suivante.
  - [x] Fermer l’instance de GW GUI utilisée pour constater l’extrapolation qui déplace la mise en page.
  - [x] Retirer l’insertion de ligne, puis afficher dans le dialogue Options un post-it jaune flottant de taille fixe 380 × 240, sous l’icône et sous le bord du champ associé, entièrement contraint à Options, sans pousser ni masquer ce champ, avec une barre de défilement uniquement lorsque le texte dépasse.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore après ce retour au comportement demandé et corriger uniquement les erreurs qu’il introduit.
  - [x] Lire les erreurs produites par l’instance actuelle de GW GUI et identifier précisément leur cause avant toute nouvelle modification.
  - [x] Fermer l’instance de GW GUI après la lecture de ses erreurs.
  - [x] Corriger uniquement les erreurs relevées, réduire la taille fixe du post-it et lui donner un effet de papier autocollant sans bande noire ni sortie hors du dialogue Options.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore après ces corrections et corriger uniquement les erreurs qu’elles introduisent.
  - [x] Fermer l’instance de développement de GW GUI avant de produire le paquet Debug demandé.
  - [x] Exécuter scripts/build.ps1 -Configuration Debug et produire build/Debug/GW GUI/gwgui.exe pour le test manuel.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier chaque champ approuvé dans les onglets Amiga et Atari : icône toujours visible, aide courte d’une ligne au survol et post-it au clic.
  - [x] Dans la même exécution, vérifier qu’une touche et le clic suivant ferment le post-it, que le défilement n’apparaît qu’en cas de dépassement et qu’aucune icône n’est présente sur un bouton ou un titre.
  - [x] Dans la même exécution, vérifier au minimum le français, l’anglais et une langue de droite à gauche, puis vérifier que le changement de langue actualise le libellé, l’infobulle et le post-it.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.
