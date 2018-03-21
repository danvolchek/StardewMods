using System;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
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
            lastDialogueUp = false;
            numberOfSeenDialogues = 0;
            ModConfig config = helper.ReadConfig<ModConfig>();
            shouldHideInfoDialogueBox = config.KeepItems && config.KeepMoney && config.RememberMineLevels;

            PlayerStateManager.SetConfig(config);
            GameEvents.HalfSecondTick += this.HalfSecondTick;
        }

        private void HalfSecondTick(object sender, EventArgs e)
        {
            //If the state is not saved, and the player has just died, save the player's state.
            if (PlayerStateManager.state == null && Game1.killScreen)
            {
                numberOfSeenDialogues = 0;
                lastDialogueUp = false;

                PlayerStateManager.SaveState();
                Monitor.Log($"Saved state! State: {PlayerStateManager.state.money} {PlayerStateManager.state.deepestMineLevel} {PlayerStateManager.state.lowestLevelReached} {PlayerStateManager.state.inventory.FindAll(item => item != null).Count}", LogLevel.Trace);
            }
            //If the state has been saved and the player can move, reset the player's old state.
            else if (PlayerStateManager.state != null && Game1.CurrentEvent == null && Game1.player.CanMove)
            {
                
                Monitor.Log($"Restoring state! Current State: {Game1.player.Money} {Game1.player.deepestMineLevel} {(Game1.mine == null ? -1 : Game1.mine.lowestLevelReached)}  {Game1.player.items.FindAll(item => item != null).Count}", LogLevel.Trace);
                Monitor.Log($"Saved State: {PlayerStateManager.state.money} {PlayerStateManager.state.deepestMineLevel} {PlayerStateManager.state.lowestLevelReached} {PlayerStateManager.state.inventory.FindAll(item => item != null).Count}", LogLevel.Trace);
                PlayerStateManager.LoadState();
            }

            //Count the number of dialogues that have appeared since the player died. Close the fourth box if all the information in is being reset.
            if (shouldHideInfoDialogueBox && PlayerStateManager.state != null && Game1.dialogueUp != lastDialogueUp)
            {
                if (Game1.dialogueUp)
                {
                    numberOfSeenDialogues++;
                    Monitor.Log($"Dialogue changed! {numberOfSeenDialogues}", LogLevel.Trace);

                    if (numberOfSeenDialogues == 4 && Game1.activeClickableMenu is DialogueBox dialogueBox)
                    {
                        dialogueBox.closeDialogue();
                    }
                }

            }

            lastDialogueUp = Game1.dialogueUp;
        }
    }
}
