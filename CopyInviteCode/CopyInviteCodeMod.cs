using CopyInviteCode.ClipboardManagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Threading;

namespace CopyInviteCode
{
    public class CopyInviteCodeMod : Mod
    {
        private ClipboardItem clipboardItem;

        private readonly IDictionary<GamePlatform, IClipboardManager> clipboardManagers = new Dictionary<GamePlatform, IClipboardManager>
        {
            [GamePlatform.Linux] = new LinuxClipboardManager(),
            [GamePlatform.Mac] = new MacClipboardManager(),
            [GamePlatform.Windows] = new WindowsClipboardManager()
        };

        private Texture2D clipboardTexture;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.clipboardTexture = helper.Content.Load<Texture2D>("assets/clipboard.png");
            this.clipboardItem = new ClipboardItem(this.clipboardTexture);

            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // When a menu changes, check if its the invite code menu. If so, add the copy to clipboard option.
            if (e.NewMenu is ConfirmationDialog confDialog && this.Helper.Reflection.GetField<string>(confDialog, "message").GetValue().StartsWith(GetFirstPartOfInviteMessage()))
                this.AddCopyToClipboardOption(confDialog);
        }

        /// <summary>Adds a copy to clipboard option for a <see cref="ConfirmationDialog" />.</summary>
        private void AddCopyToClipboardOption(ConfirmationDialog confDialog)
        {
            confDialog.cancelButton = new ClickableTextureComponent("COPY",
                new Rectangle(
                    confDialog.xPositionOnScreen + confDialog.width - IClickableMenu.borderWidth -
                    IClickableMenu.spaceToClearSideBorder - 64,
                    confDialog.yPositionOnScreen + confDialog.height - IClickableMenu.borderWidth -
                    IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, this.clipboardTexture,
                new Rectangle(0, 0, 64, 64), 1f);
            this.Helper.Reflection.GetField<ConfirmationDialog.behavior>(confDialog, "onCancel")
                .SetValue(this.CopyDialog);

            if (Game1.options.SnappyMenus)
            {
                confDialog.populateClickableComponentList();
                confDialog.snapToDefaultClickableComponent();
            }
        }

        /// <summary>Method to be called when player clicks the copy button.</summary>
        private void CopyDialog(Farmer who)
        {
            ConfirmationDialog confDialog = Game1.activeClickableMenu as ConfirmationDialog;

            if (confDialog == null)
                return;

            string code = this.Helper.Reflection.GetField<string>(confDialog, "message").GetValue()
                .Replace(GetFirstPartOfInviteMessage(), "")
                .Trim();

            this.SetClipboardText(code);
        }

        /// <summary>Set clipboard text, as well as show an indicator it was copied.</summary>
        private void SetClipboardText(string text)
        {
            Thread thread = new Thread(() => this.SetClipboardTextImpl(text));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        /// <summary>Actually set clipboard text, as well as show an indicator it was copied.</summary>
        private void SetClipboardTextImpl(string text)
        {
            this.clipboardManagers[Constants.TargetPlatform].SetText(text);

            Game1.addHUDMessage(new HUDMessage("Copied code to clipboard!", 1, false, Color.White, this.clipboardItem));
        }

        /// <summary>Gets the non-invite code part of the invite message string.</summary>
        private static string GetFirstPartOfInviteMessage()
        {
            return Game1.content.LoadString("Strings\\UI:Server_InviteCode").Replace("{0}", "").Trim();
        }
    }
}
