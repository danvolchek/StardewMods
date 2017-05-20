using System;
using System.Collections.Generic;
using CustomElementHandler;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;

namespace GiantCropRing
{

    public class GiantRing : Ring, ISaveElement
    {
        public override string DisplayName
        {
            get { return name; }
            set { name = value; }

            }

        public static Texture2D texture;
        public static new int price;
        public GiantRing()
        {
            build();
        }

       

        private void build()
        {
            this.category = -96;
            this.name = "Giant Crop Ring";
            this.description = "Increases the chance of growing giant crops if you wear it before going to sleep.";
            this.indexInTileSheet = -1;
            this.uniqueID = Game1.year + Game1.dayOfMonth + Game1.timeOfDay + this.indexInTileSheet + Game1.player.getTileX() + (int)Game1.stats.MonstersKilled + (int)Game1.stats.itemsCrafted;

        }


        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            spriteBatch.Draw(texture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)) * scaleSize, new Rectangle?(Game1.getSourceRectForStandardTileSheet(texture, 0, 16, 16)), Color.White * transparency, 0.0f, new Vector2(8f, 8f) * scaleSize, scaleSize * (float)Game1.pixelZoom, SpriteEffects.None, layerDepth);
        }

        public override Item getOne()
        {
            return (Item)new GiantRing();
        }

        public object getReplacement()
        {
            if (Game1.player.leftRing == this || Game1.player.rightRing == this)
            {
                return new Ring(517);
            }
            else
            {
                return new Chest(true);
            }
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> savedata = new Dictionary<string, string>();
            savedata.Add("name", name);
            return savedata;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
           
                build();
            
        }
    }
}
