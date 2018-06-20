using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A2_Q5_ThroughSacredGround
    {
        private static readonly TgtPosition AltarTgt = new TgtPosition("Altar location", "dungeon_church_relic_altar_v01_01_c2r1.tgt");

        private static Monster Geofri => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Archbishop_Geofri_the_Abashed)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Chest Altar => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Altar)
            .FirstOrDefault<Chest>();

        private static WalkablePosition CachedGeofriPos
        {
            get => CombatAreaCache.Current.Storage["GeofriPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["GeofriPosition"] = value;
        }

        private static CachedObject CachedAltar
        {
            get => CombatAreaCache.Current.Storage["Altar"] as CachedObject;
            set => CombatAreaCache.Current.Storage["Altar"] = value;
        }

        public static void Tick()
        {
            if (!World.Act2.Crypt2.IsCurrentArea)
                return;

            var geofri = Geofri;
            if (geofri != null)
            {
                CachedGeofriPos = geofri.IsDead ? null : geofri.WalkablePosition();
            }

            if (CachedAltar == null)
            {
                var altar = Altar;
                if (altar != null)
                {
                    CachedAltar = new CachedObject(altar);
                }
            }
        }

        public static async Task<bool> GrabGoldenHand()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.GoldenHand))
                return false;

            if (World.Act2.Crypt2.IsCurrentArea)
            {
                var geofriPos = CachedGeofriPos;
                if (geofriPos != null)
                {
                    await Helpers.MoveAndWait(geofriPos);
                    return true;
                }

                if (await Helpers.OpenQuestChest(CachedAltar))
                    return true;

                AltarTgt.Come();
                return true;
            }
            await Travel.To(World.Act2.Crypt2);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act2.ForestEncampment.IsCurrentArea)
            {
                if (QuestManager.GetState(Quests.ThroughSacredGround) == 0)
                    return false;

                if (Helpers.PlayerHasQuestItem(QuestItemMetadata.BookSacredGround))
                {
                    if (!await Helpers.UseQuestItem(QuestItemMetadata.BookSacredGround))
                        ErrorManager.ReportError();

                    return true;
                }
                if (!await TownNpcs.Yeena.OpenDialogPanel())
                {
                    ErrorManager.ReportError();
                    return true;
                }
                if (LokiPoe.InGameState.NpcDialogUi.DialogEntries.Any(d => d.Text.EqualsIgnorecase("Jewel Reward")))
                {
                    var reward = Settings.Instance.GetRewardForQuest(Quests.ThroughSacredGround.Id + "b");

                    if (!await TownNpcs.Yeena.TakeReward(reward, "Jewel Reward"))
                        ErrorManager.ReportError();

                    return true;
                }
                if (LokiPoe.InGameState.NpcDialogUi.DialogEntries.Any(d => d.Text.EqualsIgnorecase("Fellshrine Reward")))
                {
                    if (!await TownNpcs.Yeena.TakeReward(null, "Fellshrine Reward"))
                        ErrorManager.ReportError();
                }
                return true;
            }
            await Travel.To(World.Act2.ForestEncampment);
            return true;
        }
    }
}