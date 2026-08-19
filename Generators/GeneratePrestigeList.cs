using Newtonsoft.Json;

namespace Melancholy
{
    public class GeneratePrestigeList
    {
        public static async Task GenerateList()
        {
            var characters = new Classes.PrestigeData<Classes.PrestigeCharacter>();
            var items = new List<Classes.PrestigeItem>();
            var addons = new List<Classes.PrestigeItem>();
            var offerings = new List<Classes.PrestigeItem>();

            characters.list.AddRange(Classes.Ids.DlcIds.Select(character => new Classes.PrestigeCharacter
            {
                characterName = character.CharacterName,
                prestigeLevel = (Market.PrestigeLevel == 0 ? new Random().Next(Market.PrestigeLevelMinimum, Market.PrestigeLevelMaximum) : Market.PrestigeLevel)
            }));

            var characterJson = JsonConvert.SerializeObject(characters, Formatting.Indented);
            await File.WriteAllTextAsync("Other/CustomCharacterData.json", characterJson);

            items.AddRange(Classes.Ids.ItemIds.Select(item => new Classes.PrestigeItem
            {
                name = item.ItemId,
                quantity = (Market.ItemAmount == 0 ? new Random().Next(8, 88) : Market.ItemAmount)
            }));

            var itemJson = JsonConvert.SerializeObject(items, Formatting.Indented);
            await File.WriteAllTextAsync("Other/CustomItemAmount.json", itemJson);

            addons.AddRange(Classes.Ids.AddonIds.Select(addon => new Classes.PrestigeItem
            {
                name = addon.ItemId,
                quantity = (Market.ItemAmount == 0 ? new Random().Next(8, 88) : Market.ItemAmount)
            }));

            var addonJson = JsonConvert.SerializeObject(addons, Formatting.Indented);
            await File.WriteAllTextAsync("Other/CustomAddonAmount.json", addonJson);

            offerings.AddRange(Classes.Ids.OfferingIds.Select(offering => new Classes.PrestigeItem
            {
                name = offering.ItemId,
                quantity = (Market.ItemAmount == 0 ? new Random().Next(8, 88) : Market.ItemAmount)
            }));

            var offeringJson = JsonConvert.SerializeObject(offerings, Formatting.Indented);
            await File.WriteAllTextAsync("Other/CustomOfferingAmount.json",  offeringJson);
        }
    }
}
