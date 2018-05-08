using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using static BetterArtisanGoodIcons.ArtisanGoodsManager;
using static StardewValley.Object;

namespace BetterArtisanGoodIcons
{
    /// <summary>Manages textures for an artisan good.</summary>
    internal class ArtisanGoodTextureManager
    {
        private Texture2D spriteSheet;
        private IDictionary<int, Rectangle> positions = new Dictionary<int, Rectangle>();
        private int artisanGoodIndex;

        private static readonly string[] HoneyFlowerTypes = Enum.GetNames(typeof(HoneyType));

        /// <summary>Construct an instance, building the positions dictionary.</summary>
        internal ArtisanGoodTextureManager(Texture2D texture, ArtisanGood good, int category)
        {
            this.spriteSheet = texture;
            this.artisanGoodIndex = (int)good;

            List<Tuple<int, string>> names = new List<Tuple<int, string>>();

            //Get all item ids and names for a given category
            foreach (KeyValuePair<int, string> item in Game1.objectInformation)
            {
                string[] parts = item.Value.Split('/');
                if (parts[3] == $"Basic {category}")
                    if (good != ArtisanGood.Honey || HoneyFlowerTypes.Contains(parts[0].Replace(" ", "")))
                        names.Add(new Tuple<int, string>(item.Key, parts[0]));
            }

            if (good == ArtisanGood.Honey)
                names.Add(new Tuple<int, string>((int)HoneyType.Wild, "Wild"));

            //Sort names alphabetically
            names.Sort((T1, T2) => T1.Item2.CompareTo(T2.Item2));

            //Get sprite positions, assuming alphabetical order and 4 sprites in each row
            int x = 0;
            int y = 0;
            foreach (Tuple<int, string> item in names)
            {
                this.positions[item.Item1] = new Rectangle(x, y, 16, 16);
                x += 16;
                if (x == 16 * 4)
                {
                    x = 0;
                    y += 16;
                }
            }
        }

        /// <summary>Gets the position for the given non-honey preserved item.</summary>
        internal void GetPosition(int preservedIndex, out Rectangle mainRectangle, out Rectangle iconRectangle)
        {
            if (preservedIndex == 0)
            {
                mainRectangle = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.artisanGoodIndex, 16, 16);
                iconRectangle = Rectangle.Empty;
                return;
            }

            mainRectangle = this.positions[preservedIndex];
            iconRectangle = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, preservedIndex, 16, 16);
        }

        /// <summary>Gets the position for the given honey item.</summary>
        internal void GetPositionHoney(string honeyName, out Rectangle mainRectangle, out Rectangle iconRectangle)
        {
            honeyName = honeyName.Replace(" ", "");
            string matchingType = HoneyFlowerTypes.FirstOrDefault(item => honeyName.Contains(item));
            if (matchingType != null)
            {
                int index = (int)((HoneyType)Enum.Parse(typeof(HoneyType), matchingType));
                mainRectangle = this.positions[index];
                iconRectangle = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, index == -1 ? 421 : index, 16, 16);
                return;
            }

            iconRectangle = Rectangle.Empty;
            mainRectangle = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.artisanGoodIndex, 16, 16);
        }

        /// <summary>Gets the spritesheet for the given non-honey preserved item.</summary>
        internal Texture2D GetSpritesheet(int regentIndex)
        {
            if (regentIndex == 0)
                return Game1.objectSpriteSheet;

            return this.spriteSheet;
        }

        /// <summary>Gets the spritesheet for the given honey preserved item.</summary>
        internal Texture2D GetSpritesheetHoney(string name)
        {
            if (name == "Honey")
                return Game1.objectSpriteSheet;

            return this.spriteSheet;
        }

        /// <summary>Whether this manager can draw textures for the given item.</summary>
        internal bool CanHandle(int itemIndex)
        {
            return this.artisanGoodIndex == itemIndex;
        }
    }
}