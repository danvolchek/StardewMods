using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModUpdateMenu.Updates;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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

        private int currentSortColumn = 1;
        private int currentSortDirection = 1;

        private IList<ModStatus> originalStatuses;

        private IList<ModStatus> statuses;
        private readonly IList<ClickableComponent> components = new List<ClickableComponent>();

        private int numDisplayableMods;
        private int displayIndex;

        private string _SMAPIText;

        private string SMAPIText
        {
            get => this._SMAPIText;
            set
            {
                this._SMAPIText = value;
                this.SMAPIHeight = SpriteText.getHeightOfString(value);
                this.SMAPIWidth = SpriteText.getWidthOfString(value);
            }
        }

        private int SMAPIHeight;
        private int SMAPIWidth;
        private ClickableComponent SMAPIComponent;

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
            Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(this.width, ((IClickableMenu)this).height - 100, 0, 0);
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)centeringOnScreen.X, (int)centeringOnScreen.Y, this.width, ((IClickableMenu)this).height - 150, Color.White, 4f, true);

            if (this.SMAPIText != null)
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), 20, ((IClickableMenu)this).height - 25 - this.SMAPIHeight / 2, (int)(this.SMAPIWidth * 1.09), (int)(this.SMAPIHeight * 1.5) + 5, Color.White, 4f, true);
                SpriteText.drawString(b, this.SMAPIText, 50, ((IClickableMenu)this).height - 30, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);
            }

            int startX = (int)centeringOnScreen.X + 32;
            int startY = (int)centeringOnScreen.Y + 32;

            for (int i = 0; i < UpdateMenu.sections.Length; i++)
            {
                double width = this.GetColumnWidth(i);
                int xOffset = 0;
                for (int j = 0; j < i; j++)
                    xOffset += this.GetColumnWidth(j);

                if (i != 0)
                    SpriteText.drawString(b, UpdateMenu.HeaderDivider, startX + xOffset - this.headerDividerDimensions.X, startY, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);

                string headerText = UpdateMenu.sections[i] +
                                    (this.currentSortColumn == i ? (this.currentSortDirection == 0 ? " A" : " V") : "");
                xOffset += (int)(width / 2) - SpriteText.getWidthOfString(UpdateMenu.sections[i]) / 2;
                SpriteText.drawString(b, headerText, startX + xOffset, startY, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);
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
                    while (SpriteText.getWidthOfString(modName) > this.GetColumnWidth(0))
                    {
                        modName = modName.Substring(0, modName.Length - 1);
                    }

                    if (modName != status.ModName)
                        modName = modName.Substring(0, modName.Length - 3) + "...";
                    SpriteText.drawString(b, modName, startX, yOffset, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);
                    SpriteText.drawString(b, status.UpdateStatus.ToString(), startX + (int)this.GetColumnWidth(0), yOffset, 9999, -1, 9999, 1f, 0.88f, false, -1, "", UpdateMenu.GetColorForStatus(status));
                    SpriteText.drawString(b, status.UpdateURLType, startX + (int)this.GetColumnWidth(0) + (int)this.GetColumnWidth(1), yOffset, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);
                    yOffset += 64;
                }

                if (this.hoverText != null)
                {
                    int xPos = Game1.getMouseX() + 32;
                    if (xPos > Game1.viewport.Width / 2)
                        xPos -= this.hoverTextDimensions.X + 32;
                    int yPos = Game1.getMouseY() + 32 + 16;
                    if (yPos > Game1.viewport.Height * (3.0 / 4))
                        yPos -= this.hoverTextDimensions.Y + 64;
                    IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPos, yPos - 16, this.hoverTextDimensions.X + 32, this.hoverTextDimensions.Y + 32, Color.White);
                    SpriteText.drawString(b, this.hoverText, xPos + 16, yPos, 9999, -1, 9999, 1f, 0.88f, false, -1, "", SpriteText.color_Gray);
                }

                if (this.statuses != null)
                {
                    int numSteps = this.originalStatuses.Count - this.numDisplayableMods;
                    yOffset = (int)((((float)this.displayIndex) / numSteps) * (((IClickableMenu)this).height - 200 + 16));

                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(325, 448, 5, 17),
                        (int)centeringOnScreen.X + this.width,
                        (int)centeringOnScreen.Y + yOffset, 16, 32, Color.White, 4f, false);
                }
            }
        }

        private int GetColumnWidth(int i)
        {
            double divisor;
            if (i == 0)
                divisor = 2;
            else if (i == 1)
                divisor = 4.3;
            else
                divisor = 4;
            return (int)(this.width / divisor);
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
            if (this.SMAPIText != null && this.SMAPIComponent.containsPoint(x, y))
            {
                try
                {
                    Process.Start("https://smapi.io");
                }
                catch
                {
                }
            }
            else
                this.CheckPosition(x, y, this.ComponentClicked);
        }

        public override void performHoverAction(int x, int y)
        {
            if (this.SMAPIText != null && this.SMAPIComponent.containsPoint(x, y))
            {
                this.hoverText = "Click to go to: https://smapi.io";
                this.SplitHoverText();
                this.hoverTextDimensions = UpdateMenu.GetDimensions(this.hoverText);
            }
            else
                this.CheckPosition(x, y, this.ComponentHovered, () => this.hoverText = null);
        }

        private void CheckPosition(int x, int y, ComponentAction action, Action onNoMatch = null)
        {
            if (this.components.Count == 0)
                return;

            for (int i = 0; i < 3; i++)
                if (this.components[i].containsPoint(x, y))
                {
                    action(null, i);
                    return;
                }
            for (int i = 3; i < this.components.Count; i++)
            {
                if (this.components[i].containsPoint(x, y))
                {
                    action(this.statuses[(i - 3) / UpdateMenu.sections.Length], i % UpdateMenu.sections.Length);
                    return;
                }
            }

            onNoMatch?.Invoke();
        }

        private delegate void ComponentAction(ModStatus which, int offset);

        private void ComponentHovered(ModStatus which, int offset)
        {
            if (which == null)
            {
                if (this.currentSortColumn == -1 || offset != this.currentSortColumn)
                {
                    this.hoverText = "Click to Sort By:^Mod ";
                    switch (offset)
                    {
                        case 0:
                            this.hoverText += "Name";
                            break;

                        case 1:
                            this.hoverText += "Status";
                            break;

                        case 2:
                            this.hoverText += "Link";
                            break;
                    }
                }
                else if (offset == this.currentSortColumn)
                    this.hoverText = "Currently Sorting:^" +
                                     (this.currentSortDirection == 0 ? "Ascending" : "Descending");
            }
            else
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
                                if (which.ErrorReason != null)
                                {
                                    this.hoverText += "^Some update errors happened,^see log for more info.";
                                }
                                break;

                            case UpdateStatus.OutOfDate:
                                this.hoverText = $"You have: {which.CurrentVersion}^Latest version: {which.NewVersion}";
                                break;

                            case UpdateStatus.Skipped:
                                this.hoverText = $"Updates not checked: ^{which.ErrorReason}";
                                break;
                        }
                        break;

                    case 2:
                        this.hoverText = which.UpdateURLType != "???" ? $"Click to go to: ^{which.UpdateURL}" : "Unknown update link.";
                        break;
                }

            this.SplitHoverText();
            this.hoverTextDimensions = UpdateMenu.GetDimensions(this.hoverText);
        }

        private void SplitHoverText()
        {
            if (this.hoverText == null)
                return;

            IList<string> lines = this.hoverText.Split('^');
            IList<string> result = new List<string>();
            foreach (string line in lines)
            {
                if (line.Length > 30)
                {
                    IEnumerable<string> parts = this.GetSmallerParts(line, ' ');
                    if (parts.Count() == 1)
                        parts = this.GetSmallerParts(line, '/');
                    foreach (string part in parts)
                        result.Add(part);
                }
                else
                    result.Add(line);
            }

            this.hoverText = string.Join("^", result);
        }

        private IEnumerable<string> GetSmallerParts(string line, char separator)
        {
            IList<string> result = new List<string>();

            StringBuilder curr = new StringBuilder();
            foreach (string part in line.Split(separator))
            {
                curr.Append(part);
                curr.Append(separator);
                if (curr.ToString().Length > 15)
                {
                    result.Add(curr.ToString());
                    curr.Clear();
                }
            }
            if (curr.Length != 0)
                result.Add(curr.ToString());

            if (result.Count == 0)
                result.Add(line);

            return result;
        }

        private void ComponentClicked(ModStatus which, int offset)
        {
            if (which == null)
            {
                if (this.currentSortColumn == -1)
                {
                    this.currentSortColumn = offset;
                    this.currentSortDirection = 0;
                }
                else if (this.currentSortColumn == offset)
                {
                    switch (this.currentSortDirection)
                    {
                        case 0:
                            this.currentSortDirection = 1;
                            break;

                        case 1:
                            this.currentSortColumn = -1;
                            this.currentSortDirection = 0;
                            break;
                    }
                }
                else
                {
                    this.currentSortColumn = offset;
                    this.currentSortDirection = 0;
                }

                this.displayIndex = 0;

                this.UpdateComponents();

                if (this.currentSortColumn != -1)
                    this.ComponentHovered(null, this.currentSortColumn);
            }
            else if (offset == 2)
            {
                Game1.playSound("bigSelect");
                if (which.UpdateURLType != "???")
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
            this.originalStatuses = statuses?.ToList();
            this.displayIndex = 0;
            this.currentSortColumn = 1;
            this.currentSortDirection = 1;

            this.UpdateComponents();
        }

        public void NotifySMAPI(ISemanticVersion version)
        {
            if (version == null)
                this.SMAPIText = "Error checking for SMAPI update";
            else if (version == Constants.ApiVersion)
                this.SMAPIText = "SMAPI is up to date";
            else
                this.SMAPIText = $"New SMAPI available: {version}";

            this.SMAPIComponent = new ClickableComponent(new Rectangle(20, ((IClickableMenu)this).height - 25 - this.SMAPIHeight / 2, (int)(this.SMAPIWidth * 1.09), (int)(this.SMAPIHeight * 1.5) + 5), "");
        }

        public void Activated()
        {
            this.SizeMaybeChanged();
        }

        private void UpdateComponents()
        {
            IEnumerable<ModStatus> temp = this.originalStatuses?.Select(item => item);
            if (temp != null && this.currentSortColumn != -1)
            {
                switch (this.currentSortColumn)
                {
                    case 0:
                        temp = temp.OrderBy(status => status.ModName);
                        break;

                    case 1:
                        temp = temp.OrderBy(status => status.UpdateStatus);
                        break;

                    case 2:
                        temp = temp.OrderBy(status => status.UpdateURLType);
                        break;
                }

                if (this.currentSortDirection == 1)
                    temp = temp.Reverse();
            }

            this.SMAPIComponent = new ClickableComponent(new Rectangle(20, ((IClickableMenu)this).height - 25 - this.SMAPIHeight / 2, (int)(this.SMAPIWidth * 1.09), (int)(this.SMAPIHeight * 1.5) + 5), "");

            this.UpdateComponentsImpl(temp?.Skip(this.displayIndex).ToList());
        }

        private void UpdateComponentsImpl(IEnumerable<ModStatus> statuses)
        {
            this.statuses = statuses?.ToList();
            this.components.Clear();

            if (this.statuses == null)
                return;

            Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(this.width, (((IClickableMenu)this).height - 100), 0, 0);
            int startX = (int)centeringOnScreen.X + 32;
            int startY = (int)centeringOnScreen.Y + 32;

            for (int i = 0; i < UpdateMenu.sections.Length; i++)
            {
                int xOffset = 0;
                for (int j = 0; j < i; j++)
                    xOffset += this.GetColumnWidth(j);

                this.components.Add(new ClickableComponent(new Rectangle(startX + xOffset, startY, this.GetColumnWidth(i), 64), ""));
            }

            startX += this.headerDividerDimensions.X;
            int yOffset = startY + 64 + 16;
            for (int i = 0; i < this.statuses.Count; i++)
            {
                this.components.Add(new ClickableComponent(new Rectangle(startX, yOffset, this.GetColumnWidth(0), 64), ""));
                this.components.Add(new ClickableComponent(new Rectangle(startX + this.GetColumnWidth(0), yOffset, this.GetColumnWidth(1), 64), ""));
                this.components.Add(new ClickableComponent(new Rectangle(startX + this.GetColumnWidth(0) + this.GetColumnWidth(1), yOffset, this.GetColumnWidth(2), 64), ""));

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
            ((IClickableMenu)this).height = Game1.viewport.Height - 50;
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
            return new Point((int)(p.X * (2 + (input.Contains("^") ? 0.1 : 0))), p.Y * 2);
        }

        private static int GetColorForStatus(ModStatus status)
        {
            switch (status.UpdateStatus)
            {
                case UpdateStatus.OutOfDate:
                    return SpriteText.color_Orange;

                case UpdateStatus.UpToDate:
                    return status.ErrorReason != null ? SpriteText.color_Cyan : SpriteText.color_Green;

                case UpdateStatus.Skipped:
                    return SpriteText.color_Purple;
            }

            return SpriteText.color_White;
        }
    }
}
