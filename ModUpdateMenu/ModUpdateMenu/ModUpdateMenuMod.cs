using System;
using System.Collections.Generic;
using System.Threading;
using ModUpdateMenu.Menus;
using ModUpdateMenu.Updates;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ModUpdateMenu
{
    public class ModUpdateMenuMod : Mod, INotifiable
    {
        private UpdateButton button;
        private UpdateMenu menu;

        public override void Entry(IModHelper helper)
        {
            this.button = new UpdateButton(helper);
            this.menu = new UpdateMenu();

            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
            GraphicsEvents.OnPostRenderEvent += this.GraphicsEvents_OnPostRenderHudEvent;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;

            new Thread(() =>
            {
                Thread.Sleep(4000);
                this.Notify(new List<ModStatus>()
                {
                    new ModStatus(UpdateStatus.UpToDate, "Stack Everything", "Cat", "https://www.nexusmods.com/stardewvalley/mods/2053", "2.7.0-beta"),
                    new ModStatus(UpdateStatus.UpToDate, "Content Patcher", "Pathoschild", "https://www.nexusmods.com/stardewvalley/mods/1915", "1.4.-beta.3"),
                    /*new ModStatus(UpdateStatus.Error, "Shmoopyaaaaaaaaaa", "bloooooooooooorg", "https://www.nexusmods.com/stardewvalley/mods/2223", "1.0.0", null, "No update key found."),
                    new ModStatus(UpdateStatus.Skipped, "bldargo", "flargo", "https://github.com/babies", "1.0.0", null, "Too old my bibba"),
                    new ModStatus(UpdateStatus.Error, "Shmoopyaaaaaaaaaa", "bloooooooooooorg", "https://www.nexusmods.com/stardewvalley/mods/2223", "1.0.0", null, "No update key found."),
                    new ModStatus(UpdateStatus.OutOfDate, "Doopy", "baby jesus", "https://community.playstarbound.com/threads/treetransplant.135549/", "1.1.0", "1.2.0"),
                    new ModStatus(UpdateStatus.OutOfDate, "Doopy", "baby jesus", "https://community.playstarbound.com/threads/treetransplant.135549/", "1.1.0", "1.2.0"),
                    new ModStatus(UpdateStatus.Error, "Shmoopyaaaaaaaaaa", "bloooooooooooorg", "https://www.nexusmods.com/stardewvalley/mods/2223", "1.0.0", null, "No update key found."),
                    new ModStatus(UpdateStatus.Skipped, "bldargo", "flargo", "https://github.com/babies", "1.0.0", null, "Too old my bibba")*/
                });
            }).Start();

            //new SMAPIUpdateManager(this);
        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (e.Button != SButton.MouseLeft)
                return;

            if (this.button.PointContainsButton(e.Cursor.ScreenPixels) &&
                Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu == null)
            {
                TitleMenu.subMenu = this.menu;
                this.menu.Activated();
                Game1.playSound("newArtifact");
            }
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is TitleMenu titleMenu && TitleMenu.subMenu == null && !this.Helper.Reflection
                    .GetField<bool>(titleMenu, "isTransitioningButtons").GetValue())
                this.button.ShowUpdateButton = true;
            else
                this.button.ShowUpdateButton = false;
        }

        private void GraphicsEvents_OnPostRenderHudEvent(object sender, EventArgs e)
        {
            this.button.Draw(Game1.spriteBatch);
        }

        public void Notify(IList<ModStatus> statuses)
        {
            this.menu.Notify(statuses);
            this.button.Notify(statuses);
        }
    }
}