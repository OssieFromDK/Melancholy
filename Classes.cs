namespace Melancholy
{
    public static class Classes
    {
        public class Settings
        {
            public string AesKey { get; set; } = string.Empty;
            public string MappingsPath { get; set; } = string.Empty;
            public string PakPath { get; set; } = string.Empty;
        }

        public class Base
        {
            public string CharacterType { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string FilePath { get; set; } = string.Empty;
        }

        public class Character : Base
        {
            public string CharacterName { get; set; } = string.Empty;
            public string CharacterIndex { get; set; } = string.Empty;
            public string CharacterDefaultItem { get; set; } = string.Empty;
        }

        public class ItemOfferingPerk : Base
        {
            public string ItemId { get; set; } = string.Empty;
            public string Rarity { get; set; } = string.Empty;
            public string Availability { get; set; } = string.Empty;
            public bool ShouldBeInInventory { get; set; } = true;
            public string EventId { get; set; } = string.Empty;
        }

        public class ItemAddon : ItemOfferingPerk
        {
            public string CharacterDefaultItem { get; set; } = string.Empty;
        }

        public class Customization
        {
            public string CosmeticId { get; set; } = string.Empty;
            public string CosmeticName { get; set; } = string.Empty;
            public string CosmeticDescription { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string AssociatedCharacterIndex { get; set; } = string.Empty;
            public string Rarity { get; set; } = string.Empty;
            public string IsInStore { get; set; } = string.Empty;
            public string EventId { get; set; } = string.Empty;
            public string Availability { get; set; } = string.Empty;
            public string FilePath { get; set; } = string.Empty;
            public bool IsLegacy { get; set; } = false;
            public bool IsExclusive { get; set; } = false;
        }

        public class Outfit
        {
            public string OutfitId { get; set; } = string.Empty;
            public string OutfitName { get; set; } = string.Empty;
            public string OutfitDescription { get; set; } = string.Empty;
            public string CollectionName { get; set; } = string.Empty;
            public string Availability { get; set; } = string.Empty;
            public string FilePath { get; set; } = string.Empty;
        }

        public static class FilePaths
        {
            public static readonly List<string> CustomizationItemDb = [];
            public static readonly List<string> OutfitDb = [];
            public static readonly List<string> CharacterDescriptionDb = [];
            public static readonly List<string> ItemDb = [];
            public static readonly List<string> ItemAddonDb = [];
            public static readonly List<string> OfferingDb = [];
            public static readonly List<string> PerkDb = [];
        }

        public static class Ids
        {
            public static readonly List<Customization> CosmeticIds = [];
            public static readonly List<Outfit> OutfitIds = [];
            public static readonly List<Character> DlcIds = [];
            public static readonly List<ItemOfferingPerk> ItemIds = [];
            public static readonly List<ItemAddon> AddonIds = [];
            public static readonly List<ItemOfferingPerk> OfferingIds = [];
            public static readonly List<ItemOfferingPerk> PerkIds = [];
        }

        public class Market
        {
            public int code { get; set; }
            public string message { get; set; } = string.Empty;
            public MarketData data { get; set; } = new();
        }

        public class MarketData
        {
            public string playerId { get; set; } = string.Empty;
            public List<InventoryItem> inventory { get; set; } = [];
        }

        public class InventoryItem
        {
            public long lastUpdatedAt { get; set; }
            public string objectId { get; set; } = string.Empty;
            public int quantity { get; set; }
        }

        public class Player
        {
            public int TotalXp { get; set; }
            public int LevelVersion { get; set; }
            public int Level { get; set; }
            public int PrestigeLevel { get; set; }
            public int CurrentXp { get; set; }
            public int CurrentXpUpperBound { get; set; }
        }

        public class Bloodweb
        {
            public List<string> paths { get; set; } = [];
            public List<Ring> ringData { get; set; } = [];
        }

        public class Ring
        {
            public List<Node> nodeData { get; set; } = [];
        }

        public class Node
        {
            public string contentId { get; set; } = string.Empty;
            public int nodeId { get; set; }
            public string state { get; set; } = "Collected";
        }

        public class InventoryItemBloodweb
        {
            public string ItemId { get; set; } = string.Empty;
            public string CharacterType { get; set; } = string.Empty;
            public string CharacterDefaultItem { get; set; } = string.Empty;
            public bool ShouldBeInInventory { get; set; } = true;
            public string EventId { get; set; } = string.Empty;
            public string Availability { get; set; } = string.Empty;
        }

        public class ItemBloodweb
        {
            public string itemId { get; set; } = string.Empty;
            public int quantity { get; set; }
        }

        public class BloodwebData
        {
            public bool bloodwebLevelChanged { get; set; } = false;
            public List<object> updatedWallets { get; set; } = [];
            public string characterName { get; set; } = "";
            public int bloodwebLevel { get; set; } = 50;
            public int prestigeLevel { get; set; } = 10;
            public Bloodweb bloodWebData { get; set; } = BloodwebGenerator.Make_Bloodweb("EPlayerRole::VE_Camper", "", false);
            public List<ItemBloodweb> characterItems { get; set; } = BloodwebGenerator.TivoTigs;
            public int legacyPrestigeLevel { get; set; } = 3;
        }

        public class CharacterItem
        {
            public string characterName { get; set; } = string.Empty;
            public int legacyPrestigeLevel { get; set; } = 3;
            public List<ItemBloodweb> characterItems { get; set; } = [];
            public int bloodWebLevel { get; set; } = 50;
            public Bloodweb bloodWebData { get; set; } = BloodwebGenerator.Make_Bloodweb("EPlayerRole::VE_Camper", "");
            public int prestigeLevel { get; set; } = 10;
        }

        public class GetAllData
        {
            public List<CharacterItem> list { get; set; } = [];
        }

        public class PrestigeCharacter
        {
            public string characterName { get; set; } = string.Empty;
            public int prestigeLevel { get; set; } = 10;
            public int bloodWebLevel { get; set; } = 50;
        }

        public class PrestigeItem
        {
            public string name { get; set; } = string.Empty;
            public int quantity { get; set; } = 0;
        }

        public class PrestigeData<T>
        {
            public List<T> list { get; set; } = [];
        }
    }
}
