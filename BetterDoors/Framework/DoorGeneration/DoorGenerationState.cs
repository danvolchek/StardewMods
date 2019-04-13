using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using BetterDoors.Framework.Enums;

namespace BetterDoors.Framework.DoorGeneration
{
    internal class DoorGenerationState
    {
        internal const int DisplayScaleFactor = 4;
        internal static readonly Point DoorSize = new Point(DoorSpriteGenerator.TileSize * 4, DoorSpriteGenerator.TileSize * 3);
        internal const int MaxDoorsInSheet = 5440;

        public int GeneratedTextureCount { get; private set; }
        public int NumberOfDoorsLeft { get; private set; }
        public Texture2D Texture { get; private set; }
        public Color[] TextureData { get; private set; }
        public Point TexturePoint = Point.Zero;
        public GeneratedTileSheetInfo TileSheetInfo { get; private set; }
        public int Width => this.Texture.Width;

        private readonly string locationName;
        private readonly string modId;
        private readonly GraphicsDevice graphicsDevice;

        public DoorGenerationState(string locationName, string modId, GraphicsDevice device, int numberOfDoorsLeft)
        {
            this.locationName = locationName;
            this.modId = modId;
            this.graphicsDevice = device;
            this.NumberOfDoorsLeft = numberOfDoorsLeft;
        }

        public KeyValuePair<string, Texture2D> CreateNewTexture()
        {
            this.Texture?.SetData(this.TextureData);

            int doorsThatCanFit = Math.Min(DoorGenerationState.MaxDoorsInSheet, this.NumberOfDoorsLeft);
            int numRows = (int)Math.Ceiling(doorsThatCanFit / 64.0);
            this.Texture = new Texture2D(this.graphicsDevice, numRows > 1 ? 4096 : doorsThatCanFit * DoorSize.X, numRows > 1 ? numRows * DoorSize.Y : DoorSize.Y);
            this.TextureData = new Color[this.Width * this.Texture.Height];

            this.NumberOfDoorsLeft -= doorsThatCanFit;

            string id = $"z_{this.modId}_{this.locationName}_{this.GeneratedTextureCount}";
            this.GeneratedTextureCount++;
            this.TileSheetInfo = new GeneratedTileSheetInfo(new Point(this.Width / 16, this.Texture.Height / 16), id, id);

            return new KeyValuePair<string, Texture2D>(id, this.Texture);
        }

        public bool FinishSprite(out KeyValuePair<string, Texture2D> finishedTexture)
        {
            finishedTexture = new KeyValuePair<string, Texture2D>();

            if (this.MovePosition() && this.NumberOfDoorsLeft > 0)
            {
                finishedTexture = this.CreateNewTexture();
                return true;
            }

            return false;
        }

        public GeneratedDoorTileInfo CreateTileInfo(bool firstFrameIsClosed)
        {
            return new GeneratedDoorTileInfo(this.TileSheetInfo, this.CreateCollisionInfo(Utils.StateToXIndexOffset(State.Closed, firstFrameIsClosed)), this.GetTileIndex(), firstFrameIsClosed);
        }

        public void ReverseAnimationOrder()
        {
            this.SwapFrame(this.TexturePoint, new Point(this.TexturePoint.X + (DoorSize.X * 3) / 4, this.TexturePoint.Y));
            this.SwapFrame(new Point(this.TexturePoint.X + (DoorSize.X * 1) / 4, this.TexturePoint.Y), new Point(this.TexturePoint.X + (DoorSize.X * 2) / 4, this.TexturePoint.Y));
        }

        public void ReflectOverYAxis(IList<Color> source, Point sourcePosition, int sourceWidth)
        {
            foreach (Tuple<int, int> indices in EnumerateOver(DoorSize.X, DoorSize.Y, sourceWidth, this.Width, sourcePosition, this.TexturePoint, x => DoorSize.X - x - 1))
            {
                this.TextureData[indices.Item2] = source[indices.Item1];
            }
        }

        public void Copy(IList<Color> source, Point sourcePosition, int sourceWidth)
        {
            foreach (Tuple<int, int> indices in EnumerateOver(DoorSize.X, DoorSize.Y, sourceWidth, this.Width, sourcePosition, this.TexturePoint, x => x))
            {
                this.TextureData[indices.Item2] = source[indices.Item1];
            }
        }

        private bool MovePosition()
        {
            this.TexturePoint.X += DoorGenerationState.DoorSize.X;
            if (this.TexturePoint.X != this.Width)
                return false;

            this.TexturePoint.X = 0;
            this.TexturePoint.Y += DoorGenerationState.DoorSize.Y;

            if (this.Texture.Height - this.TexturePoint.Y >= DoorGenerationState.DoorSize.Y)
                return false;

            this.TexturePoint.Y = 0;
            return true;
        }

        private Rectangle CreateCollisionInfo(int closedTileOffset)
        {
            int left = DoorSpriteGenerator.TileSize;
            int top = DoorSpriteGenerator.TileSize;
            int right = 0;
            int bottom = 0;

            int xOffset = this.TexturePoint.X + DoorSpriteGenerator.TileSize * closedTileOffset;
            int yOffset = this.TexturePoint.Y + DoorSpriteGenerator.TileSize * 2;

            Color blank = new Color(0, 0, 0, 0);

            for (int x = 0; x < DoorSpriteGenerator.TileSize; x++)
            {
                for (int y = 0; y < DoorSpriteGenerator.TileSize; y++)
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

        private int GetTileIndex()
        {
            return Utils.ConvertPositionToTileIndex(this.Width, DoorSpriteGenerator.TileSize, this.TexturePoint);
        }

        private void SwapFrame(Point first, Point second)
        {
            foreach (Tuple<int, int> indices in EnumerateOver(DoorSize.X / 4, DoorSize.Y, this.Width, this.Width, first, second, x => x))
            {
                Color buffer = this.TextureData[indices.Item1];
                this.TextureData[indices.Item1] = this.TextureData[indices.Item2];
                this.TextureData[indices.Item2] = buffer;
            }
        }

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
