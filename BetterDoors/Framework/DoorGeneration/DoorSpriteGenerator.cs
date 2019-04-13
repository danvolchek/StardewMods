using BetterDoors.Framework.ContentPacks;
using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using System.IO;

namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>
    /// Generates alternate door sprites.
    /// </summary>
    internal class DoorSpriteGenerator
    {
        internal const int TileSize = 16;

        private readonly IMonitor monitor;
        private readonly GraphicsDevice device;
        private readonly string modId;
        private readonly GeneratedSpriteManager manager;
        private readonly IList<LoadedContentPackDoorEntry> contentPacks;

        public DoorSpriteGenerator(GeneratedSpriteManager manager, string modId, IMonitor monitor, GraphicsDevice device, IList<LoadedContentPackDoorEntry> contentPacks)
        {
            this.manager = manager;
            this.modId = modId;
            this.monitor = monitor;
            this.device = device;
            this.contentPacks = contentPacks;
        }

        public IDictionary<string, Texture2D> GenerateDoorSprites(string locationName)
        {
            IDictionary<string, Texture2D> generatedTextures = new Dictionary<string, Texture2D>();

            // Fields needed and modified during generation.
            DoorGenerationState state = new DoorGenerationState(locationName, this.modId, this.device, this.contentPacks.Count * 2);
            generatedTextures.Add(state.CreateNewTexture());

            foreach (LoadedContentPackDoorEntry info in this.contentPacks)
            {
                Color[] spriteData = new Color[info.Texture.Width * info.Texture.Height];
                info.Texture.GetData(spriteData);

                Point infoTileIndex = Utils.ConvertTileIndexToPosition(info.Texture.Width, DoorSpriteGenerator.TileSize, info.Entry.TopLeftTileIndex);

                bool vlRequested = this.manager.IsSpriteRequested(info.ModId, info.Entry.Name, Orientation.Vertical, OpeningDirection.Left);
                bool hrRequested = this.manager.IsSpriteRequested(info.ModId, info.Entry.Name, Orientation.Horizontal, OpeningDirection.Right);
                bool vrRequested = this.manager.IsSpriteRequested(info.ModId, info.Entry.Name, Orientation.Vertical, OpeningDirection.Right);
                bool hlRequested = this.manager.IsSpriteRequested(info.ModId, info.Entry.Name, Orientation.Horizontal, OpeningDirection.Left);

                if (vlRequested || hrRequested)
                {
                    //Copy original sprite        -> Vertical Left + Horizontal Right
                    state.Copy(spriteData, infoTileIndex, info.Texture.Width);
                    if (vlRequested)
                        this.manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Vertical, OpeningDirection.Left, state.CreateTileInfo(true));
                    if (hrRequested)
                        this.manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Horizontal, OpeningDirection.Right, state.CreateTileInfo(false));

                    if (state.FinishSprite(out KeyValuePair<string, Texture2D> finishedTexture))
                        generatedTextures.Add(finishedTexture);
                }

                if (vrRequested || hlRequested)
                {
                    //Reflect + reverse VL sprite -> Vertical Right
                    state.ReflectOverYAxis(spriteData, infoTileIndex, info.Texture.Width);
                    if (vrRequested)
                        this.manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Vertical, OpeningDirection.Right, state.CreateTileInfo(false));
                    if (hlRequested)
                        this.manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Horizontal, OpeningDirection.Left, state.CreateTileInfo(true));

                    if (state.FinishSprite(out KeyValuePair<string, Texture2D>  finishedTexture))
                        generatedTextures.Add(finishedTexture);
                }
            }

            state.Texture.SetData(state.TextureData);

            //TODO: remove
            //state.Texture.SaveAsPng(File.Create($"{state.TileSheetInfo.TileSheetId}.png"), state.Width, state.Texture.Height);

            if (state.NumberOfDoorsLeft != 0)
            {
                this.monitor.Log($"Something went wrong when generating door sprites - there are {state.NumberOfDoorsLeft} remaining but all content packs have been used. Please let the Better Doors author know!", LogLevel.Error);
            }

            return generatedTextures;
        }
    }
}