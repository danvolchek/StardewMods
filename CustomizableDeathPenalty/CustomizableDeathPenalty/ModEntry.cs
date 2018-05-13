using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace CustomizableDeathPenalty
{
    public class ModEntry : Mod
    {
        private bool lastDialogueUp;
        private int numberOfSeenDialogues;
        private bool shouldHideInfoDialogueBox;

        public override void Entry(IModHelper helper)
        {
            this.lastDialogueUp = false;
            this.numberOfSeenDialogues = 0;
            var config = helper.ReadConfig<ModConfig>();
            this.shouldHideInfoDialogueBox = config.KeepItems && config.KeepMoney && config.RememberMineLevels;

            PlayerStateManager.SetConfig(config);
            GameEvents.HalfSecondTick += this.HalfSecondTick;
        }

        private void HalfSecondTick(object sender, EventArgs e)
        {
            //If the state is not saved, and the player has just died, save the player's state.
            if (PlayerStateManager.state == null && Game1.killScreen)
            {
                this.numberOfSeenDialogues = 0;
                this.lastDialogueUp = false;

                PlayerStateManager.SaveState();
                this.Monitor.Log(
                    $"Saved state! State: {PlayerStateManager.state.money} {PlayerStateManager.state.deepestMineLevel} {PlayerStateManager.state.lowestLevelReached} {PlayerStateManager.state.inventory.Count(item => item != null)}",
                    LogLevel.Trace);
            }
            //If the state has been saved and the player can move, reset the player's old state.
            else if (PlayerStateManager.state != null && Game1.CurrentEvent == null && Game1.player.CanMove)
            {
                this.Monitor.Log(
                    $"Restoring state! Current State: {Game1.player.Money} {Game1.player.deepestMineLevel} {(Game1.mine == null ? -1 : MineShaft.lowestLevelReached)}  {Game1.player.Items.Count(item => item != null)}",
                    LogLevel.Trace);
                this.Monitor.Log(
                    $"Saved State: {PlayerStateManager.state.money} {PlayerStateManager.state.deepestMineLevel} {PlayerStateManager.state.lowestLevelReached} {PlayerStateManager.state.inventory.Count(item => item != null)}",
                    LogLevel.Trace);
                PlayerStateManager.LoadState();
            }

            //Count the number of dialogues that have appeared since the player died. Close the fourth box if all the information in is being reset.
            if (this.shouldHideInfoDialogueBox && PlayerStateManager.state != null &&
                Game1.dialogueUp != this.lastDialogueUp)
                if (Game1.dialogueUp)
                {
                    this.numberOfSeenDialogues++;
                    this.Monitor.Log($"Dialogue changed! {this.numberOfSeenDialogues}", LogLevel.Trace);

                    if (this.numberOfSeenDialogues == 4 && Game1.activeClickableMenu is DialogueBox dialogueBox)
                        dialogueBox.closeDialogue();
                }

            this.lastDialogueUp = Game1.dialogueUp;
        }
    }
}