using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterArtisanGoodIcons
{
	/// <summary>Draws different icons for different Artisan Good types.</summary>
	/// <remarks>Honey does not save the original item in <see cref="StardewValley.Object.preservedParentSheetIndex"/> so we have to use its name to determine its type, resulting in
	/// honey and non-honey versions of things.</remarks>
	public class BetterArtisanGoodIconsMod : Mod
	{
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			ArtisanGoodsManager.Init(this);

			Harmony harmony = new Harmony(this.ModManifest.UniqueID);

			try
			{
				// Patch the original `StardewValley` methods with the patch methods we wrote for them.
				// The parameter types of the original methods we are looking for are necessary to avoid `AmbiguousMatchException` errors due to not being specific enough in our search.
				harmony.Patch(
					original: AccessTools.Method(typeof(SObject), nameof(SObject.drawWhenHeld), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(Farmer) }),
					prefix: new HarmonyMethod(typeof(Patches.SObjectPatches), nameof(Patches.SObjectPatches.DrawWhenHeld_Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(SObject), nameof(SObject.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
					prefix: new HarmonyMethod(typeof(Patches.SObjectPatches), nameof(Patches.SObjectPatches.DrawInMenu_Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(SObject), nameof(SObject.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					prefix: new HarmonyMethod(typeof(Patches.SObjectPatches), nameof(Patches.SObjectPatches.Draw_Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					prefix: new HarmonyMethod(typeof(Patches.FurniturePatches), nameof(Patches.FurniturePatches.Draw_Prefix))
				);
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(BetterArtisanGoodIconsMod)}.{nameof(Entry)} to register draw methods to patch with Harmony:\n{ex}", LogLevel.Error);
            }
		}
	}
}
