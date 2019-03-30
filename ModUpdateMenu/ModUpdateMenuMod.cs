using System;
using System.Collections.Generic;
using System.Linq;
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
        private ModUpdateMenuConfig config;

        private IList<ModStatus> currentStatuses;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.config = this.Helper.ReadConfig<ModUpdateMenuConfig>();
            this.button = new UpdateButton(helper);

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Display.Rendered += this.OnRendered;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        /// <summary>Raised after the game is launched, right before the first update tick. </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.menu = new UpdateMenu();
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
                    catch (Exception ex)
                    {
                        this.Monitor.Log("Failed retrieving update info from SMAPI: ", LogLevel.Debug);
                        this.Monitor.Log(ex.ToString(), LogLevel.Debug);
                        this.Notify(null);
                        this.NotifySMAPI(null);
                        break;
                    }
                }
            }).Start();
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button != SButton.MouseLeft)
                return;

            if (this.button.PointContainsButton(e.Cursor.ScreenPixels) && Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu == null)
            {
                TitleMenu.subMenu = this.menu;
                this.menu.Activated();
                Game1.playSound("newArtifact");
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            this.button.ShowUpdateButton = this.ShouldDrawUpdateButton();
        }

        /// <summary>Raised after the game draws to the sprite patch in a draw tick, just before the final sprite batch is rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRendered(object sender, RenderedEventArgs e)
        {
            if (Game1.activeClickableMenu is TitleMenu && this.ShouldDrawUpdateButton())
                this.button.Draw(e.SpriteBatch);
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