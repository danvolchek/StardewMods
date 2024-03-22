using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace BetterAutomaticGates.Framework
{
    /// <summary>Gate related extension methods for <see cref="Fence"/> and <see cref="GameLocation"/>.</summary>
    internal static class GateExtensions
    {
        /*********
        ** Public methods
        *********/

        /// <summary>Sets this gate's position.</summary>
        /// <param name="gate">The gate to set the position for.</param>
        /// <param name="location">The location to look for adjacent gates in.</param>
        /// <param name="who">The player changing the gate position.</param>
        /// <param name="open">Whether the gate should be open or not.</param>
        public static void SetGatePosition(this Fence gate, GameLocation location, Farmer who, bool open)
        {
            if (gate.gatePosition.Value == (open ? 0 : 88))
            {
                who.currentLocation.playSound("doorClose");
            }

            gate.gatePosition.Value = open ? 88 : 0;

            if (gate.TryGetAdjacentGate(location, out Fence adjacent))
            {
                adjacent.gatePosition.Value = open ? 88 : 0;
            }
        }

        /// <summary>Gets a gate at the given position.</summary>
        /// <param name="location">The location to look in.</param>
        /// <param name="position">The position to look at.</param>
        /// <param name="gate">The found gate, or null if not found.</param>
        /// <returns>Whether a gate was found or not.</returns>
        public static bool TryGetGate(this GameLocation location, Vector2 position, out Fence gate)
        {
            gate = null;

            //if (location.objects.TryGetValue(position, out SObject obj) && obj is Fence found && found.isGate.Value && found.getDrawSum(location) != 0)
            if (location.objects.TryGetValue(position, out SObject obj) && obj is Fence found && found.isGate.Value && found.getDrawSum() != 0)
            {
                gate = found;
            }

            return gate != null;
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Tries to get this gate's adjacent gate.</summary>
        /// <param name="gate">The gate to look for adjacent gates for.</param>
        /// <param name="location">The location to look in.</param>
        /// <param name="adjacent">The found gate, or null if not found.</param>
        /// <returns>Whether a gate was found or not.</returns>
        private static bool TryGetAdjacentGate(this Fence gate, GameLocation location, out Fence adjacent)
        {
            adjacent = null;
            Vector2 positionOffset = gate.GetAdjacentGatePositionOffset(location);

            //return positionOffset != Vector2.Zero && location.TryGetGate(gate.TileLocation + positionOffset, out adjacent) && GateExtensions.IsOppositeDirection(gate.getDrawSum(location), adjacent.getDrawSum(location));
            return positionOffset != Vector2.Zero && location.TryGetGate(gate.TileLocation + positionOffset, out adjacent) && GateExtensions.IsOppositeDirection(gate.getDrawSum(), adjacent.getDrawSum());
        }

        /// <summary>Gets the position offset of where this gate's adjacent position should be.</summary>
        /// <param name="gate">The gate to get the offset for.</param>
        /// <param name="location">The location to look in.</param>
        /// <returns>The offset, or zero if none was found.</returns>
        private static Vector2 GetAdjacentGatePositionOffset(this Fence gate, GameLocation location)
        {
            //switch (gate.getDrawSum(location))
            switch (gate.getDrawSum())
            {
                case 10:
                    return new Vector2(1, 0);

                case 100:
                    return new Vector2(-1, 0);

                case 500:
                    return new Vector2(0, -1);

                case 1000:
                    return new Vector2(0, 1);

                default:
                    return Vector2.Zero;
            }
        }

        /// <summary>Checks whether two draw sums are facing each other.</summary>
        /// <param name="sum1">The first draw sum.</param>
        /// <param name="sum2">The second draw sum.</param>
        /// <returns>Whether the two draw sums face each other.</returns>
        private static bool IsOppositeDirection(int sum1, int sum2)
        {
            return (sum1 == 10 && sum2 == 100) || (sum2 == 10 && sum1 == 100) || (sum1 == 500 && sum2 == 1000) || (sum2 == 500 && sum1 == 1000);
        }
    }
}
