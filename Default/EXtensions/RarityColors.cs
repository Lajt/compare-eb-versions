using System.Windows.Media;
using Loki.Game.GameData;

namespace Default.EXtensions
{
    public static class RarityColors
    {
        public static readonly SolidColorBrush Normal =
            new SolidColorBrush(Color.FromRgb(200, 200, 200));

        public static readonly SolidColorBrush Magic =
            new SolidColorBrush(Color.FromRgb(136, 136, 255));

        public static readonly SolidColorBrush Rare =
            new SolidColorBrush(Color.FromRgb(255, 255, 119));

        public static readonly SolidColorBrush Unique =
            new SolidColorBrush(Color.FromRgb(175, 96, 37));

        public static readonly SolidColorBrush Currency =
            new SolidColorBrush(Color.FromRgb(170, 158, 130));

        public static readonly SolidColorBrush Gem =
            new SolidColorBrush(Color.FromRgb(27, 162, 155));

        public static readonly SolidColorBrush Quest =
            new SolidColorBrush(Color.FromRgb(74, 230, 58));

        public static readonly SolidColorBrush Card =
            new SolidColorBrush(Color.FromRgb(170, 230, 230));

        public static readonly SolidColorBrush Prophecy =
            new SolidColorBrush(Color.FromRgb(181, 75, 255));

        public static SolidColorBrush FromRarity(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Normal:
                    return Normal;

                case Rarity.Magic:
                    return Magic;

                case Rarity.Rare:
                    return Rare;

                case Rarity.Unique:
                    return Unique;

                case Rarity.Currency:
                    return Currency;

                case Rarity.Gem:
                    return Gem;

                case Rarity.Quest:
                    return Quest;
            }
            return null;
        }
    }
}