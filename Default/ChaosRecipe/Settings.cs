using Loki;
using Loki.Common;

namespace Default.ChaosRecipe
{
    public class Settings : JsonSettings
    {
        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = new Settings());

        private Settings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "ChaosRecipe.json"))
        {
        }

        public string StashTab { get; set; }
        public int MinILvl { get; set; } = 60;
        public bool AlwaysUpdateStashData { get; set; }
        public int[] MaxItemCounts { get; set; } = {2, 2, 2, 2, 2, 10, 20, 20};

        public int GetMaxItemCount(int itemType)
        {
            return MaxItemCounts[itemType];
        }
    }
}