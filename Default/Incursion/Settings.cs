using System;
using System.Collections.Generic;
using Loki;
using Loki.Common;
using Newtonsoft.Json;

namespace Default.Incursion
{
    public class Settings : JsonSettings
    {
        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = new Settings());

        private Settings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "Incursion.json"))
        {
            InitRoomList();
            IncursionRooms.Sort((r1, r2) => string.CompareOrdinal(r1.Name, r2.Name));
        }

        public bool PortalBeforeIncursion { get; set; }
        public bool LeaveAfterIncursion { get; set; }
        public List<RoomEntry> IncursionRooms { get; set; } = new List<RoomEntry>();

        public bool SkipTemple { get; set; }
        public bool IgnoreBossroom { get; set; }
        public bool TrackMobInTemple { get; set; } = true;
        public int ExplorationPercent { get; set; } = 90;

        private static List<RoomEntry> GetDefaultRoomList()
        {
            return new List<RoomEntry>
            {
                new RoomEntry("Poison Garden", 56998, 35549, 7899),
                new RoomEntry("Sacrificial Chamber", 50849, 53673, 63046),
                new RoomEntry("Tempest Generator", 37180, 6913, 5133),
                new RoomEntry("Trap Workshop", 4946, 14070, 11681),
                new RoomEntry("Surveyor's Study", 54001, 61696, 54094),
                new RoomEntry("Royal Meeting Room", 42889, 12574, 63579),
                new RoomEntry("Storage Room", 25231, 63562, 49758),
                new RoomEntry("Corruption Chamber", 7940, 961, 57960),
                new RoomEntry("Explosives Room", 51256, 12263, 62364),
                new RoomEntry("Armourer's Workshop", 47840, 33086, 51778),
                new RoomEntry("Sparring Room", 20517, 46313, 57246),
                new RoomEntry("Guardhouse", 47693, 23727, 39495),
                new RoomEntry("Splinter Research Lab", 36698, 48392, 10187),
                new RoomEntry("Gemcutter's Workshop", 29190, 33628, 45452),
                new RoomEntry("Vault", 36945, 32923, 1833),
                new RoomEntry("Jeweller's Workshop", 56310, 57786, 6047),
                new RoomEntry("Workshop", 27815, 11869, 55471),
                new RoomEntry("Shrine of Empowerment", 1623, 47887, 36762),
                new RoomEntry("Pools of Restoration", 29045, 46208, 16697),
                new RoomEntry("Hatchery", 28140, 27848, 59890),
                new RoomEntry("Flame Workshop", 22115, 565, 19827),
                new RoomEntry("Lightning Workshop", 62908, 42207, 65226)
            };
        }

        private void InitRoomList()
        {
            if (IncursionRooms.Count == 0)
            {
                IncursionRooms = GetDefaultRoomList();
            }
            else
            {
                var defaultRooms = GetDefaultRoomList();
                foreach (var dEntry in defaultRooms)
                {
                    var jEntry = IncursionRooms.Find(r => r.Name == dEntry.Name);
                    if (jEntry != null)
                    {
                        dEntry.PriorityAction = jEntry.PriorityAction;
                        dEntry.NoChange = jEntry.NoChange;
                        dEntry.NoUpgrade = jEntry.NoUpgrade;
                    }
                }
                IncursionRooms = defaultRooms;
            }
        }

        [JsonIgnore]
        public static readonly PriorityAction[] PriorityActions =
        {
            PriorityAction.Doors, PriorityAction.Changing, PriorityAction.Upgrading
        };
    }

    public class RoomEntry
    {
        private readonly int[] _ids = new int[3];

        public string Name { get; set; }
        public PriorityAction PriorityAction { get; set; }
        public bool NoChange { get; set; }
        public bool NoUpgrade { get; set; }

        public RoomEntry(string name, int id1, int id2, int id3)
        {
            Name = name;
            _ids[0] = id1;
            _ids[1] = id2;
            _ids[2] = id3;
        }

        public bool IdEquals(int id)
        {
            return Array.IndexOf(_ids, id) >= 0;
        }
    }

    public enum PriorityAction
    {
        Doors,
        Changing,
        Upgrading
    }
}