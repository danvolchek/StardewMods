using System;
using System.Collections.Generic;
using System.Reflection;
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
            ModUpdateMenuConfig config = this.Helper.ReadConfig<ModUpdateMenuConfig>();
            this.button = new UpdateButton(helper);
            this.menu = new UpdateMenu(config.HideSkippedMods);

            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
            GraphicsEvents.OnPostRenderEvent += this.GraphicsEvents_OnPostRenderHudEvent;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;

            new Thread(() =>
            {
                IUpdateStatusRetriever statusRetriever = new UpdateStatusRetriever(this.Helper);
                int attempts = 20;
                while (true)
                {
                    Thread.Sleep(2000);
                    try
                    {
                        if (statusRetriever.GetUpdateStatuses(out IList<ModStatus> statuses))
                        {
                            this.Notify(statuses);
                            break;
                        }

                        attempts--;
                        if(attempts == 0)
                            throw new Exception("All update attempts failed.");
                    }
                    catch (Exception e)
                    {
                        this.Monitor.Log("Failed retreiving update info from SMAPI: ", LogLevel.Debug);
                        this.Monitor.Log(e.ToString(), LogLevel.Debug);
                        this.Notify(null);
                        break;
                    }
                }
            }).Start();
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
            this.button.ShowUpdateButton = this.ShouldDrawUpdateButton();
        }

        private void GraphicsEvents_OnPostRenderHudEvent(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is TitleMenu titleMenu && this.ShouldDrawUpdateButton())
                this.button.Draw(Game1.spriteBatch);
        }

        public void Notify(IList<ModStatus> statuses)
        {
            this.menu.Notify(statuses);
            this.button.Notify(statuses);
        }

        private bool ShouldDrawUpdateButton()
        {
            if (!(Game1.activeClickableMenu is TitleMenu titleMenu))
                return false;

            return TitleMenu.subMenu == null && !this.GetPrivateBool(titleMenu, "isTransitioningButtons") &&
                   (this.GetPrivateBool(titleMenu,"titleInPosition") && !this.GetPrivateBool(titleMenu, "transitioningCharacterCreationMenu"));
        }

        private bool GetPrivateBool(object obj, string name)
        {
            return this.Helper.Reflection.GetField<bool>(obj, name).GetValue();
        }

    }
}