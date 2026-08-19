using Newtonsoft.Json;

namespace Melancholy
{
    public static class Player
    {
        public static int PlayerLevel { get; set; }
        public static int DevotionLevel { get; set; }

        public static async Task Generate_PlayerLevel()
        {
            Classes.Player player = new()
            {
                TotalXp = 1337,
                LevelVersion = 1,
                Level = PlayerLevel,
                PrestigeLevel = DevotionLevel,
                CurrentXp = 1337,
                CurrentXpUpperBound = 1337
            };

            var json = JsonConvert.SerializeObject(player, Formatting.Indented);
            await File.WriteAllTextAsync("Files/PlayerLevel.json", json);
            Console.WriteLine("PlayerLevel.json generated");
        }
    }
}
