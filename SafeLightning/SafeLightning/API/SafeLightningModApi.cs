using Microsoft.Xna.Framework;
using static SafeLightning.SafeLightningMod;

namespace SafeLightning.API
{
    /// <summary>
    ///     An API for other mods to let this one know they will create lightning.
    /// </summary>
    public class SafeLightningApi
    {
        internal StrikeLightningDelegate methodToCall;

        public SafeLightningApi(StrikeLightningDelegate methodToCall)
        {
            this.methodToCall = methodToCall;
        }

        /// <summary>
        ///     Method to call when another mod wants to safely create lightning.
        /// </summary>
        /// <param name="position">Where to create the lightning</param>
        /// <param name="effects">Whether to create appropriate sound and visual effects</param>
        public void StrikeLightningSafely(Vector2 position, bool effects = true)
        {
            this.methodToCall(position, effects);
        }
    }
}