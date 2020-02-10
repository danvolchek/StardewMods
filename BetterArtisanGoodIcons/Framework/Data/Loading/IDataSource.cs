using BetterArtisanGoodIcons.Framework.Data.Format.Unloaded;
using StardewModdingAPI;

namespace BetterArtisanGoodIcons.Framework.Data.Loading
{
    internal interface IDataSource
    {
        UnloadedData UnloadedData { get; }

        IManifest Manifest { get; }

        T Load<T>(string path);
    }
}
