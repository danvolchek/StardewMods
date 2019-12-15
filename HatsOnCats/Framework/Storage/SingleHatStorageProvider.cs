using System;
using System.Collections.Generic;
using System.Linq;
using HatsOnCats.Framework.Interfaces;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;

namespace HatsOnCats.Framework.Storage
{
    internal class SingleHatStorageProvider: IHatStorageProvider
    {
        private const string SaveKey = "Single-Hat-Storage";

        private readonly IDictionary<Character, Hat> hatsByCharacter = new Dictionary<Character, Hat>();
        private readonly LocationFinder locationFinder = new LocationFinder();
        private readonly IMonitor monitor;

        public SingleHatStorageProvider(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        public bool AddHat(Character character, Hat hat)
        {
            if (this.hatsByCharacter.ContainsKey(character))
            {
                return false;
            }

            this.hatsByCharacter[character] = hat;
            return true;
        }

        public bool RemoveHat(Character character, out Hat hat)
        {
            if (this.hatsByCharacter.TryGetValue(character, out hat))
            {
                this.hatsByCharacter.Remove(character);
                return true;
            }

            return false;
        }

        public bool HasHat(Character character)
        {
            return this.hatsByCharacter.ContainsKey(character);
        }

        public bool GetHats(Character character, out IEnumerable<Hat> hats)
        {
            hats = null;

            if (this.hatsByCharacter.TryGetValue(character, out Hat hat))
            {
               hats = new [] {hat};
               return true;
            }

            return false;
        }

        public bool CanHandle(Character character)
        {
            switch (character.Sprite.UniqueName())
            {
                case "horse":
                case string farmer when farmer.Contains("farmer"):
                case string child when child.Contains("toddler") || child.Contains("baby"):
                    return false;
                default:
                    return true;
            }
        }

        public void Serialize(IDataHelper helper)
        {
            IDictionary<SaveItem, int> saveData = new Dictionary<SaveItem, int>();
            foreach (KeyValuePair<Character, Hat> kvp in this.hatsByCharacter)
            {
                SaveItem item = new SaveItem(kvp.Key.currentLocation.Name, kvp.Key.GetType().FullName, kvp.Key.Name, kvp.Key.Position);
                saveData[item] = kvp.Value.which.Value;
                this.monitor.Log($"Saved a hat for '{item}'. The hat is {kvp.Value.which.Value}. :)", LogLevel.Trace);
            }

            helper.WriteSaveData(SingleHatStorageProvider.SaveKey, saveData);
        }

        public void Deserialize(IDataHelper helper)
        {
            IDictionary<SaveItem, int> saveData = helper.ReadSaveData<IDictionary<SaveItem, int>>(SingleHatStorageProvider.SaveKey) ?? new Dictionary<SaveItem, int>();

            this.hatsByCharacter.Clear();

            foreach (KeyValuePair<SaveItem, int> kvp in saveData)
            {
                GameLocation location = Game1.getLocationFromName(kvp.Key.Location);
                List<Character> candidateCharacters = new List<Character>();
                if (location != null)
                {
                    if (kvp.Key.Type == typeof(FarmAnimal).FullName && location is AnimalHouse house)
                    {
                        candidateCharacters.AddRange(house.Animals.Values.Where(animal => animal.Name == kvp.Key.Name));
                    }
                    else
                    {
                        candidateCharacters.AddRange(location.characters.Where(character => character.Name == kvp.Key.Name));
                    }
                }

                this.PlaceHatOn(candidateCharacters, kvp.Key, kvp.Value);
            }
        }

        private void PlaceHatOn(List<Character> characters, SaveItem item, int whichHat)
        {
            if (characters.Count == 0)
            {
                this.monitor.Log($"Couldn't put a hat on {item}. The hat {whichHat} is lost. :(", LogLevel.Trace);
            }
            else if (characters.Count == 1)
            {
                this.AddHat(characters[0], new Hat(whichHat));
                this.monitor.Log($"Put a hat on '{item}'. The hat is {whichHat}. :)", LogLevel.Trace);
            }
            else
            {
                this.PlaceHatOn(characters.Where(character => character.Position == item.Position).ToList(), item, whichHat);
            }
        }
    }
}
