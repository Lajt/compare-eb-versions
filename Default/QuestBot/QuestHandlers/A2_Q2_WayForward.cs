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
    public static class A2_Q2_WayForward
    {
        private static readonly TgtPosition ArteriTgt = new TgtPosition("Captain Arteri location", "tent_mid_square_alteri_v01_01_c2r1.tgt");
        private static readonly TgtPosition SealTgt = new TgtPosition("Thaumetic Seal location", "forest_mountainpass_v01_01_c2r4.tgt");

        private static Monster CaptainArteri => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Captain_Arteri)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static NetworkObject ThaumeticSeal => LokiPoe.ObjectManager.Objects
            .Find(o => o.Metadata == "Metadata/QuestObjects/Spikes/SpikeBlockageButton");

        private static WalkablePosition CachedArteriPos
        {
            get => CombatAreaCache.Current.Storage["ArteriPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["ArteriPosition"] = value;
        }

        private static CachedObject CachedSeal
        {
            get => CombatAreaCache.Current.Storage["ThaumeticSeal"] as CachedObject;
            set => CombatAreaCache.Current.Storage["ThaumeticSeal"] = value;
        }

        public static void Tick()
        {
            if (!World.Act2.WesternForest.IsCurrentArea)
                return;

            var arteri = CaptainArteri;
            if (arteri != null)
            {
                CachedArteriPos = arteri.WalkablePosition();
            }

            if (CachedSeal == null)
            {
                var seal = ThaumeticSeal;
                if (seal != null)
                {
                    CachedSeal = new CachedObject(seal);
                }
            }
        }

        public static async Task<bool> KillArteri()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.ThaumeticEmblem))
                return false;

            if (World.Act2.WesternForest.IsCurrentArea)
            {
                var arteriPos = CachedArteriPos;
                if (arteriPos != null)
                {
                    await Helpers.MoveAndWait(arteriPos);
                }
                else
                {
                    ArteriTgt.Come();
                }
                return true;
            }
            await Travel.To(World.Act2.WesternForest);
            return true;
        }

        public static async Task<bool> OpenPath()
        {
            if (!Helpers.PlayerHasQuestItem(QuestItemMetadata.ThaumeticEmblem))
                return false;

            if (World.Act2.WesternForest.IsCurrentArea)
            {
                if (await Helpers.HandleQuestObject(CachedSeal))
                    return true;

                SealTgt.Come();
                return true;
            }
            await Travel.To(World.Act2.WesternForest);
            return true;
        }

        public static async Task<bool> TakeReward()
        {
            return await Helpers.TakeQuestReward(
                World.Act1.LioneyeWatch,
                TownNpcs.Bestel,
                "Road Reward",
                book: QuestItemMetadata.BookWayForward);
        }
    }
}