using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A3_Q2_VictarioSecrets
    {
        public const int MinHaveAllBustsState = 3;

        private static bool _haveAllBusts;

        public static readonly HashSet<int> HaveAllBustsStates = new HashSet<int>
        {
            5,
            7,
            9,
            13,
            17,
            21
        };

        private static bool HaveAllBustsByState
        {
            get
            {
                var state = QuestManager.GetStateInaccurate(Quests.VictarioSecrets);
                return state <= MinHaveAllBustsState || HaveAllBustsStates.Contains(state);
            }
        }

        private static IEnumerable<Chest> VictarioStashes => LokiPoe.ObjectManager.Objects
            .Where<Chest>(c => c.Metadata.Contains("QuestChests/Victario/Stash"));

        private static List<CachedObject> CachedVictarioStashes
        {
            get
            {
                var stashes = CombatAreaCache.Current.Storage["VictarioStashes"] as List<CachedObject>;
                if (stashes == null)
                {
                    stashes = new List<CachedObject>(3);
                    CombatAreaCache.Current.Storage["VictarioStashes"] = stashes;
                }
                return stashes;
            }
        }

        public static void Tick()
        {
            _haveAllBusts = Helpers.PlayerHasQuestItemAmount(QuestItemMetadata.Bust, 3) || HaveAllBustsByState;

            if (!World.Act3.Sewers.IsCurrentArea)
                return;

            foreach (var stash in VictarioStashes)
            {
                var opened = stash.IsOpened;
                var id = stash.Id;
                var cachedStashes = CachedVictarioStashes;
                var index = cachedStashes.FindIndex(c => c.Id == stash.Id);

                if (index >= 0)
                {
                    if (opened)
                    {
                        GlobalLog.Warn($"[VictarioSecrets] Removing opened {stash.WalkablePosition()}");
                        cachedStashes.RemoveAt(index);
                    }
                }
                else
                {
                    if (!opened)
                    {
                        var pos = stash.WalkablePosition();
                        GlobalLog.Warn($"[VictarioSecrets] Registering {pos}");
                        cachedStashes.Add(new CachedObject(id, pos));
                    }
                }
            }
        }

        public static async Task<bool> GrabBusts()
        {
            if (_haveAllBusts)
                return false;

            if (World.Act3.Sewers.IsCurrentArea)
            {
                if (await Helpers.OpenQuestChest(CachedVictarioStashes.FirstOrDefault()))
                    return true;

                await Helpers.Explore();
                return true;
            }
            await Travel.To(World.Act3.Sewers);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act3.SarnEncampment,
                TownNpcs.Hargan,
                "Platinum Bust Reward",
                book: QuestItemMetadata.BookVictario);
        }
    }
}