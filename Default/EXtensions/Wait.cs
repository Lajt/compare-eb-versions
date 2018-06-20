using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Default.EXtensions.Global;
using Loki.Bot;
using Loki.Bot.Pathfinding;
using Loki.Game;

namespace Default.EXtensions
{
    public static class Wait
    {
        public static async Task<bool> For(Func<bool> condition, string desc, int step = 100, int timeout = 3000)
        {
            return await For(condition, desc, () => step, timeout);
        }

        public static async Task<bool> For(Func<bool> condition, string desc, Func<int> step, int timeout = 3000)
        {
            if (condition())
                return true;

            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeout)
            {
                await StuckDetectionSleep(step());
                GlobalLog.Debug($"[WaitFor] Waiting for {desc} ({Math.Round(timer.ElapsedMilliseconds / 1000f, 2)}/{timeout / 1000f})");
                if (condition())
                    return true;
            }
            GlobalLog.Error($"[WaitFor] Wait for {desc} timeout.");
            return false;
        }

        public static async Task<bool> ForAreaChange(uint areaHash, int timeout = 40000)
        {
            return await For(() => ExilePather.AreaHash != areaHash, "area change", 500, timeout);
        }

        // Requires more work, "Fail to join any instances" case
        public static async Task<bool> ForAreaChangeV2(uint areaHash, string loadingAreaText)
        {
            bool isOnLoadingScreen = false;
            int timeout = 5000;
            var timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeout)
            {
                if (ExilePather.AreaHash != areaHash)
                {
                    GlobalLog.Debug("[WaitForAreaChange] Area hash has been changed.");
                    return true;
                }
                if (LokiPoe.InGameState.IsEnteringAreaTextShown)
                {
                    GlobalLog.Debug("[WaitForAreaChange] Entering area text is shown.");
                    isOnLoadingScreen = true;
                    break;
                }
                var text = LokiPoe.InGameState.EnteringAreaText;
                if (!string.IsNullOrEmpty(text) && text != loadingAreaText)
                {
                    GlobalLog.Debug("[WaitForAreaChange] Entering area text has been changed.");
                    isOnLoadingScreen = true;
                    break;
                }
                await Coroutine.Sleep(50);
                GlobalLog.Debug($"[WaitForAreaChange] Waiting for loading screen ({Math.Round(timer.ElapsedMilliseconds / 1000f, 2)}/{timeout / 1000f})");
            }

            if (!isOnLoadingScreen)
            {
                GlobalLog.Error("[WaitForAreaChange] Wait for loading screen timeout.");
                return false;
            }

            timeout = 1000 * 60 * 5;
            timer = Stopwatch.StartNew();
            while (timer.ElapsedMilliseconds < timeout)
            {
                if (ExilePather.AreaHash != areaHash)
                    return true;

                await Coroutine.Sleep(500);
                GlobalLog.Debug($"[WaitForAreaChange] Waiting for area hash change ({Math.Round(timer.ElapsedMilliseconds / 1000f, 2)}/{timeout / 1000f})");
            }
            GlobalLog.Error("[WaitForAreaChange] Wait for area hash change timeout.");
            return false;
        }

        public static async Task Sleep(int ms)
        {
            await Coroutine.Sleep(ms);
        }

        public static async Task SleepSafe(int ms)
        {
            await Coroutine.Sleep(Math.Max(LatencyTracker.Current, ms));
        }

        public static async Task SleepSafe(int min, int max)
        {
            int latency = LatencyTracker.Current;
            if (latency > max)
            {
                await Coroutine.Sleep(latency);
            }
            else
            {
                await Coroutine.Sleep(LokiPoe.Random.Next(min, max + 1));
            }
        }

        public static async Task StuckDetectionSleep(int ms)
        {
            StuckDetection.Reset();
            await Coroutine.Sleep(ms);
        }

        public static async Task LatencySleep()
        {
            var ms = Math.Max((int) (LatencyTracker.Current * 1.15), 25);
            GlobalLog.Debug($"[LatencySleep] {ms} ms.");
            await Coroutine.Sleep(ms);
        }

        public static async Task ArtificialDelay()
        {
            var settings = Settings.Instance;
            var ms = LokiPoe.Random.Next(settings.MinArtificialDelay, settings.MaxArtificialDelay + 1);
            GlobalLog.Debug($"[ArtificialDelay] Now waiting for {ms} ms.");
            await Coroutine.Sleep(ms);
        }
    }
}