using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A10_Q6_EndToHunger
    {
        private static readonly TgtPosition KivataRoomTgt = new TgtPosition("Kitava room", "act10_kitava_arena_v01_01_c12r13.tgt");

        private static readonly WalkablePosition KitavaWalkablePos = new WalkablePosition("Walkable position in front of Kitava", 1830, 3155);

        private static Npc _sin;
        private static Monster _kitava;
        private static Monster _kitavaHeart;

        public static void Tick()
        {
        }

        public static async Task<bool> KillKitava()
        {
            if (World.Act10.FeedingTrough.IsCurrentArea)
            {
                UpdateKitavaFightObjects();

                if (KitavaWalkablePos.PathExists)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Kitava2))
                        return true;

                    if (_kitavaHeart != null && _kitavaHeart.IsTargetable)
                    {
                        await Helpers.MoveAndWait(_kitavaHeart.WalkablePosition());
                        return true;
                    }
                    if (_kitava != null)
                    {
                        if (_kitava.IsDead)
                        {
                            await Wait.For(() => World.Act11.Oriath.IsCurrentArea, "Waiting for Kitava fight ending", 500, 7000);
                            return false;
                        }
                        if (!_kitava.IsActive)
                        {
                            await Helpers.MoveAndWait(KitavaWalkablePos, "Waiting for Kitava, the Insatiable");
                        }
                        else
                        {
                            KitavaWalkablePos.Come();
                        }
                    }
                    return true;
                }
                if (_sin != null && _sin.IsTargetable && _sin.HasNpcFloatingIcon)
                {
                    var pos = _sin.WalkablePosition();
                    if (pos.IsFar)
                    {
                        pos.Come();
                    }
                    else
                    {
                        await Helpers.TalkTo(_sin);
                    }
                    return true;
                }
                await Helpers.MoveAndTakeLocalTransition(KivataRoomTgt);
                return true;
            }

            if (World.Act11.Oriath.IsCurrentArea)
                return false;

            await Travel.To(World.Act10.FeedingTrough);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act11.Oriath,
                TownNpcs.Lani_A11,
                "Kitava Reward",
                book: QuestItemMetadata.BookKitava);
        }

        private static void UpdateKitavaFightObjects()
        {
            _sin = null;
            _kitava = null;
            _kitavaHeart = null;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var mob = obj as Monster;
                if (mob != null && mob.Rarity == Rarity.Unique)
                {
                    var name = mob.Name;
                    if (name == "Kitava, the Insatiable")
                    {
                        _kitava = mob;
                    }
                    else if (name == "Kitava's Heart")
                    {
                        _kitavaHeart = mob;
                    }
                    continue;
                }
                var npc = obj as Npc;
                if (npc != null && npc.Metadata == "Metadata/NPC/Act10/SinTrough")
                {
                    _sin = npc;
                }
            }
        }
    }
}