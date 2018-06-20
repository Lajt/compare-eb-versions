using System.Collections.ObjectModel;
using Loki;
using Loki.Common;
using Newtonsoft.Json;

namespace Default.Chicken
{
    public class Settings : JsonSettings
    {
        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = new Settings());

        private Settings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "Chicken.json"))
        {
        }

        public bool HpEnabled { get; set; }
        public bool EsEnabled { get; set; }

        public int HpThreshold { get; set; } = 30;
        public int EsThreshold { get; set; } = 30;

        public bool OnSightEnabled { get; set; }
        public ObservableCollection<MonsterEntry> Monsters { get; set; } = new ObservableCollection<MonsterEntry>();

        [JsonIgnore]
        public static OnSightAction[] OnSightActions { get; set; } = {OnSightAction.Chicken, OnSightAction.Ignore};

        public class MonsterEntry
        {
            private const string DefaultName = "UniqueMonsterName";

            private string _name = DefaultName;

            public string Name
            {
                get => _name;
                set => _name = string.IsNullOrWhiteSpace(value) ? DefaultName : value.Trim();
            }

            public int Range { get; set; } = 250;
            public OnSightAction Action { get; set; }
        }
    }

    public enum OnSightAction
    {
        Chicken,
        Ignore
    }
}