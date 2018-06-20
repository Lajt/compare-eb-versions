﻿using System;
using System.Collections.Generic;
using System.IO;
using Default.EXtensions;
using Loki.Game.GameData;
using Loki.Game.Objects;
using Newtonsoft.Json;

namespace Default.ChaosRecipe
{
    public class RecipeData
    {
        private readonly int[] _counts = new int[RecipeItemType.TotalTypeCount];

        public void IncreaseItemCount(int itemType)
        {
            ++_counts[itemType];
        }

        public int GetItemCount(int itemType)
        {
            return _counts[itemType];
        }

        public void Reset()
        {
            for (int i = 0; i < _counts.Length; i++)
            {
                _counts[i] = 0;
            }
        }

        public void SyncWithStashTab()
        {
            Reset();

            foreach (var item in Inventories.StashTabItems)
            {
                if (!IsItemForChaosRecipe(item, out var type))
                    continue;

                IncreaseItemCount(type);
            }

            SaveToJson(ChaosRecipe.StashDataPath);
        }

        public bool HasCompleteSet()
        {
            return GetItemCount(RecipeItemType.Ring) >= 2 &&
                   GetItemCount(RecipeItemType.Amulet) >= 1 &&
                   GetItemCount(RecipeItemType.Belt) >= 1 &&
                   GetItemCount(RecipeItemType.Gloves) >= 1 &&
                   GetItemCount(RecipeItemType.Boots) >= 1 &&
                   GetItemCount(RecipeItemType.Helmet) >= 1 &&
                   GetItemCount(RecipeItemType.BodyArmor) >= 1 &&
                   GetItemCount(RecipeItemType.Weapon) >= 1;
        }

        public void Log()
        {
            GlobalLog.Info($"[ChaosRecipe] Weapons: {GetItemCount(RecipeItemType.Weapon)}");
            GlobalLog.Info($"[ChaosRecipe] Body armors: {GetItemCount(RecipeItemType.BodyArmor)}");
            GlobalLog.Info($"[ChaosRecipe] Helmets: {GetItemCount(RecipeItemType.Helmet)}");
            GlobalLog.Info($"[ChaosRecipe] Boots: {GetItemCount(RecipeItemType.Boots)}");
            GlobalLog.Info($"[ChaosRecipe] Gloves: {GetItemCount(RecipeItemType.Gloves)}");
            GlobalLog.Info($"[ChaosRecipe] Belts: {GetItemCount(RecipeItemType.Belt)}");
            GlobalLog.Info($"[ChaosRecipe] Amulets: {GetItemCount(RecipeItemType.Amulet)}");
            GlobalLog.Info($"[ChaosRecipe] Rings: {GetItemCount(RecipeItemType.Ring)}");
        }

        public void SaveToJson(string path)
        {
            var json = JsonConvert.SerializeObject(_counts, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static RecipeData LoadFromJson(string path)
        {
            var data = new RecipeData();

            if (!File.Exists(path))
                return data;

            var json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                GlobalLog.Info("[ChaosRecipe] Fail to load stash data from json. File is empty.");
                return data;
            }
            int[] jsonCounts;
            try
            {
                jsonCounts = JsonConvert.DeserializeObject<int[]>(json);
            }
            catch (Exception)
            {
                GlobalLog.Info("[ChaosRecipe] Fail to load stash data from json. Exception during json deserialization.");
                return data;
            }
            if (jsonCounts == null)
            {
                GlobalLog.Info("[ChaosRecipe] Fail to load stash data from json. Json deserealizer returned null.");
                return data;
            }
            Array.Copy(jsonCounts, data._counts, data._counts.Length);
            return data;
        }

        public static bool IsItemForChaosRecipe(Item item, out int itemType)
        {
            itemType = RecipeItemType.None;

            if (item.IsIdentified ||
                item.RarityLite() != Rarity.Rare ||
                item.SocketCount >= 6 ||
                item.ItemLevel < Settings.Instance.MinILvl)
                return false;

            return ClassToTypeDict.TryGetValue(item.Class, out itemType);
        }

        private static readonly Dictionary<string, int> ClassToTypeDict = new Dictionary<string, int>
        {
            [ItemClasses.TwoHandAxe] = RecipeItemType.Weapon,
            [ItemClasses.TwoHandMace] = RecipeItemType.Weapon,
            [ItemClasses.TwoHandSword] = RecipeItemType.Weapon,
            [ItemClasses.Bow] = RecipeItemType.Weapon,
            [ItemClasses.Staff] = RecipeItemType.Weapon,

            [ItemClasses.BodyArmour] = RecipeItemType.BodyArmor,
            [ItemClasses.Helmet] = RecipeItemType.Helmet,
            [ItemClasses.Boots] = RecipeItemType.Boots,
            [ItemClasses.Gloves] = RecipeItemType.Gloves,
            [ItemClasses.Belt] = RecipeItemType.Belt,
            [ItemClasses.Amulet] = RecipeItemType.Amulet,
            [ItemClasses.Ring] = RecipeItemType.Ring
        };
    }
}