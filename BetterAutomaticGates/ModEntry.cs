using BetterAutomaticGates.Framework;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace BetterAutomaticGates
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/

        /// <summary>Gates currently near any players.</summary>
        private ISet<Fence> gatesNearPlayers = new HashSet<Fence>();

        /// <summary>The mod configuration.</summary>
        private ModConfig config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>s
        public override void Entry(IModHelper helper)
        {
            this.config = this.Helper.ReadConfig<ModConfig>();
            this.Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady && e.IsMultipleOf(10))
            {
                this.ToggleGates(Game1.currentLocation);
            }
        }

        /// <summary>Toggles gates as necessary.</summary>
        /// <param name="location">The location to toggle gates in.</param>
        private void ToggleGates(GameLocation location)
        {
            // Get currently near gates
            ISet<Fence> nearGates = new HashSet<Fence>(this.GetGatesNearLocalPlayer(location));

            // Find gates that entered and exited the range
            ISet<Fence> newInRangeGates = new HashSet<Fence>(nearGates);
            ISet<Fence> newOutOfRangeGates = new HashSet<Fence>(this.gatesNearPlayers);
            newInRangeGates.ExceptWith(this.gatesNearPlayers);
            newOutOfRangeGates.ExceptWith(nearGates);

            // Open gates that are in newly range
            foreach (Fence gate in newInRangeGates)
            {
                gate.SetGatePosition(location, Game1.player, true);
            }

            // Close gates that are newly out of range and not near other players
            foreach (Fence gate in newOutOfRangeGates.Where(gate => !this.IsGateNearAnyPlayer(gate, location)))
            {
                gate.SetGatePosition(location, Game1.player, false);

                if (!location.TryGetGate(gate.TileLocation, out Fence _))
                {
                    this.CloseAdjacentFences(location, gate.TileLocation);
                }
            }

            this.gatesNearPlayers = nearGates;
        }

        /// <summary>Closes the fences adjacent to the given position, if no one is near them.</summary>
        /// <param name="location">The location to look in.</param>
        /// <param name="position">The position to search around.</param>
        private void CloseAdjacentFences(GameLocation location, Vector2 position)
        {
            foreach (Vector2 offset in new[] { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1) })
            {
                if (location.TryGetGate(position + offset, out Fence gate) && !this.IsGateNearAnyPlayer(gate, location))
                {
                    gate.SetGatePosition(location, Game1.player, false);
                }
            }
        }

        /// <summary>Gets whether the gate is near any player in the given location.</summary>
        /// <param name="gate">The gate to look for.</param>
        /// <param name="location">The location to look in.</param>
        /// <returns>Whether a player is near the gate.</returns>
        private bool IsGateNearAnyPlayer(Fence gate, GameLocation location)
        {
            return location.farmers.Select(player => new Vector2(player.getTileX(), player.getTileY())).Any(position => this.GetGatesNearPosition(location, position).Contains(gate));
        }

        /// <summary>Gets all gates near the local player.</summary>
        /// <param name="location">The location to look in.</param>
        /// <returns>The gates that were found.</returns>
        private IEnumerable<Fence> GetGatesNearLocalPlayer(GameLocation location)
        {
            return this.GetGatesNearPosition(location, new Vector2(Game1.player.getTileX(), Game1.player.getTileY()));
        }

        /// <summary>Gets all gates near the given position.</summary>
        /// <param name="location">The location to look in.</param>
        /// <param name="position">The position to search at.</param>
        /// <returns>The gates that were found.</returns>
        private IEnumerable<Fence> GetGatesNearPosition(GameLocation location, Vector2 position)
        {
            for (int i = -1 * this.config.GateToggleRadius; i <= this.config.GateToggleRadius; i++)
            {
                for (int j = -1 * this.config.GateToggleRadius; j <= this.config.GateToggleRadius; j++)
                {
                    if (location.TryGetGate(position + new Vector2(i, j), out Fence fence))
                    {
                        yield return fence;
                    }
                }
            }
        }
    }
}
