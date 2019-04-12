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
        private readonly GeneratedSpriteManager manager;

        public DoorSpriteGenerator(GeneratedSpriteManager manager, IMonitor monitor, GraphicsDevice device)
        {
            this.manager = manager;
            this.monitor = monitor;
            this.device = device;
        }

        public IDictionary<string, Texture2D> GenerateDoorSprites(IList<LoadedContentPackDoorEntry> contentPacks)
        {
            IDictionary<string, Texture2D> generatedTextures = new Dictionary<string, Texture2D>();

            // Fields needed and modified during generation.
            DoorGenerationState state = new DoorGenerationState(this.device, contentPacks.Count * 4);
            generatedTextures.Add(state.CreateNewTexture());

            this.monitor.Log($"Generating {state.NumberOfDoorsLeft - state.NumberOfDoorsLeft / 4} door sprites.", LogLevel.Trace);

            foreach (LoadedContentPackDoorEntry info in contentPacks)
            {
                Color[] spriteData = new Color[info.Texture.Width * info.Texture.Height];
                info.Texture.GetData(spriteData);


                //Copy original sprite        -> Vertical Left
                Point originalPoint = state.TexturePoint;
                state.Copy(spriteData, Utils.ConvertTileIndexToPosition(info.Texture.Width, DoorSpriteGenerator.TileSize, info.Entry.TopLeftTileIndex), info.Texture.Width);
                this.manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Vertical, OpeningDirection.Left, state.CreateTileInfo());

                if (state.FinishSprite(out KeyValuePair<string, Texture2D> finishedTexture))
                    generatedTextures.Add(finishedTexture);


                //Reflect + reverse VL sprite -> Vertical Right
                state.ReflectOverYAxis(originalPoint);
                state.ReverseAnimationOrder();
                this.manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Vertical, OpeningDirection.Right, state.CreateTileInfo());

                if (state.FinishSprite(out finishedTexture))
                    generatedTextures.Add(finishedTexture);


                //Copy    + reverse VL sprite -> Horizontal Right
                Point reOrderedPoint = state.TexturePoint;
                state.Copy(state.TextureData, originalPoint, state.Width);
                state.ReverseAnimationOrder();
                this.manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Horizontal, OpeningDirection.Right, state.CreateTileInfo());

                if (state.FinishSprite(out finishedTexture))
                    generatedTextures.Add(finishedTexture);


                //Reflect + reverse HR sprite -> Horizontal Left
                state.ReflectOverYAxis(reOrderedPoint);
                state.ReverseAnimationOrder();
                this.manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Horizontal, OpeningDirection.Left, state.CreateTileInfo());

                if (state.FinishSprite(out finishedTexture))
                    generatedTextures.Add(finishedTexture);
            }

            state.Texture.SetData(state.TextureData);

            state.Texture.SaveAsPng(File.Create($"{state.TileSheetInfo.TileSheetId}.png"), state.Width, state.Texture.Height);

            if (state.NumberOfDoorsLeft != 0)
            {
                this.monitor.Log($"Something went wrong when generating door sprites - there are {state.NumberOfDoorsLeft} remaining but all content packs have been used. Please let the Better Doors author know!", LogLevel.Error);
            }
            else
            {
                this.monitor.Log("Successfully generated door sprites.", LogLevel.Trace);
            }

            return generatedTextures;
        }
    }
}