using ModUpdateMenu.Menus;
using ModUpdateMenu.Updates;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ModUpdateMenu
{
    public class ModUpdateMenuMod : Mod, INotifiable
    {
        private UpdateButton button;
        private UpdateMenu menu;
        private ModUpdateMenuConfig config;

        private IList<ModStatus> currentStatuses = null;

        public override void Entry(IModHelper helper)
        {
            this.config = this.Helper.ReadConfig<ModUpdateMenuConfig>();
            this.button = new UpdateButton(helper);
            this.menu = new UpdateMenu();

            GameEvents.UpdateTick += this.GameEvents_UpdateTick;
            GraphicsEvents.OnPostRenderEvent += this.GraphicsEvents_OnPostRenderHudEvent;
            InputEvents.ButtonPressed += this.InputEvents_ButtonPressed;

            new Thread(() =>
            {
                IUpdateStatusRetriever statusRetriever = new UpdateStatusRetriever(this.Helper);
                int attempts = 50;
                while (true)
                {
                    Thread.Sleep(1000);
                    try
                    {
                        if (statusRetriever.GetUpdateStatuses(out IList<ModStatus> statuses))
                        {
                            if (this.currentStatuses != null && this.currentStatuses.Count == statuses.Count)
                            {
                                this.Notify(statuses);

                                try
                                {
                                    this.NotifySMAPI(statusRetriever.GetSMAPIUpdateVersion());
                                }
                                catch
                                {
                                    this.NotifySMAPI(null);
                                }


                                break;
                            }
                            else
                                this.currentStatuses = statuses;

                        }

                        attempts--;
                        if (attempts == 0)
                            throw new Exception("All update attempts failed.");
                    }
                    catch (Exception e)
                    {
                        this.Monitor.Log("Failed retrieving update info from SMAPI: ", LogLevel.Debug);
                        this.Monitor.Log(e.ToString(), LogLevel.Debug);
                        this.Notify(null);
                        this.NotifySMAPI(null);
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
            if (Game1.activeClickableMenu is TitleMenu && this.ShouldDrawUpdateButton())
                this.button.Draw(Game1.spriteBatch);
        }

        public void NotifySMAPI(ISemanticVersion version)
        {
            //Debug Info
            //this.Monitor.Log($"SMAPI: {version}");
            this.menu.NotifySMAPI(version);
            this.button.NotifySMAPI(version);
        }

        public void Notify(IList<ModStatus> statuses)
        {
            //Debug Info
            /*if(statuses != null)
                foreach(ModStatus status in statuses.OrderByDescending(item => item.UpdateStatus))
                    this.Monitor.Log($"{status.UpdateStatus} {status.CurrentVersion} {status.NewVersion} {status.UpdateURL} {status.ErrorReason} {status.ModName}");
            else
                this.Monitor.Log("Statuses are null");*/

            if (this.config.HideSkippedMods)
                statuses = statuses?.Where(status => status.UpdateStatus != UpdateStatus.Skipped).ToList();

            this.menu.Notify(statuses);
            this.button.Notify(statuses);
        }

        private bool ShouldDrawUpdateButton()
        {
            if (!(Game1.activeClickableMenu is TitleMenu titleMenu))
                return false;

            return TitleMenu.subMenu == null && !this.GetPrivateBool(titleMenu, "isTransitioningButtons") &&
                   (this.GetPrivateBool(titleMenu, "titleInPosition") && !this.GetPrivateBool(titleMenu, "transitioningCharacterCreationMenu"));
        }

        private bool GetPrivateBool(object obj, string name)
        {
            return this.Helper.Reflection.GetField<bool>(obj, name).GetValue();
        }
    }
}