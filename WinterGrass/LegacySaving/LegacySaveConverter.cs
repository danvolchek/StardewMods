using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.IO;

namespace WinterGrass.LegacySaving
{
    /// <summary>Handles reading and then deleting legacy save files.</summary>
    internal class LegacySaveConverter
    {
        /// <summary>The path to the legacy saves directory.</summary>
        private readonly string saveDirectory;
        /// <summary>The path to the current legacy save file.</summary>
        private string saveFile;

        /// <summary>Constructs an instance.</summary>
        /// <param name="directoryPath">The path to the legacy saves directory.</param>
        public LegacySaveConverter(string directoryPath)
        {
            this.saveDirectory = Path.Combine(directoryPath, "Saved Grass");
        }

        /// <summary>Sets the save file path.</summary>
        public void SetSaveFilePath()
        {
            this.saveFile = Path.Combine(this.saveDirectory, $"{Constants.SaveFolderName}.txt");
        }

        /// <summary>Adds grass from the legacy save file format to the game.</summary>
        public void AddGrassFromLegacySaveFile()
        {
            if (!File.Exists(this.saveFile))
            {
                return;
            }
           
            foreach (KeyValuePair<string, Dictionary<Vector2, LegacyGrassSave>> item in this.ReadFromFile(this.saveFile))
            {
                GameLocation l = Game1.getLocationFromName(item.Key);
                if (l != null)
                {
                    foreach (KeyValuePair<Vector2, LegacyGrassSave> item2 in item.Value)
                    {
                        Grass grass = new Grass(item2.Value.GrassType, item2.Value.NumWeeds);
                        grass.grassSourceOffset.Value = 80;

                        l.terrainFeatures[item2.Key] = grass;
                    }
                }
            }
        }

        /// <summary>Deletes the save file for the currently loaded save, and the entire folder if no saves are left.</summary>
        public void DeleteSaveFile()
        {
            if(File.Exists(this.saveFile))
                File.Delete(this.saveFile);

            if (Directory.Exists(this.saveDirectory) && Directory.GetFiles(this.saveDirectory).Length == 0)
                Directory.Delete(this.saveDirectory);
        }

        /// <summary>Reads saved grass data from a file.</summary>
        /// <param name="path">The file to read.</param>
        /// <returns>The read data.</returns>
        /// <remarks>Not sure why I decided to write this myself.</remarks>
        private Dictionary<string, Dictionary<Vector2, LegacyGrassSave>> ReadFromFile(string path)
        {
            Dictionary<string, Dictionary<Vector2, LegacyGrassSave>> savedItems = new Dictionary<string, Dictionary<Vector2, LegacyGrassSave>>();

            using (StreamReader file = new StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    string name = line.Substring(0, line.IndexOf('-'));
                    savedItems[name] = new Dictionary<Vector2, LegacyGrassSave>();

                    line = line.Substring(line.IndexOf('-') + 1);

                    while (line.Length != 0)
                    {
                        line = this.ReadInt(line, out int x);
                        line = this.ReadInt(line, out int y);
                        line = this.ReadInt(line, out int type);
                        line = this.ReadInt(line, out int weeds);

                        savedItems[name][new Vector2(x, y)] = new LegacyGrassSave(type, weeds);
                    }
                }
            }

            return savedItems;
            
        }

        /// <summary>Reads an int according to the save file format</summary>
        /// <param name="line"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private string ReadInt(string line, out int x)
        {
            x = int.Parse(line.Substring(0, line.IndexOf(",")));
            return line.Substring(line.IndexOf(",") + 1);
        }
    }
}
