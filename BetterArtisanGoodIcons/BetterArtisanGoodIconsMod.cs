using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
			ArtisanGoodsManager.Init(this.Helper, this.Monitor);
			Patches.Init(this.Monitor);

			Harmony harmony = new Harmony(this.ModManifest.UniqueID);

			try
			{
				harmony.Patch(
					original: TryGetMethodInfo(typeof(SObject), nameof(SObject.drawWhenHeld)),
					prefix: new HarmonyMethod(TryGetMethodInfo(typeof(Patches.SObjectPatches), nameof(Patches.SObjectPatches.DrawWhenHeld_Prefix)))
				);
				harmony.Patch(
					original: TryGetMethodInfo(typeof(SObject), nameof(SObject.drawInMenu)),
					prefix: new HarmonyMethod(TryGetMethodInfo(typeof(Patches.SObjectPatches), nameof(Patches.SObjectPatches.DrawInMenu_Prefix)))
				);
				harmony.Patch(
                    // Specify parameter types of `draw` method since `StardewValley.Object` has two of them
                    original: TryGetMethodInfo(typeof(SObject), nameof(SObject.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                    prefix: new HarmonyMethod(TryGetMethodInfo(typeof(Patches.SObjectPatches), nameof(Patches.SObjectPatches.Draw_Prefix)))
                );
                harmony.Patch(
                    original: TryGetMethodInfo(typeof(Furniture), nameof(Furniture.draw)),
                    prefix: new HarmonyMethod(TryGetMethodInfo(typeof(Patches.FurniturePatches), nameof(Patches.FurniturePatches.Draw_Prefix)))
                );
            }
			catch (Exception ex)
			{
                Monitor.Log($"Failed in {nameof(BetterArtisanGoodIconsMod)}.{nameof(Entry)} to register draw methods to patch with Harmony:\n{ex}", LogLevel.Error);
            }
		}

		private static MethodInfo TryGetMethodInfo(Type type, string methodName, Type[] parameterTypes = null)
		{
			MethodInfo methodInfo = parameterTypes == null
				? type.GetMethod(methodName)
				: type.GetMethod(methodName, parameterTypes);

			if (methodInfo == null)
			{
				throw new Exception($"Failed to find method {type.FullName}.{methodName} to patch");
            }

			return methodInfo;
        }
	}
}
