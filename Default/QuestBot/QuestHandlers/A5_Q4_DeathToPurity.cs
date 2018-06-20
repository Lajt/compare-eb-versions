using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A5_Q4_DeathToPurity
    {
        private static readonly TgtPosition SanctumOfInnocenceTgt = new TgtPosition("Sanctum of Innocence", "transition_chamber_to_boss_v01_01_c4r4.tgt");

        private static NetworkObject _avariusRoomObj;
        private static Monster _avarius;
        private static Monster _innocence;
        private static Npc _sin;

        public static void Tick()
        {
        }

        public static async Task<bool> KillAvarius()
        {
            if (World.Act5.ChamberOfInnocence.IsCurrentArea)
            {
                UpdateAvariusFightObjects();

                if (_avariusRoomObj != null)
                {
                    if (_sin != null)
                        return false;

                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Avarius))
                        return true;

                    if (_innocence != null)
                    {
                        await Helpers.MoveToBossOrAnyMob(_innocence);
                        return true;
                    }
                    if (_avarius != null)
                    {
                        await Helpers.MoveToBossOrAnyMob(_avarius);
                        return true;
                    }
                    await Helpers.MoveAndWait(_avariusRoomObj.WalkablePosition(), "Waiting for any Avarius fight object");
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(SanctumOfInnocenceTgt);
                return true;
            }
            await Travel.To(World.Act5.ChamberOfInnocence);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act5.OverseerTower,
                TownNpcs.Lani,
                "Avarius Reward",
                Quests.DeathToPurity.Id);
        }

        private static void UpdateAvariusFightObjects()
        {
            _avariusRoomObj = null;
            _avarius = null;
            _innocence = null;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var metadata = obj.Metadata;

                var mob = obj as Monster;
                if (mob != null && mob.Rarity == Rarity.Unique)
                {
                    if (metadata == "Metadata/Monsters/AvariusCasticus/AvariusCasticus")
                    {
                        _avarius = mob;
                    }
                    else if (metadata == "Metadata/Monsters/AvariusCasticus/AvariusCasticusDivine")
                    {
                        _innocence = mob;
                    }
                    continue;
                }
                var npc = obj as Npc;
                if (npc != null && npc.Metadata == "Metadata/NPC/Act5/SinInnocence")
                {
                    _sin = npc;
                    continue;
                }
                if (metadata == "Metadata/Monsters/AvariusCasticus/ArenaMiddle")
                {
                    _avariusRoomObj = obj;
                }
            }
        }
    }
}