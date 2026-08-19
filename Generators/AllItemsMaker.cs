using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Melancholy
{
    internal class AllItemsMaker
    {

        public static void GenerateAllItems()
        {
            JArray ItemsJson = JsonConvert.DeserializeObject<JArray>(File.ReadAllText($"IDs/Items.json"));
            JArray OfferingsJson = JsonConvert.DeserializeObject<JArray>(File.ReadAllText($"IDs/Offerings.json"));
            JArray AddonsJson = JsonConvert.DeserializeObject<JArray>(File.ReadAllText($"IDs/Addons.json"));

            ItemsJson.Merge(OfferingsJson);
            ItemsJson.Merge(AddonsJson);

            List<string> strings = new List<string>();

            foreach (JObject Obj in ItemsJson)
            {
                if (!(bool)Obj["ShouldBeInInventory"]) continue;
                strings.Add(Obj["ItemId"].ToString());
            }

            strings.Sort();

            File.WriteAllText($"Files/AllItems.json", JsonConvert.SerializeObject(strings));

            // Perks

            JArray PerkList = JsonConvert.DeserializeObject<JArray>(File.ReadAllText($"IDs/Perks.json"));

            List<string> strings_perks = new List<string>();

            foreach (JObject Obj in PerkList)
            {
                if (!(bool)Obj["ShouldBeInInventory"]) continue;
                strings_perks.Add(Obj["ItemId"].ToString());
            }

            strings_perks.Sort();

            File.WriteAllText($"Files/AllPerks.json", JsonConvert.SerializeObject(strings_perks));


            // Cosmetics

            JArray CosmeticsJSON = JsonConvert.DeserializeObject<JArray>(File.ReadAllText($"IDs/Cosmetics.json"));
            JArray OutfitsJSON = JsonConvert.DeserializeObject<JArray>(File.ReadAllText($"IDs/Outfits.json"));

            List<string> FinalArray = new();

            foreach (JObject CosmeticObject in CosmeticsJSON)
            {
                if ((bool)CosmeticObject["IsExclusive"]) continue;
                FinalArray.Add(CosmeticObject["CosmeticId"].ToString());
            }

            foreach (JObject CosmeticObject in OutfitsJSON)
            {
                FinalArray.Add(CosmeticObject["OutfitId"].ToString());
            }

            FinalArray.Sort();

            File.WriteAllText($"Files/AllCosmetics.json", JsonConvert.SerializeObject(FinalArray));
        }
    }
}