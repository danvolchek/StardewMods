using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModUpdateMenu.Updates;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ModUpdateMenu.Menus
{
    internal class UpdateButton : INotifiable
    {
        private readonly Texture2D statusTexture;
        private readonly ClickableTextureComponent updateButton;
        private bool wasUpdateButtonHovered;

        internal bool ShowUpdateButton { get; set; }
        internal ButtonStatus ButtonStatus { get; set; } = ButtonStatus.Unknown;
        internal ButtonStatus SMAPIButtonStatus { get; set; } = ButtonStatus.Unknown;

        private Point mousePosition = Point.Zero;

        public UpdateButton(IModHelper helper)
        {
            Texture2D buttonTexture = helper.Content.Load<Texture2D>("assets/updateButton.png");
            this.statusTexture = helper.Content.Load<Texture2D>("assets/statusIcons.png");

            this.updateButton = new ClickableTextureComponent(
                new Rectangle(36, Game1.viewport.Height - 150 - 48, 81, 75), buttonTexture, new Rectangle(0, 0, 27, 25),
                3, false);

            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Input.CursorMoved += this.OnCursorMoved;
            helper.Events.Display.WindowResized += this.OnWindowResized;
        }

        /// <summary>Raised after the game window is resized.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWindowResized(object sender, WindowResizedEventArgs e)
        {
            this.updateButton.bounds.Y = Game1.viewport.Height - 150 - 48;
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!this.ShowUpdateButton)
                return;

            if (this.mousePosition != Point.Zero)
                this.updateButton.tryHover(this.mousePosition.X, this.mousePosition.Y, 0.25f);
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (!this.ShowUpdateButton)
                return;

            this.mousePosition = new Point((int)e.NewPosition.ScreenPixels.X, (int)e.NewPosition.ScreenPixels.Y);
            bool isUpdateButtonHovered = this.updateButton.containsPoint(this.mousePosition.X, this.mousePosition.Y);
            if (isUpdateButtonHovered != this.wasUpdateButtonHovered)
            {
                this.updateButton.sourceRect.X += this.wasUpdateButtonHovered ? -27 : 27;
                if (!this.wasUpdateButtonHovered)
                    Game1.playSound("Cowboy_Footstep");
            }
            this.wasUpdateButtonHovered = isUpdateButtonHovered;
        }

        internal bool PointContainsButton(Vector2 p)
        {
            return this.updateButton.containsPoint((int)p.X, (int)p.Y);
        }

        public void Draw(SpriteBatch b)
        {
            if (!this.ShowUpdateButton)
                return;
            this.updateButton.draw(Game1.spriteBatch);

            Vector2 position = new Vector2(this.updateButton.bounds.X + this.updateButton.bounds.Width * 2 / 3, this.updateButton.bounds.Y - this.updateButton.bounds.Height * 1 / 3);

            b.Draw(this.statusTexture, position, new Rectangle(this.StatusToOffset(this.ButtonStatus), 0, 16, 16), Color.White, 0.0f, Vector2.Zero, 3 * Vector2.One, SpriteEffects.None, 0.99f);

            Vector2 SMAPIPosition = new Vector2(this.updateButton.bounds.X - 16, this.updateButton.bounds.Y - this.updateButton.bounds.Height * 1 / 3);

            b.Draw(this.statusTexture, SMAPIPosition, new Rectangle(this.StatusToOffset(this.SMAPIButtonStatus), 0, 16, 16), Color.White, 0.0f, Vector2.Zero, 3 * Vector2.One, SpriteEffects.None, 0.99f);

            if (Game1.activeClickableMenu is TitleMenu titleMenu)
                titleMenu.drawMouse(b);
        }

        private int StatusToOffset(ButtonStatus status)
        {
            switch (status)
            {
                case ButtonStatus.Updates:
                    return 0;
                case ButtonStatus.NoUpdates:
                    return 16;
                case ButtonStatus.Error:
                    return 32;
                case ButtonStatus.Unknown:
                default:
                    return 48;
            }
        }

        public void Notify(IList<ModStatus> statuses)
        {
            if (statuses == null)
                this.ButtonStatus = ButtonStatus.Error;
            else if (statuses.Any())
            {
                this.ButtonStatus = statuses.All(item => item.UpdateStatus != UpdateStatus.OutOfDate) ? ButtonStatus.NoUpdates : ButtonStatus.Updates;
            }
            else
                this.ButtonStatus = ButtonStatus.Unknown;
        }

        public void NotifySMAPI(ISemanticVersion version)
        {
            if (version == null)
                this.SMAPIButtonStatus = ButtonStatus.Error;
            else if (version == Constants.ApiVersion)
                this.SMAPIButtonStatus = ButtonStatus.NoUpdates;
            else
                this.SMAPIButtonStatus = ButtonStatus.Updates;
        }
    }
}