using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BetterDoors.Framework.ContentPacks
{
    /// <summary>A loaded content pack door.</summary>
    internal class ContentPackDoor
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod id providing this door.</summary>
        public string ModId { get; }

        /// <summary>The loaded texture.</summary>
        public Texture2D Texture { get; }

        /// <summary>The name of this door.</summary>
        public string Name { get; }

        /// <summary>The start position in pixels in the sheet.</summary>
        public Point StartPosition { get; }

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modId">The mod id.</param>
        /// <param name="texture">The loaded texture.</param>
        /// <param name="name">The name of this door.</param>
        /// <param name="startPosition">The start position in pixels in the sheet.</param>
        public ContentPackDoor(string modId, Texture2D texture, string name, Point startPosition)
        {
            this.ModId = modId;
            this.Texture = texture;
            this.Name = name;
            this.StartPosition = startPosition;
        }
    }
}