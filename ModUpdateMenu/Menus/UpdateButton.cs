using System;
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

        private Point mousePosition;

        public UpdateButton(IModHelper helper)
        {
            Texture2D buttonTexture = helper.Content.Load<Texture2D>("assets/updateButton.png");
            this.statusTexture = helper.Content.Load<Texture2D>("assets/statusIcons.png");

            this.updateButton = new ClickableTextureComponent(
                new Rectangle(36, Game1.viewport.Height - 150 - 48, 81, 75), buttonTexture, new Rectangle(0, 0, 27, 25),
                3, false);

            ControlEvents.MouseChanged += this.ControlEvents_MouseChanged;
            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
            GraphicsEvents.Resize += this.GraphicsEvents_Resize;
        }

        private void GraphicsEvents_Resize(object sender, EventArgs e)
        {
            this.updateButton.bounds.Y = Game1.viewport.Height - 150 - 48;
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            if (!this.ShowUpdateButton)
                return;

            if (this.mousePosition != null)
                this.updateButton.tryHover(this.mousePosition.X, this.mousePosition.Y, 0.25f);


        }

        private void ControlEvents_MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            if (!this.ShowUpdateButton)
                return;

            bool isUpdateButtonHovered = this.updateButton.containsPoint(e.NewPosition.X, e.NewPosition.Y);

            if (isUpdateButtonHovered != this.wasUpdateButtonHovered)
            {
                this.updateButton.sourceRect.X += this.wasUpdateButtonHovered ? -27 : 27;
                if (!this.wasUpdateButtonHovered)
                    Game1.playSound("Cowboy_Footstep");
            }

            this.wasUpdateButtonHovered = isUpdateButtonHovered;

            this.mousePosition = e.NewPosition;
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