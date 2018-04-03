using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using xTile.Layers;
using xTile.Tiles;

namespace DesertObelisk
{
    public class AssetModifier : IAssetLoader, IAssetEditor
    {
        private Texture2D obeliskTexture = null;
        private int desertWarpX;

        private IModHelper helper;

        public AssetModifier(IModHelper helper, IMonitor monitor, int desertWarpX)
        {
            this.helper = helper;
            this.desertWarpX = desertWarpX;
            IManifest loadedManifest = null;

            foreach (IContentPack contentPack in helper.GetContentPacks())
            {
                if (obeliskTexture != null)
                {
                    monitor.Log($"Could not load obelisk texture from {contentPack.Manifest.Name} ({contentPack.Manifest.UniqueID}) because {loadedManifest.Name} ({loadedManifest.UniqueID}) is already loaded.", LogLevel.Warn);
                    continue;
                }

                try
                {
                    obeliskTexture = contentPack.LoadAsset<Texture2D>("assets/Desert Obelisk.png");
                    loadedManifest = contentPack.Manifest;

                    monitor.Log($"Loaded obelisk texture from {loadedManifest.Name} ({loadedManifest.UniqueID}).", LogLevel.Debug);
                }
                catch
                {
                    monitor.Log($"Could not load obelisk texture from {contentPack.Manifest.Name} ({contentPack.Manifest.UniqueID}) because no valid texture was found.", LogLevel.Warn);
                    monitor.Log($"The accepted format is an image named 'Desert Obelisk.png' inside a folder named 'assets' in the root content pack folder.", LogLevel.Warn);
                }
            }

            if (obeliskTexture == null)
            {
                if (helper.ModRegistry.IsLoaded("Lita.StarblueValley"))
                {
                    monitor.Log("No valid content pack was found but Starblue Valley is installed, using Starblue obelisk texture.", LogLevel.Trace);
                    obeliskTexture = helper.Content.Load<Texture2D>("assets/Desert Obelisk Starblue.png");
                    loadedManifest = helper.ModRegistry.Get("Lita.StarblueValley");
                }
                else
                {
                    monitor.Log("Loaded default obelisk texture because no content packs or Starblue Valley were found/valid.", LogLevel.Trace);
                    obeliskTexture = helper.Content.Load<Texture2D>("assets/Desert Obelisk.png");
                }
            }    

            helper.Content.AssetLoaders.Add(this);
            helper.Content.AssetEditors.Add(this);

            GameEvents.OneSecondTick += this.OneSecondTick;
        }

        private void OneSecondTick(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            GameLocation desert = Game1.getLocationFromName("Desert");
            TileSheet markerTileSheet = new TileSheet("zDesertObeliskTileSheet", desert.map, helper.Content.GetActualAssetKey("assets/markerTiles") + ".xnb", new xTile.Dimensions.Size(2, 3), new xTile.Dimensions.Size(16, 16));

            desert.map.AddTileSheet(markerTileSheet);
            Layer frontLayer = desert.map.GetLayer("Front");
            Layer buildingsLayer = desert.map.GetLayer("Buildings");
            frontLayer.Tiles[desertWarpX, 40] = new StaticTile(frontLayer, markerTileSheet, BlendMode.Alpha, 0);
            frontLayer.Tiles[desertWarpX + 1, 40] = new StaticTile(frontLayer, markerTileSheet, BlendMode.Alpha, 1);
            frontLayer.Tiles[desertWarpX, 41] = new StaticTile(frontLayer, markerTileSheet, BlendMode.Alpha, 2);
            frontLayer.Tiles[desertWarpX + 1, 41] = new StaticTile(frontLayer, markerTileSheet, BlendMode.Alpha, 3);
            buildingsLayer.Tiles[desertWarpX, 42] = new StaticTile(buildingsLayer, markerTileSheet, BlendMode.Alpha, 4);
            buildingsLayer.Tiles[desertWarpX + 1, 42] = new StaticTile(buildingsLayer, markerTileSheet, BlendMode.Alpha, 5);

            GameEvents.OneSecondTick -= this.OneSecondTick;
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(@"Data\Blueprints");
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals(@"Data\Blueprints"))
            {
                asset.AsDictionary<string, string>().Data.Add("Desert Obelisk", "337 10 768 10/3/3/-1/-1/-2/-1/null/Desert Obelisk/Warps you to the desert./Buildings/none/48/128/-1/null/Farm/1000000/true");
            }
        }

        public bool CanLoad<Texture2D>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(@"Buildings\Desert Obelisk");
        }

        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)obeliskTexture;
        }
    }
}
