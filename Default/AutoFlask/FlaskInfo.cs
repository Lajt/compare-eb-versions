using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Default.EXtensions;

namespace Default.AutoFlask
{
    public class FlaskInfo
    {
        public bool HasLifeFlask;
        public bool HasManaFlask;
        public bool HasQsilverFlask;
        public bool HasTriggerFlask;

        public bool HasAntiFreeze;
        public bool HasAntiShock;
        public bool HasAntiIgnite;
        public bool HasAntiPoison;
        public bool HasAntiCurse;
        public bool HasAntiBleed;

        public readonly List<TriggerFlask> TriggerFlasks = new List<TriggerFlask>();

        public void AddTriggerFlask(int slot, string name, string effect, List<FlaskTrigger> triggers)
        {
            if (!TriggerFlasks.Exists(f => f.Name == name))
            {
                TriggerFlasks.Add(new TriggerFlask(name, effect, triggers));
            }
        }

        public void Log()
        {
            var sb = new StringBuilder("[FlaskInfo] Flags: ");
            foreach (var p in GetType().GetFields())
            {
                if (p.FieldType != typeof(bool))
                    continue;

                var value = (bool) p.GetValue(this);
                if (value) sb.Append($"{p.Name}, ");
            }

            sb.Length -= 2;
            sb.Append('.');
            GlobalLog.Info(sb.ToString());

            foreach (var flask in TriggerFlasks)
            {
                foreach (var trigger in flask.Triggers)
                {
                    GlobalLog.Info($"[{flask.Name}] {trigger}.");
                }
            }
        }

        public class TriggerFlask
        {
            public readonly string Name;
            public readonly string Effect;
            public readonly List<FlaskTrigger> Triggers;
            public readonly Stopwatch PostUseDelay;

            public TriggerFlask(string name, string effect, List<FlaskTrigger> triggers)
            {
                Name = name;
                Effect = effect;
                Triggers = triggers;
                PostUseDelay = new Stopwatch();

                if (triggers.Exists(t => t.Type == TriggerType.Attack))
                    PostUseDelay.Start();
            }
        }
    }
}