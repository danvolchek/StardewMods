using BetterArtisanGoodIcons.Framework;
using BetterArtisanGoodIcons.Framework.Data.Caching;
using BetterArtisanGoodIcons.Framework.Data.Format.Cached;
using BetterArtisanGoodIcons.Framework.Data.Format.Loaded;
using BetterArtisanGoodIcons.Framework.Data.Loading;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SObject = StardewValley.Object;

namespace BetterArtisanGoodIcons
{
    /// <summary>Draws different icons for different Artisan Good types.</summary>
    public class ModEntry : Mod
    {
        internal static ModEntry Instance;

        private ModConfig config;

        private Loader loader;
        private Cacher cacher;

        private LoadedDefinition[] loadedData;
        private IDictionary<int, CachedDefinition> cachedData;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.loader = new Loader(this.Monitor);
            this.cacher = new Cacher(this.Monitor);

            Instance = this;
            this.config = helper.ReadConfig<ModConfig>();
            this.loadedData = this.LoadDataSources();
            this.cachedData = this.cacher.Cache(this.loadedData);

            Harmony harmony = new Harmony(this.Helper.ModRegistry.ModID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            this.Helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            this.Helper.Events.GameLoop.ReturnedToTitle += this.GameLoop_ReturnedToTitle;
        }

        public bool GetDrawInfo(SObject output, out Texture2D textureSheet, out Rectangle mainPosition, out Rectangle iconPosition)
        {
            textureSheet = default;
            mainPosition = default;
            iconPosition = default;

            if (output == null ||
                !this.cachedData.TryGetValue(output.ParentSheetIndex, out CachedDefinition positionsBySource) ||
                !positionsBySource.TryGetValue(output.preservedParentSheetIndex.Value != -1 ? Math.Abs(output.preservedParentSheetIndex.Value) : -1, out mainPosition, out textureSheet))
            {
                return false;
            }

            if (!this.config.DisableSmallSourceIcons)
            {
                iconPosition = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, output.preservedParentSheetIndex.Value, 16, 16);
            }

            return true;
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            this.cachedData = this.cacher.Cache(this.loadedData);
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            this.cachedData = this.cacher.Cache(this.loadedData);
        }

        private LoadedDefinition[] LoadDataSources()
        {
            return this.ComplainAboutConflicts(this.LoadDataSources(this.GetDataSources()).ToArray()).ToArray();
        }

        private IEnumerable<LoadedData> LoadDataSources(IEnumerable<IDataSource> sources)
        {
            foreach (IDataSource source in sources)
            {
                yield return this.loader.Load(source);
            }
        }

        private IEnumerable<IDataSource> GetDataSources()
        {
            foreach (IContentPack pack in this.Helper.ContentPacks.GetOwned())
            {
                yield return new ContentPackDataSource(pack);
            }

            yield return new BAGIDataSource(this.Helper);
        }

        private IEnumerable<LoadedDefinition> ComplainAboutConflicts(LoadedData[] data)
        {
            // If mod a overwrites source item 1 for artisan good 2, and mod b tries to do the same thing, it's only allowed if a says overwrite: yes and b says overwrite:no.
            // The idea being a comes with the mod that introduces the items, and so it allows others to do their thing, but b is an overwrite.
            // Handle overwrites
            return data.SelectMany(thing => thing.ArtisanGoods);
        }
    }
}
