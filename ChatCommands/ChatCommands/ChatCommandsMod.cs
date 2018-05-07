using ChatCommands.ClassReplacements;
using ChatCommands.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace ChatCommands
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ChatCommandsMod : Mod, ICommandHandler
    {
        //TODO: MOUSE TO CLICK/DRAG SCROLL

        private CommandValidator commandValidator;
        private NotifyingTextWriter consoleNotifier;
        private ChatCommandsConfig modConfig;

        private const int BaseWaitPeriod = 15;

        private int repeatWaitPeriod = BaseWaitPeriod;

        public override void Entry(IModHelper helper)
        {
            this.commandValidator = new CommandValidator(helper.ConsoleCommands);
            this.consoleNotifier = new NotifyingTextWriter(Console.Out, this.OnLineWritten);

            Console.SetOut(this.consoleNotifier);
            SaveEvents.AfterLoad += this.SaveEvents_AfterLoad;
            GameEvents.SecondUpdateTick += this.GameEvents_HalfSecondTick;

            this.modConfig = helper.ReadConfig<ChatCommandsConfig>();

            new ListenCommand(this.Monitor, this.modConfig, this.consoleNotifier).Register(helper.ConsoleCommands);
        }

        /// <summary>
        /// Resend the left, right, up, or down keys if one of them is being held.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameEvents_HalfSecondTick(object sender, EventArgs e)
        {
            if (Game1.chatBox == null || !Game1.chatBox.isActive())
                return;

            bool isLeftDown = Keyboard.GetState().IsKeyDown(Keys.Left);
            bool isRightDown = Keyboard.GetState().IsKeyDown(Keys.Right);
            bool isUpDown = Keyboard.GetState().IsKeyDown(Keys.Up);
            bool isDownDown = Keyboard.GetState().IsKeyDown(Keys.Down);

            if (isLeftDown ^ isRightDown ^ isUpDown ^ isDownDown)
            {
                Keys downKey = isLeftDown ? Keys.Left : (isRightDown ? Keys.Right : (isUpDown ? Keys.Up : Keys.Down));

                if (this.repeatWaitPeriod != 0)
                    this.repeatWaitPeriod--;

                if (this.repeatWaitPeriod == 0)
                {
                    Game1.chatBox.receiveKeyPress(downKey);
                    if (isUpDown || isDownDown)
                        this.repeatWaitPeriod = BaseWaitPeriod;
                }
            }
            else
            {
                this.repeatWaitPeriod = BaseWaitPeriod;
            }
        }

        /// <summary>
        /// Whether this <see cref="ICommandHandler"/> can handle the given input.
        /// </summary>
        public bool CanHandle(string input)
        {
            return input.Length > 1 && this.commandValidator.IsValidCommand(input.Substring(1));
        }

        /// <summary>
        /// Handles the given input.
        /// </summary>
        public void Handle(string input)
        {
            input = input.Substring(1);
            string[] parts = Utils.ParseArgs(input);

            if (parts[0] == "halp")
                parts[0] = "help";

            this.consoleNotifier.Notify(true);
            this.Helper.ConsoleCommands.Trigger(parts[0], parts.Skip(1).ToArray());
            this.consoleNotifier.Notify(false);
        }

        /// <summary>
        /// Replace the game's chatbox with a <see cref="CommandChatBox"/>.
        /// </summary>
        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            if (Game1.chatBox != null && Game1.chatBox is CommandChatBox) return;
            if (Game1.chatBox != null)
                Game1.onScreenMenus.Remove(Game1.chatBox);
            Game1.chatBox = new CommandChatBox(this.Helper, this, this.modConfig);
            Game1.onScreenMenus.Add(Game1.chatBox);
            this.Monitor.Log("Replaced Chatbox", LogLevel.Trace);
        }

        /// <summary>
        /// When a line is written to the console, add it to the chatbox.
        /// </summary>
        private void OnLineWritten(char[] buffer, int index, int count)
        {
            string toWrite = string.Join("", buffer.Skip(index).Take(count)).Trim();
            string noPrefix = Utils.StripSMAPIPrefix(toWrite).Trim();

            if (Utils.ShouldIgnore(noPrefix))
                return;
            if (this.consoleNotifier.IsNotifying())
                toWrite = noPrefix;

            if (!string.IsNullOrWhiteSpace(toWrite))
                (Game1.chatBox as CommandChatBox)?.AddConsoleMessage(toWrite, Utils.ConvertConsoleColorToColor(Console.ForegroundColor));
        }
    }
}