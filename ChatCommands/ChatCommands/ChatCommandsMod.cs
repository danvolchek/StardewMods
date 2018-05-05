using ChatCommands.ClassReplacements;
using ChatCommands.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Linq;
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

        private int repeatWaitPeriod = 20;

        private Keys downKey = Keys.None;

        public override void Entry(IModHelper helper)
        {
            this.commandValidator = new CommandValidator(helper.ConsoleCommands);
            this.consoleNotifier = new NotifyingTextWriter(Console.Out, this.OnLineWritten);

            Console.SetOut(this.consoleNotifier);
            SaveEvents.AfterLoad += this.SaveEvents_AfterLoad;
            GameEvents.SecondUpdateTick += this.GameEvents_HalfSecondTick;

            this.modConfig = helper.ReadConfig<ChatCommandsConfig>();

            // ReSharper disable once ObjectCreationAsStatement
            new ListenCommand(this.Monitor, this.modConfig, this.consoleNotifier).Register(helper.ConsoleCommands);
        }

        //Resend left and right arrow keys if they're being held down
        private void GameEvents_HalfSecondTick(object sender, EventArgs e)
        {
            if (Game1.chatBox == null || !Game1.chatBox.isActive())
                return;

            bool isLeftDown = Keyboard.GetState().IsKeyDown(Keys.Left);
            bool isRightDown = Keyboard.GetState().IsKeyDown(Keys.Right);
            if (isLeftDown ^ isRightDown)
            {
                if ((isLeftDown && this.downKey == Keys.Left) || (isRightDown && this.downKey == Keys.Right))
                {
                    if(this.repeatWaitPeriod !=0)
                        this.repeatWaitPeriod--;
                }
                else
                {
                    this.repeatWaitPeriod = 15;
                }

                this.downKey = isLeftDown ? Keys.Left : Keys.Right;
                if(this.repeatWaitPeriod == 0)
                    Game1.chatBox.receiveKeyPress(this.downKey);
            }
            else
            {
                this.downKey = Keys.None;
                this.repeatWaitPeriod = 15;
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
            Game1.chatBox = new CommandChatBox(this.Helper.Reflection, this, this.modConfig);
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
                Game1.chatBox?.addMessage(toWrite, Utils.ConvertConsoleColorToColor(Console.ForegroundColor));
        }
    }
}