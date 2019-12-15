using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace HatsOnCats.Framework
{
    internal class HatPatcher
    {
        private readonly HarmonyInstance harmony;
        private readonly IMonitor monitor;

        private static Character current;

        public HatPatcher(IMonitor monitor, HarmonyInstance harmony)
        {
            this.monitor = monitor;
            this.harmony = harmony;
        }
        private static void CharacterDrawPatch(Character __instance)
        {
            current = __instance;
        }

        private static void SpriteBatchDrawPatch(SpriteBatch __instance, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, float layerDepth, Vector2 origin, float scale, SpriteEffects effects, float rotation)
        {
            if (current == null)
            {
                return;
            }

            if (current.Sprite.Texture == texture)
            {
                ModEntry.Instance.DrawHats(current, __instance, position, sourceRectangle, origin, rotation, effects, layerDepth);
            }
        }


        public void Patch(IEnumerable<Type> types)
        {
            foreach (Type t in types)
            {
                this.Patch(t);
            }

            Type[] args = {typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float)};
            MethodInfo postfix = this.GetType().GetMethod(nameof(this.SpriteBatchDrawPatch), BindingFlags.NonPublic | BindingFlags.Static);
            this.PatchDraw(typeof(SpriteBatch), typeof(SpriteBatch).GetMethod(nameof(SpriteBatch.Draw), args), postfix, false);
        }

        private void Patch(Type t)
        {
            string name = nameof(Character.draw);

            MethodInfo original = t.GetMethod(name, new[] { typeof(SpriteBatch) });
            MethodInfo prefix = this.GetType().GetMethod(nameof(this.CharacterDrawPatch), BindingFlags.NonPublic | BindingFlags.Static);
            this.PatchDraw(t, original, prefix);

            original = t.GetMethod(name, new[] { typeof(SpriteBatch), typeof(float) });
            this.PatchDraw(t, original, prefix);
        

            original = t.GetMethod(name, new[] { typeof(SpriteBatch), typeof(int), typeof(float) });
            this.PatchDraw(t, original, prefix);

            original = t.GetMethod(nameof(Character.drawAboveAlwaysFrontLayer), new[] { typeof(SpriteBatch)});
            this.PatchDraw(t, original, prefix);

            if (typeof(Monster).IsAssignableFrom(t))
            {
                original = t.GetMethod(nameof(Monster.drawAboveAllLayers), new[] { typeof(SpriteBatch) });
                this.PatchDraw(t, original, prefix);
            }
        }

        private void PatchDraw(Type t, MethodInfo original, MethodInfo patch, bool prefix=true)
        {
            this.monitor.Log($"Patching {t.Name}.{original.Name} with {(prefix ? "prefix":"postfix")} {typeof(HatPatcher).Name}.{patch.Name}", LogLevel.Trace);
            if (prefix)
            {
                this.harmony.Patch(original, prefix: new HarmonyMethod(patch));
            }
            else
            {
                this.harmony.Patch(original, postfix: new HarmonyMethod(patch));
            }
        }
    }
}
