using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A2_Q6_DealWithBandits
    {
        public static readonly TgtPosition AliraTgt = new TgtPosition("Alira camp", "treewitch_camp_v01_01_c2r3.tgt");
        public static readonly TgtPosition KraitynTgt = new TgtPosition("Kraityn camp", "bridge_large_v01_01_c5r19.tgt | bridge_large_v01_01_c5r23.tgt");
        public static readonly TgtPosition OakTgt = new TgtPosition("Oak camp", "cliffpathconnection_gate_v01_01_c2r1.tgt");

        private static Monster Alira => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Alira_Darktongue)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Monster Kraityn => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Kraityn_Scarbearer)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Monster Oak => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Oak_Skullbreaker)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static WalkablePosition CachedAliraPos
        {
            get => CombatAreaCache.Current.Storage["AliraPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["AliraPosition"] = value;
        }

        private static WalkablePosition CachedKraitynPos
        {
            get => CombatAreaCache.Current.Storage["KraitynPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["KraitynPosition"] = value;
        }

        private static WalkablePosition CachedOakPos
        {
            get => CombatAreaCache.Current.Storage["OakPosition"] as WalkablePosition;
            set => CombatAreaCache.Current.Storage["OakPosition"] = value;
        }

        public static void Tick()
        {
            var id = World.CurrentArea.Id;
            if (id == World.Act2.WesternForest.Id)
            {
                var alira = Alira;
                if (alira != null)
                {
                    CachedAliraPos = alira.WalkablePosition();
                }
                return;
            }
            if (id == World.Act2.BrokenBridge.Id)
            {
                var kraityn = Kraityn;
                if (kraityn != null)
                {
                    CachedKraitynPos = kraityn.WalkablePosition();
                }
                return;
            }
            if (id == World.Act2.Wetlands.Id)
            {
                var oak = Oak;
                if (oak != null)
                {
                    CachedOakPos = oak.WalkablePosition();
                }
            }
        }

        public static async Task<bool> KillAlira()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.AmuletAlira))
                return false;

            if (World.Act2.WesternForest.IsCurrentArea)
            {
                var aliraPos = CachedAliraPos;
                if (aliraPos != null)
                {
                    if (aliraPos.IsFar)
                    {
                        aliraPos.Come();
                        return true;
                    }

                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Alira))
                        return true;

                    var alira = Alira;
                    if (alira != null && alira.Reaction == Reaction.Npc)
                    {
                        if (!await BanditHelper.Kill(alira))
                        {
                            ErrorManager.ReportError();
                            return true;
                        }
                        if (!await Wait.For(() => alira.Fresh().Reaction == Reaction.Enemy, "Alira becomes hostile"))
                            ErrorManager.ReportError();
                    }
                    return true;
                }
                AliraTgt.Come();
                return true;
            }
            await Travel.To(World.Act2.WesternForest);
            return true;
        }

        public static async Task<bool> HelpAlira()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.Apex))
                return false;

            if (World.Act2.WesternForest.IsCurrentArea)
            {
                var aliraPos = CachedAliraPos;
                if (aliraPos != null)
                {
                    if (aliraPos.IsFar)
                    {
                        aliraPos.Come();
                        return true;
                    }
                    var alira = Alira;
                    if (alira != null && alira.Reaction == Reaction.Npc)
                    {
                        if (!await BanditHelper.Help(alira))
                            ErrorManager.ReportError();
                    }
                    return true;
                }
                AliraTgt.Come();
                return true;
            }
            await Travel.To(World.Act2.WesternForest);
            return true;
        }

        public static async Task<bool> KillKraityn()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.AmuletKraityn))
                return false;

            if (World.Act2.BrokenBridge.IsCurrentArea)
            {
                var kraitynPos = CachedKraitynPos;
                if (kraitynPos != null)
                {
                    if (kraitynPos.IsFar)
                    {
                        kraitynPos.Come();
                        return true;
                    }

                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Kraityn))
                        return true;

                    var kraityn = Kraityn;
                    if (kraityn != null && kraityn.Reaction == Reaction.Npc)
                    {
                        if (!await BanditHelper.Kill(kraityn))
                        {
                            ErrorManager.ReportError();
                            return true;
                        }
                        if (!await Wait.For(() => kraityn.Fresh().Reaction == Reaction.Enemy, "Kraityn becomes hostile"))
                            ErrorManager.ReportError();
                    }
                    return true;
                }
                KraitynTgt.Come();
                return true;
            }
            await Travel.To(World.Act2.BrokenBridge);
            return true;
        }

        public static async Task<bool> HelpKraityn()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.Apex))
                return false;

            if (World.Act2.BrokenBridge.IsCurrentArea)
            {
                var kraitynPos = CachedKraitynPos;
                if (kraitynPos != null)
                {
                    if (kraitynPos.IsFar)
                    {
                        kraitynPos.Come();
                        return true;
                    }
                    var kraityn = Kraityn;
                    if (kraityn != null && kraityn.Reaction == Reaction.Npc)
                    {
                        if (!await BanditHelper.Help(kraityn))
                            ErrorManager.ReportError();
                    }
                    return true;
                }
                KraitynTgt.Come();
                return true;
            }
            await Travel.To(World.Act2.BrokenBridge);
            return true;
        }

        public static async Task<bool> KillOak()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.AmuletOak))
                return false;

            if (World.Act2.Wetlands.IsCurrentArea)
            {
                var oakPos = CachedOakPos;
                if (oakPos != null)
                {
                    if (oakPos.IsFar)
                    {
                        oakPos.Come();
                        return true;
                    }

                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Oak))
                        return true;

                    var oak = Oak;
                    if (oak != null && oak.Reaction == Reaction.Npc)
                    {
                        if (!await BanditHelper.Kill(oak))
                        {
                            ErrorManager.ReportError();
                            return true;
                        }
                        if (!await Wait.For(() => oak.Fresh().Reaction == Reaction.Enemy, "Oak becomes hostile"))
                            ErrorManager.ReportError();
                    }
                    return true;
                }
                OakTgt.Come();
                return true;
            }
            await Travel.To(World.Act2.Wetlands);
            return true;
        }

        public static async Task<bool> HelpOak()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.Apex))
                return false;

            if (World.Act2.Wetlands.IsCurrentArea)
            {
                var oakPos = CachedOakPos;
                if (oakPos != null)
                {
                    if (oakPos.IsFar)
                    {
                        oakPos.Come();
                        return true;
                    }
                    var oak = Oak;
                    if (oak != null && oak.Reaction == Reaction.Npc)
                    {
                        if (!await BanditHelper.Help(oak))
                            ErrorManager.ReportError();
                    }
                    return true;
                }
                OakTgt.Come();
                return true;
            }
            await Travel.To(World.Act2.Wetlands);
            return true;
        }

        public static async Task<bool> HelpEramir()
        {
            if (Helpers.PlayerHasQuestItem(QuestItemMetadata.Apex))
                return false;

            if (World.Act2.ForestEncampment.IsCurrentArea)
            {
                if (!await TownNpcs.Eramir.TakeReward(null, "Take the Apex"))
                    ErrorManager.ReportError();

                return true;
            }
            await Travel.To(World.Act2.ForestEncampment);
            return true;
        }
    }
}