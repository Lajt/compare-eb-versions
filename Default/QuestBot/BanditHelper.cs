using System.Threading.Tasks;
using System.Windows.Forms;
using Default.EXtensions;
using Loki.Game;
using Loki.Game.Objects;

namespace Default.QuestBot
{
    public static class BanditHelper
    {
        public const string EramirName = "Eramir";
        public const string AliraName = "Alira";
        public const string KraitynName = "Kraityn";
        public const string OakName = "Oak";

        public static async Task<bool> Kill(NetworkObject bandit)
        {
            if (bandit == null)
            {
                GlobalLog.Error("[KillBandit] Bandit object is null.");
                return false;
            }

            await bandit.WalkablePosition().ComeAtOnce();

            if (!await OpenBanditPanel(bandit))
                return false;

            var err = LokiPoe.InGameState.BanditPanel.KillBandit();
            if (err != LokiPoe.InGameState.TalkToBanditResult.None)
            {
                GlobalLog.Error($"[KillBandit] Fail to select \"Kill\" option. Error: \"{err}\".");
                return false;
            }
            return true;
        }

        public static async Task<bool> Help(NetworkObject bandit)
        {
            if (bandit == null)
            {
                GlobalLog.Error("[HelpBandit] Bandit object is null.");
                return false;
            }

            var banditPos = bandit.WalkablePosition();
            await banditPos.ComeAtOnce();

            if (!await PlayerAction.Interact(bandit))
                return false;

            const int maxAttempts = 10;
            for (int i = 1; i <= maxAttempts; ++i)
            {
                await Wait.SleepSafe(200);

                if (LokiPoe.InGameState.BanditPanel.IsOpened ||
                    !LokiPoe.InGameState.NpcDialogUi.IsOpened ||
                    LokiPoe.InGameState.NpcDialogUi.DialogDepth == 1)
                    break;

                GlobalLog.Debug($"[HelpBandit] Pressing ESC to close the topmost NPC dialog ({i}/{maxAttempts}).");
                LokiPoe.Input.SimulateKeyEvent(Keys.Escape, true, false, false);
            }

            if (LokiPoe.InGameState.BanditPanel.IsOpened)
            {
                var err = LokiPoe.InGameState.BanditPanel.HelpBandit();
                if (err != LokiPoe.InGameState.TalkToBanditResult.None)
                {
                    GlobalLog.Error($"[HelpBandit] Fail to select \"Help\" option. Error: \"{err}\".");
                    return false;
                }
            }
            return await bandit.Fresh().AsTownNpc().TakeReward(null, "Get the Apex");
        }

        private static async Task<bool> OpenBanditPanel(NetworkObject bandit)
        {
            if (LokiPoe.InGameState.BanditPanel.IsOpened)
                return true;

            if (!await PlayerAction.Interact(bandit))
                return false;

            const int maxAttempts = 10;
            for (int i = 1; i <= maxAttempts; ++i)
            {
                if (LokiPoe.InGameState.BanditPanel.IsOpened)
                    return true;

                GlobalLog.Debug($"[OpenBanditPanel] Pressing ESC to close the topmost NPC dialog ({i}/{maxAttempts}).");
                LokiPoe.Input.SimulateKeyEvent(Keys.Escape, true, false, false);
                await Wait.SleepSafe(500);
            }
            GlobalLog.Error("[OpenBanditPanel] All attempts have been spent.");
            return false;
        }
    }
}