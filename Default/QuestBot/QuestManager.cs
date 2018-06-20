using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.QuestBot.QuestHandlers;
using Loki;
using Loki.Game;
using Loki.Game.GameData;
using Newtonsoft.Json;

namespace Default.QuestBot
{
    public static class QuestManager
    {
        private static int CurrentAct => World.CurrentArea.Act;

        private static Dictionary<string, KeyValuePair<DatQuestWrapper, DatQuestStateWrapper>> _questStates;
        private static Dictionary<string, DatWorldAreaWrapper> _availableWaypoints;

        public static QuestHandler GetQuestHandler()
        {
            // Enter Lioneye's Watch if we are in Twilight Strand
            if (World.Act1.TwilightStrand.IsCurrentArea)
            {
                return CreateQuestHander(Quests.EnemyAtTheGate, "Enter Lioneye's Watch",
                    A1_Q1_EnemyAtTheGate.EnterLioneyeWatch, A1_Q1_EnemyAtTheGate.Tick);
            }

            if (Settings.Instance.CheckGrindingFirst)
            {
                var grinding = GetGrindingHandler(int.MaxValue);

                if (grinding != null)
                    return grinding;
            }

            // Travel to last opened act if we are in hideout
            if (World.CurrentArea.IsHideoutArea)
            {
                var lastAct = World.LastOpenedAct;
                UpdateGuiAndLog("Travel", $"Go to last opened act: {lastAct.Name}");
                return new QuestHandler(() => TravelTo(lastAct), null);
            }

            UpdateStatesAndWaypoints();

            var act = 1;
            var town = World.Act1.LioneyeWatch;

            // Enemy at the Gate
            var quest = Quests.EnemyAtTheGate;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                if (state == 1)
                    return CreateQuestHander(quest, "Take reward", A1_Q1_EnemyAtTheGate.TakeReward, null);

                GlobalLog.Error($"[EnemyAtTheGate] Unknown quest state and area combination. State: {state}. Area: {World.CurrentArea.Name}.");
                return null;
            }

            // Mercy Mission
            quest = Quests.MercyMission;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= 2 || Helpers.PlayerHasQuestItem(QuestItemMetadata.MedicineChest))
                        return CreateQuestHander(quest, "Take reward", A1_Q2_MercyMission.TakeReward, null);

                    return CreateQuestHander(quest, "Kill Hailrake", A1_Q2_MercyMission.KillHailrake, A1_Q2_MercyMission.Tick);
                }
            }

            // Dirty Job
            quest = Quests.DirtyJob;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 4
                        ? CreateQuestHander(quest, "Clear Fetid Pool", A1_Q3_DirtyJob.ClearFetidPool, A1_Q3_DirtyJob.Tick)
                        : CreateQuestHander(quest, "Take reward", A1_Q3_DirtyJob.TakeReward, null);
                }
            }

            // Breaking Some Eggs
            quest = Quests.BreakingSomeEggs;
            if (QuestIsNotCompleted(quest))
            {
                if (Helpers.PlayerHasQuestItemAmount(QuestItemMetadata.Glyph, 3))
                    return CreateQuestHander(quest, "Open Submerged Passage", A1_Q4_BreakingSomeEggs.OpenPassage, A1_Q4_BreakingSomeEggs.Tick);

                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return state >= 3
                    ? CreateQuestHander(quest, "Grab glyphs", A1_Q4_BreakingSomeEggs.GrabGlyphs, A1_Q4_BreakingSomeEggs.Tick)
                    : CreateQuestHander(quest, "Take reward", A1_Q4_BreakingSomeEggs.TakeReward, null);
            }

            // Dweller of the Deep
            quest = Quests.DwellerOfTheDeep;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 4
                        ? CreateQuestHander(quest, "Kill Deep Dweller", A1_Q5_DwellerOfTheDeep.KillDweller, A1_Q5_DwellerOfTheDeep.Tick)
                        : CreateQuestHander(quest, "Take reward", A1_Q5_DwellerOfTheDeep.TakeReward, null);
                }
            }

            // Caged Brute
            quest = Quests.CagedBrute;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                // Check wp here, after 3.0 tends to stuck on "Kill Brutus" when he is killed
                if (state <= 2 || IsWaypointOpened(World.Act1.PrisonerGate))
                    return CreateQuestHander(quest, "Take Tarkleigh reward", A1_Q6_CagedBrute.TakeTarkleighReward, null);

                if (state == 4)
                    return CreateQuestHander(quest, "Take Nessa reward", A1_Q6_CagedBrute.TakeNessaReward, null);

                if (state == 3 || state == 5 || state == 6)
                    return CreateQuestHander(quest, "Kill Brutus", A1_Q6_CagedBrute.KillBrutus, A1_Q6_CagedBrute.Tick);

                return CreateQuestHander(quest, "Enter Prison", A1_Q6_CagedBrute.EnterPrison, A1_Q6_CagedBrute.Tick);
            }

            // Marooned Mariner
            quest = Quests.MaroonedMariner;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    if (state == 4 || state == 5 || Helpers.PlayerHasQuestItem(QuestItemMetadata.Allflame))
                        return CreateQuestHander(quest, "Kill Captain Fairgraves", A1_Q7_MaroonedMariner.KillFairgraves, A1_Q7_MaroonedMariner.Tick);

                    if (state <= 3)
                        return CreateQuestHander(quest, "Take reward", A1_Q7_MaroonedMariner.TakeReward, null);

                    return CreateQuestHander(quest, "Grab Allflame", A1_Q7_MaroonedMariner.GrabAllflame, A1_Q7_MaroonedMariner.Tick);
                }
            }

            // Sirens Cadence
            quest = Quests.SirensCadence;
            if (QuestIsNotCompleted(quest))
            {
                if (IsWaypointOpened(World.Act2.SouthernForest) && !IsWaypointOpened(World.Act2.ForestEncampment))
                    return CreateQuestHander(quest, "Enter Forest Encampment", A1_Q8_SirensCadence.EnterForestEncampment, A1_Q8_SirensCadence.Tick);

                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                if (state == 6 || state == 1)
                    return CreateQuestHander(quest, "Take reward", A1_Q8_SirensCadence.TakeReward, null);

                return CreateQuestHander(quest, "Kill Merveil", A1_Q8_SirensCadence.KillMerveil, A1_Q8_SirensCadence.Tick);
            }

            // Enter Forest Encampment
            if (!IsWaypointOpened(World.Act2.ForestEncampment))
                return CreateQuestHander(quest, "Enter Forest Encampment", A1_Q8_SirensCadence.EnterForestEncampment, A1_Q8_SirensCadence.Tick);

            act = 2;
            town = World.Act2.ForestEncampment;

            // Sharp and Cruel
            quest = Quests.SharpAndCruel;
            if (QuestIsNotCompleted(quest, 1))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state <= 1)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return state >= 6
                    ? CreateQuestHander(quest, "Kill Weaver", A2_Q1_SharpAndCruel.KillWeaver, A2_Q1_SharpAndCruel.Tick)
                    : CreateQuestHander(quest, "Take reward", A2_Q1_SharpAndCruel.TakeReward, null);
            }

            // The Way Forward
            quest = Quests.WayForward;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (Helpers.PlayerHasQuestItem(QuestItemMetadata.ThaumeticEmblem))
                        return CreateQuestHander(quest, "Open path", A2_Q2_WayForward.OpenPath, A2_Q2_WayForward.Tick);

                    // this quest spans across two acts, we cannot get an accurate state for it in Act 2
                    int state;
                    if (CurrentAct == 1)
                    {
                        state = GetState(quest);
                        if (state == 0)
                        {
                            CompletedQuests.Instance.Add(quest);
                            return QuestHandler.QuestAddedToCache;
                        }
                    }
                    else
                    {
                        state = GetStateInaccurate(quest);
                    }
                    return state >= 4
                        ? CreateQuestHander(quest, "Kill Arteri", A2_Q2_WayForward.KillArteri, A2_Q2_WayForward.Tick)
                        : CreateQuestHander(quest, "Take reward", A2_Q2_WayForward.TakeReward, null);
                }
            }

            // The Great White Beast
            quest = Quests.GreatWhiteBeast;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 3
                        ? CreateQuestHander(quest, "Kill Great White Beast", A2_Q3_GreatWhiteBeast.KillWhiteBeast, A2_Q3_GreatWhiteBeast.Tick)
                        : CreateQuestHander(quest, "Take reward", A2_Q3_GreatWhiteBeast.TakeReward, null);
                }
            }

            // Intruders in Black
            quest = Quests.IntrudersInBlack;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }

                if (state == 1 || Helpers.PlayerHasQuestItem(QuestItemMetadata.BalefulGem))
                    return CreateQuestHander(quest, "Take reward", A2_Q4_IntrudersInBlack.TakeReward, null);

                return CreateQuestHander(quest, "Grab Baleful Gem", A2_Q4_IntrudersInBlack.GrabBalefulGem, A2_Q4_IntrudersInBlack.Tick);
            }

            // Through Sacred Ground
            quest = Quests.ThroughSacredGround;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= 6 || Helpers.PlayerHasQuestItem(QuestItemMetadata.GoldenHand))
                        return CreateQuestHander(quest, "Take reward", A2_Q5_ThroughSacredGround.TakeReward, null);

                    return CreateQuestHander(quest, "Grab Golden Hand", A2_Q5_ThroughSacredGround.GrabGoldenHand, A2_Q5_ThroughSacredGround.Tick);
                }
            }

            // Deal with Bandits
            quest = Quests.DealWithBandits;
            if (QuestIsNotCompleted(quest, 1))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state <= 1)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }

                var handler = AnalyzeBandits();
                if (handler == null)
                {
                    GlobalLog.Error($"[DealWithBandits] Fail to analyze bandit amulets. Quest state: {state}.");
                    return null;
                }
                return handler;
            }

            // Shadow of the Vaal
            quest = Quests.ShadowOfVaal;
            if (!IsWaypointOpened(World.Act3.CityOfSarn))
                return CreateQuestHander(quest, "Kill Vaal Oversoul", A2_Q7_ShadowOfVaal.KillVaal, A2_Q7_ShadowOfVaal.Tick);

            act = 3;
            town = World.Act3.SarnEncampment;

            // Lost in Love
            quest = Quests.LostInLove;
            if (QuestIsNotCompleted(quest, 1))
            {
                if (CurrentAct != act)
                {
                    // at this stage we cannot be sure that Sarn Encampment waypoint is opened
                    var act3Area = IsWaypointOpened(World.Act3.SarnEncampment)
                        ? World.Act3.SarnEncampment
                        : World.Act3.CityOfSarn;

                    return CreateQuestHander(quest, "Travel to Act 3", () => TravelTo(act3Area), null);
                }

                int state = GetState(quest);
                if (state <= 1)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                if (state >= 10)
                    return CreateQuestHander(quest, "Free Clarissa", A3_Q1_LostInLove.FreeClarissa, A3_Q1_LostInLove.Tick);

                if (!IsWaypointOpened(World.Act3.SarnEncampment))
                    return CreateQuestHander(quest, "Enter Sarn Encampment", A3_Q1_LostInLove.EnterSarnEncampment, null);

                if (state == 2 || state == 5 || Helpers.PlayerHasQuestItem(QuestItemMetadata.TolmanBracelet))
                    return CreateQuestHander(quest, "Take Clarissa reward", A3_Q1_LostInLove.TakeClarissaReward, null);

                if (state == 3 || state == 4)
                    return CreateQuestHander(quest, "Take Maramoa reward", A3_Q1_LostInLove.TakeMaramoaReward, null);

                return CreateQuestHander(quest, "Grab Bracelet", A3_Q1_LostInLove.GrabBracelet, A3_Q1_LostInLove.Tick);
            }

            // Victario Secrets
            quest = Quests.VictarioSecrets;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (Helpers.PlayerHasQuestItemAmount(QuestItemMetadata.Bust, 3))
                        return CreateQuestHander(quest, "Take reward", A3_Q2_VictarioSecrets.TakeReward, null);

                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= A3_Q2_VictarioSecrets.MinHaveAllBustsState || A3_Q2_VictarioSecrets.HaveAllBustsStates.Contains(state))
                        return CreateQuestHander(quest, "Take reward", A3_Q2_VictarioSecrets.TakeReward, null);

                    return CreateQuestHander(quest, "Grab busts", A3_Q2_VictarioSecrets.GrabBusts, A3_Q2_VictarioSecrets.Tick);
                }
            }

            // Swig of Hope (early decanter)
            quest = Quests.SwigOfHope;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (GetStateInaccurate(quest) >= 8 && !Helpers.PlayerHasQuestItem(QuestItemMetadata.DecanterSpiritus))
                    return CreateQuestHander(quest, "Grab Decanter Spiritus", A3_Q6_SwigOfHope.GrabDecanter, A3_Q6_SwigOfHope.Tick);
            }

            // Ribbon Spool
            quest = Quests.RibbonSpool;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }

                if (state != 1 && !Helpers.PlayerHasQuestItem(QuestItemMetadata.RibbonSpool))
                    return CreateQuestHander(quest, "Grab Ribbon Spool", A3_Q3_GemlingQueen.GrabRibbon, A3_Q3_GemlingQueen.Tick);

                // we are not exiting here since we want to grab both Spool and Sulphite before going to Dialla
            }

            // Fiery Dust
            quest = Quests.FieryDust;
            if (QuestIsNotCompleted(quest, 3))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state <= 3)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }

                if (state != 4 && !Helpers.PlayerHasQuestItem(QuestItemMetadata.ThaumeticSulphite))
                    return CreateQuestHander(quest, "Grab Thaumetic Sulphite", A3_Q3_GemlingQueen.GrabSulphite, A3_Q3_GemlingQueen.Tick);

                if (GetState(Quests.RibbonSpool) != 0)
                    return CreateQuestHander(Quests.RibbonSpool, "Take reward", A3_Q3_GemlingQueen.TakeRibbonReward, A3_Q3_GemlingQueen.Tick);

                return CreateQuestHander(quest, "Take reward", A3_Q3_GemlingQueen.TakeTalcReward, A3_Q3_GemlingQueen.Tick);
            }

            // Sever the Right Hand
            quest = Quests.SeverRightHand;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return state >= 2
                    ? CreateQuestHander(quest, "Kill General Gravicius", A3_Q4_SeverRightHand.KillGravicius, A3_Q4_SeverRightHand.Tick)
                    : CreateQuestHander(quest, "Take reward", A3_Q4_SeverRightHand.TakeReward, null);
            }

            // Piety Pets
            quest = Quests.PietyPets;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return state >= 4
                    ? CreateQuestHander(quest, "Kill Piety", A3_Q5_PietyPets.KillPiety, A3_Q5_PietyPets.Tick)
                    : CreateQuestHander(quest, "Take reward", A3_Q5_PietyPets.TakeReward, null);
            }

            // Swig of Hope
            quest = Quests.SwigOfHope;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    if (state > 6)
                    {
                        // 7 = decanter has been already delivered
                        if (state != 7 && !Helpers.PlayerHasQuestItem(QuestItemMetadata.DecanterSpiritus))
                            return CreateQuestHander(quest, "Grab Decanter Spiritus", A3_Q6_SwigOfHope.GrabDecanter, A3_Q6_SwigOfHope.Tick);

                        // 9 = plum has been already delivered
                        if (state != 9 && !Helpers.PlayerHasQuestItem(QuestItemMetadata.ChitusPlum))
                            return CreateQuestHander(quest, "Grab Chitus Plum", A3_Q6_SwigOfHope.GrabPlum, A3_Q6_SwigOfHope.Tick);
                    }
                    return CreateQuestHander(quest, "Take reward", A3_Q6_SwigOfHope.TakeReward, A3_Q6_SwigOfHope.Tick);
                }
            }

            // Fixture of Fate
            quest = Quests.FixtureOfFate;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (Helpers.PlayerHasQuestItemAmount(QuestItemMetadata.GoldenPage, 4))
                        return CreateQuestHander(quest, "Take reward", A3_Q7_FixtureOfFate.TakeReward, A3_Q7_FixtureOfFate.Tick);

                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 3
                        ? CreateQuestHander(quest, "Grab Golden Pages", A3_Q7_FixtureOfFate.GrabPages, A3_Q7_FixtureOfFate.Tick)
                        : CreateQuestHander(quest, "Take reward", A3_Q7_FixtureOfFate.TakeReward, A3_Q7_FixtureOfFate.Tick);
                }
            }

            // Sceptre of God
            quest = Quests.SceptreOfGod;
            if (!IsWaypointOpened(World.Act4.Aqueduct))
                return CreateQuestHander(quest, "Kill Dominus", A3_Q8_SceptreOfGod.KillDominus, A3_Q8_SceptreOfGod.Tick);

            // Enter Highgate
            if (!IsWaypointOpened(World.Act4.Highgate))
                return CreateQuestHander(quest, "Enter Highgate", A3_Q8_SceptreOfGod.EnterHighgate, null);

            act = 4;
            town = World.Act4.Highgate;

            // Breaking the Seal
            quest = Quests.BreakingSeal;
            if (QuestIsNotCompleted(quest))
            {
                if (Helpers.PlayerHasQuestItem(QuestItemMetadata.DeshretBanner))
                    return CreateQuestHander(quest, "Break Deshret Seal", A4_Q1_BreakingSeal.BreakSeal, A4_Q1_BreakingSeal.Tick);

                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return state >= 3
                    ? CreateQuestHander(quest, "Kill Voll", A4_Q1_BreakingSeal.KillVoll, A4_Q1_BreakingSeal.Tick)
                    : CreateQuestHander(quest, "Take reward", A4_Q1_BreakingSeal.TakeReward, null);
            }

            // Indomitable Spirit
            quest = Quests.IndomitableSpirit;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 3
                        ? CreateQuestHander(quest, "Free Deshret Spirit", A4_Q2_IndomitableSpirit.FreeDeshret, A4_Q2_IndomitableSpirit.Tick)
                        : CreateQuestHander(quest, "Take reward", A4_Q2_IndomitableSpirit.TakeReward, null);
                }
            }

            // King of Fury
            quest = Quests.KingOfFury;
            if (QuestIsNotCompleted(quest))
            {
                if (Helpers.PlayerHasQuestItem(QuestItemMetadata.EyeOfFury))
                    return CreateQuestHander(quest, "Bring Eye of Fury to Dialla", A4_Q3_KingOfFury.TurnInQuest, A4_Q3_KingOfFury.Tick);

                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                if (state >= 4)
                    return CreateQuestHander(quest, "Kill Kaom", A4_Q3_KingOfFury.KillKaom, A4_Q3_KingOfFury.Tick);

                if (state == 1)
                    return CreateQuestHander(quest, "Take reward", A4_Q4_KingOfDesire.TakeReward, null);
            }

            // King of Desire
            quest = Quests.KingOfDesire;
            if (QuestIsNotCompleted(quest))
            {
                if (Helpers.PlayerHasQuestItem(QuestItemMetadata.EyeOfDesire))
                    return CreateQuestHander(quest, "Bring Eye of Desire to Dialla", A4_Q4_KingOfDesire.TurnInQuest, A4_Q4_KingOfDesire.Tick);

                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                if (state >= 4)
                    return CreateQuestHander(quest, "Kill Daresso", A4_Q4_KingOfDesire.KillDaresso, A4_Q4_KingOfDesire.Tick);

                if (state == 1)
                    return CreateQuestHander(quest, "Take reward", A4_Q4_KingOfDesire.TakeReward, null);
            }

            // Eternal Nightmare
            quest = Quests.EternalNightmare;
            if (QuestIsNotCompleted(quest, 1))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state <= 1)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }

                if (state == 2 || state == 3)
                    return CreateQuestHander(quest, "Talk to Tasuni", A4_Q5_EternalNightmare.TalkToTasuni, null);

                return CreateQuestHander(quest, "Kill Malachai", A4_Q5_EternalNightmare.KillMalachai, A4_Q5_EternalNightmare.Tick);
            }

            // Enter Overseer Tower
            quest = Quests.ReturnToOriath;
            if (!IsWaypointOpened(World.Act5.OverseerTower))
                return CreateQuestHander(quest, "Enter Overseer Tower", A5_Q1_ReturnToOriath.EnterOverseerTower, A5_Q1_ReturnToOriath.Tick);

            act = 5;
            town = World.Act5.OverseerTower;

            // Return to Oriath
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return CreateQuestHander(quest, "Take reward", A5_Q1_ReturnToOriath.TakeReward, null);
            }

            // In Service to Science
            quest = Quests.InServiceToScience;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= 3 || Helpers.PlayerHasQuestItem(QuestItemMetadata.Miasmeter))
                        return CreateQuestHander(quest, "Take reward", A5_Q2_InServiceToScience.TakeReward, null);

                    return CreateQuestHander(quest, "Grab Miasmeter", A5_Q2_InServiceToScience.GrabMiasmeter, A5_Q2_InServiceToScience.Tick);
                }
            }

            // Key to Freedom
            quest = Quests.KeyToFreedom;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }

                if (state <= 2 || Helpers.PlayerHasQuestItem(QuestItemMetadata.EyesOfZeal))
                    return CreateQuestHander(quest, "Take reward", A5_Q3_KeyToFreedom.TakeReward, null);

                return CreateQuestHander(quest, "Kill Justicar Casticus", A5_Q3_KeyToFreedom.KillCasticus, A5_Q3_KeyToFreedom.Tick);
            }

            // Death to Purity
            quest = Quests.DeathToPurity;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return state >= 4
                    ? CreateQuestHander(quest, "Kill Avarius", A5_Q4_DeathToPurity.KillAvarius, A5_Q4_DeathToPurity.Tick)
                    : CreateQuestHander(quest, "Take reward", A5_Q4_DeathToPurity.TakeReward, null);
            }

            // Just ensure we have Ruined Square wp at this stage
            // because 1 main and 2 optional quests lead there
            if (!IsWaypointOpened(World.Act5.RuinedSquare))
            {
                UpdateGuiAndLog(Quests.RavenousGod.Name, "Travel to Ruined Square");
                return new QuestHandler(A5_Q7_RavenousGod.TalkToBannonAndGetToSquare, null);
            }

            // King's Feast
            quest = Quests.KingFeast;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 4
                        ? CreateQuestHander(quest, "Kill Utula", A5_Q5_KingFeast.KillUtula, A5_Q5_KingFeast.Tick)
                        : CreateQuestHander(quest, "Take reward", A5_Q5_KingFeast.TakeReward, null);
                }
            }

            // Kitava's Torments
            quest = Quests.KitavaTorments;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (Helpers.PlayerHasQuestItemAmount(QuestItemMetadata.KitavaTorment, 3))
                        return CreateQuestHander(quest, "Take reward", A5_Q6_KitavaTorments.TakeReward, null);

                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 4
                        ? CreateQuestHander(quest, "Grab Kitava's Torments", A5_Q6_KitavaTorments.GrabTorments, A5_Q6_KitavaTorments.Tick)
                        : CreateQuestHander(quest, "Take reward", A5_Q6_KitavaTorments.TakeReward, null);
                }
            }

            // Ravenous God
            quest = Quests.RavenousGod;
            if (!IsWaypointOpened(World.Act6.LioneyeWatch))
            {
                if (IsWaypointOpened(World.Act5.CathedralRooftop))
                    return CreateQuestHander(quest, "Sail to Wraeclast", A5_Q7_RavenousGod.SailToWraeclast, null);

                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return state >= 7
                    ? CreateQuestHander(quest, "Grab Sign of Purity", A5_Q7_RavenousGod.GrabSignOfPurity, A5_Q7_RavenousGod.Tick)
                    : CreateQuestHander(quest, "Kill Kitava", A5_Q7_RavenousGod.KillKitava, A5_Q7_RavenousGod.Tick);
            }

            act = 6;
            town = World.Act6.LioneyeWatch;

            // Fallen from Grace
            quest = Quests.FallenFromGrace;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 3
                        ? CreateQuestHander(quest, "Clear Twilight Strand", A6_Q1_FallenFromGrace.ClearStrand, A6_Q1_FallenFromGrace.Tick)
                        : CreateQuestHander(quest, "Take reward", A6_Q1_FallenFromGrace.TakeReward, null);
                }
            }

            // Bestel's Epic
            quest = Quests.BestelEpic;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= 3 || Helpers.PlayerHasQuestItem(QuestItemMetadata.BestelManuscript))
                        return CreateQuestHander(quest, "Take reward", A6_Q2_BestelEpic.TakeReward, null);

                    return CreateQuestHander(quest, "Grab Bestel's Manuscript", A6_Q2_BestelEpic.GrabManuscript, A6_Q2_BestelEpic.Tick);
                }
            }

            // Father of War
            quest = Quests.FatherOfWar;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                if (state >= 8)
                    return CreateQuestHander(quest, "Grab Eye of Conquest", A6_Q3_FatherOfWar.GrabEyeOfConquest, A6_Q3_FatherOfWar.Tick);

                if (state >= 3)
                    return CreateQuestHander(quest, "Kill Tukohama", A6_Q3_FatherOfWar.KillTukohama, A6_Q3_FatherOfWar.Tick);

                return CreateQuestHander(quest, "Take reward", A6_Q3_FatherOfWar.TakeReward, null);
            }

            // Essence of Umbra
            quest = Quests.EssenceOfUmbra;
            if (QuestIsNotCompleted(quest))
            {
                if (!IsWaypointOpened(World.Act6.PrisonerGate))
                    return CreateQuestHander(quest, "Kill Shavronne", A6_Q4_EssenceOfUmbra.KillShavronne, A6_Q4_EssenceOfUmbra.Tick);

                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return CreateQuestHander(quest, "Take reward", A6_Q4_EssenceOfUmbra.TakeReward, null);
            }

            // Cloven One
            quest = Quests.ClovenOne;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 3
                        ? CreateQuestHander(quest, "Kill Abberath", A6_Q5_ClovenOne.KillAbberath, A6_Q5_ClovenOne.Tick)
                        : CreateQuestHander(quest, "Take reward", A6_Q5_ClovenOne.TakeReward, null);
                }
            }

            // Puppet Mistress
            quest = Quests.PuppetMistress;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 3
                        ? CreateQuestHander(quest, "Kill Ryslatha", A6_Q6_PuppetMistress.KillRyslatha, A6_Q6_PuppetMistress.Tick)
                        : CreateQuestHander(quest, "Take reward", A6_Q6_PuppetMistress.TakeReward, null);
                }
            }

            // Brine King
            quest = Quests.BrineKing;
            if (!IsWaypointOpened(World.Act7.BridgeEncampment))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);

                if (state >= 10 || state == 6)
                    return CreateQuestHander(quest, "Grab Black Flag", A6_Q7_BrineKing.GrabBlackFlag, A6_Q7_BrineKing.Tick);

                if (state == 8 || state == 9)
                    return CreateQuestHander(quest, "Fuel the Beacon", A6_Q7_BrineKing.FuelBeacon, A6_Q7_BrineKing.Tick);

                if (state == 7)
                    return CreateQuestHander(quest, "Light the Beacon", A6_Q7_BrineKing.LightBeacon, A6_Q7_BrineKing.Tick);

                if (!IsWaypointOpened(World.Act6.BrineKingReef))
                    return CreateQuestHander(quest, "Sail to Brine King's Reef", A6_Q7_BrineKing.SailToReef, A6_Q7_BrineKing.Tick);

                return CreateQuestHander(quest, "Kill Brine King and Sail to Act 7", A6_Q7_BrineKing.KillBrineKingAndSailToAct7, A6_Q7_BrineKing.Tick);
            }

            if (A6_Q7_BrineKing.OriginalCombatRange != null)
            {
                A6_Q7_BrineKing.RestoreCombatRange();
            }

            act = 7;
            town = World.Act7.BridgeEncampment;

            // Silver Locket
            quest = Quests.SilverLocket;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= 3 || Helpers.PlayerHasQuestItem(QuestItemMetadata.SilverLocket))
                        return CreateQuestHander(quest, "Take reward", A7_Q1_SilverLocket.TakeReward, null);

                    return CreateQuestHander(quest, "Grab Silver Locket", A7_Q1_SilverLocket.GrabSilverLocket, A7_Q1_SilverLocket.Tick);
                }
            }

            // Essence of Artist
            quest = Quests.EssenceOfArtist;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                if (state >= 11)
                    return CreateQuestHander(quest, "Grab Maligaro's Map", A7_Q2_EssenceOfArtist.GrabMaligaroMap, A7_Q2_EssenceOfArtist.Tick);

                if (state >= 3)
                    return CreateQuestHander(quest, "Kill Maligaro", A7_Q2_EssenceOfArtist.KillMaligaro, A7_Q2_EssenceOfArtist.Tick);

                return CreateQuestHander(quest, "Take reward", A7_Q2_EssenceOfArtist.TakeReward, null);
            }

            // Web of Secrets
            quest = Quests.WebOfSecrets;
            if (QuestIsNotCompleted(quest, 1))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state <= 1)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }

                if (state <= 3 || Helpers.PlayerHasQuestItem(QuestItemMetadata.BlackVenom))
                    return CreateQuestHander(quest, "Take Obsidian Key", A7_Q3_WebOfSecrets.TakeObsidianKey, A7_Q3_WebOfSecrets.Tick);

                // Normally bot should pick up the Black Venom during "Essence of Artist" quest
                return CreateQuestHander(quest, "Kill Maligaro", A7_Q2_EssenceOfArtist.KillMaligaro, A7_Q2_EssenceOfArtist.Tick);
            }

            // The Master of a Million Faces
            quest = Quests.MasterOfMillionFaces;
            if (QuestIsNotCompleted(quest, 3))
            {
                if (!IsWaypointOpened(World.Act7.NorthernForest))
                    return CreateQuestHander(quest, "Kill Ralakesh", A7_Q4_MasterOfMillionFaces.KillRalakesh, A7_Q4_MasterOfMillionFaces.Tick);

                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state <= 3)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return CreateQuestHander(quest, "Take reward", A7_Q4_MasterOfMillionFaces.TakeReward, null);
            }

            // In Memory of Greust
            quest = Quests.InMemoryOfGreust;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    if (state >= 5)
                        return CreateQuestHander(quest, "Take Greust's Necklace", A7_Q5_InMemoryOfGreust.TakeNecklace, null);

                    if (state >= 3)
                        return CreateQuestHander(quest, "Place Greust's Necklace", A7_Q5_InMemoryOfGreust.PlaceNecklace, A7_Q5_InMemoryOfGreust.Tick);

                    return CreateQuestHander(quest, "Take reward", A7_Q5_InMemoryOfGreust.TakeReward, null);
                }
            }

            // Lighting the Way
            quest = Quests.LightingTheWay;
            if (QuestIsNotCompleted(quest, 3))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state <= 3)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return CreateQuestHander(quest, "Grab Fireflies", A7_Q6_LightingTheWay.GrabFireflies, A7_Q6_LightingTheWay.Tick);
            }

            // Queen of Despair
            quest = Quests.QueenOfDespair;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 3
                        ? CreateQuestHander(quest, "Kill Gruthkul", A7_Q7_QueenOfDespair.KillGruthkul, A7_Q7_QueenOfDespair.Tick)
                        : CreateQuestHander(quest, "Take reward", A7_Q7_QueenOfDespair.TakeReward, null);
                }
            }

            // Kishara Star
            quest = Quests.KisharaStar;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= 3 || Helpers.PlayerHasQuestItem(QuestItemMetadata.KisharaStar))
                        return CreateQuestHander(quest, "Take reward", A7_Q8_KisharaStar.TakeReward, null);

                    return CreateQuestHander(quest, "Grab Kishara's Star", A7_Q8_KisharaStar.GrabKisharaStar, A7_Q8_KisharaStar.Tick);
                }
            }

            // Mother of Spiders
            quest = Quests.MotherOfSpiders;
            if (!IsWaypointOpened(World.Act8.SarnRamparts))
                return CreateQuestHander(quest, "Kill Arakaali", A7_Q9_MotherOfSpiders.KillArakaali, A7_Q9_MotherOfSpiders.Tick);

            // Enter Sarn Encampment
            if (!IsWaypointOpened(World.Act8.SarnEncampment))
                return CreateQuestHander(quest, "Enter Sarn Encampment", A7_Q9_MotherOfSpiders.EnterSarnEncampment, null);

            act = 8;
            town = World.Act8.SarnEncampment;

            // Essence of Hag
            quest = Quests.EssenceOfHag;
            if (QuestIsNotCompleted(quest))
            {
                if (!IsWaypointOpened(World.Act8.DoedreCesspool))
                    return CreateQuestHander(quest, "Kill Doedre", A8_Q1_EssenceOfHag.KillDoedre, A8_Q1_EssenceOfHag.Tick);

                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return CreateQuestHander(quest, "Take reward", A8_Q1_EssenceOfHag.TakeReward, null);
            }

            // Love is Dead
            quest = Quests.LoveIsDead;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    if (state >= 7)
                        return CreateQuestHander(quest, "Grab Ankh of Eternity", A8_Q2_LoveIsDead.GrabAnkh, A8_Q2_LoveIsDead.Tick);

                    if (state >= 4 || Helpers.PlayerHasQuestItem(QuestItemMetadata.AnkhOfEternity))
                        return CreateQuestHander(quest, "Kill Tolman", A8_Q2_LoveIsDead.KillTolman, A8_Q2_LoveIsDead.Tick);

                    return CreateQuestHander(quest, "Take reward", A8_Q2_LoveIsDead.TakeReward, null);
                }
            }

            // Gemling Legion
            quest = Quests.GemlingLegion;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 3
                        ? CreateQuestHander(quest, "Kill Gemlings", A8_Q3_GemlingLegion.KillGemlings, A8_Q3_GemlingLegion.Tick)
                        : CreateQuestHander(quest, "Take reward", A8_Q3_GemlingLegion.TakeReward, null);
                }
            }

            // Wings of Vastiri
            quest = Quests.WingsOfVastiri;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= 2 || Helpers.PlayerHasQuestItem(QuestItemMetadata.WingsOfVastiri))
                        return CreateQuestHander(quest, "Take reward", A8_Q4_WingsOfVastiri.TakeReward, null);

                    return CreateQuestHander(quest, "Kill Hector Titucius", A8_Q4_WingsOfVastiri.GrabWings, A8_Q4_WingsOfVastiri.Tick);
                }
            }

            // Reflection of Terror
            quest = Quests.ReflectionOfTerror;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 3
                        ? CreateQuestHander(quest, "Kill Yugul", A8_Q5_ReflectionOfTerror.KillYugul, A8_Q5_ReflectionOfTerror.Tick)
                        : CreateQuestHander(quest, "Take reward", A8_Q5_ReflectionOfTerror.TakeReward, null);
                }
            }

            // Lunar Eclipse
            quest = Quests.LunarEclipse;
            if (!IsWaypointOpened(World.Act9.BloodAqueduct))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);

                if (state == 2 || state == 3)
                    return CreateQuestHander(quest, "Kill Solaris and Lunaris", A8_Q6_LunarEclipse.KillSolarisLunaris, A8_Q6_LunarEclipse.Tick);

                if (state == 4)
                    return CreateQuestHander(quest, "Grab Sun Orb", A8_Q6_LunarEclipse.GrabSunOrb, null);

                return CreateQuestHander(quest, "Grab Moon Orb", A8_Q6_LunarEclipse.GrabMoonOrb, null);
            }

            // Enter Highgate
            if (!IsWaypointOpened(World.Act9.Highgate))
                return CreateQuestHander(quest, "Enter Highgate", A8_Q6_LunarEclipse.EnterHighgate, null);

            act = 9;
            town = World.Act9.Highgate;

            // Storm Blade
            quest = Quests.StormBlade;
            if (Settings.Instance.IsQuestEnabled(Quests.QueenOfSands)) // controlled by Queen of Sands
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= 3 || Helpers.PlayerHasQuestItem(QuestItemMetadata.StormBlade))
                        return CreateQuestHander(quest, "Take reward", A9_Q1_StormBlade.TakeReward, null);

                    return CreateQuestHander(quest, "Grab Storm Blade", A9_Q1_StormBlade.GrabStormBlade, A9_Q1_StormBlade.Tick);
                }
            }

            // Queen of Sands
            quest = Quests.QueenOfSands;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state >= 8)
                        return CreateQuestHander(quest, "Talk to Sin", A9_Q2_QueenOfSands.TalkToSin, A9_Q2_QueenOfSands.Tick);

                    if (state == 7)
                        return CreateQuestHander(quest, "Take Bottled Storm", A9_Q2_QueenOfSands.TakeBottledStorm, A9_Q2_QueenOfSands.Tick);

                    if (state >= 3)
                        return CreateQuestHander(quest, "Kill Shakari", A9_Q2_QueenOfSands.KillShakari, A9_Q2_QueenOfSands.Tick);

                    return CreateQuestHander(quest, "Take reward", A9_Q2_QueenOfSands.TakeReward, null);
                }
            }

            // Fastis Fortuna
            quest = Quests.FastisFortuna;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= 3 || Helpers.PlayerHasQuestItem(QuestItemMetadata.CalendarOfFortune))
                        return CreateQuestHander(quest, "Take reward", A9_Q3_FastisFortuna.TakeReward, null);

                    return CreateQuestHander(quest, "Grab Calendar of Fortune", A9_Q3_FastisFortuna.GrabCalendar, A9_Q3_FastisFortuna.Tick);
                }
            }

            // The Ruler of Highgate
            quest = Quests.RulerOfHighgate;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest, 1))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state <= 1)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    // In case user manually talked to Tasuni
                    if (state == 4)
                        return CreateQuestHander(quest, "Take reward", A9_Q4_RulerOfHighgate.TakeTasuniReward, null);

                    if (state <= 6 || Helpers.PlayerHasQuestItem(QuestItemMetadata.SekhemaFeather))
                        return CreateQuestHander(quest, "Take reward", A9_Q4_RulerOfHighgate.TakeIrashaReward, null);

                    return CreateQuestHander(quest, "Kill Garukhan", A9_Q4_RulerOfHighgate.KillGarukhan, A9_Q4_RulerOfHighgate.Tick);
                }
            }

            // Recurring Nightmare
            quest = Quests.RecurringNightmare;
            if (!IsWaypointOpened(World.Act10.OriathDocks))
            {
                if (A9_Q5_RecurringNightmare.HaveAnyIngredient)
                    return CreateQuestHander(quest, "Turn in the ingredient", A9_Q5_RecurringNightmare.TurnInIngredient, null);

                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);

                if (state >= 17)
                {
                    if (state != 23 && !Helpers.PlayerHasQuestItem(QuestItemMetadata.BasiliskAcid))
                        return CreateQuestHander(quest, "Grab Basilisk Acid", A9_Q5_RecurringNightmare.GrabAcid, A9_Q5_RecurringNightmare.Tick);

                    if (state != 22 && !Helpers.PlayerHasQuestItem(QuestItemMetadata.TrarthanPowder))
                        return CreateQuestHander(quest, "Grab Trarthan Powder", A9_Q5_RecurringNightmare.GrabPowder, A9_Q5_RecurringNightmare.Tick);
                }
                return CreateQuestHander(quest, "Kill Depraved Trinity", A9_Q5_RecurringNightmare.KillTrinity, A9_Q5_RecurringNightmare.Tick);
            }

            act = 10;
            town = World.Act10.OriathDocks;

            // Safe Passage
            quest = Quests.SafePassage;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return state >= 5
                    ? CreateQuestHander(quest, "Save Bannon", A10_Q1_SafePassage.SaveBannon, A10_Q1_SafePassage.Tick)
                    : CreateQuestHander(quest, "Take reward", A10_Q1_SafePassage.TakeReward, null);
            }

            // No Love for Old Ghosts
            quest = Quests.NoLoveForOldGhosts;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= 3 || Helpers.PlayerHasQuestItem(QuestItemMetadata.ElixirOfAllure))
                        return CreateQuestHander(quest, "Take reward", A10_Q2_NoLoveForOldGhosts.TakeReward, null);

                    return CreateQuestHander(quest, "Grab Elixir of Allure", A10_Q2_NoLoveForOldGhosts.GrabElixir, A10_Q2_NoLoveForOldGhosts.Tick);
                }
            }

            // Vilenta's Vengeance
            quest = Quests.VilentaVengeance;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }
                    return state >= 3
                        ? CreateQuestHander(quest, "Kill Vilenta", A10_Q3_VilentaVengeance.KillVilenta, A10_Q3_VilentaVengeance.Tick)
                        : CreateQuestHander(quest, "Take reward", A10_Q3_VilentaVengeance.TakeReward, null);
                }
            }

            // Map to Tsoatha
            quest = Quests.MapToTsoatha;
            if (Settings.Instance.IsQuestEnabled(quest))
            {
                if (QuestIsNotCompleted(quest))
                {
                    if (CurrentAct != act)
                        return CreateTravelHander(quest, town, act);

                    int state = GetState(quest);
                    if (state == 0)
                    {
                        CompletedQuests.Instance.Add(quest);
                        return QuestHandler.QuestAddedToCache;
                    }

                    if (state <= 3 || Helpers.PlayerHasQuestItem(QuestItemMetadata.Teardrop))
                        return CreateQuestHander(quest, "Take reward", A10_Q4_MapToTsoatha.TakeReward, null);

                    return CreateQuestHander(quest, "Grab Teardrop", A10_Q4_MapToTsoatha.GrabTeardrop, A10_Q4_MapToTsoatha.Tick);
                }
            }

            // Death and Rebirth
            quest = Quests.DeathAndRebirth;
            if (QuestIsNotCompleted(quest))
            {
                if (Helpers.PlayerHasQuestItem(QuestItemMetadata.StaffOfPurity))
                    return CreateQuestHander(quest, "Turn in Staff of Purity", A10_Q5_DeathAndRebirth.TurnInStaff, null);

                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return state >= 5
                    ? CreateQuestHander(quest, "Kill Avarius", A10_Q5_DeathAndRebirth.KillAvarius, A10_Q5_DeathAndRebirth.Tick)
                    : CreateQuestHander(quest, "Take reward", A10_Q5_DeathAndRebirth.TakeReward, null);
            }

            // An End to Hunger
            quest = Quests.EndToHunger;
            if (!IsWaypointOpened(World.Act11.Oriath))
                return CreateQuestHander(quest, "Kill Kitava", A10_Q6_EndToHunger.KillKitava, A10_Q6_EndToHunger.Tick);

            act = 11;
            town = World.Act11.Oriath;

            // An End to Hunger (Epilogue)
            quest = Quests.EndToHungerEpilogue;
            if (QuestIsNotCompleted(quest))
            {
                if (CurrentAct != act)
                    return CreateTravelHander(quest, town, act);

                int state = GetState(quest);
                if (state == 0)
                {
                    CompletedQuests.Instance.Add(quest);
                    return QuestHandler.QuestAddedToCache;
                }
                return CreateQuestHander(quest, "Take reward", A10_Q6_EndToHunger.TakeReward, null);
            }

            var grind = GetGrindingHandler(int.MaxValue);

            if (grind != null)
                return grind;

            return QuestHandler.AllQuestsDone;
        }

        public static int GetState(DatQuestWrapper quest)
        {
            var state = LokiPoe.InGameState.GetCurrentQuestStateAccurate(quest.Id);
            if (state == null) return byte.MaxValue;
            return state.Id;
        }

        public static int GetStateInaccurate(DatQuestWrapper quest)
        {
            var state = LokiPoe.InGameState.GetCurrentQuestState(quest.Id);
            if (state == null) return byte.MaxValue;
            return state.Id;
        }

        public static void UpdateGuiAndLog(string questName, string questState)
        {
            Settings.Instance.CurrentQuestName = questName;
            Settings.Instance.CurrentQuestState = questState;
            GlobalLog.Warn($"[QuestManager] {questName} - {questState}");
        }

        private static bool QuestIsNotCompleted(DatQuestWrapper quest, int completedState = 0)
        {
            if (_questStates.TryGetValue(quest.Id, out KeyValuePair<DatQuestWrapper, DatQuestStateWrapper> kvp))
            {
                var state = kvp.Value;
                if (state != null && state.Id <= completedState)
                    return false;
            }
            else
            {
                GlobalLog.Error($"[QuestManager] Quest state dictionary does not contain \"{quest.Id}\".");
                ErrorManager.ReportCriticalError();
            }

            if (CompletedQuests.Instance.Contains(quest))
                return false;

            return true;
        }

        private static bool IsWaypointOpened(AreaInfo area)
        {
            return _availableWaypoints.ContainsKey(area.Id);
        }

        private static QuestHandler CreateQuestHander(DatQuestWrapper quest, string state, Func<Task<bool>> execute, Action tick)
        {
            ClearStatesAndWaypoints();

            var index = Quests.All.FindIndex(q => q.Id == quest.Id);
            var grinding = GetGrindingHandler(index);

            if (grinding != null)
                return grinding;

            UpdateGuiAndLog(quest.Name, state);
            return new QuestHandler(execute, tick);
        }

        private static QuestHandler GetGrindingHandler(int questIndex)
        {
            var myLevel = LokiPoe.Me.Level;

            foreach (var rule in Settings.Instance.GrindingRules)
            {
                if (myLevel >= rule.LevelCap)
                    continue;

                var ruleIndex = Quests.All.FindIndex(q => q.Id == rule.Quest.Id);

                if (ruleIndex < questIndex)
                {
                    GrindingHandler.SetGrindingRule(rule);
                    return new QuestHandler(GrindingHandler.Execute, null);
                }
            }
            return null;
        }

        private static QuestHandler CreateTravelHander(DatQuestWrapper quest, AreaInfo area, int act)
        {
            ClearStatesAndWaypoints();

            UpdateGuiAndLog(quest.Name, $"Travel to Act {act}");
            return new QuestHandler(() => TravelTo(area), null);
        }

        private static void UpdateStatesAndWaypoints()
        {
            _questStates = LokiPoe.InGameState.GetCurrentQuestStates();
            _availableWaypoints = LokiPoe.InstanceInfo.AvailableWaypoints;
        }

        private static void ClearStatesAndWaypoints()
        {
            _questStates = null;
            _availableWaypoints = null;
        }

        private static async Task<bool> TravelTo(AreaInfo area)
        {
            if (area.IsCurrentArea)
                return false;

            await Travel.To(area);
            return true;
        }

        private static QuestHandler AnalyzeBandits()
        {
            var quest = Quests.DealWithBandits;
            var chosenBandit = Settings.Instance.GetRewardForQuest(quest.Id);

            if (chosenBandit != BanditHelper.AliraName && !Helpers.PlayerHasQuestItem(QuestItemMetadata.AmuletAlira))
                return CreateQuestHander(quest, "Kill Alira", A2_Q6_DealWithBandits.KillAlira, A2_Q6_DealWithBandits.Tick);

            if (chosenBandit != BanditHelper.KraitynName && !Helpers.PlayerHasQuestItem(QuestItemMetadata.AmuletKraityn))
                return CreateQuestHander(quest, "Kill Kraityn", A2_Q6_DealWithBandits.KillKraityn, A2_Q6_DealWithBandits.Tick);

            if (chosenBandit != BanditHelper.OakName && !Helpers.PlayerHasQuestItem(QuestItemMetadata.AmuletOak))
                return CreateQuestHander(quest, "Kill Oak", A2_Q6_DealWithBandits.KillOak, A2_Q6_DealWithBandits.Tick);

            if (chosenBandit == BanditHelper.EramirName)
                return CreateQuestHander(quest, "Help Eramir", A2_Q6_DealWithBandits.HelpEramir, A2_Q6_DealWithBandits.Tick);

            if (chosenBandit == BanditHelper.AliraName)
                return CreateQuestHander(quest, "Help Alira", A2_Q6_DealWithBandits.HelpAlira, A2_Q6_DealWithBandits.Tick);

            if (chosenBandit == BanditHelper.KraitynName)
                return CreateQuestHander(quest, "Help Kraityn", A2_Q6_DealWithBandits.HelpKraityn, A2_Q6_DealWithBandits.Tick);

            if (chosenBandit == BanditHelper.OakName)
                return CreateQuestHander(quest, "Help Oak", A2_Q6_DealWithBandits.HelpOak, A2_Q6_DealWithBandits.Tick);

            return null;
        }

        public class CompletedQuests
        {
            private readonly string _path = Path.Combine(Configuration.Instance.Path, "CompletedQuests.json");

            private static CompletedQuests _instance;
            public static CompletedQuests Instance => _instance ?? (_instance = new CompletedQuests());

            private Dictionary<string, CharacterData> _cache;

            private CompletedQuests()
            {
                if (File.Exists(_path))
                {
                    Load();
                }
                else
                {
                    _cache = new Dictionary<string, CharacterData>();
                }
            }

            public void Add(DatQuestWrapper quest)
            {
                var me = LokiPoe.Me;
                var name = me.Name;
                if (_cache.TryGetValue(name, out var entry))
                {
                    entry.Class = me.Class;
                    entry.Level = me.Level;
                    entry.Quests.Add(quest.Id);
                }
                else
                {
                    var newEntry = new CharacterData();
                    newEntry.Class = me.Class;
                    newEntry.Level = me.Level;
                    newEntry.Quests.Add(quest.Id);
                    _cache.Add(name, newEntry);
                }

                Save();

                GlobalLog.Debug($"[QuestManager] Quest \"{quest.Name}\"({quest.Id}) has been added to completed quests cache.");
            }

            public bool Contains(DatQuestWrapper quest)
            {
                return _cache.TryGetValue(LokiPoe.Me.Name, out var entry) && entry.Quests.Contains(quest.Id);
            }

            public void Verify()
            {
                var me = LokiPoe.Me;
                var name = me.Name;
                if (_cache.TryGetValue(name, out var entry))
                {
                    if (me.Class != entry.Class || me.Level < entry.Level)
                    {
                        GlobalLog.Info("[QuestBot] Removing outdated character entry from completed quest cache.");
                        _cache.Remove(name);
                        Save();
                    }
                }
            }

            private void Save()
            {
                var json = JsonConvert.SerializeObject(_cache, Formatting.Indented);
                File.WriteAllText(_path, json);
            }

            private void Load()
            {
                var json = File.ReadAllText(_path);

                if (string.IsNullOrWhiteSpace(json))
                {
                    GlobalLog.Info("[QuestBot] Clearing current completed quest cache. Json file is empty.");
                    Clear();
                    return;
                }
                try
                {
                    _cache = JsonConvert.DeserializeObject<Dictionary<string, CharacterData>>(json);
                }
                catch (Exception)
                {
                    GlobalLog.Info("[QuestBot] Clearing current completed quest cache. Exception during json deserialization.");
                    Clear();
                    return;
                }
                if (_cache == null)
                {
                    GlobalLog.Info("[QuestBot] Clearing current completed quest cache. Json deserealizer returned null.");
                    Clear();
                }
            }

            private void Clear()
            {
                _cache = new Dictionary<string, CharacterData>();
                File.Delete(_path);
            }

            private class CharacterData
            {
                public CharacterClass Class;
                public int Level;
                public readonly HashSet<string> Quests = new HashSet<string>();
            }
        }
    }
}