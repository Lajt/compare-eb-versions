using System.Threading.Tasks;
using System.Windows.Controls;
using Default.EXtensions.Global;
using log4net;
using Loki.Bot;
using Loki.Common;
using settings = Default.EXtensions.Settings;

namespace Default.EXtensions
{
    public class EXtensions : IContent, IUrlProvider
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private Gui _gui;

        public static void AbandonCurrentArea()
        {
            var botName = BotManager.Current.Name;
            if (botName.Contains("QuestBot"))
            {
                Travel.RequestNewInstance(World.CurrentArea);
            }
            else if (botName.Contains("MapBot"))
            {
                BotManager.Current.Message(new Message("MB_set_is_on_run", null, false));
            }
        }

        #region Unused interface methods

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public MessageResult Message(Message message)
        {
            return MessageResult.Unprocessed;
        }

        public void Initialize()
        {
            Log.DebugFormat("[EXtensions] Initialize");
        }

        public void Deinitialize()
        {
            Log.DebugFormat("[EXtensions] Deinitialize");
        }

        public string Name => "EXtensions";
        public string Description => "Global logic used by bot bases.";
        public string Author => "ExVault";
        public string Version => "1.3";
        public JsonSettings Settings => settings.Instance;
        public UserControl Control => _gui ?? (_gui = new Gui());
        public string Url => "https://www.thebuddyforum.com/threads/cross-bot-settings-description.290808/";

        #endregion
    }
}