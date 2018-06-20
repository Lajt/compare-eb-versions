using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using Default.EXtensions;
using Default.EXtensions.Global;
using log4net;
using Loki;
using Loki.Bot;
using Loki.Common;
using Loki.Game.GameData;
using Loki.Game.Objects;
using settings = Default.ChaosRecipe.Settings;

namespace Default.ChaosRecipe
{
    public class ChaosRecipe : IPlugin, IStartStopEvents, IUrlProvider
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private Gui _gui;

        public static readonly string StashDataPath = Path.Combine(Configuration.Instance.Path, "ChaosRecipeStashData.json");

        public static readonly RecipeData StashData = RecipeData.LoadFromJson(StashDataPath);
        public static readonly RecipeData PickupData = new RecipeData();

        public static bool ShouldPickup(Item item)
        {
            if (!RecipeData.IsItemForChaosRecipe(item, out var type))
                return false;

            if (StashData.GetItemCount(type) + PickupData.GetItemCount(type) >= settings.Instance.GetMaxItemCount(type))
                return false;

            Log.Warn($"[ChaosRecipe] Adding \"{item.Name}\" (iLvl: {item.ItemLevel}) for pickup.");
            PickupData.IncreaseItemCount(type);
            return true;
        }

        public void Start()
        {
            var taskManager = BotStructure.TaskManager;
            taskManager.AddBefore(new StashRecipeTask(), "IdTask");
            taskManager.AddAfter(new SellRecipeTask(), "VendorTask");
        }

        public void Enable()
        {
            if (!CombatAreaCache.AddPickupItemEvaluator("ChaosRecipePickupEvaluator", ShouldPickup))
                Log.Error("[ChaosRecipe] Fail to add pickup item evaluator.");
        }

        public void Disable()
        {
            if (!CombatAreaCache.RemovePickupItemEvaluator("ChaosRecipePickupEvaluator"))
                Log.Error("[ChaosRecipe] Fail to remove pickup item evaluator.");
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == Events.Messages.AreaChanged)
            {
                var area = message.GetInput<DatWorldAreaWrapper>(3);
                if (area.IsTown || area.IsHideoutArea)
                {
                    Log.Info("[ChaosRecipe] Resetting pickup data.");
                    PickupData.Reset();
                }
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        #region Unused interface methods

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Deinitialize()
        {
        }

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public string Name => "ChaosRecipe";
        public string Description => "Plugin for doing chaos recipes.";
        public string Author => "ExVault";
        public string Version => "1.0";
        public UserControl Control => _gui ?? (_gui = new Gui());
        public JsonSettings Settings => settings.Instance;
        public string Url => "https://www.thebuddyforum.com/threads/chaosrecipe-settings-description.416051/";

        #endregion
    }
}