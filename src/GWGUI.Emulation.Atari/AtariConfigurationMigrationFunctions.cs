using System.Text.Json;

namespace GWGUI.Emulation.Atari;

internal static class AtariConfigurationMigrationFunctions
{
    internal static AtariConfigurationDocument MigrateToCurrent(JsonElement root)
    {
        if (!root.TryGetProperty(AtariConfigurationMigrationConstants.SchemaVersionPropertyName,
                out var schemaProperty)
            || !schemaProperty.TryGetInt32(out var schemaVersion))
            throw new InvalidDataException(AtariConfigurationStoreConstants.UnsupportedSchemaError);
        return schemaVersion switch
        {
            AtariConstants.CurrentConfigurationSchemaVersion =>
                root.Deserialize<AtariConfigurationDocument>(AtariConfigurationStoreConstants.JsonOptions)
                ?? throw new InvalidDataException(AtariConfigurationStoreConstants.EmptyDocumentError),
            _ => throw new InvalidDataException(AtariConfigurationStoreConstants.UnsupportedSchemaError)
        };
    }
}
