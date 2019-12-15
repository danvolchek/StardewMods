using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>Encapsulates the state needed while generating sprites.</summary>
    internal class DoorGenerationState
    {
        /*********
        ** Constants
        *********/

        /// <summary>The factor by which sprites are scaled before display.</summary>
        public const int DisplayScaleFactor = 4;

        /// <summary>The size of a door animation in tiles.</summary>
        public static readonly Point DoorAnimationSize = new Point(Utils.TileSize * 4, Utils.TileSize * 3);

        /// <summary>The max number of door animations in one sheet.</summary>
        public const int MaxDoorsInSheet = 5440;

        /*********
        ** Accessors
        *********/

        /// <summary>The current number of doors left to be generated.</summary>
        public int NumberOfDoorsLeft { get; private set; }

        /// <summary>The current number of generated textures.</summary>
        public Texture2D Texture { get; private set; }

        /// <summary>The current texture data.</summary>
        public Color[] TextureData { get; private set; }

        /// <summary>The width of the current tile sheet.</summary>
        private int Width => this.Texture.Width;

        /*********
        ** Fields
        *********/

        /// <summary>Info about the generated tile sheet.</summary>
        private GeneratedTileSheetInfo tileSheetInfo;

        /// <summary>The current number of generated textures.</summary>
        private int generatedTextureCount;

        /// <summary>The current position where doors are being drawn.</summary>
        private Point texturePoint = Point.Zero;

        /// <summary>A unique string used to generate tile sheet ids.</summary>
        private readonly string locationName;

        /// <summary>A unique string used to generate tile sheet ids.</summary>
        private readonly string modId;

        /// <summary>Graphics device used to create textures.</summary>
        private readonly GraphicsDevice graphicsDevice;

        /*********
        ** Public methods
        *********/

        /// <summary>Construct an instance.</summary>
        /// <param name="locationName">A unique string used to generate tile sheet ids.</param>
        /// <param name="modId">A unique string used to generate tile sheet ids.</param>
        /// <param name="device">Graphics device used to create textures.</param>
        /// <param name="numberOfDoorsLeft">The current number of doors left to be generated.</param>
        public DoorGenerationState(string locationName, string modId, GraphicsDevice device, int numberOfDoorsLeft)
        {
            this.locationName = locationName;
            this.modId = modId;
            this.graphicsDevice = device;
            this.NumberOfDoorsLeft = numberOfDoorsLeft;
        }

        /// <summary>Creates a new texture, updating the relevant state.</summary>
        /// <returns>The asset key and texture that was created.</returns>
        public KeyValuePair<string, Texture2D> CreateNewTexture()
        {
            this.Texture?.SetData(this.TextureData);

            int doorsThatCanFit = Math.Min(DoorGenerationState.MaxDoorsInSheet, this.NumberOfDoorsLeft);
            int numRows = (int)Math.Ceiling(doorsThatCanFit / 64.0);
            this.Texture = new Texture2D(this.graphicsDevice, numRows > 1 ? 4096 : doorsThatCanFit * DoorGenerationState.DoorAnimationSize.X, numRows > 1 ? numRows * DoorGenerationState.DoorAnimationSize.Y : DoorGenerationState.DoorAnimationSize.Y);
            this.TextureData = new Color[this.Width * this.Texture.Height];

            this.NumberOfDoorsLeft -= doorsThatCanFit;

            string id = $"z_{this.modId}_{this.locationName}_{this.generatedTextureCount}";
            this.generatedTextureCount++;
            this.tileSheetInfo = new GeneratedTileSheetInfo(new Point(this.Width / 16, this.Texture.Height / 16), id, id);

            return new KeyValuePair<string, Texture2D>(id, this.Texture);
        }

        /// <summary>Finish drawing one door animation, updating the relevant state and creating a new texture if the old one is full.</summary>
        /// <param name="finishedTexture">The old, full texture info.</param>
        /// <returns>Whether a new texture was created</returns>
        public bool FinishAnimation(out KeyValuePair<string, Texture2D> finishedTexture)
        {
            finishedTexture = new KeyValuePair<string, Texture2D>();

            if (this.MovePosition() && this.NumberOfDoorsLeft > 0)
            {
                finishedTexture = this.CreateNewTexture();
                return true;
            }

            return false;
        }

        /// <summary>Creates generated tile info based on the current state.</summary>
        /// <param name="firstFrameIsClosed">Whether the first frame is closed or open.</param>
        /// <returns>The created tile info.</returns>
        public GeneratedDoorTileInfo CreateTileInfo(bool firstFrameIsClosed)
        {
            return new GeneratedDoorTileInfo(this.tileSheetInfo, this.CreateCollisionInfo(Utils.StateToXIndexOffset(State.Closed, firstFrameIsClosed)), this.GetTileIndex(), firstFrameIsClosed);
        }

        /// <summary>Reverses the animation order of the current animation.</summary>
        /// <remarks>Currently unused, left here for potential future use.</remarks>
        public void ReverseAnimationOrder()
        {
            this.SwapFrame(this.texturePoint, new Point(this.texturePoint.X + (DoorAnimationSize.X * 3) / 4, this.texturePoint.Y));
            this.SwapFrame(new Point(this.texturePoint.X + (DoorAnimationSize.X * 1) / 4, this.texturePoint.Y), new Point(this.texturePoint.X + (DoorAnimationSize.X * 2) / 4, this.texturePoint.Y));
        }

        /// <summary>Reflects the given door animation over the y axis into the current animation slot.</summary>
        /// <param name="source">The source texture data.</param>
        /// <param name="sourcePosition">The source position where the animation begins.</param>
        /// <param name="sourceWidth">The width of the source texture, in pixels.</param>
        public void ReflectOverYAxis(IList<Color> source, Point sourcePosition, int sourceWidth)
        {
            foreach (Tuple<int, int> indices in EnumerateOver(DoorAnimationSize.X, DoorAnimationSize.Y, sourceWidth, this.Width, sourcePosition, this.texturePoint, x => DoorAnimationSize.X - x - 1))
            {
                this.TextureData[indices.Item2] = source[indices.Item1];
            }
        }

        /// <summary>Copies the given door animation into the current animation slot.</summary>
        /// <param name="source">The source texture data.</param>
        /// <param name="sourcePosition">The source position where the animation begins.</param>
        /// <param name="sourceWidth">The width of the source texture, in pixels.</param>
        public void Copy(IList<Color> source, Point sourcePosition, int sourceWidth)
        {
            foreach (Tuple<int, int> indices in EnumerateOver(DoorAnimationSize.X, DoorAnimationSize.Y, sourceWidth, this.Width, sourcePosition, this.texturePoint, x => x))
            {
                this.TextureData[indices.Item2] = source[indices.Item1];
            }
        }

        /// <summary>Moves the current animation slot to the next one.</summary>
        /// <returns>Whether the current texture is full.</returns>
        private bool MovePosition()
        {
            this.texturePoint.X += DoorGenerationState.DoorAnimationSize.X;
            if (this.texturePoint.X != this.Width)
                return false;

            this.texturePoint.X = 0;
            this.texturePoint.Y += DoorGenerationState.DoorAnimationSize.Y;

            if (this.Texture.Height - this.texturePoint.Y >= DoorGenerationState.DoorAnimationSize.Y)
                return false;

            this.texturePoint.Y = 0;
            return true;
        }

        /// <summary>Creates collision info of a tile based on where the pixels are in the base of the closed frame.</summary>
        /// <param name="closedTileOffset">The tile index offset of the closed frame.</param>
        /// <returns>The collision info.</returns>
        private Rectangle CreateCollisionInfo(int closedTileOffset)
        {
            int left = Utils.TileSize;
            int top = Utils.TileSize;
            int right = 0;
            int bottom = 0;

            int xOffset = this.texturePoint.X + Utils.TileSize * closedTileOffset;
            int yOffset = this.texturePoint.Y + Utils.TileSize * 2;

            Color blank = new Color(0, 0, 0, 0);

            for (int x = 0; x < Utils.TileSize; x++)
            {
                for (int y = 0; y < Utils.TileSize; y++)
                {
                    bool isFilledIn = this.TextureData[Utils.CollapseDimension(this.Width, x + xOffset, y + yOffset)] != blank;

                    if (isFilledIn)
                    {
                        if (x < left)
                            left = x;
                        if (x > right)
                            right = x;
                        if (y < top)
                            top = y;
                        if (y > bottom)
                            bottom = y;
                    }
                }
            }

            left *= DoorGenerationState.DisplayScaleFactor;
            right *= DoorGenerationState.DisplayScaleFactor;
            top *= DoorGenerationState.DisplayScaleFactor;
            bottom *= DoorGenerationState.DisplayScaleFactor;

            return new Rectangle(left, top, right - left, bottom - top);
        }

        /// <summary>Gets the tile index based on the current generation state.</summary>
        /// <returns>The tile index.</returns>
        private int GetTileIndex()
        {
            return Utils.ConvertPositionToTileIndex(this.Width, Utils.TileSize, this.texturePoint);
        }

        /// <summary>Swaps two animation frames in the current animation slot.</summary>
        /// <param name="first">The top left point of the first frame to swap.</param>
        /// <param name="second">The top left point of the second frame to swap.</param>
        private void SwapFrame(Point first, Point second)
        {
            foreach (Tuple<int, int> indices in EnumerateOver(DoorAnimationSize.X / 4, DoorAnimationSize.Y, this.Width, this.Width, first, second, x => x))
            {
                Color buffer = this.TextureData[indices.Item1];
                this.TextureData[indices.Item1] = this.TextureData[indices.Item2];
                this.TextureData[indices.Item2] = buffer;
            }
        }

        /// <summary>Enumerates over a rectangular subset of pixels.</summary>
        /// <param name="xLength">The x length to iterate over.</param>
        /// <param name="yLength">The y length to iterate over.</param>
        /// <param name="firstWidth">The width of the first texture, in pixels.</param>
        /// <param name="secondWidth">The width of the second texture, in pixels.</param>
        /// <param name="first">The top left point to start from in the first texture, in pixels.</param>
        /// <param name="second">The top left point to start from in the second texture, in pixels.</param>
        /// <param name="secondXFunc">A custom function to apply to the x value of the second texture.</param>
        /// <returns>An enumeration of points.</returns>
        private static IEnumerable<Tuple<int, int>> EnumerateOver(int xLength, int yLength, int firstWidth, int secondWidth, Point first, Point second, Func<int, int> secondXFunc)
        {
            for (int y = 0; y < yLength; y++)
            {
                int firstY = first.Y + y;
                int secondY = second.Y + y;
                for (int x = 0; x < xLength; x++)
                {
                    int firstX = first.X + x;
                    int secondX = second.X + secondXFunc(x);

                    yield return new Tuple<int, int>(Utils.CollapseDimension(firstWidth, firstX, firstY), Utils.CollapseDimension(secondWidth, secondX, secondY));
                }
            }
        }
    }
}
