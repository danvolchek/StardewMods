using System;
using System.Collections.Generic;
using HatsOnCats.Framework.Configuration;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace HatsOnCats.Framework.Offsets
{
    internal class ConfigurableOffsetProviderFactory
    {
        private const string DefaultsFile = "assets/defaults.json";

        private readonly IDataHelper helper;
        public ConfigurableOffsetProviderFactory(IDataHelper helper)
        {
            this.helper = helper;
        }
        public IEnumerable<ConfigurableOffsetProvider> CreateHandlers()
        {
            ModConfig defaultConfig = this.helper.ReadJsonFile<ModConfig>(DefaultsFile) ?? new ModConfig();

            foreach (string name in this.Names)
            {
                if (!defaultConfig.Offsets.TryGetValue(name, out IDictionary<Frame, Offset> config))
                {
                   throw new Exception($"Missing default configuration for '{name}'. Make sure the file '{DefaultsFile}' exists. If not, re-download the mod.");
                }

                yield return new ConfigurableOffsetProvider(name, config);
            }
        }

        private readonly IEnumerable<string> Names = new[]
        {
            //NPCs
            "Abigail",
            "Alex",
            "Caroline",
            "Clint",
            "ClothesTherapyCharacters",
            "Demetrius",
            "Dwarf",
            "Elliott",
            "Emily",
            "Evelyn",
            "George",
            "Governor",
            "Gunther",
            "Gus",
            "Haley",
            "Harvey",
            "Henchman",
            "Jas",
            "Jodi",
            "Junimo",
            "Kent",
            "SeaMonsterKrobus",
            "Krobus_Trenchcoat",
            "Krobus",
            "Leah",
            "Lewis",
            "Linus",
            "Marcello",
            "Mariner",
            "Marlon",
            "Marnie",
            "Maru_Hospital",
            "Maru",
            "MrQi",
            "Pam",
            "Penny",
            "Pierre",
            "Robin",
            "Robot",
            "Sam",
            "Sandy",
            "Sebastian",
            "Shane_JojaMart",
            "Shane",
            "TrashBear",
            "Vincent",
            "Willy",
            "Wizard",
            // Farm animals
            "BabyBlue Chicken",
            "BabyBrown Chicken",
            "BabyBrown Cow",
            "BabyGoat",
            "BabyPig",
            "BabyRabbit",
            "BabySheep",
            "BabyVoid Chicken",
            "BabyWhite Chicken",
            "BabyWhite Cow",
            "Blue Chicken",
            "Brown Chicken",
            "Brown Cow",
            "Dinosaur",
            "Duck",
            "Goat",
            "Pig",
            "Rabbit",
            "ShearedSheep",
            "Sheep",
            "Void Chicken",
            "White Chicken",
            "White Cow",
            // Misc
            "Cat2",
            "Cat1",
            "Cat",
            "Dog2",
            "Dog1",
            "Dog",
            "Bear",
            "Junimo",
            "WeddingOutfits",
            // Monsters
            "Frost Bat",
            "Lava Bat",
            "Iridium Bat",
            "Bat",
            "Big Slime",
            "Bug",
            "Crab",
            "Duggy",
            "Dust Spirit",
            "Fly",
            "Ghost",
            "Green Slime",
            "Grub",
            "Lava Crab",
            "Metal Head",
            "Mummy",
            "Pepper Rex",
            "Serpent",
            "Shadow Brute",
            "Shadow Guy",
            "Shadow Shaman",
            "Skeleton",
            "Squid Kid",
            "Stone Golem",
            "Wilderness Golem"
        };
    }
}
