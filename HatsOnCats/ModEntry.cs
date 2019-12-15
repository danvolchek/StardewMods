using System;
using Harmony;
using HatsOnCats.Framework;
using HatsOnCats.Framework.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HatsOnCats.Framework.Configuration;
using HatsOnCats.Framework.Debug;
using HatsOnCats.Framework.Offsets;
using HatsOnCats.Framework.Storage;
using Microsoft.Xna.Framework;
using StardewValley.Characters;
using StardewValley.Monsters;

namespace HatsOnCats
{
    public class ModEntry : Mod
    {
        internal static ModEntry Instance;

        private ConfigurationManager configurationManager;
        private AggregateOffsetProvider offsetProvider;
        private CharacterFinder characterFinder = new CharacterFinder();

        private IHatStorageProvider storageProvider;
        private readonly IHatDrawer drawer = new HatDrawer();

        //TODO: In general: check when clicked, offsets
        public override void Entry(IModHelper helper)
        {
            ModEntry.Instance = this;

            IEnumerable<ConfigurableOffsetProvider> handlers = new ConfigurableOffsetProviderFactory(this.Helper.Data).CreateHandlers().ToList();

            this.offsetProvider = new AggregateOffsetProvider(handlers);
            this.configurationManager = new ConfigurationManager(this.Helper, handlers);
            this.storageProvider = new SingleHatStorageProvider(this.Monitor);
            this.configurationManager.UpdateConfig();
            this.PatchDrawMethods();

            this.Helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            this.Helper.Events.GameLoop.Saving += this.GameLoop_Saving;
            this.Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;

            AnimatedSpriteDrawer debugDrawer = new AnimatedSpriteDrawer(this.Helper, this.offsetProvider, this.drawer);
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            if (e.Button == SButton.O)
            {
                //Game1.currentLocation.characters.Add(new Serpent(new Vector2(10f, 10f)));
                Character c = Game1.currentLocation.characters.OfType<Character>().FirstOrDefault();
                if (c == null)
                {
                    this.Monitor.Log("Can't add a hat to null char.");
                } else
                {
                    this.Monitor.Log($"Added hat to {c.Name}");
                    this.storageProvider.AddHat(c, new Hat(0));
                }
            } else if (e.Button == SButton.P)
            {
                this.configurationManager.UpdateConfig();
            } else if (e.Button.IsUseToolButton() && this.characterFinder.TryGetCharacterAt(e.Cursor.ScreenPixels, out Character character) && this.storageProvider.CanHandle(character))
            {
                if (e.IsDown(SButton.LeftShift))
                {
                    this.storageProvider.RemoveHat(character, out Hat oldHat);

                    if (!Game1.player.addItemToInventoryBool(oldHat))
                    {
                        Game1.createItemDebris(oldHat, character.position, character.facingDirection);
                    }
                }else if (Game1.player.CurrentItem is Hat newHat)
                {
                    this.storageProvider.AddHat(character, newHat);
                    Game1.player.Items[Game1.player.CurrentToolIndex] = null;
                }
            }
        }

        public void DrawHats(Character character, SpriteBatch spriteBatch, Vector2 position, Rectangle? sourceRectangle, Vector2 origin, float rotation, SpriteEffects effects, float layerDepth)
        {
            if (this.storageProvider.CanHandle(character) && this.storageProvider.GetHats(character, out IEnumerable<Hat> hats))
            {
                if(this.offsetProvider.GetOffset(character.Sprite.UniqueName(), sourceRectangle.GetValueOrDefault(), effects, out Vector2 offset))
                {
                    this.drawer.DrawHats(hats, this.ToDrawDirection(character.FacingDirection), spriteBatch, position, origin + offset, rotation, layerDepth);
                }
            }
        }

        private int ToDrawDirection(int direction)
        {
            switch (direction)
            {
                case 0:
                    direction = 3;
                    break;
                case 2:
                    direction = 0;
                    break;
                case 3:
                    direction = 2;
                    break;
            }

            return direction;
        }

        private void PatchDrawMethods()
        {
            HatPatcher patcher = new HatPatcher(this.Monitor, HarmonyInstance.Create(this.Helper.ModRegistry.ModID));
            patcher.Patch(typeof(Game1).Assembly.GetTypes().Where(type => typeof(Character).IsAssignableFrom(type)));
        }
        
        private void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            this.storageProvider.Serialize(this.Helper.Data);
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            this.storageProvider.Deserialize(this.Helper.Data);
        }


    }
}
