using GWGUI.Emulation.Acorn.Common.Machines.Common.Dictionaries;

namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Dictionaries;

/// <summary>Compatibilité des ROM connues avec chaque machine BBC Micro, indépendamment du backend.</summary>
/// <remarks>Seul le Model A est évalué dans ce jalon. Les autres profils restent Unknown :
/// leurs règles ne sont pas déduites de celles du Model A. Les ROM connues non retenues sont
/// incompatibles avec ce Model A de base ; une identité absente du catalogue reste inconnue.</remarks>
internal static class BbcMicroFirmwareCompatibilityCatalog
{
    private static IReadOnlyDictionary<MachineModel, IReadOnlyDictionary<string, EmulationFirmwareCompatibility>> ByModel { get; } =
        new Dictionary<MachineModel, IReadOnlyDictionary<string, EmulationFirmwareCompatibility>>
        {
            [MachineModel.BbcMicroModelA] = new Dictionary<string, EmulationFirmwareCompatibility>(StringComparer.OrdinalIgnoreCase)
            {
                ["6c8daba70374e5aa3a6402f24cdc5f8677d58a0f"] = EmulationFirmwareCompatibility.Official,
                // Variantes nationales substituables ; les relevés MAME signalés imparfaits restent partiels.
                ["9ab9d24b845fe2964fba2f4770d54025d2c8026a"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["07cd9b7edbed6ef7b527622533d6957f5d56aa91"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["ec4bc515e28e851a6433f7ca0a11ede0f1d21a68"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["1e471a1b5670d7163e9f62d31be7cab0330a07cd"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["7a273ad7270507dca4ce621fc1e6b51a1ac25085"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["c5ce7fe84df6de6a317fa0e87bda413c82c04618"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["2d1ff08b4dda15cf70832881750a962189455f41"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["5a93445dde03066073e2909a935900e5f8439d81"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["890f6e3e7fab3340f75b85e93ff29332bc9ecb2e"] = EmulationFirmwareCompatibility.Official,
                ["4a7393f3a45ea309f744441c16723e2ef447a281"] = EmulationFirmwareCompatibility.Official,
                ["4b0ece6dc139d5d3f4fabd023716fb6f25149b80"] = EmulationFirmwareCompatibility.Official,
                ["dca33995c0d008a527efe923d03333394b01022c"] = EmulationFirmwareCompatibility.Official,
                ["d35f6723132aabe3c4d00fc16fd9ecc6768df753"] = EmulationFirmwareCompatibility.Official,
                ["0d9bcaf6a393c9ce2359ed700ddb53c232c2c45d"] = EmulationFirmwareCompatibility.Official,
                ["2b6aecd33a43f296c20832524e47cc7e3a9c3b17"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["aadf90338ee9d1c85dfa73beba50e930c2a38f10"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["83d91d089dca092d2c8b7c3650ff8143c9069b89"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["b39014610a968789afd7695aa04d1277d874405c"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["0a07161fa2fbc102fb306233bd81790c7b04f793"] = EmulationFirmwareCompatibility.PartiallyCompatible,
                ["88c93df1854f5fbe6cd6e5f0e29a8bf4ea3b5614"] = EmulationFirmwareCompatibility.PartiallyCompatible
            }
        };

    internal static EmulationFirmwareCompatibility Get(MachineModel model, string sha1)
    {
        if (!FirmwareCatalog.BySha1.ContainsKey(sha1) || !ByModel.TryGetValue(model, out var profile)) return EmulationFirmwareCompatibility.Unknown;
        return profile.GetValueOrDefault(sha1, EmulationFirmwareCompatibility.Incompatible);
    }
}
