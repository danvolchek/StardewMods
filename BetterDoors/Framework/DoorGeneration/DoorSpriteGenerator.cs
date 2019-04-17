using BetterDoors.Framework.ContentPacks;
using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;

namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>Generates door sprites.</summary>
    internal class DoorSpriteGenerator
    {
        /*********
        ** Fields
        *********/
        /// <summary>The info manager where generated sprite info is stored.</summary>
        private readonly GeneratedDoorTileInfoManager manager;

        /// <summary>Content packs used to load doors.</summary>
        private readonly IList<ContentPackDoor> contentPacks;

        /// <summary>Unique id used to create tile sheet ids.</summary>
        private readonly string modId;

        /// <summary>Encapsulates monitoring and logging for a given module.</summary>
        private readonly IMonitor monitor;

        /// <summary>Graphics device used to create textures.</summary>
        private readonly GraphicsDevice device;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="manager">The info manager where generated sprite info is stored.</param>
        /// <param name="contentPacks">Content packs used to load doors.</param>
        /// <param name="modId">Unique id used to create tile sheet ids.</param>
        /// <param name="monitor">Encapsulates monitoring and logging for a given module.</param>
        /// <param name="device">Graphics device used to create textures.</param>
        public DoorSpriteGenerator(GeneratedDoorTileInfoManager manager, IList<ContentPackDoor> contentPacks, string modId, IMonitor monitor, GraphicsDevice device)
        {
            this.manager = manager;
            this.modId = modId;
            this.monitor = monitor;
            this.device = device;
            this.contentPacks = contentPacks;
        }

        /// <summary>Generates door sprites.</summary>
        /// <param name="locationName">A unique location named used to create tile sheet ids.</param>
        /// <returns>A map of asset key -> generated texture.</returns>
        public IDictionary<string, Texture2D> GenerateDoorSprites(string locationName)
        {
            IDictionary<string, Texture2D> generatedTextures = new Dictionary<string, Texture2D>();

            DoorGenerationState state = new DoorGenerationState(locationName, this.modId, this.device, this.contentPacks.Count * 2);
            generatedTextures.Add(state.CreateNewTexture());

            foreach (ContentPackDoor info in this.contentPacks)
            {
                Color[] spriteData = new Color[info.Texture.Width * info.Texture.Height];
                info.Texture.GetData(spriteData);

                bool vlRequested = this.manager.IsDoorRequested(info.ModId, info.Name, Orientation.Vertical, OpeningDirection.Left);
                bool hrRequested = this.manager.IsDoorRequested(info.ModId, info.Name, Orientation.Horizontal, OpeningDirection.Right);
                bool vrRequested = this.manager.IsDoorRequested(info.ModId, info.Name, Orientation.Vertical, OpeningDirection.Right);
                bool hlRequested = this.manager.IsDoorRequested(info.ModId, info.Name, Orientation.Horizontal, OpeningDirection.Left);

                if (vlRequested || hrRequested)
                {
                    //Copy original sprite        -> Vertical Left + Horizontal Right
                    state.Copy(spriteData, info.StartPosition, info.Texture.Width);
                    if (vlRequested)
                        this.manager.RegisterGeneratedTileInfo(info.ModId, info.Name, Orientation.Vertical, OpeningDirection.Left, state.CreateTileInfo(true));
                    if (hrRequested)
                        this.manager.RegisterGeneratedTileInfo(info.ModId, info.Name, Orientation.Horizontal, OpeningDirection.Right, state.CreateTileInfo(false));

                    if (state.FinishAnimation(out KeyValuePair<string, Texture2D> finishedTexture))
                        generatedTextures.Add(finishedTexture);
                }

                if (vrRequested || hlRequested)
                {
                    //Reflect + reverse VL sprite -> Vertical Right + Horizontal Left
                    state.ReflectOverYAxis(spriteData, info.StartPosition, info.Texture.Width);
                    if (vrRequested)
                        this.manager.RegisterGeneratedTileInfo(info.ModId, info.Name, Orientation.Vertical, OpeningDirection.Right, state.CreateTileInfo(false));
                    if (hlRequested)
                        this.manager.RegisterGeneratedTileInfo(info.ModId, info.Name, Orientation.Horizontal, OpeningDirection.Left, state.CreateTileInfo(true));

                    if (state.FinishAnimation(out KeyValuePair<string, Texture2D>  finishedTexture))
                        generatedTextures.Add(finishedTexture);
                }
            }

            state.Texture.SetData(state.TextureData);

            //state.Texture.SaveAsPng(File.Create($"{state.TileSheetInfo.TileSheetId}.png"), state.Width, state.Texture.Height);

            if (state.NumberOfDoorsLeft != 0)
            {
                this.monitor.Log($"Something went wrong when generating door sprites - there are {state.NumberOfDoorsLeft} remaining but all content packs have been used. Please let the Better Doors author know!", LogLevel.Error);
            }

            return generatedTextures;
        }
    }
}