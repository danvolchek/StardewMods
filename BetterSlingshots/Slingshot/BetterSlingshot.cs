using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Projectiles;
using SObject = StardewValley.Object;

namespace BetterSlingshots.Slingshot
{
    internal class BetterSlingshot : StardewValley.Tools.Slingshot, IActionButtonAware
    {
        /// <summary>Current auto fire index, ranges from 0 - <see cref="autoFireRate" />.</summary>
        private int autoFireCounter;

        /// <summary>The fire rate for this slingshot.</summary>
        private readonly int autoFireRate;

        private readonly IReflectedField<bool> baseCanPlaySound;
        private NetEvent0 bFinishEvent;
        private readonly BetterSlingshotsConfig config;
        private bool couldHaveFired;
        private bool didFire;
        private bool isActionButtonDown;

        private readonly IReflectionHelper reflection;

        //Automatic fire handling

        private readonly bool isAutomatic;

        /// <summary>The last shot fired, used for reloading.</summary>
        private SObject lastShotProjectile;

        private Vector2 startingMousePos;

        private readonly bool improperlyConstructed;

        public BetterSlingshot() : base(32)
        {
            this.improperlyConstructed = true;
        }

        public BetterSlingshot(IReflectionHelper reflection, BetterSlingshotsConfig config, SObject currentProjectile,
            bool isActionButtonDown, int which) : base(which)
        {
            this.attachments[0] = currentProjectile;
            this.config = config;
            this.isActionButtonDown = isActionButtonDown;
            this.autoFireRate = this.GetFireRate();
            this.baseCanPlaySound = reflection.GetField<bool>(this, "canPlaySound");
            this.isAutomatic =
                config.AutomaticSlingshots.IndexOf(
                    Enum.GetName(typeof(SlingshotType), SlingshotManager.GetTypeFromIndex(this.InitialParentTileIndex)),
                    StringComparison.InvariantCultureIgnoreCase) != -1;

            this.reflection = reflection;
        }

        /// <summary>Sets the action button state.</summary>
        public void SetActionButtonDownState(bool which)
        {
            this.isActionButtonDown = which;
        }

        /// <summary>Fires a projectile.</summary>
        public void Fire(GameLocation location, int x, int y, int power, Farmer who)
        {
            this.IndexOfMenuItemView = this.InitialParentTileIndex;

            if (this.attachments[0] == null)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"));
                return;
            }

            if (this.GetPullBackRange(x, y, who) <= 4 || this.baseCanPlaySound.GetValue())
                return;
            this.didFire = true;
            this.lastShotProjectile = this.attachments[0];
            SObject one = (SObject)this.attachments[0].getOne();
            if (!this.config.InfiniteAmmo)
                --this.attachments[0].Stack;
            if (this.attachments[0].Stack <= 0 && !this.Reload())
                this.attachments[0] = null;

            int num5 = 1;
            BasicProjectile.onCollisionBehavior collisionBehavior = null;
            string collisionSound = "hammer";
            float num6 = 1f;
            if (this.InitialParentTileIndex == 33)
                num6 = 2f;
            else if (this.InitialParentTileIndex == 34)
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
                    collisionBehavior = BasicProjectile.explodeOnImpact;
                    collisionSound = "explosion";
                    break;
            }

            if (one.Category == -5)
                collisionSound = "slimedead";

            Vector2 velocity = this.GetDirection() * (15 + Game1.random.Next(4, 6)) * (1f + who.weaponSpeedModifier);

            BasicProjectile basicProjectile = new BasicProjectile(
                (int) ((double) num6 * (num5 + Game1.random.Next(-(num5 / 2), num5 + 2)) *
                       (1.0 + who.attackIncreaseModifier)), one.ParentSheetIndex, 0, 0,
                (float) (Math.PI / (64.0 + Game1.random.Next(-63, 64))), velocity.X, velocity.Y,
                this.GetStartProjectilePosition(who), collisionSound, "", false, true, location, who, true,
                collisionBehavior) {IgnoreLocationCollision = Game1.currentLocation.currentEvent != null};


            location.projectiles.Add(basicProjectile);
        }

        /// <summary>When the slingshot is active, handle auto firing.</summary>
        private void HandleAutoFireTick()
        {
            if (!this.isAutomatic)
                return;

            this.autoFireCounter = (this.autoFireCounter + 1) % this.autoFireRate;

            if (this.autoFireCounter == 0)
            {
                this.couldHaveFired = true;
                if (this.isActionButtonDown)
                    this.Fire(Game1.currentLocation, Game1.getMouseX(), Game1.getMouseY(), 1, this.lastUser);
            }
        }

        /// <summary>Reloads the slingshot.</summary>
        public bool Reload()
        {
            if (!this.config.AutoReload || this.lastShotProjectile == null)
                return false;

            for (int i = 0; i < this.lastUser.Items.Count; i++)
                if (this.lastUser.Items[i] is SObject obj && obj.canStackWith(this.lastShotProjectile))
                {
                    this.attachments[0] = obj;
                    this.lastShotProjectile = obj;
                    this.lastUser.Items[i] = null;
                    return true;
                }

            return false;
        }

        /// <summary>Gets the fire rate for this slingshot.</summary>
        public int GetFireRate()
        {
            int speed;
            switch (SlingshotManager.GetTypeFromIndex(this.InitialParentTileIndex))
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

            return speed / (this.config.RapidFire ? 2 : 1);
        }

        /// <summary>Gets the pull back range for for this slingshot.</summary>
        private int GetPullBackRange(int x, int y, Farmer who)
        {
            return this.GetPullBackRange(x, y, who.getStandingX() + Game1.viewport.X,
                who.getStandingY() + Game1.viewport.Y - Game1.tileSize);
        }

        /// <summary>Gets the pull back range for for this slingshot.</summary>
        private int GetPullBackRange(int x, int y, int x2, int y2)
        {
            return Math.Min(20, (int)Vector2.Distance(new Vector2(x2, y2), new Vector2(x, y)) / 20);
        }

        /// <summary>Gets the position projectiles should actually start from.</summary>
        private Vector2 GetStartProjectilePosition(Farmer who)
        {
            return new Vector2(who.getStandingX() - Game1.tileSize / 2, who.getStandingY() - Game1.tileSize);
        }

        /// <summary>Gets the direction a projectile should fire in.</summary>
        private Vector2 GetDirection()
        {
            Vector2 mousePosition = this.GetMousePosition();

            Vector2 playerPosition = new Vector2(this.lastUser.getStandingX(),
                this.lastUser.getStandingY() - Game1.tileSize / 2);

            Vector2 direction = mousePosition - playerPosition;
            direction = direction / (int)Math.Sqrt(Math.Pow(direction.X, 2) + Math.Pow(direction.Y, 2));

            if (!this.config.DisableReverseAiming)
                direction *= -1;
            return direction;
        }

        /// <summary>Gets the mouse position, accounting for game pads.</summary>
        private Vector2 GetMousePosition()
        {
            Vector2 mousePosition = new Vector2(Game1.getOldMouseX() + Game1.viewport.X,
                Game1.getOldMouseY() + Game1.viewport.Y);

            if (this.didStartWithGamePad())
                mousePosition = this.lastUser.getStandingPosition() +
                                new Vector2(Game1.oldPadState.ThumbSticks.Left.X,
                                    -Game1.oldPadState.ThumbSticks.Left.Y) * Game1.tileSize * 4f;

            return mousePosition;
        }

        /*****
         * Overrides
         *****/

        /// <summary>Draws recticles in the correct places.</summary>
        public override void draw(SpriteBatch b)
        {
            if (this.improperlyConstructed)
            {
                base.draw(b);
                return;
            }

            if (this.lastUser == null || !this.lastUser.usingSlingshot || !this.lastUser.IsLocalPlayer)
                return;

            Vector2 playerPosition = new Vector2(this.lastUser.getStandingX(),
                this.lastUser.getStandingY() - Game1.tileSize / 2);

            /*//Draw player position
            Vector2 direction = GetDirection();
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, playerPosition), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43, -1, -1)), Color.White, 0.0f, new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), 1f, SpriteEffects.None, 0.999999f);
            */

            if (this.config.ShowActualMousePositionWhenAiming)
                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.GetMousePosition()),
                    Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43, -1, -1), Color.White, 0.0f,
                    new Vector2(Game1.tileSize / 2, Game1.tileSize / 2), 1f, SpriteEffects.None, 0.999999f);


            Vector2 markerPosition = playerPosition + 181 * this.GetDirection();
            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, markerPosition),
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43, -1, -1), Color.White, 0.0f,
                new Vector2(Game1.tileSize / 2, Game1.tileSize / 2), 1f, SpriteEffects.None, 0.999999f);
        }

        /// <summary>
        ///     Calls <see cref="Fire(GameLocation, int, int, int, StardewValley.Farmer)" /> if this is not an automatic
        ///     slingshot.
        /// </summary>
        public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        {
            if (this.improperlyConstructed)
            {
                base.DoFunction(location, x, y, power, who);
                this.bFinishEvent.Fire();
                return;
            }

            if (!this.baseCanPlaySound.GetValue())
                if (this.GetPullBackRange(x, y, (int)this.startingMousePos.X, (int)this.startingMousePos.Y) > 4)
                    if (!this.isAutomatic || (this.couldHaveFired && !this.didFire))
                        this.Fire(location, x, y, power, who);

            this.baseCanPlaySound.SetValue(true);
            this.bFinishEvent.Fire();
        }

        private bool addedField;

        public void PrepareForFiring()
        {
            this.bFinishEvent = this.reflection.GetField<NetEvent0>(Game1.player.CurrentTool, "finishEvent").GetValue();
            this.reflection.GetField<NetEvent0>(this, "finishEvent").SetValue(this.bFinishEvent);

            this.reflection.GetField<NetPoint>(this, "aimPos").SetValue(this.reflection.GetField<NetPoint>(Game1.player.CurrentTool, "aimPos").GetValue());

           // this.NetFields.AddField(this.bFinishEvent);
        }

        /// <summary>Sets up the slingshot for firing.</summary>
        public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
        {
            if (this.improperlyConstructed)
                return base.beginUsing(location, x, y, who);

            if (this.config.CanMoveWhileFiring)
                who.canMove = true;
            this.lastShotProjectile = null;
            this.startingMousePos = new Vector2(x, y);
            return base.beginUsing(location, x, y, who);
        }

        /// <summary>Draws the player correctly and handles auto aiming.</summary>
        public override void tickUpdate(GameTime time, Farmer who)
        {
            if (this.improperlyConstructed)
            {
                base.tickUpdate(time, who);
                return;
            }

            this.bFinishEvent.Poll();
            this.lastUser = who;
            if (!who.usingSlingshot)
                return;
            if (who.IsLocalPlayer)
            {

                this.HandleAutoFireTick();
                Point point = Game1.getMousePosition();
                if (this.didStartWithGamePad())
                {
                    point = Utility.Vector2ToPoint(this.lastUser.getStandingPosition() +
                                                   new Vector2(Game1.oldPadState.ThumbSticks.Left.X,
                                                       -Game1.oldPadState.ThumbSticks.Left.Y) * Game1.tileSize * 4f);
                    point.X -= Game1.viewport.X;
                    point.Y -= Game1.viewport.Y;
                }

                int num1 = point.X + Game1.viewport.X;
                int num2 = point.Y + Game1.viewport.Y;

                ++this.mouseDragAmount;
                who.faceGeneralDirection(new Vector2(num1, num2), 0, true);

                this.aimPos.X = num1;
                this.aimPos.Y = num2;
                
                if (this.baseCanPlaySound.GetValue() &&
                    (Math.Abs(num1 - this.lastClickX) > 8 || Math.Abs(num2 - this.lastClickY) > 8) &&
                    this.mouseDragAmount > 4)
                {
                    who.currentLocation.playSound("slingshot");
                    this.baseCanPlaySound.SetValue(false);
                }

                this.lastClickX = num1;
                this.lastClickY = num2;
                Game1.mouseCursor = -1;
            }
            int num = who.FacingDirection == 3 || who.FacingDirection == 1 ? 1 : (who.FacingDirection == 0 ? 2 : 0);
            who.FarmerSprite.setCurrentFrame(42 + num);

        }
    }
}