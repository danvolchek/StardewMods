using BetterArtisanGoodIcons.Framework.Data.Format;

namespace BetterArtisanGoodIcons.Framework.Data.Loading
{
    internal static class VanillaArtisanGood
    {
        public static readonly ItemIndicator Jelly = new ItemIndicator("Jelly");

        public static readonly ItemIndicator Pickles = new ItemIndicator("Pickles");

        public static readonly ItemIndicator Wine = new ItemIndicator("Wine");

        public static readonly ItemIndicator Juice = new ItemIndicator("Juice");

        public static readonly ItemIndicator Honey = new ItemIndicator("Honey");

        public static readonly ItemIndicator Base = new ItemIndicator(0);

        public static readonly string[] All = {"Jelly", "Pickles", "Wine", "Juice", "Honey"};


        public const int SpriteSize = 16;
    }
}
