using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace HatsOnCats.Framework.Interfaces
{
    internal interface IHatStorageProvider
    {
        bool AddHat(Character character, Hat hat);
        bool RemoveHat(Character character, out Hat hat);
        bool HasHat(Character character);
        bool GetHats(Character character, out IEnumerable<Hat> hats);
        bool CanHandle(Character character);
        void Serialize(IDataHelper helper);
        void Deserialize(IDataHelper helper);
    }
}
