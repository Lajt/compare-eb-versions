using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Default.EXtensions;
using Loki.Game.GameData;

namespace Default.QuestBot
{
    public partial class Gui : UserControl
    {
        public Gui()
        {
            InitializeComponent();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            CharClassComboBox.SelectedItem = CharacterClass.None;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetIfEmpty(sender as ComboBox);
        }

        private void Combobox_OnLoaded(object sender, RoutedEventArgs e)
        {
            ResetIfEmpty(sender as ComboBox);
        }

        private static void ResetIfEmpty(ComboBox box)
        {
            if (box?.SelectedIndex < 0)
            {
                box.SelectedIndex = 0;
            }
        }

        public class RewardConverter : IMultiValueConverter
        {
            public static readonly RewardConverter Instance = new RewardConverter();

            private static readonly RewardEntry Any = new RewardEntry(Settings.DefaultRewardName, Rarity.Normal);

            private static readonly List<RewardEntry> NoClass = new List<RewardEntry> {new RewardEntry(Settings.UnsetClassRewardName, Rarity.Normal)};

            private static readonly List<RewardEntry> Bandits = new List<RewardEntry>
            {
                new RewardEntry(BanditHelper.EramirName, Rarity.Normal),
                new RewardEntry(BanditHelper.AliraName, Rarity.Normal),
                new RewardEntry(BanditHelper.KraitynName, Rarity.Normal),
                new RewardEntry(BanditHelper.OakName, Rarity.Normal)
            };

            private static readonly List<RewardEntry> ThresholdJewels = new List<RewardEntry>
            {
                Any,
                new RewardEntry("Collateral Damage", Rarity.Unique),
                new RewardEntry("Fight for Survival", Rarity.Unique),
                new RewardEntry("First Snow", Rarity.Unique),
                new RewardEntry("Frozen Trail", Rarity.Unique),
                new RewardEntry("Hazardous Research", Rarity.Unique),
                new RewardEntry("Inevitability", Rarity.Unique),
                new RewardEntry("Omen on the Winds", Rarity.Unique),
                new RewardEntry("Overwhelming Odds", Rarity.Unique),
                new RewardEntry("Rapid Expansion", Rarity.Unique),
                new RewardEntry("Ring of Blades", Rarity.Unique),
                new RewardEntry("Spreading Rot", Rarity.Unique),
                new RewardEntry("Violent Dead", Rarity.Unique),
                new RewardEntry("Wildfire", Rarity.Unique),
            };

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                var questId = (string) values[0];
                var charClass = (CharacterClass) values[1];

                if (charClass == CharacterClass.None)
                    return NoClass;

                if (questId == Quests.DealWithBandits.Id)
                    return Bandits;

                if (questId == Quests.DeathToPurity.Id)
                    return ThresholdJewels;

                var datRewards = Dat.QuestRewards
                    .Where(r => r.Quest.Id == questId && (r.Class == charClass || r.Class == CharacterClass.None))
                    .ToList();

                if (datRewards.Count == 0)
                    return new List<RewardEntry> {new RewardEntry("Error! No values.", Rarity.Normal)};

                var rewardEntries = new List<RewardEntry> {Any};
                rewardEntries.AddRange(datRewards.Select(r => new RewardEntry(r.Item.Name, r.Rarity)));
                return rewardEntries;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class RarityToColorConverter : IValueConverter
        {
            public static readonly RarityToColorConverter Instance = new RarityToColorConverter();

            private static readonly HashSet<string> WhiteColorOverride = new HashSet<string>
            {
                Settings.DefaultRewardName,
                BanditHelper.EramirName,
                BanditHelper.AliraName,
                BanditHelper.KraitynName,
                BanditHelper.OakName
            };

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var reward = (RewardEntry) value;
                var name = reward.Name;
                var rarity = reward.Rariry;

                if (WhiteColorOverride.Contains(name))
                    return Brushes.White;

                //for some reason unique jewel rewards for "Through Sacred Ground" have Quest rarity
                if (rarity == Rarity.Quest && name.ContainsIgnorecase("jewel"))
                    return RarityColors.Unique;

                return RarityColors.FromRarity(rarity);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class CharClassToBoolConverter : IValueConverter
        {
            public static readonly CharClassToBoolConverter Instance = new CharClassToBoolConverter();

            // used for element's IsEnabled
            // if parameter is true and class is not selected, element (character selection box) will be enabled
            // if parameter is false and class is not selected, element (quest reward selection box) will be disabled
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var charClass = (CharacterClass) value;
                return (bool) parameter ? charClass == CharacterClass.None : charClass != CharacterClass.None;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class RewardEntry
        {
            public string Name { get; set; }
            public Rarity Rariry { get; set; }

            public RewardEntry(string name, Rarity rariry)
            {
                Name = name;
                Rariry = rariry;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private void AddRuleButton_OnClick(object sender, RoutedEventArgs e)
        {
            var rule = new Settings.GrindingRule();
            rule.Areas.Add(new Settings.GrindingArea());
            Settings.Instance.GrindingRules.Add(rule);
        }

        private void DeleteRuleButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var rule = (Settings.GrindingRule) button.DataContext;
            Settings.Instance.GrindingRules.Remove(rule);
        }

        private void AddAreaButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var areas = (ObservableCollection<Settings.GrindingArea>) button.Tag;
            areas.Add(new Settings.GrindingArea());
        }

        private void AreaSelectionComboBox_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var cb = (ComboBox) sender;
            var area = (Settings.GrindingArea) cb.DataContext;
            var rule = (Settings.GrindingRule) cb.Tag;
            rule.Areas.Remove(area);
        }
    }
}