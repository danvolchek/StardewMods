using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Linq;

namespace WindEffects.Framework
{
    internal class Wave
    {
        private double xPos;
        private double yPos;

        private double amplitudeModifier;
        private double periodModifier;

        private readonly double sinRot;
        private readonly double cosRot;

        private readonly double dX;
        private readonly double dY;

        private double v;

        public Wave(double xPos, double yPos, double amplitudeModifier, double periodModifier, double rotation, double v)
        {
            this.xPos = xPos;
            this.yPos = yPos;
            this.amplitudeModifier = amplitudeModifier;
            this.periodModifier = periodModifier;

            // make rotation passed in the direction actually traveling
            rotation += Math.PI / 2;

            this.sinRot = Math.Sin(rotation);
            this.cosRot = Math.Cos(rotation);

            this.dX = Math.Cos(rotation - Math.PI / 2);
            this.dY = Math.Sin(rotation - Math.PI / 2);

            this.v = v;
        }

        public void Update()
        {
            this.xPos += this.v * this.dX;
            this.yPos += this.v * this.dY;
        }

        public void DebugDraw(SpriteBatch spriteBatch)
        {
            Vector2[] points = this.GetPoints(10);

            float minX = 99999;
            float minY = 99999;
            float maxX = 0;
            float maxY = 0;

            for (int i = 0; i < points.Length - 1; i++)
            {
                // wave itself
                spriteBatch.DrawLine(points[i], points[i + 1], Color.Red, 5);

                minX = Math.Min(minX, points[i].X);
                minY = Math.Min(minY, points[i].Y);
                maxX = Math.Max(maxX, points[i].X);
                maxY = Math.Max(maxY, points[i].Y);
            }

            minX = Math.Min(minX, points[points.Length - 1].X);
            minY = Math.Min(minY, points[points.Length - 1].Y);
            maxX = Math.Max(maxX, points[points.Length - 1].X);
            maxY = Math.Max(maxY, points[points.Length - 1].Y);

            // center indicator
            spriteBatch.DrawLine(new Vector2(minX, minY), new Vector2(maxX, maxY), Color.Blue, 5);
            spriteBatch.DrawLine(new Vector2(minX, maxY), new Vector2(maxX, minY), Color.Blue, 5);

            float middleX = (minX + maxX) / 2;
            float middleY = (minY + maxY) / 2;

            // rotation indicator
            //imgur.spriteBatch.DrawLine(new Vector2(middleX, middleY), new Vector2((float)(middleX + 50 * cosRot), (float) (middleY + 50 * sinRot)), Color.Green, 5);

            // direction indicator
            spriteBatch.DrawLine(new Vector2(middleX, middleY), new Vector2((float)(middleX + 50 * dX), (float)(middleY + 50 * dY)), Color.Pink, 5);
        }

        public Vector2[] TilesTouched()
        {
            // points is in local coordinate system, add the viewport to get absolute map tile
            return this.GetPoints(10).Select(point => new Vector2((int)(Game1.viewport.X + point.X) / Game1.tileSize, (int)(Game1.viewport.Y + point.Y) / Game1.tileSize)).ToArray();

        }

        public bool IsLeft()
        {
            return this.dX < 0;
        }


        private Vector2[] GetPoints(int granularity)
        {
            Vector2[] points = new Vector2[granularity];

            double angleIncrement = (Math.PI / periodModifier) / (granularity - 1);

            for (int i = 0; i < granularity; i++)
            {
                // sine curve
                float x = (float)(i * angleIncrement - (Math.PI / periodModifier) / 2);
                float y = (float)(amplitudeModifier * Math.Sin((Math.PI * 3 / 4) + i * angleIncrement * (Math.PI / 2) * periodModifier));

                // rotate
                double rotatedX = x * this.cosRot - y * this.sinRot;
                double rotatedY = x * this.sinRot + y * this.cosRot;

                // add offset, translate to right coordinate system
                points[i] = Game1.GlobalToLocal(new Vector2((float)(xPos + rotatedX), (float)(yPos + rotatedY)));

            }

            return points;
        }
    }
}
