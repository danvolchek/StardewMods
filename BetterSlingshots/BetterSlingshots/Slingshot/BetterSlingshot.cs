using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using SObject = StardewValley.Object;

namespace BetterSlingshots.Slingshot
{
    internal class BetterSlingshot : StardewValley.Tools.Slingshot, IActionButtonAware
    {
        private IReflectionHelper reflection;
        private BetterSlingshotsConfig config;
        private IReflectedField<bool> baseCanPlaySound;

        /// <summary>The last shot fired, used for reloading.</summary>
        private SObject lastShotProjectile = null;

        private Vector2 startingMousePos;

        //Automatic fire handling

        private bool isAutomatic;
        /// <summary>Current auto fire index, ranges from 0 - <see cref="autoFireRate"/>.</summary>
        private int autoFireCounter = 0;
        /// <summary>The fire rate for this slingshot.</summary>
        private int autoFireRate;
        private bool isActionButtonDown;
        private bool didFire = false;
        private bool couldHaveFired = false;

        public BetterSlingshot(IReflectionHelper reflection, BetterSlingshotsConfig config, SObject currentProjectile, bool isActionButtonDown, int which) : base(which)
        {
            this.attachments[0] = currentProjectile;
            this.reflection = reflection;
            this.config = config;
            this.isActionButtonDown = isActionButtonDown;
            this.autoFireRate = this.GetFireRate();
            this.baseCanPlaySound = reflection.GetField<bool>(this, "canPlaySound");
            this.isAutomatic = config.AutomaticSlingshots.IndexOf(Enum.GetName(typeof(SlingshotType), SlingshotManager.GetTypeFromIndex(this.initialParentTileIndex)), StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        /// <summary>Fires a projectile.</summary>
        public void Fire(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            if (this.attachments[0] == null)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"));
                return;
            }

            if (GetPullBackRange(x, y, who) <= 4 || this.baseCanPlaySound.GetValue())
                return;
            didFire = true;
            lastShotProjectile = this.attachments[0];
            SObject one = (SObject)this.attachments[0].getOne();
            if (!config.InfiniteAmmo)
                --this.attachments[0].Stack;
            if (this.attachments[0].Stack <= 0 && !this.Reload())
                this.attachments[0] = null;

            int num5 = 1;
            BasicProjectile.onCollisionBehavior collisionBehavior = null;
            string collisionSound = "hammer";
            float num6 = 1f;
            if (this.initialParentTileIndex == 33)
                num6 = 2f;
            else if (this.initialParentTileIndex == 34)
                num6 = 4f;
            switch (one.ParentSheetIndex)
            {
                case 378:
                    num5 = 10;
                    ++one.ParentSheetIndex;
                    break;

                case 380:
                    num5 = 20;
                    ++one.ParentSheetIndex;
                    break;

                case 382:
                    num5 = 15;
                    ++one.ParentSheetIndex;
                    break;

                case 384:
                    num5 = 30;
                    ++one.ParentSheetIndex;
                    break;

                case 386:
                    num5 = 50;
                    ++one.ParentSheetIndex;
                    break;

                case 388:
                    num5 = 2;
                    ++one.ParentSheetIndex;
                    break;

                case 390:
                    num5 = 5;
                    ++one.ParentSheetIndex;
                    break;

                case 441:
                    num5 = 20;
                    collisionBehavior = new BasicProjectile.onCollisionBehavior(BasicProjectile.explodeOnImpact);
                    collisionSound = "explosion";
                    break;
            }
            if (one.category == -5)
                collisionSound = "slimedead";

            List<Projectile> projectiles = location.projectiles;

            Vector2 velocity = GetDirection() * (15 + Game1.random.Next(4, 6)) * (1f + who.weaponSpeedModifier);

            BasicProjectile basicProjectile = new BasicProjectile((int)((double)num6 * (num5 + Game1.random.Next(-(num5 / 2), num5 + 2)) * (1.0 + who.attackIncreaseModifier)), one.ParentSheetIndex, 0, 0, (float)(Math.PI / (64.0 + Game1.random.Next(-63, 64))), velocity.X, velocity.Y, GetStartProjectilePosition(who), collisionSound, "", false, true, who, true, collisionBehavior)
            {
                ignoreLocationCollision = Game1.currentLocation.currentEvent != null
            };
            projectiles.Add(basicProjectile);
        }

        /// <summary>When the slingshot is active, handle auto firing.</summary>
        private void HandleAutoFireTick()
        {
            if (!this.isAutomatic)
                return;

            this.autoFireCounter = (autoFireCounter + 1) % autoFireRate;

            if (autoFireCounter == 0)
            {
                couldHaveFired = true;
                if (isActionButtonDown)
                    this.Fire(Game1.currentLocation, Game1.getMouseX(), Game1.getMouseY(), 1, Game1.player);
            }

        }

        /// <summary>Reloads the slingshot.</summary>
        public bool Reload()
        {
            if (!config.AutoReload || lastShotProjectile == null)
                return false;

            for (int i = 0; i < Game1.player.Items.Count; i++)
            {
                if (Game1.player.Items[i] is SObject obj && obj.canStackWith(lastShotProjectile))
                {
                    this.attachments[0] = obj;
                    lastShotProjectile = obj;
                    Game1.player.Items[i] = null;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Gets the fire rate for this slingshot.</summary>
        public int GetFireRate()
        {
            int speed;
            switch (SlingshotManager.GetTypeFromIndex(this.initialParentTileIndex))
            {
                default:
                case SlingshotType.Basic:
                    speed = 25;
                    break;
                case SlingshotType.Master:
                    speed = 20;
                    break;
                case SlingshotType.Galaxy:
                    speed = 15;
                    break;
            }
            return speed / (config.RapidFire ? 2 : 1);
        }

        /// <summary>Gets the pull back range for for this slingshot.</summary>
        private int GetPullBackRange(int x, int y, StardewValley.Farmer who)
        {
            return GetPullBackRange(x, y, who.getStandingX() + Game1.viewport.X, who.getStandingY() + Game1.viewport.Y - Game1.tileSize);
        }

        /// <summary>Gets the pull back range for for this slingshot.</summary>
        private int GetPullBackRange(int x, int y, int x2, int y2)
        {
            return Math.Min(20, (int)Vector2.Distance(new Vector2(x2, y2), new Vector2(x, y)) / 20);
        }

        /// <summary>Gets the position projectiles should actually start from.</summary>
        private Vector2 GetStartProjectilePosition(StardewValley.Farmer who)
        {
            return new Vector2(who.getStandingX() - Game1.tileSize / 2, who.getStandingY() - Game1.tileSize);
        }

        /// <summary>Gets the direction a projectile should fire in.</summary>
        private Vector2 GetDirection()
        {
            Vector2 mousePosition = GetMousePosition();

            Vector2 playerPosition = new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY() - Game1.tileSize / 2);

            Vector2 direction = mousePosition - playerPosition;
            direction = direction / ((int)Math.Sqrt(Math.Pow(direction.X, 2) + Math.Pow(direction.Y, 2)));

            if (!config.DisableReverseAiming)
                direction *= -1;
            return direction;
        }

        /// <summary>Gets the mouse position, accounting for game pads.</summary>
        private Vector2 GetMousePosition()
        {
            Vector2 mousePosition = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y);

            if (this.didStartWithGamePad())
            {
                mousePosition = Game1.player.getStandingPosition() + new Vector2(Game1.oldPadState.ThumbSticks.Left.X, -Game1.oldPadState.ThumbSticks.Left.Y) * (float)Game1.tileSize * 4f;
            }

            return mousePosition;
        }

        /*****
         * Overrides
         *****/

        /// <summary>Draws recticles in the correct places.</summary>
        public override void draw(SpriteBatch b)
        {
            if (!Game1.player.usingSlingshot)
                return;

            Vector2 playerPosition = new Vector2(Game1.player.getStandingX(), Game1.player.getStandingY() - Game1.tileSize / 2);

            /*//Draw player position
            Vector2 direction = GetDirection();
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, playerPosition), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43, -1, -1)), Color.White, 0.0f, new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), 1f, SpriteEffects.None, 0.999999f);
            */

            if (config.ShowActualMousePositionWhenAiming)
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, GetMousePosition()), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43, -1, -1)), Color.White, 0.0f, new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), 1f, SpriteEffects.None, 0.999999f);


            Vector2 markerPosition = playerPosition + 181 * GetDirection();
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, markerPosition), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43, -1, -1)), Color.White, 0.0f, new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), 1f, SpriteEffects.None, 0.999999f);
        }

        /// <summary>Calls <see cref="Fire(GameLocation, int, int, int, StardewValley.Farmer)"/> if this is not an automatic slingshot.</summary>
        public override void DoFunction(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            this.indexOfMenuItemView = this.initialParentTileIndex;
            who.usingSlingshot = false;
            who.canReleaseTool = true;
            who.usingTool = false;
            who.canMove = true;

            if (!this.baseCanPlaySound.GetValue())
            {
                if(this.GetPullBackRange(x, y, (int)startingMousePos.X, (int)startingMousePos.Y) > 4)
                    if(!this.isAutomatic || (this.couldHaveFired && !this.didFire))
                        this.Fire(location, x, y, power, who);
            }

            baseCanPlaySound.SetValue(true);
            who.Halt();
        }

        /// <summary>Sets up the slingshot for firing.</summary>
        public override bool beginUsing(GameLocation location, int x, int y, StardewValley.Farmer who)
        {
            if (config.CanMoveWhileFiring)
                who.canMove = true;
            lastShotProjectile = null;
            startingMousePos = new Vector2(x, y);
            return base.beginUsing(location, x, y, who);
        }

        /// <summary>Draws the player correctly and handles auto aiming.</summary>
        public override void tickUpdate(GameTime time, StardewValley.Farmer who)
        {
            if (!who.usingSlingshot)
                return;
            this.HandleAutoFireTick();
            Point point = Game1.getMousePosition();
            if (this.didStartWithGamePad())
            {
                point = Utility.Vector2ToPoint(Game1.player.getStandingPosition() + new Vector2(Game1.oldPadState.ThumbSticks.Left.X, -Game1.oldPadState.ThumbSticks.Left.Y) * (float)Game1.tileSize * 4f);
                point.X -= Game1.viewport.X;
                point.Y -= Game1.viewport.Y;
            }
            int num1 = point.X + Game1.viewport.X;
            int num2 = point.Y + Game1.viewport.Y;

            this.mouseDragAmount = this.mouseDragAmount + 1;
            who.faceGeneralDirection(new Vector2((float)num1, (float)num2), 0);
            //Currently buggy (only works for basic slingshot, intersects with body (no fix for this until 1.3))
            //if (!config.DisableReverseAiming)
            who.faceDirection((who.FacingDirection + 2) % 4);
            int num3 = who.FacingDirection == 3 || who.FacingDirection == 1 ? 1 : (who.FacingDirection == 0 ? 2 : 0);
            who.FarmerSprite.setCurrentFrame(42 + num3);
            if (this.baseCanPlaySound.GetValue() && (Math.Abs(num1 - this.lastClickX) > 8 || Math.Abs(num2 - this.lastClickY) > 8) && this.mouseDragAmount > 4)
            {
                Game1.playSound("slingshot");
                this.baseCanPlaySound.SetValue(false);
            }
            this.lastClickX = num1;
            this.lastClickY = num2;
            Game1.mouseCursor = -1;
        }

        /// <summary>Sets the action button state.</summary>
        public void SetActionButtonDownState(bool which)
        {
            this.isActionButtonDown = which;
        }
    }
}