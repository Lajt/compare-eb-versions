using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Default.EXtensions;
using Loki.Bot;
using Loki.Game.GameData;

namespace Default.QuestBot
{
    [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
    public static class Quests
    {
        /************************************
         *             Act 1
         ***********************************/

        [QuestId("a1q1")]
        public static readonly DatQuestWrapper EnemyAtTheGate;

        [QuestId("a1q5")]
        public static readonly DatQuestWrapper MercyMission;

        [QuestId("a1q8")]
        public static readonly DatQuestWrapper DirtyJob;

        [QuestId("a1q4")]
        public static readonly DatQuestWrapper BreakingSomeEggs;

        [QuestId("a1q7")]
        public static readonly DatQuestWrapper DwellerOfTheDeep;

        [QuestId("a1q2")]
        public static readonly DatQuestWrapper CagedBrute;

        [QuestId("a1q6")]
        public static readonly DatQuestWrapper MaroonedMariner;

        [QuestId("a1q3")]
        public static readonly DatQuestWrapper SirensCadence;


        /************************************
         *             Act 2
         ***********************************/

        [QuestId("a2q4")]
        public static readonly DatQuestWrapper SharpAndCruel;

        [QuestId("a1q9")]
        public static readonly DatQuestWrapper WayForward;

        [QuestId("a2q10")]
        public static readonly DatQuestWrapper GreatWhiteBeast;

        [QuestId("a2q6")]
        public static readonly DatQuestWrapper IntrudersInBlack;

        [QuestId("a2q5")]
        public static readonly DatQuestWrapper ThroughSacredGround;

        [QuestId("a2q7")]
        public static readonly DatQuestWrapper DealWithBandits;

        // Not used anywhere
        //[QuestId("a2q9")]
        //public static readonly DatQuestWrapper RootOfTheProblem;

        [QuestId("a2q8")]
        public static readonly DatQuestWrapper ShadowOfVaal;


        /************************************
         *             Act 3
         ***********************************/

        [QuestId("a3q1")]
        public static readonly DatQuestWrapper LostInLove;

        [QuestId("a3q11")]
        public static readonly DatQuestWrapper VictarioSecrets;

        // Not used anywhere
        //[QuestId("a3q3")]
        //public static readonly DatQuestWrapper GemlingQueen;

        [QuestId("a3q4")]
        public static readonly DatQuestWrapper RibbonSpool;

        [QuestId("a3q5")]
        public static readonly DatQuestWrapper FieryDust;

        [QuestId("a3q8")]
        public static readonly DatQuestWrapper SeverRightHand;

        [QuestId("a3q9")]
        public static readonly DatQuestWrapper PietyPets;

        [QuestId("a3q13")]
        public static readonly DatQuestWrapper SwigOfHope;

        [QuestId("a3q12")]
        public static readonly DatQuestWrapper FixtureOfFate;

        [QuestId("a3q10")]
        public static readonly DatQuestWrapper SceptreOfGod;


        /************************************
         *             Act 4
         ***********************************/

        [QuestId("a4q2")]
        public static readonly DatQuestWrapper BreakingSeal;

        [QuestId("a4q6")]
        public static readonly DatQuestWrapper IndomitableSpirit;

        [QuestId("a4q3")]
        public static readonly DatQuestWrapper KingOfFury;

        [QuestId("a4q4")]
        public static readonly DatQuestWrapper KingOfDesire;

        [QuestId("a4q1")]
        public static readonly DatQuestWrapper EternalNightmare;


        /************************************
         *             Act 5
         ***********************************/

        [QuestId("a5q1b")]
        public static readonly DatQuestWrapper ReturnToOriath;

        [QuestId("a5q3")]
        public static readonly DatQuestWrapper InServiceToScience;

        [QuestId("a5q2")]
        public static readonly DatQuestWrapper KeyToFreedom;

        [QuestId("a5q4")]
        public static readonly DatQuestWrapper DeathToPurity;

        [QuestId("a5q5")]
        public static readonly DatQuestWrapper KingFeast;

        [QuestId("a5q7")]
        public static readonly DatQuestWrapper KitavaTorments;

        [QuestId("a5q6")]
        public static readonly DatQuestWrapper RavenousGod;


        /************************************
        *             Act 6
        ***********************************/

        [QuestId("a6q4")]
        public static readonly DatQuestWrapper FallenFromGrace;

        [QuestId("a6q5")]
        public static readonly DatQuestWrapper BestelEpic;

        [QuestId("a6q3")]
        public static readonly DatQuestWrapper FatherOfWar;

        [QuestId("a6q2")]
        public static readonly DatQuestWrapper EssenceOfUmbra;

        [QuestId("a6q7")]
        public static readonly DatQuestWrapper ClovenOne;

        [QuestId("a6q6")]
        public static readonly DatQuestWrapper PuppetMistress;

        [QuestId("a6q1")]
        public static readonly DatQuestWrapper BrineKing;


        /************************************
        *             Act 7
        ***********************************/

        [QuestId("a7q5")]
        public static readonly DatQuestWrapper SilverLocket;

        [QuestId("a7q2")]
        public static readonly DatQuestWrapper EssenceOfArtist;

        [QuestId("a7q3")]
        public static readonly DatQuestWrapper WebOfSecrets;

        [QuestId("a7q1")]
        public static readonly DatQuestWrapper MasterOfMillionFaces;

        [QuestId("a7q8")]
        public static readonly DatQuestWrapper InMemoryOfGreust;

        [QuestId("a7q7")]
        public static readonly DatQuestWrapper LightingTheWay;

        [QuestId("a7q9")]
        public static readonly DatQuestWrapper QueenOfDespair;

        [QuestId("a7q6")]
        public static readonly DatQuestWrapper KisharaStar;

        [QuestId("a7q4")]
        public static readonly DatQuestWrapper MotherOfSpiders;


        /************************************
        *             Act 8
        ***********************************/

        [QuestId("a8q1")]
        public static readonly DatQuestWrapper EssenceOfHag;

        [QuestId("a8q6")]
        public static readonly DatQuestWrapper LoveIsDead;

        [QuestId("a8q7")]
        public static readonly DatQuestWrapper GemlingLegion;

        [QuestId("a8q5")]
        public static readonly DatQuestWrapper WingsOfVastiri;

        [QuestId("a8q4")]
        public static readonly DatQuestWrapper ReflectionOfTerror;

        // Not used anywhere
        //[QuestId("a8q3")]
        //public static readonly DatQuestWrapper SolarEclipse;

        [QuestId("a8q2")]
        public static readonly DatQuestWrapper LunarEclipse;


        /************************************
        *             Act 9
        ***********************************/

        [QuestId("a9q3")]
        public static readonly DatQuestWrapper StormBlade;

        [QuestId("a9q5")]
        public static readonly DatQuestWrapper QueenOfSands;

        [QuestId("a9q4")]
        public static readonly DatQuestWrapper FastisFortuna;

        [QuestId("a9q2")]
        public static readonly DatQuestWrapper RulerOfHighgate;

        [QuestId("a9q1")]
        public static readonly DatQuestWrapper RecurringNightmare;


        /************************************
        *             Act 10
        ***********************************/

        [QuestId("a10q1")]
        public static readonly DatQuestWrapper SafePassage;

        [QuestId("a10q4")]
        public static readonly DatQuestWrapper NoLoveForOldGhosts;

        [QuestId("a10q6")]
        public static readonly DatQuestWrapper VilentaVengeance;

        [QuestId("a10q5")]
        public static readonly DatQuestWrapper MapToTsoatha;

        [QuestId("a10q2")]
        public static readonly DatQuestWrapper DeathAndRebirth;

        [QuestId("a10q3")]
        public static readonly DatQuestWrapper EndToHunger;


        /************************************
        *             Act 11
        ***********************************/

        [QuestId("a10q3a11")]
        public static readonly DatQuestWrapper EndToHungerEpilogue;


        public static readonly List<DatQuestWrapper> All;

        static Quests()
        {
            All = new List<DatQuestWrapper>();
            var questDict = Dat.Quests.ToDictionary(q => q.Id);
            bool error = false;

            foreach (var field in typeof(Quests).GetFields())
            {
                var attr = field.GetCustomAttribute<QuestId>();

                if (attr == null)
                    continue;

                if (questDict.TryGetValue(attr.Id, out var quest))
                {
                    field.SetValue(null, quest);
                    All.Add(quest);
                }
                else
                {
                    GlobalLog.Error($"[Quests] Cannot initialize \"{field.Name}\" field. DatQuests does not contain quest with \"{attr.Id}\" id.");
                    error = true;
                }
            }
            if (error) BotManager.Stop();
        }

        [AttributeUsage(AttributeTargets.Field)]
        public class QuestId : Attribute
        {
            public readonly string Id;

            public QuestId(string id)
            {
                Id = id;
            }
        }
    }
}