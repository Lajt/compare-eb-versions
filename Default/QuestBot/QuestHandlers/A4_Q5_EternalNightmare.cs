using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.Global;
using Default.EXtensions.Positions;
using Loki.Game;
using Loki.Game.GameData;
using Loki.Game.Objects;

namespace Default.QuestBot.QuestHandlers
{
    public static class A4_Q5_EternalNightmare
    {
        private const string ShavronneName = "Shavronne of Umbra";
        private const string DoedreName = "Doedre Darktongue";
        private const string MaligaroName = "Maligaro, The Inquisitor";

        private static readonly TgtPosition ShavronneTgt = new TgtPosition(ShavronneName, "shavronn_Area_v01_01_c5r5.tgt");
        private static readonly TgtPosition DoedreTgt = new TgtPosition(DoedreName, "DoedreStones_Area_v01_01_c5r5.tgt");
        private static readonly TgtPosition MaligaroTgt = new TgtPosition(MaligaroName, "Maligaro_Area_v01_01_c5r5.tgt");

        private static Monster Shavronne => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Shavronne_of_Umbra)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Monster Doedre => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Doedre_Darktongue)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static Monster Maligaro => LokiPoe.ObjectManager.GetObjects(LokiPoe.ObjectManager.PoeObjectEnum.Maligaro_The_Inquisitor)
            .FirstOrDefault<Monster>(m => m.Rarity == Rarity.Unique);

        private static List<Miniboss> Minibosses
        {
            get
            {
                var minibosses = CombatAreaCache.Current.Storage["Minibosses"] as List<Miniboss>;
                if (minibosses == null)
                {
                    ShavronneTgt.Initialize();
                    DoedreTgt.Initialize();
                    MaligaroTgt.Initialize();

                    minibosses = new List<Miniboss>
                    {
                        new Miniboss(ShavronneName, ShavronneTgt, null, false),
                        new Miniboss(DoedreName, DoedreTgt, null, false),
                        new Miniboss(MaligaroName, MaligaroTgt, null, false),
                    };
                    CombatAreaCache.Current.Storage["Minibosses"] = minibosses;
                }
                return minibosses;
            }
        }

        private static Miniboss _currentMiniboss;
        private static bool _tpAfterLastMiniBossKilled;

        private static Monster _abominationPiety;
        private static Npc _bellyPiety;

        private static Npc _harvestPiety;
        private static NetworkObject _malachaiRoomObj;
        private static NetworkObject _coreMouth;
        private static AreaTransition _blackCoreTransition;
        private static AreaTransition _blackHeartTransition;
        private static AreaTransition _highgateTransition;
        private static Monster _malachaiPiety;
        private static Monster _malachai;
        private static Monster _beastHeart;

        public static void Tick()
        {
            if (World.Act4.Harvest.IsCurrentArea)
            {
                if (_currentMiniboss != null)
                {
                    var name = _currentMiniboss.Name;
                    Monster mob = null;

                    if (name == ShavronneName)
                        mob = Shavronne;
                    else if (name == DoedreName)
                        mob = Doedre;
                    else if (name == MaligaroName)
                        mob = Maligaro;

                    if (mob == null)
                        return;

                    if (mob.IsDead)
                    {
                        _currentMiniboss.Killed = true;
                        _currentMiniboss = null;

                        if (Minibosses.All(m => m.Killed))
                            _tpAfterLastMiniBossKilled = true;

                        return;
                    }

                    var pos = mob.WalkablePosition();

                    if (_currentMiniboss.Position == null)
                        GlobalLog.Warn($"[EternalNightmare] Registering {pos}");

                    _currentMiniboss.Position = pos;
                }
            }
        }

        private static async Task KillMinibosses()
        {
            if (_currentMiniboss == null)
                _currentMiniboss = Minibosses.Where(b => !b.Killed).OrderBy(b => b.TgtPosition.DistanceSqr).FirstOrDefault();

            // ReSharper disable once PossibleNullReferenceException
            var pos = _currentMiniboss.Position;
            if (pos != null)
            {
                await Helpers.MoveAndWait(pos);
                return;
            }
            _currentMiniboss.TgtPosition.Come();
        }

        public static async Task<bool> KillMalachai()
        {
            if (World.Act4.Harvest.IsCurrentArea)
            {
                UpdateMalachaiFightObjects();

                if (_malachaiRoomObj != null)
                {
                    if (_highgateTransition != null)
                        return false;

                    if (_blackHeartTransition != null && _blackHeartTransition.IsTargetable && _blackHeartTransition.Distance < 80)
                    {
                        if (!await PlayerAction.TakeTransition(_blackHeartTransition))
                            ErrorManager.ReportError();

                        return true;
                    }
                    if (_beastHeart != null && _beastHeart.IsTargetable && !_beastHeart.IsDead)
                    {
                        _beastHeart.WalkablePosition().Come();
                        return true;
                    }
                    if (_malachaiPiety != null && _malachaiPiety.Reaction == Reaction.Enemy)
                    {
                        var pos = _malachaiPiety.WalkablePosition();
                        if (pos.IsFar)
                        {
                            pos.Come();
                        }
                        return true;
                    }
                    if (_malachai != null)
                    {
                        await Helpers.MoveAndWait(_malachai.WalkablePosition());
                        return true;
                    }
                    if (_malachaiPiety != null && _malachaiPiety.Reaction == Reaction.Friendly && _malachaiPiety.Distance > 15)
                    {
                        _malachaiPiety.WalkablePosition().Come();
                        return true;
                    }
                    GlobalLog.Debug("Waiting for any Malachai fight object");
                    await Wait.StuckDetectionSleep(500);
                    return true;
                }

                if (Minibosses.Any(b => !b.Killed))
                {
                    await KillMinibosses();
                    return true;
                }
                if (_tpAfterLastMiniBossKilled)
                {
                    await PlayerAction.TpToTown();
                    _tpAfterLastMiniBossKilled = false;
                    return true;
                }
                if (_harvestPiety != null && _harvestPiety.IsTargetable && _harvestPiety.HasNpcFloatingIcon)
                {
                    var pietyPos = _harvestPiety.WalkablePosition();
                    if (pietyPos.IsFar)
                    {
                        pietyPos.Come();
                    }
                    else
                    {
                        await Helpers.TalkTo(_harvestPiety);
                    }
                    return true;
                }
                if (_coreMouth != null && _coreMouth.IsTargetable && _coreMouth.Distance < 80)
                {
                    await _coreMouth.WalkablePosition().ComeAtOnce();

                    if (!await PlayerAction.Interact(_coreMouth, () => !_coreMouth.IsTargetable, "Black Core Mouth opening"))
                        ErrorManager.ReportError();

                    return true;
                }
                if (_blackCoreTransition != null && _blackCoreTransition.IsTargetable && _blackCoreTransition.Distance < 80)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.Malachai))
                        return true;

                    await _blackCoreTransition.WalkablePosition().ComeAtOnce();

                    if (!await PlayerAction.TakeTransition(_blackCoreTransition))
                        ErrorManager.ReportError();

                    return true;
                }
            }
            if (World.Act4.BellyOfBeast2.IsCurrentArea)
            {
                UpdateBellyObjects();

                if (_abominationPiety != null)
                {
                    if (await Helpers.StopBeforeBoss(Settings.BossNames.PietyAbomination))
                        return true;

                    await Helpers.MoveAndWait(_abominationPiety.WalkablePosition());
                    return true;
                }
                if (_bellyPiety != null)
                {
                    if (!_bellyPiety.IsTargetable)
                    {
                        await Helpers.MoveAndWait(_bellyPiety.WalkablePosition());
                        return true;
                    }
                    if (_bellyPiety.HasNpcFloatingIcon)
                    {
                        var pietyPos = _bellyPiety.WalkablePosition();
                        if (pietyPos.IsFar)
                        {
                            pietyPos.Come();
                        }
                        else
                        {
                            await Helpers.TalkTo(_bellyPiety);
                        }
                        return true;
                    }
                }
            }
            await Travel.To(World.Act4.Harvest);
            return true;
        }

        public static async Task<bool> TalkToTasuni()
        {
            if (World.Act4.Highgate.IsCurrentArea)
            {
                await TownNpcs.Tasuni.Position.ComeAtOnce();

                var tasuniObj = TownNpcs.Tasuni.NpcObject;

                if (!tasuniObj.HasNpcFloatingIcon)
                    return false;

                await Helpers.TalkTo(tasuniObj);
                return true;
            }
            if (World.Act4.Harvest.IsCurrentArea)
            {
                UpdateMalachaiFightObjects();

                if (_highgateTransition != null && _highgateTransition.PathExists())
                {
                    if (!await PlayerAction.TakeTransition(_highgateTransition))
                        ErrorManager.ReportError();
                }
                else
                {
                    await Travel.To(World.Act4.Highgate);
                }
                return true;
            }
            await Travel.To(World.Act4.Highgate);
            return true;
        }

        private static void UpdateMalachaiFightObjects()
        {
            _harvestPiety = null;
            _malachaiRoomObj = null;
            _coreMouth = null;
            _blackCoreTransition = null;
            _blackHeartTransition = null;
            _highgateTransition = null;
            _malachaiPiety = null;
            _malachai = null;
            _beastHeart = null;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var metadata = obj.Metadata;

                var transition = obj as AreaTransition;
                if (transition != null)
                {
                    if (metadata.Contains("Act4/CoreTransition"))
                    {
                        _blackCoreTransition = transition;
                        continue;
                    }
                    if (metadata.Contains("Act4/MalachaiDeathPortal"))
                    {
                        _highgateTransition = transition;
                        continue;
                    }
                    // This transition does not have unique Metadata
                    if (transition.Name == "The Black Heart")
                    {
                        _blackHeartTransition = transition;
                    }
                    continue;
                }
                var mob = obj as Monster;
                if (mob != null && mob.Rarity == Rarity.Unique)
                {
                    if (metadata.Contains("Malachai/MalachaiBoss"))
                    {
                        _malachai = mob;
                        continue;
                    }
                    if (metadata.Contains("Malachai/BeastHeart"))
                    {
                        _beastHeart = mob;
                        continue;
                    }
                    if (metadata.Contains("Axis/Piety"))
                    {
                        _malachaiPiety = mob;
                    }
                    continue;
                }
                var npc = obj as Npc;
                if (npc != null)
                {
                    if (obj.Metadata.Contains("Act4/PietyHarvest"))
                    {
                        _harvestPiety = npc;
                    }
                    continue;
                }
                if (metadata == "Metadata/Monsters/Malachai/ArenaMiddle")
                {
                    _malachaiRoomObj = obj;
                    continue;
                }
                if (metadata.Contains("Act4/CoreMouth"))
                {
                    _coreMouth = obj;
                }
            }
        }

        private static void UpdateBellyObjects()
        {
            _abominationPiety = null;
            _bellyPiety = null;

            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                var mob = obj as Monster;
                if (mob != null)
                {
                    if (mob.Metadata.Contains("Axis/PietyBeastBoss"))
                    {
                        _abominationPiety = mob;
                    }
                    continue;
                }
                var npc = obj as Npc;
                if (npc != null)
                {
                    if (npc.Metadata.Contains("Act4/PietyBelly"))
                    {
                        _bellyPiety = npc;
                    }
                }
            }
        }

        private class Miniboss
        {
            public string Name { get; }
            public TgtPosition TgtPosition { get; }
            public WalkablePosition Position { get; set; }
            public bool Killed { get; set; }

            public Miniboss(string name, TgtPosition tgtPosition, WalkablePosition position, bool killed)
            {
                Name = name;
                TgtPosition = tgtPosition;
                Position = position;
                Killed = killed;
            }
        }
    }
}