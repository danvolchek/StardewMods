using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pong.Framework.Common;
using Pong.Framework.Menus.Elements;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using IDrawable = Pong.Framework.Common.IDrawable;

namespace Pong.Framework.Menus
{
    internal abstract class Menu : IMenu
    {
        private readonly List<IDrawable> drawables = new List<IDrawable>();
        protected Modal CurrentModal;

        protected void InitDrawables()
        {
            this.drawables.AddRange(this.GetDrawables());
        }

        protected virtual IEnumerable<IDrawable> GetDrawables()
        {
            yield break;
        }

        public event EventHandler<SwitchMenuEventArgs> SwitchToNewMenu;

        public virtual bool ButtonPressed(EventArgsInput e)
        {
            if (this.CurrentModal != null)
                return this.CurrentModal.ButtonPressed(e);

            bool clicked = false;
            if (e.IsUseToolButton)
            {
                foreach (IClickable clickable in this.drawables.OfType<IClickable>())
                {
                    if (clickable.Bounds.Contains((int)e.Cursor.ScreenPixels.X, (int)e.Cursor.ScreenPixels.Y))
                    {
                        clickable.Clicked();
                        clicked = true;
                    }
                }

                foreach (IDrawable drawable in this.drawables)
                {
                    switch (drawable)
                    {
                        case IClickable clickable:
                            if (clickable.Bounds.Contains((int)e.Cursor.ScreenPixels.X, (int)e.Cursor.ScreenPixels.Y))
                            {
                                clickable.Clicked();
                                clicked = true;
                            }
                            break;
                        case ConditionalElement conditional:
                            if (conditional.GetElementForHighlight() is IClickable condClickable)
                                if (condClickable.Bounds.Contains((int)e.Cursor.ScreenPixels.X, (int)e.Cursor.ScreenPixels.Y))
                                {
                                    condClickable.Clicked();
                                    clicked = true;
                                }
                            break;
                        case ElementContainer container:
                            foreach (IClickable clickable in container.Elements.OfType<IClickable>())
                                if (clickable.Bounds.Contains((int)e.Cursor.ScreenPixels.X, (int)e.Cursor.ScreenPixels.Y))
                                {
                                    clickable.Clicked();
                                    clicked = true;
                                }
                            break;
                    }
                }
            }

            return clicked;
        }

        public virtual void MouseStateChanged(EventArgsMouseStateChanged e)
        {
            if (this.CurrentModal != null)
            {
                this.CurrentModal.MouseStateChanged(e);
                return;
            }

            foreach (IDrawable drawable in this.drawables)
            {
                switch (drawable)
                {
                    case IHighlightable highlightable:
                        highlightable.Highlighted = highlightable.Bounds.Contains(e.NewPosition);
                        break;
                    case ConditionalElement conditional:
                        if (conditional.GetElementForHighlight() is IHighlightable condHighlightable)
                            condHighlightable.Highlighted = condHighlightable.Bounds.Contains(e.NewPosition);
                        break;
                    case ElementContainer container:
                        foreach (IHighlightable element in container.Elements.OfType<IHighlightable>())
                            element.Highlighted = element.Bounds.Contains(e.NewPosition);
                        break;
                }
            }
        }

        public virtual void BeforeMenuSwitch()
        {
            
        }


        public void Draw(SpriteBatch b)
        {
            //b.Draw(AssetManager.SquareTexture, new Rectangle(0, 0, ScreenWidth, ScreenWidth), null, Color.Black);

            foreach (IDrawable drawable in this.drawables)
            {
                drawable.Draw(b);
            }

            b.Draw(Game1.mouseCursors,
                new Rectangle(Game1.oldMouseState.X - (Game1.tileSize / 4), Game1.oldMouseState.Y - (Game1.tileSize / 4), Game1.tileSize / 2, Game1.tileSize / 2),
                new Rectangle(146, 384, 9, 9), Color.White);
        }

        public abstract void Update();

        public abstract void Resize();

        public static int ScreenWidth => Game1.graphics.GraphicsDevice.Viewport.Width;

        public static int ScreenHeight => Game1.graphics.GraphicsDevice.Viewport.Height;

        protected void OnSwitchToNewMenu(IMenu newMenu)
        {
            this.BeforeMenuSwitch();
            this.SwitchToNewMenu?.Invoke(this, new SwitchMenuEventArgs(newMenu));
        }
    }
}
