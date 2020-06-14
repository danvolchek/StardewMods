using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HatsOnCats.Framework.Interfaces;
using HatsOnCats.Framework.Offsets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace HatsOnCats.Framework.DebugHats
{
    internal class AnimatedSpriteDrawer
    {
        private string textureName;
        private Texture2D texture;
        private int x;
        private int y;
        private int w;
        private int h;
        private int facingDirection;
        private bool draw;

        private AggregateOffsetProvider offsetProvider;
        private IHatDrawer drawer;

        public AnimatedSpriteDrawer(IModHelper helper, AggregateOffsetProvider offsetProvider, IHatDrawer drawer)
        {
            this.offsetProvider = offsetProvider;
            this.drawer = drawer;
            helper.Events.Display.Rendered += this.Display_Rendered;
            helper.ConsoleCommands.Add("draw", "", this.Command);
        }

        private void Command(string name, string[] args)
        {
            if (args[0] == "stop")
            {
                this.draw = false;
            }
            else if (args.Length == 1)
            {
                this.facingDirection = int.Parse(args[0]);
            }
            else if (args.Length == 2)
            {
                this.x = int.Parse(args[0]);
                this.y = int.Parse(args[1]);
            }
            else if (args.Length == 5)
            {
                this.textureName = args[0];
                this.texture = Game1.content.Load<Texture2D>(args[0]);
                this.x = int.Parse(args[1]);
                this.y = int.Parse(args[2]);
                this.w = int.Parse(args[3]);
                this.h = int.Parse(args[4]);
                this.draw = true;
            }
        }

        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if(!this.draw || this.texture == null)
            {
                return;
            }

            e.SpriteBatch.Draw(this.texture, new Vector2(50, 50), new Rectangle?(new Rectangle(this.x, this.y, this.w, this.h)), Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, 0.5f );
            if (this.offsetProvider.GetOffset(this.textureName, new Rectangle(this.x, this.y, this.w, this.h), SpriteEffects.None, out Vector2 offset))
            {
                this.drawer.DrawHats(new []{new Hat(0), }, this.facingDirection, e.SpriteBatch, new Vector2(50, 50), offset, 0, 0.5f );
            }
        }
    }
}
