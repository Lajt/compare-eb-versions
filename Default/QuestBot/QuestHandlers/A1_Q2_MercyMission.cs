using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;
using System.Linq;

namespace Default.QuestBot.QuestHandlers
{
    public static class A1_Q2_MercyMission
    {
        private static readonly TgtPosition MedicineChestTgt = new TgtPosition("Medicine Chest location", "kyrenia_boat_medicinequest_v01_01_c3r2.tgt");

        private static Monster Hailrake => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Hailrake)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static WalkablePosition CachedHailrakePos
        {
            get => CombatAreaCache.Current.Storage["HailrakePosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["HailrakePosition"] = value;
        }

        public static void Tick()
        {
            if (!World.Act1.TidalIsland.IsCurrentArea)
                return;

            var hailrake = Hailrake;
            if (hailrake != null)
            {
                CachedHailrakePos = hailrake.WalkablePosition();
            }
        }

        public static async Task<bool> KillHailrake()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.MedicineChest))
                return false;

            if (World.Act1.TidalIsland.IsCurrentArea)
            {
                var hailrakePos = CachedHailrakePos;
                if (hailrakePos != null)
                {
                    await Helpers.MoveAndWait(hailrakePos);
                }
                else
                {
                    MedicineChestTgt.Come();
                }
                return true;
            }
            await Travel.To(World.Act1.TidalIsland);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            if (World.Act1.LioneyeWatch.IsCurrentArea)
            {
                if (QuestManager.GetState(Quests.MercyMission) == 0)
                    return false;

                if (!await TownNpcs.Nessa.OpenDialogPanel())
                {
                    ErrorManager.ReportError();
                    return true;
                }
                if (LokiPoe.InGameState.NpcDialogUi.DialogEntries.Any(d => d.Text.EqualsIgnorecase("Medicine Chest Reward 2")))
                {
                    var reward = Settings.Instance.GetRewardForQuest(Quests.MercyMission.Id + "b");

                    if (!await TownNpcs.Nessa.TakeReward(reward, "Medicine Chest Reward 2"))
                        ErrorManager.ReportError();

                    return true;
                }
                if (LokiPoe.InGameState.NpcDialogUi.DialogEntries.Any(d => d.Text.EqualsIgnorecase("Medicine Chest Reward")))
                {
                    var reward = Settings.Instance.GetRewardForQuest(Quests.MercyMission.Id);

                    if (!await TownNpcs.Nessa.TakeReward(reward, "Medicine Chest Reward"))
                        ErrorManager.ReportError();
                }
                return true;
            }
            await Travel.To(World.Act1.LioneyeWatch);
            return true;
        }
    }
}