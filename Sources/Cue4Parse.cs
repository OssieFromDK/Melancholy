using CUE4Parse.Compression;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.GameTypes.DBD.Encryption.Aes;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace Melancholy
{
    public static partial class Cue4Parse
    {
        private static DefaultFileProvider? Provider { get; set; }
        public static string? CdnAccessKey { get; set; }

        public static void Initialize()
        {
            string? oodlePath = OodleHelper.OodleFileName;

            OodleHelper.DownloadOodleDll(ref oodlePath);
            if (!File.Exists(oodlePath))
            {
                Console.WriteLine($"Oodle DLL not found. Please ensure it is present in the working directory.");
                return;
            }

            OodleHelper.Initialize(oodlePath);
            ZlibHelper.Initialize();

            var versionContainer = new VersionContainer(EGame.GAME_DeadByDaylight);
            Provider = new DefaultFileProvider(
                new DirectoryInfo(Extras.Settings.PakPath),
                SearchOption.AllDirectories,
                versionContainer,
                StringComparer.OrdinalIgnoreCase
            );

            Provider.CustomEncryption = DBDAes.DbDDecrypt;

            Provider.Initialize();

            LoadDynamicContentPaks();

            Provider.MappingsContainer = new FileUsmapTypeMappingsProvider(Extras.Settings.MappingsPath);

            var aesKey = new FAesKey(Extras.Settings.AesKey);

            var requiredGuids = Provider.RequiredKeys.ToList();
            foreach (var guid in requiredGuids)
            {
                Provider.SubmitKey(guid, aesKey);
            }

            Provider.PostMount();

            var aesMax = Provider.RequiredKeys.Count + Provider.Keys.Count;
            var archiveMax = Provider.UnloadedVfs.Count + Provider.MountedVfs.Count;

            Provider.LoadVirtualPaths();

            Provider.TryChangeCulture(Provider.GetLanguageCode(ELanguage.English));
        }

        private static void LoadDynamicContentPaks()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var root = Path.Combine(localAppData, "DeadByDaylight", "Saved", "PersistentDownloadDir", "DynamicContent");
            if (!Directory.Exists(root)) return;

            foreach (var contentDir in Directory.EnumerateDirectories(root))
            {
                Console.WriteLine($"Found Dynamic Content directory {Path.GetFileName(contentDir)}, mounting provider");

                var paksRoot = Path.Combine(contentDir, "Content", "Paks");
                if (!Directory.Exists(paksRoot)) continue;

                foreach (var pattern in new[] { "*.pak", "*.utoc" })
                foreach (var file in Directory.EnumerateFiles(paksRoot, pattern, SearchOption.AllDirectories))
                    Provider!.RegisterVfs(file);
            }
        }

        public static string GetAccessKey()
        {
            if (!Provider.TrySaveAsset("/Game/Config/DefaultGame.ini", out var data) || data is null)
            {
                var candidate = Provider.Files.Values.FirstOrDefault(f => string.Equals(f.Name, "DefaultGame.ini", StringComparison.OrdinalIgnoreCase));

                if (candidate is null)
                {
                    return string.Empty;
                }

                if (!Provider.TrySaveAsset(candidate.Path, out data) || data is null)
                {
                    return string.Empty;
                }
            }

            var lastLine = "";

            using (var stream = new MemoryStream(data))
            using (var reader = new StreamReader(stream))
            {
                while (reader.ReadLine() is { } line)
                {
                    if (line.Contains("_live"))
                        lastLine = line;
                }
            }

            var match = MyRegex().Match(lastLine);
            if (!match.Success)
            {
                return string.Empty;
            }

            string replaced = match.Groups[1].Value.Replace("_", "/").Replace("-", "+");
            return replaced;
        }

        public static void Get_Files()
        {
            foreach (var kvp in Provider.Files)
            {
                switch (kvp.Value.Name)
                {
                    case "CustomizationItemDB.uasset":
                        Classes.FilePaths.CustomizationItemDb.Add(kvp.Value.Path); break;
                    case "OutfitDB.uasset":
                        Classes.FilePaths.OutfitDb.Add(kvp.Value.Path); break;
                    case "CharacterDescriptionDB.uasset":
                        Classes.FilePaths.CharacterDescriptionDb.Add(kvp.Value.Path); break;
                    case "ItemDB.uasset":
                        Classes.FilePaths.ItemDb.Add(kvp.Value.Path); break;
                    case "ItemAddonDB.uasset":
                        Classes.FilePaths.ItemAddonDb.Add(kvp.Value.Path); break;
                    case "OfferingDB.uasset":
                        Classes.FilePaths.OfferingDb.Add(kvp.Value.Path); break;
                    case "PerkDB.uasset":
                        Classes.FilePaths.PerkDb.Add(kvp.Value.Path); break;
                }

                /*/* Alternative method for adding cosmetics can be found in Cdn.cs - simply comment this part out
                 and uncomment in Cdn.cs - only necessary if BHVR decide to change game files to break this
                 #1#
                bool hasNestedIconsFolder = kvp.Value.Path.Split('/')
                                                .SkipWhile(part => part != "PlayerCards")
                                                .Any(sub => sub == "Icons" || sub == "HF2") &&
                                            !kvp.Value.NameWithoutExtension.EndsWith("_icon");

                bool isCustomizationOrPlayerCardsPath = kvp.Value.Path.Contains("UMGAssets/Icons/Customization") ||
                                                        kvp.Value.Path.Contains("UMGAssets/Icons/PlayerCards") &&
                                                        hasNestedIconsFolder;

                if (isCustomizationOrPlayerCardsPath) Classes.Ids.CosmeticIds.Add(kvp.Value.NameWithoutExtension);*/
            }

            Add_Values(Classes.FilePaths.CustomizationItemDb, "CustomizationItemDB");
            Add_Values(Classes.FilePaths.OutfitDb, "OutfitDB");
            Add_Values(Classes.FilePaths.CharacterDescriptionDb, "CharacterDescriptionDB");
            Add_Values(Classes.FilePaths.ItemDb, "ItemDB");
            Add_Values(Classes.FilePaths.ItemAddonDb, "ItemAddonDB");
            Add_Values(Classes.FilePaths.OfferingDb, "OfferingDB");
            Add_Values(Classes.FilePaths.PerkDb, "PerkDB");
        }

        private static List<string> exclusiveStrings = new List<string>
        {
            "Twitch",
            "PandaTV",
            "CartoonZ",
            "Smile",
            "AdmBahroo",
            "BloodLetting",
            "KingKong",
            "Bryce",
            "AngryPug",
            "Sxyhxy",
            "72hrs",
            "Crew01",
            "ME_007",
            "ME_008",
            "AP_008",
            "UN_Charm005",
            "UN_Charm003",
            "BA_BDG_99",
            "BA_BNR_99",
            "HU_041",
            "UN_Charm004",
            "ZO_BNR_09",
            "ZO_BDG_11",
            "DF_Torso03_01",
            "Wraith_Body_Ohmwrecker"
        };

        private static void Add_Values(List<string> list, string type)
        {
            foreach (var item in list)
            {
                var export =
                    JsonConvert.DeserializeObject<dynamic>(
                        JsonConvert.SerializeObject(Provider.LoadPackageObjects(item))) ?? new ExpandoObject();

                foreach (JProperty p in export[0]?.Rows ?? Enumerable.Empty<JProperty>())
                {
                    var properties = p.Values<JObject>();
                    foreach (var property in properties)
                    {
                        switch (type)
                        {
                            case "CustomizationItemDB":
                                string customizationId = property.Ci("customizationId")?.ToString() ?? string.Empty;
                                string localizedString = property.Ci("UIData").Ci("DisplayName").Ci("LocalizedString")?.ToString() ?? string.Empty;

                                Classes.Customization customization = new()
                                {
                                    CosmeticId = customizationId,
                                    CosmeticName = localizedString,
                                    CosmeticDescription = property.Ci("UIData").Ci("Description").Ci("LocalizedString")?.ToString() ?? string.Empty,
                                    Category = property.Ci("category")?.ToString() ?? string.Empty,
                                    AssociatedCharacterIndex = property.Ci("AssociatedCharacter")?.ToString() ?? string.Empty,
                                    Rarity = property.Ci("Rarity")?.ToString() ?? string.Empty,
                                    IsInStore = property.Ci("IsInStore")?.ToString() ?? string.Empty,
                                    EventId = property.Ci("eventID")?.ToString() ?? string.Empty,
                                    Availability = property.Ci("Availability").Ci("ItemAvailability")?.ToString() ?? string.Empty,
                                    FilePath = item ?? string.Empty,
                                    IsLegacy = property.Ci("InclusionVersion")?.ToString() == "Legacy" && Regex.IsMatch(localizedString, @"Legacy.*\b(I|II|III)\b", RegexOptions.IgnoreCase),
                                    IsExclusive = exclusiveStrings.Any(s => customizationId.Contains(s) && !customizationId.Contains("Charity")) || (localizedString.Contains("Twitchy") && !(localizedString.Contains("Flame") || localizedString.Contains("Medkit")))
                                };
                                if (!IsInBlacklist(customization.CosmeticId)) Classes.Ids.CosmeticIds.Add(customization);
                                break;
                            case "OutfitDB":
                                Classes.Outfit outfit = new()
                                {
                                    OutfitId = property.Ci("ID")?.ToString() ?? string.Empty,
                                    OutfitName = property.Ci("UIData").Ci("DisplayName").Ci("LocalizedString")?.ToString() ?? string.Empty,
                                    OutfitDescription = property.Ci("UIData").Ci("Description").Ci("LocalizedString")?.ToString() ?? string.Empty,
                                    CollectionName = property.Ci("CollectionName").Ci("LocalizedString")?.ToString() ?? string.Empty,
                                    Availability = property.Ci("Availability").Ci("ItemAvailability")?.ToString() ?? string.Empty,
                                    FilePath = item ?? string.Empty
                                };
                                if (!IsInBlacklist(outfit.OutfitId)) Classes.Ids.OutfitIds.Add(outfit);
                                break;
                            case "CharacterDescriptionDB":
                                if (property.Ci("CharacterId")?.ToString() == "None") continue;
                                Classes.Character character = new()
                                {
                                    CharacterName = property.Ci("CharacterId")?.ToString() ?? string.Empty,
                                    CharacterIndex = property.Ci("characterIndex")?.ToString() ?? string.Empty,
                                    CharacterType = property.Ci("Role")?.ToString() ?? string.Empty,
                                    CharacterDefaultItem = property.Ci("DefaultItem")?.ToString() ?? string.Empty,
                                    Name = property.Ci("DisplayName").Ci("LocalizedString")?.ToString() ?? string.Empty,
                                    FilePath = item ?? string.Empty
                                };
                                Classes.Ids.DlcIds.Add(character);
                                break;
                            case "ItemDB":
                                if (property.Ci("Type")?.ToString() != "EInventoryItemType::Power")
                                {
                                    Classes.ItemOfferingPerk itemData = new()
                                    {
                                        ItemId = property.Ci("ItemId")?.ToString() ?? string.Empty,
                                        CharacterType = property.Ci("Role")?.ToString() ?? string.Empty,
                                        Rarity = property.Ci("Rarity")?.ToString() ?? string.Empty,
                                        Availability = property.Ci("Availability").Ci("ItemAvailability")?.ToString() ?? string.Empty,
                                        Name = property.Ci("UIData").Ci("DisplayName").Ci("LocalizedString")?.ToString() ?? string.Empty,
                                        FilePath = item ?? string.Empty,
                                        ShouldBeInInventory = property.Ci("Inventory")?.Value<bool>() ?? true,
                                        EventId = property.Ci("eventID")?.ToString() ?? string.Empty
                                    };
                                    if (!IsInBlacklist(itemData.ItemId)) Classes.Ids.ItemIds.Add(itemData);
                                }
                                break;
                            case "ItemAddonDB":
                                Classes.ItemAddon itemAddon = new()
                                {
                                    ItemId = property.Ci("ItemId")?.ToString() ?? string.Empty,
                                    CharacterType = property.Ci("Role")?.ToString() ?? string.Empty,
                                    CharacterDefaultItem = property.Ci("ParentItem").Ci("itemIds")?.Count() > 0 ? (property.Ci("ParentItem").Ci("itemIds")?[0]?.ToString() ?? string.Empty) : string.Empty,
                                    Rarity = property.Ci("Rarity")?.ToString() ?? string.Empty,
                                    Availability = property.Ci("Availability").Ci("ItemAvailability")?.ToString() ?? string.Empty,
                                    Name = property.Ci("UIData").Ci("DisplayName").Ci("LocalizedString")?.ToString() ?? string.Empty,
                                    FilePath = item ?? string.Empty,
                                    ShouldBeInInventory = property.Ci("Inventory")?.Value<bool>() ?? true,
                                    EventId = property.Ci("eventID")?.ToString() ?? string.Empty
                                };
                                if (!IsInBlacklist(itemAddon.ItemId)) Classes.Ids.AddonIds.Add(itemAddon);
                                break;
                            case "OfferingDB":
                                Classes.ItemOfferingPerk offering = new()
                                {
                                    ItemId = property.Ci("ItemId")?.ToString() ?? string.Empty,
                                    CharacterType = property.Ci("Role")?.ToString() ?? string.Empty,
                                    Rarity = property.Ci("Rarity")?.ToString() ?? string.Empty,
                                    Availability = property.Ci("Availability").Ci("ItemAvailability")?.ToString() ?? string.Empty,
                                    Name = property.Ci("UIData").Ci("DisplayName").Ci("LocalizedString")?.ToString() ?? string.Empty,
                                    FilePath = item ?? string.Empty,
                                    ShouldBeInInventory = property.Ci("Inventory")?.Value<bool>() ?? true,
                                    EventId = property.Ci("eventID")?.ToString() ?? string.Empty
                                };
                                if (!IsInBlacklist(offering.ItemId)) Classes.Ids.OfferingIds.Add(offering);
                                break;
                            case "PerkDB":
                                Classes.ItemOfferingPerk perk = new()
                                {
                                    ItemId = property.Ci("ItemId")?.ToString() ?? string.Empty,
                                    CharacterType = property.Ci("Role")?.ToString() ?? string.Empty,
                                    Rarity = property.Ci("Rarity")?.ToString() ?? string.Empty,
                                    Availability = property.Ci("Availability").Ci("ItemAvailability")?.ToString() ?? string.Empty,
                                    Name = property.Ci("UIData").Ci("DisplayName").Ci("LocalizedString")?.ToString() ?? string.Empty,
                                    FilePath = item ?? string.Empty,
                                    ShouldBeInInventory = property.Ci("Inventory")?.Value<bool>() ?? true,
                                    EventId = property.Ci("eventID")?.ToString() ?? string.Empty
                                };
                                if (!IsInBlacklist(perk.ItemId)) Classes.Ids.PerkIds.Add(perk);
                                break;
                        }
                    }
                }
            }
        }

        public static bool IsListEmpty()
        {
            return Classes.Ids.CosmeticIds.Count == 0;
        }

        private static bool IsInBlacklist(string id)
        {
            if (!File.Exists("blacklist.json"))
            {
                var blacklist = new
                {
                    IDs = new List<string>()
                {
                    "Item_LamentConfiguration"
                }
                };

                File.WriteAllText("blacklist.json", JsonConvert.SerializeObject(blacklist, Formatting.Indented));
            }

            string blacklistContent = File.ReadAllText("blacklist.json");
            JObject json = JObject.Parse(blacklistContent);

            bool isBlacklisted = ((JArray)json["IDs"]!)
                .Select(v => (string?)v)
                .Any(blacklistId => blacklistId == id);

            return isBlacklisted;
        }

        [GeneratedRegex(@"Key=""(.*?)""")]
        private static partial Regex MyRegex();
    }

    internal static class JsonExt
    {
        public static JToken? Ci(this JToken? t, string name) => t is JObject o && o.TryGetValue(name, StringComparison.OrdinalIgnoreCase, out var v) ? v : null;
    }
}
