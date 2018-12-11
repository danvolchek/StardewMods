using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Objects;

namespace GiantCropRing
{
    public class GiantRing : Ring, ISaveElement
    {
        public static Texture2D texture;
        public new static int price;

        public GiantRing()
        {
            this.Build(this.getAdditionalSaveData());
        }

        public GiantRing(int id)
        {
            this.Build(new Dictionary<string, string> { { "name", "Giant Crop Ring" }, { "id", $"{id}" } });
        }

        public override string DisplayName
        {
            get => this.Name;
            set => this.Name = value;
        }

        public object getReplacement()
        {
            return new Ring(517);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            int id = this.uniqueID.Value == default(int) ? Guid.NewGuid().GetHashCode() : this.uniqueID.Value;
            Dictionary<string, string> savedata = new Dictionary<string, string> { { "name", this.Name }, {"id", $"{id}"} };
            return savedata;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            this.Build(additionalSaveData);
        }

        private void Build(IReadOnlyDictionary<string, string> additionalSaveData)
        {
            this.Category = -96;
            this.Name = "Giant Crop Ring";
            this.description = "Increases the chance of growing giant crops if you wear it before going to sleep.";
            this.uniqueID.Value = int.Parse(additionalSaveData["id"]);
            this.ParentSheetIndex = this.uniqueID.Value;
            this.indexInTileSheet.Value = this.uniqueID.Value;
        }


        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency,
            float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(texture, location + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2) * scaleSize,
                Game1.getSourceRectForStandardTileSheet(texture, 0, 16, 16), color * transparency, 0.0f,
                new Vector2(8f, 8f) * scaleSize, scaleSize * Game1.pixelZoom, SpriteEffects.None, layerDepth);
        }

        public override Item getOne()
        {
            return new GiantRing(this.uniqueID.Value);
        }
    }
}