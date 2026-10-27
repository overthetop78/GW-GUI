using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;

namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Dictionaries;

/// <summary>Identités des ROM Acorn et machines apparentées, partagées par tous les adaptateurs du module.</summary>
/// <remarks>Ce catalogue ne décide ni du modèle compatible, ni du cœur, ni des champs de configuration.
/// Les empreintes matérielles proviennent de MAME bbcb.cpp, révision 84981cc15cfc1429e48f098d80c6dd97bc82a0bc ;
/// les ROM NFS complémentaires sont identifiées sur les fichiers externes indiqués dans la documentation BBC.
/// Les règles de compatibilité appartiennent au profil matériel de chaque machine.</remarks>
internal static class FirmwareCatalog
{
    internal static IReadOnlyDictionary<string, FirmwareDefinition> BySha1 { get; } = new Dictionary<string, FirmwareDefinition>(StringComparer.OrdinalIgnoreCase)
    {
        [MemoryFirmwareConstants.AriesB32Sha1] = new(MemoryFirmwareConstants.AriesB32Name, "1.00", FirmwareCategory.MemoryExtension),
        [MemoryFirmwareConstants.IntegraBSha1] = new(MemoryFirmwareConstants.IntegraBName, "1.20", FirmwareCategory.MemoryExtension),
        // ROM des cartes mémoire, identités publiées par les définitions matérielles MAME Aries/Watford.
        ["2590596e0864b97b30050c33b21aa89bb8956204"] = new("Aries B20", "2.4", FirmwareCategory.MemoryExtension),
        ["d04bfd332d63f6f23dca2ef81a1d2074e3d88ee9"] = new("Aries B20", "2.2", FirmwareCategory.MemoryExtension),
        ["0a5efb084eb1ae353b9c79c026e9e26cd7695293"] = new("Watford Electronics 32K RAM", "2.40", FirmwareCategory.MemoryExtension),
        ["7f8536576de449916cd77488e94775ed438c5c6c"] = new("Watford Electronics 32K RAM", "2.20", FirmwareCategory.MemoryExtension),
        ["98a3084b20c92427c2e9def58ade96f42659e20a"] = new("Watford Electronics 32K RAM", "2.00", FirmwareCategory.MemoryExtension),
        ["210f6162d634fca8d75197d4f64d9b50abc01e1e"] = new("Watford Electronics 32K RAM", "1.06", FirmwareCategory.MemoryExtension),
        ["1b59a3354cfc704ad8aa60b0ed16e4cd2d815031"] = new("Watford Electronics 32K RAM", "1.00", FirmwareCategory.MemoryExtension),
        // NFS Acorn : empreintes vérifiées sur les ROM externes du dépôt historique nvg, répertoire Acorn/fs.
        ["8564d944fedd17caf2023454ba5a7c3947aedc1f"] = new("Acorn NFS", "3.34", FirmwareCategory.Network),
        ["7081f1d834645e501c8c9862cf4a50f282ca1f32"] = new("Acorn NFS", "3.35K", FirmwareCategory.Network),
        ["6c8daba70374e5aa3a6402f24cdc5f8677d58a0f"] = new("SAA5050", null, FirmwareCategory.CharacterGenerator),
        ["9ab9d24b845fe2964fba2f4770d54025d2c8026a"] = new("SAA5051", "MAME 0.203", FirmwareCategory.CharacterGenerator),
        ["07cd9b7edbed6ef7b527622533d6957f5d56aa91"] = new("SAA5051", null, FirmwareCategory.CharacterGenerator),
        ["ec4bc515e28e851a6433f7ca0a11ede0f1d21a68"] = new("SAA5052", null, FirmwareCategory.CharacterGenerator),
        ["1e471a1b5670d7163e9f62d31be7cab0330a07cd"] = new("SAA5053", null, FirmwareCategory.CharacterGenerator),
        ["7a273ad7270507dca4ce621fc1e6b51a1ac25085"] = new("SAA5054", null, FirmwareCategory.CharacterGenerator),
        ["c5ce7fe84df6de6a317fa0e87bda413c82c04618"] = new("SAA5055", null, FirmwareCategory.CharacterGenerator),
        ["2d1ff08b4dda15cf70832881750a962189455f41"] = new("SAA5056", null, FirmwareCategory.CharacterGenerator),
        ["5a93445dde03066073e2909a935900e5f8439d81"] = new("SAA5057", null, FirmwareCategory.CharacterGenerator),
        ["890f6e3e7fab3340f75b85e93ff29332bc9ecb2e"] = new("BASIC I", "I", FirmwareCategory.Basic),
        ["4a7393f3a45ea309f744441c16723e2ef447a281"] = new("BASIC II", "II", FirmwareCategory.Basic),
        ["4b0ece6dc139d5d3f4fabd023716fb6f25149b80"] = new("MOS 0.10", "0.10", FirmwareCategory.OperatingSystem),
        ["dca33995c0d008a527efe923d03333394b01022c"] = new("MOS 0.92", "0.92", FirmwareCategory.OperatingSystem),
        ["d35f6723132aabe3c4d00fc16fd9ecc6768df753"] = new("MOS 1.00", "1.00", FirmwareCategory.OperatingSystem),
        ["0d9bcaf6a393c9ce2359ed700ddb53c232c2c45d"] = new("MOS 1.20", "1.20", FirmwareCategory.OperatingSystem),
        ["2b6aecd33a43f296c20832524e47cc7e3a9c3b17"] = new("MOS NO", "1.20", FirmwareCategory.OperatingSystem),
        ["aadf90338ee9d1c85dfa73beba50e930c2a38f10"] = new("MOS DE", null, FirmwareCategory.OperatingSystem),
        ["83d91d089dca092d2c8b7c3650ff8143c9069b89"] = new("MOS US", "1.00", FirmwareCategory.OperatingSystem),
        ["b39014610a968789afd7695aa04d1277d874405c"] = new("BASIC III US", "III", FirmwareCategory.Basic),
        ["0a07161fa2fbc102fb306233bd81790c7b04f793"] = new("Hope MOS/BASIC", "1989", FirmwareCategory.OperatingSystem, Combined: true),
        ["88c93df1854f5fbe6cd6e5f0e29a8bf4ea3b5614"] = new("VIEW A2.1", "A2.1", FirmwareCategory.Language),
        ["b369809275cb67dfd8a749265e91adb2d2558ae6"] = new("CM62024 VSM", null, FirmwareCategory.Speech),
        ["66876702d1d95eecc034d20f25047f893a27cde5"] = new("VM61002 VSM", null, FirmwareCategory.Speech),
        ["dbbec4d2540a854c120be3194c7566a2b79d153b"] = new("DFS 0.9H", "0.9H", FirmwareCategory.DiscFilingSystem, DiskController: FirmwareDiskController.Intel8271),
        ["161f585dc45665ea77433c84afd2f95049f7f5a0"] = new("DFS/NET", "1.0", FirmwareCategory.DiscFilingSystem, DiskController: FirmwareDiskController.Intel8271),
        ["7e3c536baeae84d6498a14e8405319e01ee78232"] = new("DNFS", "1.20", FirmwareCategory.DiscFilingSystem, DiskController: FirmwareDiskController.Intel8271),
        ["7462ced7b83d74b822815bc00ed40a89f84e0276"] = new("Torch NET", "3.35K", FirmwareCategory.Network),
        ["4c9f7cf141ad028fe5e517c45e6e3a43b1a76d24"] = new("Hope DFS", "1.00", FirmwareCategory.DiscFilingSystem),
        ["be10872aeb88714bd56462a2e86929953dee1c01"] = new("Torch MCP", "1.00 (1.01 CI)", FirmwareCategory.Application),
        ["409762deafb76b1f86be39bfbf2f812d5de3ff92"] = new("Torch MCP", "1.22", FirmwareCategory.Application),
        ["4398ba29c871fa397654aa182c63ccdcad597625"] = new("Cisco SIST1 PROGS", null, FirmwareCategory.Application),
        ["5ea266f47ff83821ccdbec006b8506b2e892b115"] = new("Cisco SIST1 STARTUP", null, FirmwareCategory.Application),
        ["e37d043c8df30c49ba8717e1aa0b92105cb0c937"] = new("Torch UNIX Host", "1.00", FirmwareCategory.Application)
    };
}
