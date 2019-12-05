using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using SemanticVersion = StardewModdingAPI.SemanticVersion;

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
        public IList<ContentPackDoor> LoadContentPacks()
        {
            IList<ContentPackDoor> data = new List<ContentPackDoor>();

            // Validate each pack and load the tile sheets referenced in the process.
            foreach (IContentPack contentPack in this.helper.ContentPacks.GetOwned())
            {
                if (contentPack.Manifest.UniqueID.Equals("vanilla", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.errorQueue.AddError($"{contentPack.Manifest.Name} ({contentPack.Manifest.UniqueID}) - A content pack's unique id can't be {contentPack.Manifest.UniqueID}. This pack won't be loaded.");
                    continue;
                }

                ContentPack loadedPack = contentPack.ReadJsonFile<ContentPack>("content.json");

                if (!SemanticVersion.TryParse(loadedPack.Version, out ISemanticVersion version))
                {
                    this.errorQueue.AddError($"{contentPack.Manifest.Name} ({contentPack.Manifest.UniqueID}) - The version ({loadedPack.Version}) is invalid. This pack won't be loaded.");
                    continue;
                }

                if (version.IsNewerThan(this.helper.ModRegistry.Get(this.helper.ModRegistry.ModID).Manifest.Version))
                {
                    this.errorQueue.AddError($"{contentPack.Manifest.Name} ({contentPack.Manifest.UniqueID}) - ({loadedPack.Version}) is too new to be loaded. Please update Better Doors!");
                    continue;
                }

                if (!version.Equals(new SemanticVersion("1.0.0")))
                {
                    this.errorQueue.AddError($"{contentPack.Manifest.Name} ({contentPack.Manifest.UniqueID}) - Unrecognized content pack version: {loadedPack.Version}. This pack won't be loaded.");
                    continue;
                }

                ISet<string> doorNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

                foreach (KeyValuePair<string, IList<string>> entry in loadedPack.Doors)
                {
                    if (!File.Exists(Path.Combine(contentPack.DirectoryPath, entry.Key)))
                    {
                        this.QueueError(contentPack, entry.Key, $"{entry.Key} doesn't exist", false);
                        continue;
                    }

                    string imageError = null;
                    Texture2D spriteSheet = null;

                    try
                    {
                        spriteSheet = contentPack.LoadAsset<Texture2D>(entry.Key);

                        if (spriteSheet.Width % 64 != 0 || spriteSheet.Height % 48 != 0)
                        {
                            imageError = $"The dimensions of the sprite sheet are invalid. Must be a multiple of 64 x 48. Instead, they are {spriteSheet.Width} x {spriteSheet.Height}";
                        }

                    }
                    catch (ContentLoadException)
                    {
                        imageError = $"{entry.Key} isn't a valid image";
                    }

                    if (imageError != null)
                    {
                        this.QueueError(contentPack, entry.Key, imageError, false);
                        continue;
                    }

                    int count = 0;
                    foreach (string doorName in entry.Value)
                    {
                        string nameError = null;

                        if (!doorNames.Add(doorName))
                        {
                            nameError = $"{doorName} is repeated more than once";
                        }
                        else if (doorName.Contains(" "))
                        {
                            nameError = $"{doorName} can't have spaces in it";
                        }

                        if (nameError != null)
                        {
                            this.QueueError(contentPack, entry.Key, nameError, true);
                            continue;
                        }

                        if (!Utils.IsValidTile(spriteSheet.Width, spriteSheet.Height, 16, count * 4, out string tileError))
                        {
                            this.QueueError(contentPack, entry.Key, tileError, true);
                            continue;
                        }

                        data.Add(new ContentPackDoor(contentPack.Manifest.UniqueID, spriteSheet, doorName, Utils.ConvertTileIndexToPosition(spriteSheet.Width, Utils.TileSize, count*4)));

                        count++;
                    }
                }
            }

            this.monitor.Log($"Loaded {data.Count} door sprites from content packs.", LogLevel.Trace);

            this.errorQueue.PrintErrors("Found some errors when loading door sprites from content packs:");

            // Also load the vanilla door textures.
            const string vanillaPath = "LooseSprites/Cursors";
            Texture2D vanillaTexture = this.helper.Content.Load<Texture2D>(vanillaPath, ContentSource.GameContent);

            data.Add(new ContentPackDoor("vanilla", vanillaTexture, "light", new Point(512, 144)));
            data.Add(new ContentPackDoor("vanilla", vanillaTexture, "window", new Point(576, 144)));
            data.Add(new ContentPackDoor("vanilla", vanillaTexture, "saloon", new Point(640, 144)));

            return data;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Queues an error.</summary>
        /// <param name="contentPack">The content pack the error is for.</param>
        /// <param name="id">An id to use when displaying the error.</param>
        /// <param name="info">The error info.</param>
        /// <param name="forIndividualDoor">Whether the error is for one door or an entire image.</param>
        private void QueueError(IContentPack contentPack, string id, string info, bool forIndividualDoor)
        {
            this.errorQueue.AddError($"{contentPack.Manifest.Name} ({contentPack.Manifest.UniqueID}) - {id} - Found an error. Info: {info}. {(forIndividualDoor ? "This door": "Every door in this image")} won't be loaded.");
        }
    }
}