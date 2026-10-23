namespace GWGUI.Emulation.Constants;

/// <summary>Valeurs invariantes du contrat natif : commandes, entrées et dispositions binaires explicites.</summary>
/// <remarks>Ces données sont communes aux modules et aux structures ExternalCoreApi. Les tailles et décalages
/// portant le suffixe X64 décrivent cette architecture ; les codes de commandes conservent leurs indicateurs de protocole.</remarks>
public static class ExternalCoreApiConstants
{
    /// <summary>Identifiant du contexte OpenGL dans le protocole natif.</summary>
    public const int OpenGlContextType = 1;
    /// <summary>Identifiant du contexte OpenGL à profil principal dans le protocole natif.</summary>
    public const int OpenGlCoreContextType = 3;

    public const int X64PointerSize = 8;
    public const int SystemInfoSizeX64 = 32;
    public const int SystemInfoLibraryNameOffsetX64 = 0;
    public const int SystemInfoLibraryVersionOffsetX64 = 8;
    public const int SystemInfoExtensionsOffsetX64 = 16;
    public const int SystemInfoNeedFullPathOffsetX64 = 24;
    public const int SystemInfoBlockExtractOffsetX64 = 25;
    public const int GameInfoSizeX64 = 32;
    public const int GameInfoPathOffsetX64 = 0;
    public const int GameInfoDataOffsetX64 = 8;
    public const int GameInfoSizeOffsetX64 = 16;
    public const int GameInfoMetadataOffsetX64 = 24;
    public const int GeometrySizeX64 = 20;
    public const int GeometryBaseWidthOffsetX64 = 0;
    public const int GeometryBaseHeightOffsetX64 = 4;
    public const int GeometryMaximumWidthOffsetX64 = 8;
    public const int GeometryMaximumHeightOffsetX64 = 12;
    public const int GeometryAspectRatioOffsetX64 = 16;
    public const int TimingSizeX64 = 16;
    public const int TimingFramesPerSecondOffsetX64 = 0;
    public const int TimingSampleRateOffsetX64 = 8;
    public const int SystemAvInfoSizeX64 = 40;
    public const int SystemAvInfoGeometryOffsetX64 = 0;
    public const int SystemAvInfoTimingOffsetX64 = 24;
    public const int VariableSizeX64 = 16;
    public const int VariableKeyOffsetX64 = 0;
    public const int VariableValueOffsetX64 = 8;
    public const int MessageSizeX64 = 16;
    public const int MessageTextOffsetX64 = 0;
    public const int MessageFramesOffsetX64 = 8;
    public const int MessageExtendedSizeX64 = 32;
    public const int MessageExtendedTextOffsetX64 = 0;
    public const int MessageExtendedDurationOffsetX64 = 8;
    public const int MessageExtendedPriorityOffsetX64 = 12;
    public const int MessageExtendedLevelOffsetX64 = 16;
    public const int MessageExtendedTargetOffsetX64 = 20;
    public const int MessageExtendedTypeOffsetX64 = 24;
    public const int MessageExtendedProgressOffsetX64 = 28;
    public const int FunctionPointerInterfaceSizeX64 = 8;
    public const uint SetRotation = 1;
    public const uint GetOverscan = 2;
    public const uint GetCanDuplicateFrames = 3;
    public const uint SetMessage = 6;
    public const uint Shutdown = 7;
    public const uint SetPerformanceLevel = 8;
    public const uint GetSystemDirectory = 9;
    public const uint SetPixelFormat = 10;
    public const uint SetInputDescriptors = 11;
    public const uint SetKeyboardCallback = 12;
    public const uint SetDiskControl = 13;
    public const uint SetHardwareRender = 14;
    public const uint GetVariable = 15;
    public const uint SetVariables = 16;
    public const uint GetVariableUpdate = 17;
    public const uint SetSupportNoGame = 18;
    public const uint GetRumbleInterface = 23;
    public const uint GetInputDeviceCapabilities = 24;
    public const uint GetSensorInterface = 25 | ExperimentalCommandFlag;
    public const uint GetLogInterface = 27;
    public const uint GetPerformanceInterface = 28;
    public const uint GetContentDirectory = 30;
    public const uint GetSaveDirectory = 31;
    public const uint SetSystemAvInfo = 32;
    public const uint SetControllerInfo = 35;
    public const uint ExperimentalCommandFlag = 0x10000;
    public const uint SetMemoryMaps = 36 | ExperimentalCommandFlag;
    public const uint SetGeometry = 37;
    public const uint GetLanguage = 39;
    public const uint SetSupportAchievements = 42 | ExperimentalCommandFlag;
    public const uint GetVfsInterface = 45 | ExperimentalCommandFlag;
    public const uint GetLedInterface = 46 | ExperimentalCommandFlag;
    public const uint GetMidiInterface = 48 | ExperimentalCommandFlag;
    public const uint GetFastForwarding = 49 | ExperimentalCommandFlag;
    public const uint GetInputBitmasks = 51 | ExperimentalCommandFlag;
    public const uint GetCoreOptionsVersion = 52;
    public const uint SetCoreOptions = 53;
    public const uint SetCoreOptionsInternational = 54;
    public const uint SetCoreOptionsDisplay = 55;
    public const uint GetPreferredHardwareRender = 56;
    public const uint GetDiskControlVersion = 57;
    public const uint SetDiskControlExtended = 58;
    public const uint GetMessageInterfaceVersion = 59;
    public const uint SetMessageExtended = 60;
    public const uint SetAudioBufferStatusCallback = 62;
    public const uint SetMinimumAudioLatency = 63;
    public const uint SetFastForwardingOverride = 64;
    public const uint SetContentInfoOverride = 65;
    public const uint SetCoreOptionsV2 = 67;
    public const uint SetCoreOptionsV2International = 68;
    public const uint SetCoreOptionsUpdateDisplayCallback = 69;
    public const uint SetVariable = 70;
    public const uint SetNetworkPacketInterface = 78;
    public const uint SetSerializationQuirks = 87;
    public const uint PrimaryPort = 0;
    /// <summary>Classe native d’un port désactivé.</summary>
    public const uint NoDevice = 0;
    /// <summary>Classe native des boutons d’une manette ou d’un joystick numérique.</summary>
    public const uint JoypadDevice = 1;
    /// <summary>Classe native des mouvements relatifs et boutons d’une souris.</summary>
    public const uint MouseDevice = 2;
    /// <summary>Classe native des touches du clavier.</summary>
    public const uint KeyboardDevice = 3;
    /// <summary>Classe native du pistolet optique.</summary>
    public const uint LightGunDevice = 4;
    /// <summary>Classe native des sticks et pressions analogiques.</summary>
    public const uint AnalogDevice = 5;
    /// <summary>Classe native du stylet ou du point de contact absolu.</summary>
    public const uint PointerDevice = 6;
    public const uint LeftAnalogIndex = 0;
    public const uint RightAnalogIndex = 1;
    /// <summary>Sélecteur natif des pressions de boutons, distinct des deux sticks.</summary>
    public const uint ButtonAnalogIndex = 2;
    public const uint AnalogXId = 0;
    public const uint AnalogYId = 1;
    public const uint MouseXId = 0;
    public const uint MouseYId = 1;
    public const uint MouseLeftId = 2;
    public const uint MouseRightId = 3;
    public const uint MouseWheelUpId = 4;
    public const uint MouseWheelDownId = 5;
    public const uint MouseMiddleId = 6;
    /// <summary>Identifiant natif du défilement horizontal positif (vers la droite).</summary>
    public const uint MouseHorizontalWheelUpId = 7;
    /// <summary>Identifiant natif du défilement horizontal négatif (vers la gauche).</summary>
    public const uint MouseHorizontalWheelDownId = 8;
    /// <summary>Identifiant natif du quatrième bouton, correspondant au premier bouton supplémentaire.</summary>
    public const uint MouseButton4Id = 9;
    /// <summary>Identifiant natif du cinquième bouton, correspondant au deuxième bouton supplémentaire.</summary>
    public const uint MouseButton5Id = 10;
    /// <summary>Commande native de tir du pistolet.</summary>
    public const uint LightGunTriggerId = 2;
    /// <summary>Première commande auxiliaire native du pistolet.</summary>
    public const uint LightGunAuxiliaryAId = 3;
    /// <summary>Deuxième commande auxiliaire native du pistolet.</summary>
    public const uint LightGunAuxiliaryBId = 4;
    /// <summary>Commande native de démarrage du pistolet.</summary>
    public const uint LightGunStartId = 6;
    /// <summary>Commande native de sélection du pistolet.</summary>
    public const uint LightGunSelectId = 7;
    /// <summary>Troisième commande auxiliaire native du pistolet.</summary>
    public const uint LightGunAuxiliaryCId = 8;
    /// <summary>Direction haute native du pistolet.</summary>
    public const uint LightGunUpId = 9;
    /// <summary>Direction basse native du pistolet.</summary>
    public const uint LightGunDownId = 10;
    /// <summary>Direction gauche native du pistolet.</summary>
    public const uint LightGunLeftId = 11;
    /// <summary>Direction droite native du pistolet.</summary>
    public const uint LightGunRightId = 12;
    /// <summary>Coordonnée horizontale absolue native du pistolet.</summary>
    public const uint LightGunScreenXId = 13;
    /// <summary>Coordonnée verticale absolue native du pistolet.</summary>
    public const uint LightGunScreenYId = 14;
    /// <summary>État natif indiquant un pointage hors écran.</summary>
    public const uint LightGunOffscreenId = 15;
    /// <summary>Commande native de rechargement du pistolet.</summary>
    public const uint LightGunReloadId = 16;
    /// <summary>Coordonnée horizontale absolue du contact natif.</summary>
    public const uint PointerXId = 0;
    /// <summary>Coordonnée verticale absolue du contact natif.</summary>
    public const uint PointerYId = 1;
    /// <summary>État natif d’appui du contact.</summary>
    public const uint PointerPressedId = 2;
    /// <summary>Nombre de contacts actifs dans le contrat natif.</summary>
    public const uint PointerCountId = 3;
    /// <summary>État natif indiquant un contact hors écran.</summary>
    public const uint PointerOffscreenId = 15;
    public const uint JoypadMaskId = 256;
    public const short ActiveState = 1;
    public const short InactiveState = 0;
    public const int ConsumedRelativeValue = 0;
}
