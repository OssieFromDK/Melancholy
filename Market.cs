using Newtonsoft.Json;

namespace Melancholy
{
    public static class Market
    {
        private static Classes.Market marketObj = new();
        private static List<Classes.InventoryItem> inventory = [];

        public static int ItemAmount { get; set; }
        public static int PrestigeLevel { get; set; } = 9;
        public static int PrestigeLevelMinimum { get; set; } = 3;
        public static int PrestigeLevelMaximum { get; set; } = 100;

        public static async Task Generate_Market(string market)
        {
            BuildJson();

            if (market != "dlconly" && market != "tempNoCosmetics")
            {
                AddInventoryItem(inventory, Classes.Ids.CosmeticIds, 1);
                AddInventoryItem(inventory, Classes.Ids.OutfitIds, 1);
            }

            AddInventoryItem(inventory, Classes.Ids.DlcIds, 1);

            if (market == "all" || market == "tempNoCosmetics")
            {
                AddInventoryItem(inventory, Classes.Ids.ItemIds, ItemAmount);
                AddInventoryItem(inventory, Classes.Ids.AddonIds, ItemAmount);
                AddInventoryItem(inventory, Classes.Ids.OfferingIds, ItemAmount);
            }

            if (market != "dlconly" && market != "notemp")
                AddInventoryItem(inventory, Classes.Ids.PerkIds, 3);

            var json = JsonConvert.SerializeObject(marketObj, Formatting.Indented);

            var fileNameDict = new Dictionary<string, string>
            {
                { "all", "Market.json" },
                { "dlconly", "MarketDlcOnly.json" },
                { "notemp", "MarketNoSavefile.json" },
                { "tempNoCosmetics", "MarketTempWithNoCosmetics.json" },
                { "perks", "MarketWithPerks.json" }
            };

            if (fileNameDict.TryGetValue(market, out var fileName))
                await File.WriteAllTextAsync($"Files/{fileName}", json);
        }

        private static void BuildJson()
        {
            inventory = [];

            marketObj = new Classes.Market
            {
                code = 200,
                message = "OK",
                data = new Classes.MarketData
                {
                    playerId = "xxx",
                    inventory = inventory
                }
            };
        }

        private static void AddInventoryItem(List<Classes.InventoryItem> inventory, IEnumerable<object> items, int count)
        {
            var filteredItems = items
                .Where(item =>
                {
                    bool includeItem = true;

                    if (item is Classes.ItemAddon itemAddon)
                    {
                        if (!Extras.AddNonInventoryItems && !itemAddon.ShouldBeInInventory) includeItem = false;
                        if (!Extras.AddEventItems && itemAddon.EventId != "None") includeItem = false;
                    }
                    else if (item is Classes.ItemOfferingPerk itemOfferingPerk)
                    {
                        if (!Extras.AddRetiredOfferings &&
                            itemOfferingPerk.Availability == "EItemAvailability::Retired") includeItem = false;
                        if (!Extras.AddNonInventoryItems && !itemOfferingPerk.ShouldBeInInventory) includeItem = false;
                        if (!Extras.AddEventItems && itemOfferingPerk.EventId != "None") includeItem = false;
                    } 
                    else if(item is Classes.Customization customizationItem)
                    {
                        if (!Extras.AddBannersBadges && (customizationItem.Category == "ECustomizationCategory::Badge" || customizationItem.Category == "ECustomizationCategory::Banner")) includeItem = false;
                        if (!Extras.AddScaryItems && customizationItem.IsExclusive) includeItem = false;
                        if (!Extras.AddLegacy && customizationItem.IsLegacy) includeItem = false;
                    }

                    return includeItem;
                });
            
            inventory.AddRange(filteredItems
                .Select(item =>
                {
                    string id = item.ToString() ?? string.Empty;

                    if (item is Classes.Character character)
                        id = character.CharacterName;
                    else if (item is Classes.ItemOfferingPerk itemOfferingPerk)
                        id = itemOfferingPerk.ItemId;
                    else if (item is Classes.ItemAddon addon)
                        id = addon.ItemId;
                    else if (item is Classes.Customization customization)
                        id = customization.CosmeticId;
                    else if (item is Classes.Outfit outfit)
                        id = outfit.OutfitId;

                    return new Classes.InventoryItem
                    {
                        lastUpdatedAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        objectId = id,
                        quantity = (count == 0 ? new Random().Next(8, 88) : count)
                    };
                }));
        }
    }
}
