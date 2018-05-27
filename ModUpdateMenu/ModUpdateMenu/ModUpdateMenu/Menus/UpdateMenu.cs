using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModUpdateMenu.Updates;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace ModUpdateMenu.Menus
{
    internal class UpdateMenu : AboutMenu, INotifiable
    {
        private bool notified;
        private string modDivider;

        private static readonly string[] sections = { "Mod", "Status", "Link" };
        private const string UpdateProgress = "Update check in progress";
        private readonly Point updateProgressDimensions;
        private const string ErrorChecking = "Error retrieving update info :(";
        private readonly Point errorCheckingDimensions;
        private const string HeaderDivider = "|";
        private readonly Point headerDividerDimensions;

        private int numDots;
        private int dotCounter;
        private string hoverText;
        private Point hoverTextDimensions;

        private IList<ModStatus> originalStatuses;

        private IList<ModStatus> statuses;
        private IList<ClickableComponent> components = new List<ClickableComponent>();

        private int numDisplayableMods;
        private int displayIndex;
        //TODO: on resize, if new height can hold all item (what if more than curr but not all, what if smaller) update displayindex somehow
        //TODO: global stats (# mods, # skipped, #etc), smapi + sdv version in corner
        //TODO: something about links (they're lonk bois)

        public UpdateMenu()
        {
            this.SizeMaybeChanged();

            this.updateProgressDimensions = GetHalfDimensions(UpdateMenu.UpdateProgress);
            this.headerDividerDimensions = GetHalfDimensions(UpdateMenu.HeaderDivider);
            this.errorCheckingDimensions = GetHalfDimensions(UpdateMenu.ErrorChecking);

            this.numDisplayableMods = (((IClickableMenu)this).height - 150 - 64 - 16) / 64;
        }

        public override void draw(SpriteBatch b)
        {
            Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(this.width, ((IClickableMenu) this).height - 100, 0, 0);
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)centeringOnScreen.X, (int)centeringOnScreen.Y, this.width, ((IClickableMenu)this).height - 150, Color.White, 4f, true);

            int startX = (int)centeringOnScreen.X + 32;
            int startY = (int)centeringOnScreen.Y + 32;

            for (int i = 0; i < UpdateMenu.sections.Length; i++)
            {
                int xOffset = (i * this.width) / UpdateMenu.sections.Length;
                if (i != 0)
                    SpriteText.drawString(b, UpdateMenu.HeaderDivider, startX + xOffset - this.headerDividerDimensions.X, startY, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);

                xOffset += (this.width / UpdateMenu.sections.Length) / 2 - SpriteText.getWidthOfString(UpdateMenu.sections[i]) / 2;
                SpriteText.drawString(b, UpdateMenu.sections[i], startX + xOffset, startY, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);
            }


            SpriteText.drawString(b, this.modDivider, startX, startY + 32, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);

            if (!this.notified)
            {
                SpriteText.drawString(b, $"{UpdateMenu.UpdateProgress}{this.GetDots(this.numDots)}", startX + this.width / 2 - this.updateProgressDimensions.X, startY + (((IClickableMenu)this).height - 100) / 2 - this.updateProgressDimensions.Y, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);
            }
            else if (this.statuses == null)
            {
                SpriteText.drawString(b, UpdateMenu.ErrorChecking, startX + this.width / 2 - this.errorCheckingDimensions.X, startY + (((IClickableMenu)this).height - 100) / 2 - this.errorCheckingDimensions.Y, 9999, -1, 9999, 1f, 0.88f, false, -1, "", SpriteText.color_Red);
            }
            else
            {
                startX += this.headerDividerDimensions.X;
                int yOffset = startY + 64 + 16;
                foreach (ModStatus status in this.statuses)
                {
                    string modName = status.ModName;
                    while (SpriteText.getWidthOfString(modName) > this.width / UpdateMenu.sections.Length)
                    {
                        modName = modName.Substring(0, modName.Length - 1);
                    }

                    if (modName != status.ModName)
                        modName = modName.Substring(0, modName.Length - 3) + "...";
                    SpriteText.drawString(b, modName, startX, yOffset, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);
                    SpriteText.drawString(b, status.UpdateStatus.ToString(), startX + this.width / UpdateMenu.sections.Length, yOffset, 9999, -1, 9999, 1f, 0.88f, false, -1, "", UpdateMenu.GetColorForStatus(status.UpdateStatus));
                    SpriteText.drawString(b, status.UpdateURLType, startX + 2 * this.width / UpdateMenu.sections.Length, yOffset, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);
                    yOffset += 64;


                }

                if (this.hoverText != null)
                {
                    int xPos = Game1.getMouseX() + 32;
                    if (xPos > Game1.viewport.Width / 2)
                        xPos -= this.hoverTextDimensions.X + 32;
                    IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPos, Game1.getMouseY() + 32, this.hoverTextDimensions.X + 32, this.hoverTextDimensions.Y + 32, Color.White);
                    SpriteText.drawString(b, this.hoverText, xPos + 16, Game1.getMouseY() + 32 + 16, 9999, -1, 9999, 1f, 0.88f, false, -1, "", SpriteText.color_Gray);
                }
            }

        }

        public override void update(GameTime time)
        {
            base.update(time);
            if (this.notified)
                return;

            this.dotCounter++;
            if (this.dotCounter == 40)
            {
                this.numDots = (this.numDots + 1) % 4;
                this.dotCounter = 0;
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            this.CheckPosition(x, y, ComponentClicked);
        }

        public override void performHoverAction(int x, int y)
        {
            this.CheckPosition(x, y, this.ComponentHovered, () => this.hoverText = null);
        }

        private void CheckPosition(int x, int y, ComponentAction action, Action onNoMatch = null)
        {
            for (int i = 0; i < this.components.Count; i++)
            {
                if (this.components[i].containsPoint(x, y))
                {
                    action(this.statuses[i / UpdateMenu.sections.Length], i % UpdateMenu.sections.Length);
                    return;
                }
            }

            onNoMatch?.Invoke();
        }

        private delegate void ComponentAction(ModStatus which, int offset);

        private void ComponentHovered(ModStatus which, int offset)
        {
            switch (offset)
            {
                case 0:
                    this.hoverText = $"{which.ModName}^By: {which.ModAuthor}";
                    break;
                case 1:
                    switch (which.UpdateStatus)
                    {
                        case UpdateStatus.UpToDate:
                            this.hoverText = $"You have: {which.CurrentVersion}.";
                            break;
                        case UpdateStatus.OutOfDate:
                            this.hoverText = $"You have: {which.CurrentVersion}^Latest version: {which.NewVersion}";
                            break;
                        case UpdateStatus.Error:
                            this.hoverText = $"Failed to check updates: ^{which.ErrorReason}";
                            break;
                        case UpdateStatus.Skipped:
                            this.hoverText = $"Mod not loaded: ^{which.ErrorReason}";
                            break;
                    }
                    break;
                case 2:
                    this.hoverText = $"Click to go to: ^{which.UpdateURL}";
                    break;
            }

            this.hoverTextDimensions = UpdateMenu.GetDimensions(this.hoverText);
        }

        private static void ComponentClicked(ModStatus which, int offset)
        {
            if (offset == 2)
            {
                Game1.playSound("bigSelect");
                try
                {
                    Process.Start(which.UpdateURL);
                }
                catch
                {
                }
            }
        }

        public void Notify(IList<ModStatus> statuses)
        {
            this.notified = true;
            this.originalStatuses = statuses?.OrderBy(item => item.ModName).ToList();
            this.UpdateComponents();
        }

        public void Activated()
        {
            this.SizeMaybeChanged();
        }

        private void UpdateComponents()
        {
            this.UpdateComponentsImpl(this.originalStatuses?.Skip(this.displayIndex).ToList());
        }

        private void UpdateComponentsImpl(IList<ModStatus> statuses)
        {
            this.statuses = statuses;
            this.components.Clear();

            if (statuses == null)
                return;

            Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(this.width, (((IClickableMenu)this).height - 100), 0, 0);
            int startX = (int)centeringOnScreen.X + 32;
            int startY = (int)centeringOnScreen.Y + 32;

            startX += this.headerDividerDimensions.X;
            int yOffset = startY + 64 + 16;
            for (int i = 0; i < statuses.Count; i++)
            {
                this.components.Add(new ClickableComponent(new Rectangle(startX, yOffset, this.width / UpdateMenu.sections.Length, 64), ""));
                this.components.Add(new ClickableComponent(new Rectangle(startX + this.width / UpdateMenu.sections.Length, yOffset, this.width / UpdateMenu.sections.Length, 64), ""));
                this.components.Add(new ClickableComponent(new Rectangle(startX + 2 * this.width / UpdateMenu.sections.Length, yOffset, this.width / UpdateMenu.sections.Length, 64), ""));

                yOffset += 64;

                if (yOffset - startY + 64 > (((IClickableMenu)this).height - 150))
                {
                    this.statuses = this.statuses.Take(i + 1).ToList();
                    break;
                }
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (this.originalStatuses == null)
                return;

            if (direction < 0)
            {
                if (this.displayIndex < this.originalStatuses.Count)
                {
                    if (this.originalStatuses.Count - this.displayIndex > this.numDisplayableMods)
                        this.displayIndex++;
                }
            }
            else if (this.displayIndex > 0)
                this.displayIndex--;

            this.UpdateComponents();
        }


        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.SizeMaybeChanged();
        }

        private string GetDots(int num)
        {
            switch (num)
            {
                case 0:
                    return "";
                case 1:
                    return ".";
                case 2:
                    return "..";
                default:
                    return "...";
            }
        }

        private void SizeMaybeChanged()
        {
            this.width = Game1.viewport.Width - 200;
            ((IClickableMenu) this).height = Game1.viewport.Height - 50;
            this.numDisplayableMods = (((IClickableMenu)this).height - 150 - 64 - 16) / 64;
            int modDividerWidth = SpriteText.getWidthOfString("_");
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < (this.width - 64) / modDividerWidth; i++)
                builder.Append("_");
            this.modDivider = builder.ToString();
            this.UpdateComponents();
        }

        private static Point GetHalfDimensions(string input)
        {
            return new Point(SpriteText.getWidthOfString(input) / 2, SpriteText.getHeightOfString(input) / 2);
        }

        private static Point GetDimensions(string input)
        {
            Point p = GetHalfDimensions(input);
            return new Point(p.X * 2, p.Y * 2);
        }

        private static int GetColorForStatus(UpdateStatus status)
        {
            switch (status)
            {
                case UpdateStatus.Error:
                    return SpriteText.color_Red;
                case UpdateStatus.OutOfDate:
                    return SpriteText.color_Orange;
                case UpdateStatus.UpToDate:
                    return SpriteText.color_Green;
                case UpdateStatus.Skipped:
                    return SpriteText.color_Purple;
            }

            return SpriteText.color_White;
        }
    }
}
