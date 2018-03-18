using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterFruitTrees
{
    /***
     * All ye who enter here be warned - this class involves a lot of copy pasting due to non-virtual methods and is less than ideal.
     ***/
    class FruitTreeAwareJunimoHarvester : JunimoHarvester
    {
        public static IModHelper helper;

        //Private variables in JunimoHarvester
        private IReflectedField<int> fharvestTimer;
        private IReflectedField<Task> fbackgroundTask;
        private IReflectedField<float> falpha;
        private IReflectedField<float> falphaChange;
        private IReflectedField<Rectangle> fnextPosition;
        private IReflectedField<bool> fdestroy;
        private IReflectedField<JunimoHut> fhome;
        private IReflectedField<Vector2> fmotion;
        private IReflectedField<Item> flastItemHarvested;
        private IReflectedField<Color> fcolor;
        private IReflectedField<bool> freturningToEndPoint;
        private IReflectedField<bool> fisWalkingInSquare;

        //Used to only ensure one harvesting attempt per timer interval, instead of checking was >1000 and now <1000
        private bool hasAttemptedHarvestThisInterval;

        public FruitTreeAwareJunimoHarvester(Vector2 position, JunimoHut myHome, int whichJunimoNumberFromThisHut) : base(position, myHome, whichJunimoNumberFromThisHut)
        {
            fharvestTimer = helper.Reflection.GetField<int>(this, "harvestTimer");
            fbackgroundTask = helper.Reflection.GetField<Task>(this, "backgroundTask");
            falpha = helper.Reflection.GetField<float>(this, "alpha");
            falphaChange = helper.Reflection.GetField<float>(this, "alphaChange");
            fnextPosition = helper.Reflection.GetField<Rectangle>(this, "nextPosition");
            fdestroy = helper.Reflection.GetField<bool>(this, "destroy");
            fhome = helper.Reflection.GetField<JunimoHut>(this, "home");
            fmotion = helper.Reflection.GetField<Vector2>(this, "motion");
            flastItemHarvested = helper.Reflection.GetField<Item>(this, "lastItemHarvested");
            fcolor = helper.Reflection.GetField<Color>(this, "color");
            fisWalkingInSquare = helper.Reflection.GetField<bool>(this, "isWalkingInSquare");
            freturningToEndPoint = helper.Reflection.GetField<bool>(this, "returningToEndPoint");

            hasAttemptedHarvestThisInterval = false;
        }

        //Two functions here - FruitTryToHarvestHere and FruitFoundCropEndFunction - are the relevant ones that need to be modified to have harvesters be aware of fruit trees.
        //However, since these functions are not virtual, and they are not only called from virtual methods (i.e. update), we need to replace everything that calls them with
        //new versions that get called from update. So they're all copied from the base JunimoHarvester and modified to call each other, along with the actual changes in the previous 2 mentioned functions.

        //Thankfully, we only need to replace functions from this class and one in JunimoHut. This is very ugly and I wish I didn't need to do this, but it seemed like the best method
        //Given the constraints.
        public void FruitReachFirstDestinationFromHut(Character c, GameLocation l)
        {
            FruitTryToHarvestHere();
        }

        public void FruitPokeToHarvest()
        {

            if (!this.fhome.GetValue().isTilePassable(this.getTileLocation()))
            {
                this.fdestroy.SetValue(true);
            }
            else
            {
                if (this.fharvestTimer.GetValue() > 0 || Game1.random.NextDouble() >= 0.7)
                    return;
                this.FruitPathfindToNewCrop();
            }
        }

        public void FruitPathFindToNewCrop_doWork()
        {
            if (Game1.timeOfDay > 1900)
            {
                if (this.controller != null)
                    return;
                this.returnToJunimoHut(this.currentLocation);
            }
            else if (Game1.random.NextDouble() < 0.035 || this.fhome.GetValue().noHarvest)
            {
                this.FruitPathfindToRandomSpotAroundHut();
            }
            else
            {

                this.controller = new PathFindController((Character)this, this.currentLocation, new PathFindController.isAtEnd(this.FruitFoundCropEndFunction), -1, false, new PathFindController.endBehavior(this.FruitReachFirstDestinationFromHut), 100, Point.Zero);
                if (this.controller.pathToEndPoint == null || Math.Abs(this.controller.pathToEndPoint.Last<Point>().X - (this.fhome.GetValue().tileX + 1)) > 8 || Math.Abs(this.controller.pathToEndPoint.Last<Point>().Y - (this.fhome.GetValue().tileY + 1)) > 8)
                {
                    if (Game1.random.NextDouble() < 0.5 && !this.fhome.GetValue().lastKnownCropLocation.Equals(Point.Zero))
                        this.controller = new PathFindController((Character)this, this.currentLocation, this.fhome.GetValue().lastKnownCropLocation, -1, new PathFindController.endBehavior(this.FruitReachFirstDestinationFromHut), 100);
                    else if (Game1.random.NextDouble() < 0.25)
                        this.returnToJunimoHut(this.currentLocation);
                    else
                        this.FruitPathfindToRandomSpotAroundHut();
                }
                else
                    this.sprite.CurrentAnimation = (List<FarmerSprite.AnimationFrame>)null;
            }
        }

        public void FruitPathfindToNewCrop()
        {

            if (this.fbackgroundTask.GetValue() != null && !this.fbackgroundTask.GetValue().IsCompleted)
                return;

            this.fbackgroundTask.SetValue(new Task(new Action(this.FruitPathFindToNewCrop_doWork)));
            this.fbackgroundTask.GetValue().Start();
        }

        public void FruitPathfindToRandomSpotAroundHut()
        {
            this.controller = new PathFindController((Character)this, this.currentLocation, Utility.Vector2ToPoint(new Vector2((float)(this.fhome.GetValue().tileX + 1 + Game1.random.Next(-8, 9)), (float)(this.fhome.GetValue().tileY + 1 + Game1.random.Next(-8, 9)))), -1, new PathFindController.endBehavior(this.FruitReachFirstDestinationFromHut), 100);
        }

        public bool FruitFoundCropEndFunction(PathNode currentNode, Point endPoint, GameLocation location, Character c)
        {
            return (location.isCropAtTile(currentNode.x, currentNode.y) && (location.terrainFeatures[new Vector2((float)currentNode.x, (float)currentNode.y)] as HoeDirt).readyForHarvest())
                || IsAdjacentReadyToHarvestFruitTree(new Vector2(currentNode.x, currentNode.y), location);
        }

        public void FruitTryToHarvestHere()
        {

            if (this.currentLocation == null)
                return;

            if (this.currentLocation.terrainFeatures.ContainsKey(this.getTileLocation()) && this.currentLocation.terrainFeatures[this.getTileLocation()] is HoeDirt && (this.currentLocation.terrainFeatures[this.getTileLocation()] as HoeDirt).readyForHarvest()
                || IsAdjacentReadyToHarvestFruitTree(this.getTileLocation(), currentLocation))
            {
                fharvestTimer.SetValue(2000);
                hasAttemptedHarvestThisInterval = false;

            }
            else
                this.FruitPokeToHarvest();
        }

        public override void update(GameTime time, GameLocation location)
        {
            if (this.fbackgroundTask.GetValue() != null && !this.fbackgroundTask.GetValue().IsCompleted)
            {
                this.sprite.Animate(time, 8, 4, 100f);
            }
            else
            {
                NPCUpdate(time, location);
                this.forceUpdateTimer = 99999;
                if (this.eventActor)
                    return;
                if (this.fdestroy.GetValue())
                    this.falphaChange.SetValue(-0.05f);
                this.falpha.SetValue(this.falpha.GetValue() + this.falphaChange.GetValue());
                if ((double)this.falpha.GetValue() > 1.0)
                {
                    this.falpha.SetValue(1f);
                    this.hideShadow = false;
                }
                else if ((double)this.falpha.GetValue() < 0.0)
                {
                    this.falpha.SetValue(0.0f);
                    this.isInvisible = true;
                    this.hideShadow = true;
                    if (this.fdestroy.GetValue())
                    {
                        location.characters.Remove((NPC)this);
                        this.fhome.GetValue().myJunimos.Remove(this);
                    }
                }
                if (this.fharvestTimer.GetValue() > 0)
                {
                    int harvestTimer = this.fharvestTimer.GetValue();
                    this.fharvestTimer.SetValue(this.fharvestTimer.GetValue() - time.ElapsedGameTime.Milliseconds);
                    //if (harvestTimer % 200 < 50)

                    if (this.fharvestTimer.GetValue() > 1800)
                        this.sprite.CurrentFrame = 0;
                    else if (this.fharvestTimer.GetValue() > 1600)
                        this.sprite.CurrentFrame = 1;
                    else if (this.fharvestTimer.GetValue() > 1000)
                    {
                        this.sprite.CurrentFrame = 2;
                        this.shake(50);
                    }
                    else if (!hasAttemptedHarvestThisInterval && this.fharvestTimer.GetValue() < 1000)
                    {
                        hasAttemptedHarvestThisInterval = true;
                        this.sprite.CurrentFrame = 0;

                        if (this.currentLocation != null && !this.fhome.GetValue().noHarvest && ((((this.currentLocation.terrainFeatures.ContainsKey(this.getTileLocation()) && this.currentLocation.terrainFeatures[this.getTileLocation()] is HoeDirt) && (this.currentLocation.terrainFeatures[this.getTileLocation()] as HoeDirt).readyForHarvest())) || IsAdjacentReadyToHarvestFruitTree(this.getTileLocation(), currentLocation)))
                        {
                            this.sprite.CurrentFrame = 44;
                            this.flastItemHarvested.SetValue((Item)null);

                            if (this.currentLocation.terrainFeatures.ContainsKey(this.getTileLocation()) && this.currentLocation.terrainFeatures[this.getTileLocation()] is HoeDirt && (this.currentLocation.terrainFeatures[this.getTileLocation()] as HoeDirt).readyForHarvest())
                            {
                                if ((this.currentLocation.terrainFeatures[this.getTileLocation()] as HoeDirt).crop.harvest(this.getTileX(), this.getTileY(), this.currentLocation.terrainFeatures[this.getTileLocation()] as HoeDirt, this))
                                    (this.currentLocation.terrainFeatures[this.getTileLocation()] as HoeDirt).destroyCrop(this.getTileLocation(), Game1.currentLocation.Equals((object)this.currentLocation));

                            }
                            else
                                TryToActuallyHarvestFruitTree();

                            if (this.flastItemHarvested != null && this.currentLocation.Equals((object)Game1.currentLocation))
                            {
                                this.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.flastItemHarvested.GetValue().parentSheetIndex, 16, 16), 1000f, 1, 0, this.position + new Vector2(0.0f, (float)(-Game1.tileSize + 6 * Game1.pixelZoom)), false, false, (float)((double)this.getStandingY() / 10000.0 + 0.00999999977648258), 0.02f, Color.White, (float)Game1.pixelZoom, -0.01f, 0.0f, 0.0f, false)
                                {
                                    motion = new Vector2(0.08f, -0.25f)
                                });
                                if (this.flastItemHarvested.GetValue() is ColoredObject)
                                    this.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.flastItemHarvested.GetValue().parentSheetIndex + 1, 16, 16), 1000f, 1, 0, this.position + new Vector2(0.0f, (float)(-Game1.tileSize + 6 * Game1.pixelZoom)), false, false, (float)((double)this.getStandingY() / 10000.0 + 0.0149999996647239), 0.02f, (this.flastItemHarvested as ColoredObject).color, (float)Game1.pixelZoom, -0.01f, 0.0f, 0.0f, false)
                                    {
                                        motion = new Vector2(0.08f, -0.25f)
                                    });
                            }
                        }
                    }
                    else if (this.fharvestTimer.GetValue() <= 0)
                        this.FruitPokeToHarvest();
                }
                else if (!this.isInvisible && this.controller == null)
                {
                    if (this.addedSpeed > 0 || this.speed > 2 || this.isCharging)
                        this.fdestroy.SetValue(true);
                    this.fnextPosition.SetValue(this.GetBoundingBox());
                    Rectangle r = this.fnextPosition.GetValue();
                    r.X += (int)this.fmotion.GetValue().X;
                    this.fnextPosition.SetValue(r);
                    bool flag = false;
                    if (!location.isCollidingPosition(this.fnextPosition.GetValue(), Game1.viewport, (Character)this))
                    {
                        this.position.X += (float)(int)this.fmotion.GetValue().X;
                        flag = true;
                    }

                    r = this.fnextPosition.GetValue();
                    r.X -= (int)this.fmotion.GetValue().X;
                    r.Y += (int)this.fmotion.GetValue().Y;
                    this.fnextPosition.SetValue(r);

                    if (!location.isCollidingPosition(this.fnextPosition.GetValue(), Game1.viewport, (Character)this))
                    {
                        this.position.Y += (float)(int)this.fmotion.GetValue().Y;
                        flag = true;
                    }
                    if (!this.fmotion.GetValue().Equals(Vector2.Zero) & flag && Game1.random.NextDouble() < 0.005)
                        location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.random.NextDouble() < 0.5 ? 10 : 11, this.position, this.fcolor.GetValue(), 8, false, 100f, 0, -1, -1f, -1, 0)
                        {
                            motion = this.fmotion.GetValue() / 4f,
                            alphaFade = 0.01f,
                            layerDepth = 0.8f,
                            scale = 0.75f,
                            alpha = 0.75f
                        });
                    if (Game1.random.NextDouble() < 0.002)
                    {
                        switch (Game1.random.Next(6))
                        {
                            case 0:
                                this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>()
                {
                  new FarmerSprite.AnimationFrame(12, 200),
                  new FarmerSprite.AnimationFrame(13, 200),
                  new FarmerSprite.AnimationFrame(14, 200),
                  new FarmerSprite.AnimationFrame(15, 200)
                });
                                break;
                            case 1:
                                this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>()
                {
                  new FarmerSprite.AnimationFrame(44, 200),
                  new FarmerSprite.AnimationFrame(45, 200),
                  new FarmerSprite.AnimationFrame(46, 200),
                  new FarmerSprite.AnimationFrame(47, 200)
                });
                                break;
                            case 2:
                                this.sprite.CurrentAnimation = (List<FarmerSprite.AnimationFrame>)null;
                                break;
                            case 3:
                                this.jumpWithoutSound(8f);
                                this.yJumpVelocity = this.yJumpVelocity / 2f;
                                this.sprite.CurrentAnimation = (List<FarmerSprite.AnimationFrame>)null;
                                break;
                            case 4:
                                if (!this.fhome.GetValue().noHarvest)
                                {
                                    this.FruitPathfindToNewCrop();
                                    break;
                                }
                                break;
                            case 5:
                                this.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>()
                {
                  new FarmerSprite.AnimationFrame(28, 100),
                  new FarmerSprite.AnimationFrame(29, 100),
                  new FarmerSprite.AnimationFrame(30, 100),
                  new FarmerSprite.AnimationFrame(31, 100)
                });
                                break;
                        }
                    }
                }
                if (this.controller != null || !this.fmotion.GetValue().Equals(Vector2.Zero))
                {
                    this.sprite.CurrentAnimation = (List<FarmerSprite.AnimationFrame>)null;
                    if (this.moveRight || (double)Math.Abs(this.fmotion.GetValue().X) > (double)Math.Abs(this.fmotion.GetValue().Y) && (double)this.fmotion.GetValue().X > 0.0)
                    {
                        this.flip = false;
                        if (!this.sprite.Animate(time, 16, 8, 50f))
                            return;
                        this.sprite.CurrentFrame = 16;
                    }
                    else if (this.moveLeft || (double)Math.Abs(this.fmotion.GetValue().X) > (double)Math.Abs(this.fmotion.GetValue().Y) && (double)this.fmotion.GetValue().X < 0.0)
                    {
                        if (this.sprite.Animate(time, 16, 8, 50f))
                            this.sprite.CurrentFrame = 16;
                        this.flip = true;
                    }
                    else if (this.moveUp || (double)Math.Abs(this.fmotion.GetValue().Y) > (double)Math.Abs(this.fmotion.GetValue().X) && (double)this.fmotion.GetValue().Y < 0.0)
                    {
                        if (!this.sprite.Animate(time, 32, 8, 50f))
                            return;
                        this.sprite.CurrentFrame = 32;
                    }
                    else
                    {
                        if (!this.moveDown)
                            return;
                        this.sprite.Animate(time, 0, 8, 50f);
                    }
                }
                else
                {
                    if (this.sprite.CurrentAnimation != null || this.fharvestTimer.GetValue() > 0)
                        return;
                    this.sprite.Animate(time, 8, 4, 100f);
                }
            }
        }

        //We need to call the base NPC update, but we don't want to call JunimoHarvester's update function since we're replacing that. Sadly, the only way to 
        //do this is to copy the code from NPC because we have no reference to the base update function.
        private void NPCUpdate(GameTime time, GameLocation location)
        {
            if (this.freturningToEndPoint.GetValue())
            {
                this.returnToEndPoint();
                this.MovePosition(time, Game1.viewport, location);
            }
            else if (this.temporaryController != null)
            {
                if (this.temporaryController.update(time))
                    this.temporaryController = (PathFindController)null;
                this.updateEmote(time);
            }
            else
                base.update(time, location);
            if (this.textAboveHeadTimer > 0)
            {
                if (this.textAboveHeadPreTimer > 0)
                {
                    this.textAboveHeadPreTimer = this.textAboveHeadPreTimer - time.ElapsedGameTime.Milliseconds;
                }
                else
                {
                    this.textAboveHeadTimer = this.textAboveHeadTimer - time.ElapsedGameTime.Milliseconds;
                    this.textAboveHeadAlpha = this.textAboveHeadTimer <= 500 ? Math.Max(0.0f, this.textAboveHeadAlpha - 0.04f) : Math.Min(1f, this.textAboveHeadAlpha + 0.1f);
                }
            }
            if (this.fisWalkingInSquare.GetValue() && !this.freturningToEndPoint.GetValue())
                this.randomSquareMovement(time);
            if (this.Sprite != null && this.Sprite.currentAnimation != null && (!Game1.eventUp && this.Sprite.animateOnce(time)))
                this.Sprite.currentAnimation = (List<FarmerSprite.AnimationFrame>)null;
            TimeSpan timeSpan;
            if (this.movementPause > 0 && (!Game1.dialogueUp || this.controller != null))
            {
                this.freezeMotion = true;
                int movementPause = this.movementPause;
                timeSpan = time.ElapsedGameTime;
                int milliseconds = timeSpan.Milliseconds;
                this.movementPause = movementPause - milliseconds;
                if (this.movementPause <= 0)
                    this.freezeMotion = false;
            }
            if (this.shakeTimer > 0)
            {
                int shakeTimer = this.shakeTimer;
                timeSpan = time.ElapsedGameTime;
                int milliseconds = timeSpan.Milliseconds;
                this.shakeTimer = shakeTimer - milliseconds;
            }
            if (this.lastPosition.Equals(this.position))
            {
                double sinceLastMovement = (double)this.timerSinceLastMovement;
                timeSpan = time.ElapsedGameTime;
                double milliseconds = (double)timeSpan.Milliseconds;
                this.timerSinceLastMovement = (float)(sinceLastMovement + milliseconds);
            }
            else
                this.timerSinceLastMovement = 0.0f;
            if (!this.swimming)
                return;
            timeSpan = time.TotalGameTime;
            this.yOffset = (float)Math.Cos(timeSpan.TotalMilliseconds / 2000.0) * (float)Game1.pixelZoom;
            float swimTimer1 = this.swimTimer;
            double swimTimer2 = (double)this.swimTimer;
            timeSpan = time.ElapsedGameTime;
            double milliseconds1 = (double)timeSpan.Milliseconds;
            this.swimTimer = (float)(swimTimer2 - milliseconds1);
            if ((double)this.timerSinceLastMovement == 0.0)
            {
                if ((double)swimTimer1 > 400.0 && (double)this.swimTimer <= 400.0 && location.Equals((object)Game1.currentLocation))
                {
                    location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.animations, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.tileSize, Game1.tileSize), (float)(150.0 - ((double)Math.Abs(this.xVelocity) + (double)Math.Abs(this.yVelocity)) * 3.0), 8, 0, new Vector2(this.position.X, (float)(this.getStandingY() - Game1.tileSize / 2)), false, Game1.random.NextDouble() < 0.5, 0.01f, 0.01f, Color.White, 1f, 3f / 1000f, 0.0f, 0.0f, false));
                    Game1.playSound("slosh");
                }
                if ((double)this.swimTimer >= 0.0)
                    return;
                this.swimTimer = 800f;
                if (!location.Equals((object)Game1.currentLocation))
                    return;
                Game1.playSound("slosh");
                location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.animations, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.tileSize, Game1.tileSize), (float)(150.0 - ((double)Math.Abs(this.xVelocity) + (double)Math.Abs(this.yVelocity)) * 3.0), 8, 0, new Vector2(this.position.X, (float)(this.getStandingY() - Game1.tileSize / 2)), false, Game1.random.NextDouble() < 0.5, 0.01f, 0.01f, Color.White, 1f, 3f / 1000f, 0.0f, 0.0f, false));
            }
            else
            {
                if ((double)this.swimTimer >= 0.0)
                    return;
                this.swimTimer = 100f;
            }
        }


        /***
         * Actually attempts to harvest an adjacent fruit tree to the current location.
         ***/
        private void TryToActuallyHarvestFruitTree()
        {

            if (this.currentLocation == null)
                return;

            bool found = GetAdjacentReadyToHarvestFruitTree(this.getTileLocation(), this.currentLocation, out KeyValuePair<Vector2, FruitTree> tree);
            if (found)
            {

                //shake the tree without it releasing any fruit
                int fruitsOnTree = tree.Value.fruitsOnTree;
                tree.Value.fruitsOnTree = 0;
                tree.Value.performUseAction(tree.Key);
                tree.Value.fruitsOnTree = fruitsOnTree;
                StardewValley.Object result = GetFruitFromTree(tree.Value);
                if (result != null)
                    this.tryToAddItemToHut(result);
            }
        }

        /***
         * Gets a fruit from a FruitTree and updates the tree accordingly.
         ***/
        private StardewValley.Object GetFruitFromTree(FruitTree tree)
        {
            if (tree.fruitsOnTree == 0)
                return null;

            int num1 = 0;
            if (tree.daysUntilMature <= -112)
                num1 = 1;
            if (tree.daysUntilMature <= -224)
                num1 = 2;
            if (tree.daysUntilMature <= -336)
                num1 = 4;
            if (tree.struckByLightningCountdown > 0)
                num1 = 0;

            int harvestAmount = FruitTreeAwareJunimoHut.TreeHarvestAmount();
            tree.fruitsOnTree -= harvestAmount;

            return new StardewValley.Object(Vector2.Zero, tree.struckByLightningCountdown > 0 ? 382 : tree.indexOfFruit, harvestAmount)
            {
                quality = num1
            };
        }

        /***
         * Gets the first adjacent FruitTree with fruits on it. Returns whether such a tree was found, and puts the result in result.
         ***/
        private bool GetAdjacentReadyToHarvestFruitTree(Vector2 position, GameLocation location, out KeyValuePair<Vector2, FruitTree> result)
        {
            Vector2 treePos = Utility.getAdjacentTileLocations(position)
                .Where(pos => location.terrainFeatures.ContainsKey(pos) && location.terrainFeatures[pos] is FruitTree tree && FruitTreeAwareJunimoHut.CanTreeBeHarvested(tree)).FirstOrDefault();
            if (treePos == default(Vector2))
            {
                result = new KeyValuePair<Vector2, FruitTree>(Vector2.Zero, default(FruitTree));
                return false;
            }
            else
            {
                result = new KeyValuePair<Vector2, FruitTree>(treePos, location.terrainFeatures[treePos] as FruitTree);
                return true;
            }
        }

        /***
         * Gets whether there is an adjacent FruitTree with fruits on it.
         ***/
        private bool IsAdjacentReadyToHarvestFruitTree(Vector2 position, GameLocation location)
        {
            return GetAdjacentReadyToHarvestFruitTree(position, location, out KeyValuePair<Vector2, FruitTree> p);
        }


    }



}
