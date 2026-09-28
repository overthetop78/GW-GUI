namespace GWGUI.MediaFileSystems.Constants;

/// <summary>Clés et valeurs techniques partagées par l'exploration des cartouches console.</summary>
public static class ConsoleCartridgeMetadataConstants
{
    public const int FirstBank = 0;
    public const int EmptyLength = 0;
    public const int FirstStorageReference = 0;
    public const int MaximumBankCount = 4096;
    public const long FirstAddress = 0;
    public const string BankCount = "bankCount";
    public const string BankSize = "bankSize";
    public const string BankPrefix = "bank.";
    public const string LengthSuffix = ".length";
    public const string NameSuffix = ".name";
    public const string BankEntryType = "cartridge-bank";
    public const string BankAttribute = "bank";
    public const string BankNumberMetadata = "bankNumber";
    public const string AddressMetadata = "address";
    public const string BanksAttribute = "cartridge-banks";
    public const string DefaultBankNameFormat = "bank{0:D2}";
}
