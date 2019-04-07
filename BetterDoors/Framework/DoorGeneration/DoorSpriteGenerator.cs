using BetterDoors.Framework.ContentPacks;
using BetterDoors.Framework.Enums;
using BetterDoors.Framework.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace BetterDoors.Framework.DoorGeneration
{
    /// <summary>
    /// Generates alternate door sprites.
    /// </summary>
    internal class DoorSpriteGenerator
    {
        private static readonly Point DoorSize = new Point(64, 48);
        private const int MaxDoorsInSheet = 5440;


        private readonly IMonitor monitor;
        private readonly DoorAssetLoader assetLoader;
        private readonly GraphicsDevice device;

        public DoorSpriteGenerator(IContentHelper helper, IMonitor monitor, GraphicsDevice device)
        {
            this.assetLoader = new DoorAssetLoader(helper);
            this.monitor = monitor;
            this.device = device;
        }

        public GeneratedSpriteManager GenerateDoorSprites(IList<LoadedContentPackDoorEntry> contentPacks)
        {
            GeneratedSpriteManager manager = new GeneratedSpriteManager();
            IDictionary<string, Texture2D> generatedTextures = new Dictionary<string, Texture2D>();

            // Fields needed and modified during generation.
            int generatedTextureCount = 0;
            int numberOfDoorsLeft = contentPacks.Count * 4;
            Texture2D texture = null;
            Color[] textureData = null;
            Point texturePoint = Point.Zero;
            // ReSharper disable once InlineOutVariableDeclaration
            GeneratedTileSheetInfo tileSheetInfo;

            this.monitor.Log($"Generating {numberOfDoorsLeft - numberOfDoorsLeft / 4} door sprites.", LogLevel.Trace);

            generatedTextures.Add(this.CreateNewTexture(ref numberOfDoorsLeft, ref generatedTextureCount, out tileSheetInfo, ref texture, ref textureData));

            foreach (LoadedContentPackDoorEntry info in contentPacks)
            {
                Color[] spriteData = new Color[info.Texture.Width * info.Texture.Height];
                info.Texture.GetData(spriteData);


                //Copy original sprite        -> Vertical Left
                Point originalPoint = texturePoint;
                DoorSpriteGenerator.Copy(spriteData, textureData, info.Texture.Width, texture.Width, Utils.ConvertTileIndexToPosition(info.Texture.Width, 16, info.Entry.TopLeftTileIndex), texturePoint);
                manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Vertical, OpeningDirection.Left, new GeneratedDoorTileInfo(tileSheetInfo, Utils.ConvertPositionToTileIndex(texture.Width, 16, texturePoint)));

                if (this.FinishSprite(ref texturePoint, ref numberOfDoorsLeft, ref generatedTextureCount, ref tileSheetInfo, ref texture, ref textureData, out KeyValuePair<string, Texture2D> finishedTexture))
                    generatedTextures.Add(finishedTexture);


                //Reflect + reverse VL sprite -> Vertical Right
                DoorSpriteGenerator.ReflectOverYAxis(textureData, texture.Width, originalPoint, texturePoint);
                DoorSpriteGenerator.ReverseAnimationOrder(textureData, texture.Width, texturePoint);
                manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Vertical, OpeningDirection.Right, new GeneratedDoorTileInfo(tileSheetInfo, Utils.ConvertPositionToTileIndex(texture.Width, 16, texturePoint)));

                if (this.FinishSprite(ref texturePoint, ref numberOfDoorsLeft, ref generatedTextureCount, ref tileSheetInfo, ref texture, ref textureData, out finishedTexture))
                    generatedTextures.Add(finishedTexture);


                //Copy    + reverse VL sprite -> Horizontal Right
                Point reOrderedPoint = texturePoint;
                DoorSpriteGenerator.Copy(textureData, textureData, texture.Width, texture.Width, originalPoint, texturePoint);
                DoorSpriteGenerator.ReverseAnimationOrder(textureData, texture.Width, texturePoint);
                manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Horizontal, OpeningDirection.Right, new GeneratedDoorTileInfo(tileSheetInfo, Utils.ConvertPositionToTileIndex(texture.Width, 16, texturePoint)));

                if (this.FinishSprite(ref texturePoint, ref numberOfDoorsLeft, ref generatedTextureCount, ref tileSheetInfo, ref texture, ref textureData, out finishedTexture))
                    generatedTextures.Add(finishedTexture);


                //Reflect + reverse HR sprite -> Horizontal Left
                DoorSpriteGenerator.ReflectOverYAxis(textureData, texture.Width, reOrderedPoint, texturePoint);
                DoorSpriteGenerator.ReverseAnimationOrder(textureData, texture.Width, texturePoint);
                manager.RegisterDoorSprite(info.ModId, info.Entry.Name, Orientation.Horizontal, OpeningDirection.Left, new GeneratedDoorTileInfo(tileSheetInfo, Utils.ConvertPositionToTileIndex(texture.Width, 16, texturePoint)));

                if(this.FinishSprite(ref texturePoint, ref numberOfDoorsLeft, ref generatedTextureCount, ref tileSheetInfo, ref texture, ref textureData, out finishedTexture))
                    generatedTextures.Add(finishedTexture);
            }

            texture.SetData(textureData);

            texture.SaveAsPng(File.Create($"{tileSheetInfo.TileSheetId}.png"), texture.Width, texture.Height);

            if (numberOfDoorsLeft != 0)
            {
                this.monitor.Log($"Something went wrong when generating door sprites - there are {numberOfDoorsLeft} remaining but all content packs have been used. Please let the Better Doors author know!", LogLevel.Error);
            }
            else
            {
                this.monitor.Log("Successfully generated door sprites.", LogLevel.Trace);
            }

            this.assetLoader.SetTextures(generatedTextures);

            return manager;
        }

        private bool FinishSprite(ref Point texturePoint, ref int numberOfDoorsLeft, ref int generatedTextureCount, ref GeneratedTileSheetInfo tileSheetInfo, ref Texture2D texture, ref Color[] textureData, out KeyValuePair<string, Texture2D> finishedTexture)
        {
            finishedTexture = new KeyValuePair<string, Texture2D>();

            if (this.MovePosition(texture, ref texturePoint) && numberOfDoorsLeft > 0)
            {
                finishedTexture = this.CreateNewTexture(ref numberOfDoorsLeft, ref generatedTextureCount, out tileSheetInfo, ref texture, ref textureData);
                return true;
            }

            return false;
        }

        private bool MovePosition(Texture2D currentTexture, ref Point currentPosition)
        {
            this.monitor.Log($"Was at {currentPosition}.");
            currentPosition.X += DoorSize.X;
            if (currentPosition.X != currentTexture.Width) return false;

            currentPosition.X = 0;
            currentPosition.Y += DoorSize.Y;

            if (currentTexture.Height - currentPosition.Y >= DoorSize.Y) return false;

            currentPosition.Y = 0;
            return true;
        }

        private KeyValuePair<string, Texture2D> CreateNewTexture(ref int numberOfDoorsLeft, ref int generatedTextureCount, out GeneratedTileSheetInfo currentInfo, ref Texture2D texture, ref Color[] textureData)
        {
            texture?.SetData(textureData);

            int doorsThatCanFit = Math.Min(DoorSpriteGenerator.MaxDoorsInSheet, numberOfDoorsLeft);
            int numRows = (int) Math.Ceiling(doorsThatCanFit / 64.0);
            texture = new Texture2D(this.device, numRows > 1 ? 4096 : doorsThatCanFit * DoorSize.X, numRows > 1 ? numRows * DoorSize.Y : DoorSize.Y);
            textureData = new Color[texture.Width * texture.Height];

            numberOfDoorsLeft -= doorsThatCanFit;

            string id = $"z_door_sprites_{generatedTextureCount}";
            generatedTextureCount++;
            currentInfo = new GeneratedTileSheetInfo(new Point(texture.Width / 16, texture.Height / 16), id, id);

            return new KeyValuePair<string, Texture2D>(id, texture);
        }

        private static void ReverseAnimationOrder(IList<Color> data, int width, Point start)
        {
            DoorSpriteGenerator.SwapFrame(data, width, start, new Point(start.X + (DoorSize.X * 3) / 4, start.Y));
            DoorSpriteGenerator.SwapFrame(data, width, new Point(start.X + (DoorSize.X * 1) / 4, start.Y), new Point(start.X + (DoorSize.X * 2) / 4, start.Y));
        }

        private static void ReflectOverYAxis(IList<Color> data, int width, Point source, Point destination)
        {
            foreach (Tuple<int, int> indices in DoorSpriteGenerator.EnumerateOver(DoorSize.X, DoorSize.Y, width, width, source, destination, x => DoorSize.X - x - 1))
            {
                data[indices.Item2] = data[indices.Item1];
            }
        }

        private static void Copy(IList<Color> source, IList<Color> destination, int firstWidth, int secondWidth, Point sourcePosition, Point destinationPosition)
        {
            foreach (Tuple<int, int> indices in DoorSpriteGenerator.EnumerateOver(DoorSize.X, DoorSize.Y, firstWidth, secondWidth, sourcePosition, destinationPosition, x => x))
            {
                destination[indices.Item2] = source[indices.Item1];
            }
        }

        private static void SwapFrame(IList<Color> data, int width, Point first, Point second)
        {
            foreach (Tuple<int, int> indices in DoorSpriteGenerator.EnumerateOver(DoorSize.X / 4, DoorSize.Y, width, width, first, second, x => x))
            {
                Color buffer = data[indices.Item1];
                data[indices.Item1] = data[indices.Item2];
                data[indices.Item2] = buffer;
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

                    yield return new Tuple<int, int>(DoorSpriteGenerator.CollapseDimension(firstWidth, firstX, firstY), DoorSpriteGenerator.CollapseDimension(secondWidth, secondX, secondY));
                }
            }
        }

        private static int CollapseDimension(int width, int x, int y)
        {
            return y * width + x;
        }
    }
}