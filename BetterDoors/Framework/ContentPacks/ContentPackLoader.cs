using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace BetterDoors.Framework.ContentPacks
{
    /// <summary>Loads and validates content packs. Also loads vanilla doors.</summary>
    internal class ContentPackLoader
    {
        /*********
        ** Fields
        *********/
        /// <summary>Provides simplified APIs for writing mods.</summary>
        private readonly IModHelper helper;
        /// <summary>Encapsulates monitoring and logging for a given module.</summary>
        private readonly IMonitor monitor;
        /// <summary>Queues content pack loading errors.</summary>
        private readonly ErrorQueue errorQueue;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        /// <param name="monitor">Encapsulates monitoring and logging for a given module.</param>
        /// <param name="errorQueue">Queues content pack loading errors.</param>
        public ContentPackLoader(IModHelper helper, IMonitor monitor, ErrorQueue errorQueue)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.errorQueue = errorQueue;
        }

        /// <summary>Loads content packs and vanilla doors.</summary>
        /// <returns>The loaded doors.</returns>
        public IList<LoadedContentPackDoorEntry> LoadContentPacks()
        {
            IList<LoadedContentPackDoorEntry> data = new List<LoadedContentPackDoorEntry>();

            // Validate each pack and load the tile sheets referenced in the process.
            foreach (IContentPack contentPack in this.helper.ContentPacks.GetOwned())
            {
                if (contentPack.Manifest.UniqueID.Equals("vanilla", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.errorQueue.AddError($"{contentPack.Manifest.UniqueID} - A content pack's unique id can't be {contentPack.Manifest.UniqueID}. This pack won't be loaded.");
                    continue;
                }

                ContentPack loadedPack = contentPack.ReadJsonFile<ContentPack>("content.json");

                if (loadedPack.Version != new SemanticVersion(1, 0, 0))
                {
                    this.errorQueue.AddError($"{contentPack.Manifest.UniqueID} - Unrecognized content pack version: {loadedPack.Version}.  This pack won't be loaded. ");
                    continue;
                }

                string error = null;
                IDictionary<string, Texture2D> spriteSheets = new Dictionary<string, Texture2D>();
                ISet<string> spriteNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                foreach (ContentPackDoorEntry doorEntry in loadedPack.Doors)
                {
                    if (!File.Exists(Path.Combine(contentPack.DirectoryPath, doorEntry.ImageFilePath)))
                    {
                        error = $"{doorEntry.ImageFilePath} doesn't exist";
                    }
                    else if (!spriteNames.Add(doorEntry.Name))
                    {
                        error = $"{doorEntry.Name} is repeated more than once";
                    }
                    else
                    {
                        try
                        {
                            if (!spriteSheets.ContainsKey(doorEntry.ImageFilePath))
                            {
                                Texture2D spriteSheet = contentPack.LoadAsset<Texture2D>(doorEntry.ImageFilePath);
                                spriteSheets[doorEntry.ImageFilePath] = spriteSheet;

                                if (spriteSheet.Width % 64 != 0 || spriteSheet.Height % 48 != 0)
                                {
                                    error = $"The dimensions of the sprite sheet are invalid. Must be a multiple of 64 x 48. Instead, they are {spriteSheet.Width} x {spriteSheet.Height}";
                                }
                                else
                                {
                                    Utils.IsValidTile(spriteSheet.Width, spriteSheet.Height, 16, doorEntry.TopLeftTileIndex, out error);
                                }
                            }
                        }
                        catch (ContentLoadException)
                        {
                            error = $"{doorEntry.ImageFilePath} isn't a valid image";
                        }
                    }

                    if (error != null)
                    {
                        this.errorQueue.AddError($"{contentPack.Manifest.UniqueID} - {doorEntry.Name} - This entry is invalid. It won't be loaded. Info: {error}.");
                        continue;
                    }

                    data.Add(new LoadedContentPackDoorEntry(contentPack.Manifest.UniqueID, spriteSheets[doorEntry.ImageFilePath], doorEntry));
                }
            }

            this.monitor.Log($"Loaded {data.Count} door sprites from content packs.", LogLevel.Trace);

            this.errorQueue.PrintErrors("Found some errors when loading door sprites from content packs:");

            // Also load the vanilla door textures.
            const string vanillaPath = "LooseSprites/Cursors";
            Texture2D vanillaTexture = this.helper.Content.Load<Texture2D>(vanillaPath, ContentSource.GameContent);

            data.Add(new LoadedContentPackDoorEntry("vanilla", vanillaTexture, new ContentPackDoorEntry(vanillaPath, 428, "light")));
            data.Add(new LoadedContentPackDoorEntry("vanilla", vanillaTexture, new ContentPackDoorEntry(vanillaPath, 432, "window")));
            data.Add(new LoadedContentPackDoorEntry("vanilla", vanillaTexture, new ContentPackDoorEntry(vanillaPath, 436, "saloon")));

            return data;
        }
    }
}