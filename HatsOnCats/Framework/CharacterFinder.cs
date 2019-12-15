using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace HatsOnCats.Framework
{
    internal class CharacterFinder
    {
        public bool TryGetCharacterAt(Vector2 mousePosition, out Character character)
        {
            character = this.GetCharacters().FirstOrDefault(ch => this.GetRelativeBoundingBox(ch).Contains((int) mousePosition.X, (int) mousePosition.Y));
            return character != null;
        }

        private Rectangle GetBoundingBox(Character character)
        {
            if (character is FarmAnimal animal)
            {
                return animal.GetCursorPetBoundingBox();
            }

            return character.GetBoundingBox();
        }

        private Rectangle GetRelativeBoundingBox(Character character)
        {
            Rectangle bounder = this.GetBoundingBox(character);

            return new Rectangle((int)(bounder.X - Game1.viewport.X), (int)(bounder.Y - Game1.viewport.Y), bounder.Width * Game1.pixelZoom, bounder.Height * Game1.pixelZoom);
        }

        private IEnumerable<Character> GetCharacters()
        {
            List<Character> characters = Game1.currentLocation.characters.OfType<Character>().ToList();

            if (Game1.currentLocation is IAnimalLocation location)
            {
                characters.AddRange(location.Animals.Keys.OfType<Character>());
            }

            return characters;
        }
    }
}
