using System.Collections.Generic;
using System.Threading.Tasks;
using Default.EXtensions;
using Default.EXtensions.CachedObjects;
using Loki.Bot;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot
{
    public static class TownQuestgiversLogic
    {
        private static readonly List<CachedObject> Npcs = new List<CachedObject>();
        private static CachedObject _current;

        public static async Task<bool> Execute()
        {
            if (_current == null)
            {
                if ((_current = Npcs.ClosestValid()) == null)
                    return false;
            }

            var pos = _current.Position;
            var name = pos.Name;

            GlobalLog.Info($"[TalkToQuestgivers] Now going to talk to {pos}");

            if (!await pos.TryComeAtOnce())
            {
                GlobalLog.Error($"[TalkToQuestgivers] Unexpected error. \"{name}\" position is unwalkable.");
                _current.Unwalkable = true;
                _current = null;
                return true;
            }

            if (_current == null)
                return false;

            var obj = _current.Object;
            if (obj == null)
            {
                GlobalLog.Error($"[TalkToQuestgivers] Unexpected error. \"{name}\" object is null.");
                _current.Ignored = true;
                _current = null;
                return true;
            }
            if (!obj.IsTargetable)
            {
                GlobalLog.Error($"[TalkToQuestgivers] Unexpected error. \"{name}\" is untargetable.");
                _current.Ignored = true;
                _current = null;
                return true;
            }
            if (!obj.HasNpcFloatingIcon)
            {
                GlobalLog.Debug($"[TalkToQuestgivers] \"{name}\" no longer has NpcFloatingIcon.");
                Npcs.Remove(_current);
                _current = null;
                return true;
            }

            var attempts = ++_current.InteractionAttempts;
            if (attempts > 5)
            {
                GlobalLog.Error($"[TalkToQuestgivers] All attempts to interact with \"{name}\" have been spent. Now ignoring it.");
                _current.Ignored = true;
                _current = null;
                return true;
            }

            if (!await obj.AsTownNpc().Talk())
            {
                await Wait.SleepSafe(1000);
                return true;
            }

            await Coroutines.CloseBlockingWindows();
            await Wait.SleepSafe(200);

            if (!obj.Fresh().HasNpcFloatingIcon)
            {
                Npcs.Remove(_current);
                _current = null;
            }
            return true;
        }

        public static void Tick()
        {
            foreach (var obj in LokiPoe.ObjectManager.Objects)
            {
                if (!(obj is Npc npc) || npc.Name == "Navali")
                    continue;

                var id = npc.Id;
                var cached = Npcs.Find(n => n.Id == id);

                if (npc.HasNpcFloatingIcon)
                {
                    if (cached == null)
                    {
                        var pos = npc.WalkablePosition(5, 20);
                        Npcs.Add(new CachedObject(id, pos));
                        GlobalLog.Debug($"[TalkToQuestgivers] Adding \"{npc.Name}\" to the talk list.");
                    }
                }
                else
                {
                    if (cached != null)
                    {
                        GlobalLog.Debug($"[TalkToQuestgivers] Removing \"{npc.Name}\" from the talk list.");
                        if (cached == _current) _current = null;
                        Npcs.Remove(cached);
                    }
                }
            }
        }

        public static void Reset()
        {
            _current = null;
            Npcs.Clear();
        }

        public static bool ShouldExecute
        {
            get
            {
                var state = Settings.Instance.CurrentQuestState;

                if (string.IsNullOrEmpty(state))
                    return true;

                if (state.StartsWith("Talk to") || state.StartsWith("Turn in"))
                    return false;

                if (state.EndsWith("reward"))
                    return false;

                return true;
            }
        }
    }
}