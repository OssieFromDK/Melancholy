using Melancholy;
using Newtonsoft.Json;

const string SettingsFile = "settings.json";

try
{
    Console.Title = "DBD OwO";
    Extras.Header();

    if (!Directory.Exists("Files")) Directory.CreateDirectory("Files");
    if (!Directory.Exists("Other")) Directory.CreateDirectory("Other");
    if (!Directory.Exists("IDs")) Directory.CreateDirectory("IDs");

    if (File.Exists(SettingsFile))
    {
        Extras.Settings = JsonConvert.DeserializeObject<Classes.Settings>(File.ReadAllText(SettingsFile));

        Console.WriteLine(
            $"Current settings:\nPak path: {Extras.Settings.PakPath}\nAES key: {Extras.Settings.AesKey}\nMappings path: {Extras.Settings.MappingsPath}\n");
        Console.Write("Would you like to load settings? (Y/n): ");
        switch (Console.ReadLine().ToLower())
        {
            case "no":
            case "n":
                goto PopulateSettings;
            default:
                goto SkipSettings;
        }
    }

PopulateSettings:
    Extras.Settings.PakPath = Extras.PromptInput("Provide path to your paks folder: ");
    Extras.Settings.AesKey = Extras.PromptInput("Provide AES key: ");

    string directoryPath = AppDomain.CurrentDomain.BaseDirectory;
    var usmapFiles = Directory.GetFiles(directoryPath, "*.usmap");

    if (usmapFiles.Length == 0)
        throw new Exception($"No .usmap files found, contact @bhvr for the latest file or place your own in {directoryPath}");

    Console.WriteLine("Select a mappings file by entering its number:");
    for (int i = 0; i < usmapFiles.Length; i++)
    {
        Console.WriteLine($"{i + 1}. {Path.GetFileName(usmapFiles[i])}");
    }

    int choice = 0;
    while (true)
    {
        Console.Write("Enter your choice (number): ");
        if (int.TryParse(Console.ReadLine(), out choice) && choice > 0 && choice <= usmapFiles.Length) break;
        Console.WriteLine("Invalid choice, try again...");
    }

    string selectedFile = usmapFiles[choice - 1];
    Extras.Settings.MappingsPath = selectedFile;

    File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(Extras.Settings, Formatting.Indented));

SkipSettings:
    Console.Clear();
    Extras.Header();

    Console.WriteLine("Doing CUE4Parse stuffs...");

    Cue4Parse.Initialize();
    Cue4Parse.CdnAccessKey = Cue4Parse.GetAccessKey();
    Cue4Parse.Get_Files();

    if (Cue4Parse.IsListEmpty())
        throw new Exception("Mappings file outdated, contact @bhvr on Discord for the new mappings file.");

    Market.ItemAmount = Extras.PromptIntInput("Enter desired item amount (0 for random): ");
    
    Market.PrestigeLevel = Extras.PromptIntInput("Enter desired prestige for characters (0 for random): ");
    if (Market.PrestigeLevel == 0)
    {
        Market.PrestigeLevelMinimum = Extras.PromptIntInput("Enter minimum desired prestige: ");
        Market.PrestigeLevelMaximum = Extras.PromptIntInput("Enter maximum desired prestige: ");
    }
    
    Player.PlayerLevel = Extras.PromptIntInput("Enter desired player level: ");
    Player.DevotionLevel = Extras.PromptIntInput("Enter desired devotion level: ");

    Console.Clear();
    Extras.Header();

    string ShouldHaveNonInventoryItems = Extras.PromptOptionInput("Should non-inventory items be included such as Onryo Tape, Wesker Spray, etc...? (y/N): ");
    Extras.AddNonInventoryItems = ShouldHaveNonInventoryItems is "yes" or "y";
    
    string ShouldHaveRetiredOfferings = Extras.PromptOptionInput("Should retired offerings be included such as killer splinters? (y/N): ");
    Extras.AddRetiredOfferings = ShouldHaveRetiredOfferings is "yes" or "y";
    
    string ShouldHaveEventItems = Extras.PromptOptionInput("Should event items be included such as anniversary cakes? (Y/n): ");
    Extras.AddEventItems = ShouldHaveEventItems is not ("no" or "n");
   
    string ShouldHaveBannersBadges = Extras.PromptOptionInput("Should banners and badges be included? (Y/n): ");
    Extras.AddBannersBadges = ShouldHaveBannersBadges is not ("no" or "n");

    string ShouldHaveLegacy = Extras.PromptOptionInput("Should legacy (pre 2016) prestige clothing be added? (Y/n): ");
    Extras.AddLegacy = ShouldHaveLegacy is not ("no" or "n");

    string ShouldHaveScaryItems = Extras.PromptOptionInput("Should \"scary\" things be included (like dev only items etc.)? (y/N): ");
    Extras.AddScaryItems = ShouldHaveScaryItems is ("yes" or "y");

    Console.Clear();
    Extras.Header();

    //await Cdn.GetCdnFiles(); Don't care about CDN

    await Extras.MakeFile(Classes.Ids.CosmeticIds, "Cosmetics.json");
    await Extras.MakeFile(Classes.Ids.OutfitIds, "Outfits.json");
    await Extras.MakeFile(Classes.Ids.DlcIds, "Dlc.json");
    await Extras.MakeFile(Classes.Ids.ItemIds, "Items.json");
    await Extras.MakeFile(Classes.Ids.AddonIds, "Addons.json");
    await Extras.MakeFile(Classes.Ids.OfferingIds, "Offerings.json");
    await Extras.MakeFile(Classes.Ids.PerkIds, "Perks.json");

    await GeneratePrestigeList.GenerateList();

    await Market.Generate_Market("all");
    await Market.Generate_Market("dlconly");
    await Market.Generate_Market("notemp");
    await Market.Generate_Market("tempNoCosmetics");
    await Market.Generate_Market("perks");
    Console.WriteLine("Market files generated");

    await GetAll.Generate_GetAll();
    Console.WriteLine("GetAll.json generated");

    await Bloodweb.Generate_Bloodweb();
    Console.WriteLine("Bloodweb.json generated");

    await Player.Generate_PlayerLevel();

    AllItemsMaker.GenerateAllItems();

    Console.WriteLine("\nPress any key to close...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.ReadLine();
}