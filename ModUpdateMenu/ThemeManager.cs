using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Leclair.Stardew.ThemeManager
{

	/// <summary>
	/// BaseThemeData represents the barest possible set of theme data, only
	/// including properties that ThemeManager itself uses.
	///
	/// You should subclass BaseThemeData for your own mod, adding any
	/// appropriate values that you would like to be controllable via theme.
	/// </summary>
	public class BaseThemeData
	{
		/// <summary>
		/// LocalizedNames is a mapping of locale codes to human-readable theme
		/// names suitable for display in a theme selector.
		/// </summary>
		public Dictionary<string, string> LocalizedNames { get; set; }

		/// <summary>
		/// For is a list of mod UniqueIDs that this theme is meant to support.
		/// When the current theme is set to Automatic, this theme will be
		/// selected if one of the mods listed is loaded.
		/// </summary>
		public string[] For { get; set; }

		/// <summary>
		/// AssetPrefix is prepended to all asset file paths when loading
		/// assets from this theme.
		/// </summary>
		public string AssetPrefix { get; set; } = "assets";

		/// <summary>
		/// Check to see if any loaded mods are in this theme's For block.
		/// </summary>
		/// <param name="registry">Your own mod's IModRegistry helper</param>
		/// <returns>true if there are any matching mods</returns>
		public bool HasMatchingMod(IModRegistry registry)
		{
			if (For != null)
				foreach (string mod in For)
				{
					if (!string.IsNullOrEmpty(mod) && registry.IsLoaded(mod))
						return true;
				}

			return false;
		}
	}

	/// <summary>
	/// This event is emitted by ThemeManager whenever the current theme
	/// changes, whether because the themes were reload or because the user
	/// selected a different theme.
	/// </summary>
	/// <typeparam name="DataT">Your mod's BaseThemeData subclass</typeparam>
	public class ThemeChangedEventArgs<DataT> : EventArgs where DataT : BaseThemeData
	{

		/// <summary>
		/// The theme ID of the old theme
		/// </summary>
		public string OldId;
		/// <summary>
		/// The theme ID of the new theme.
		/// </summary>
		public string NewId;

		/// <summary>
		/// The theme data of the old theme
		/// </summary>
		public DataT OldData;
		/// <summary>
		/// The theme data of the new theme
		/// </summary>
		public DataT NewData;

		public ThemeChangedEventArgs(string oldId, DataT oldData, string newID, DataT newData)
		{
			OldId = oldId;
			NewId = newID;
			OldData = oldData;
			NewData = newData;
		}
	}

	/// <summary>
	/// SimpleManifest is a basic class with manifest properties we support
	/// loading from theme.json files when reading themes from your mod's
	/// "assets/themes" directory.
	///
	/// These values, if present, are used when constructing the temporary
	/// content pack. None of these are required.
	/// </summary>
	internal class SimpleManifest
	{
		public string UniqueID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Author { get; set; }
		public string Version { get; set; }
	}

	/// <summary>
	/// ThemeManager is a standalone class for adding theme support to
	/// Stardew Valley mods written in C#. It handles discovery, selection,
	/// asset loading with Content Patcher support, and emitting an event
	/// when the theme changes.
	///
	/// Ideally, this should be a drop in replacement for most mods, only
	/// requiring them to replace their <see cref="IContentHelper.Load{T}(string, ContentSource)"/>
	/// calls with <see cref="Load{T}(string)"/>.
	/// </summary>
	/// <typeparam name="DataT">Your mod's BaseThemeData subclass</typeparam>
	public class ThemeManager<DataT> where DataT : BaseThemeData
	{

		public static readonly SemanticVersion Version = new("1.1.1-assetloader");

		public static readonly string ContentPatcher_UniqueID = "Pathoschild.ContentPatcher";

		#region Internal Fields

		/// <summary>
		/// Your mod. We use the manifest, helper, and monitor frequently.
		/// </summary>
		private readonly Mod Mod;

		/// <summary>
		/// A dictionary mapping all known themes to their own UniqueIDs.
		/// </summary>
		private readonly Dictionary<string, Tuple<DataT, IContentPack>> Themes = new();

		/// <summary>
		/// Storage of the <see cref="DefaultTheme"/>. This should not be accessed directly,
		/// instead prefering the use of <see cref="DefaultTheme"/> (which is public).
		/// </summary>
		private DataT _DefaultTheme;

		/// <summary>
		/// The currently active theme. We store this directly to avoid needing
		/// constant dictionary lookups when working with a theme.
		/// </summary>
		private Tuple<DataT, IContentPack> BaseThemeData = null;

		#endregion

		#region Public Fields

		/// <summary>
		/// The Loader is the private IAssetLoader we use for redirecting asset
		/// loading through game content to allow Content Patcher to work.
		/// </summary>
		public readonly ThemeAssetLoader Loader;

		/// <summary>
		/// The AssetLoaderPrefix is prepended to asset names when redirecting
		/// asset loading through game content to allow Content Patcher access
		/// to your mod's themed resources.
		/// </summary>
		public readonly string AssetLoaderPrefix;

		/// <summary>
		/// Whether or not we are using IAssetLoader to load assets in a way
		/// that Content Patcher can intercept.
		/// </summary>
		public readonly bool UsingAssetRedirection;

		/// <summary>
		/// The EmbeddedThemesPath is the relative path to where your mod keeps
		/// its embedded themes. By default, this is <c>assets/themes</c>.
		/// </summary>
		public readonly string EmbeddedThemesPath;

		#endregion

		#region Constructor

		/// <summary>
		/// Create a new ThemeManager.
		/// </summary>
		/// <param name="mod">Your mod's Mod class. We need access to your
		/// mod's ModManifest, Helper, and Monitor.</param>
		/// <param name="selectedThemeId">The initial selected theme ID. When
		/// Discover is run, this value will be used to immediately select the
		/// desired theme. <seealso cref="SelectedThemeId"/></param>
		/// <param name="defaultTheme">The <typeparamref name="DataT"/> instance
		/// to use when the default theme is selected.
		/// <seealso cref="DefaultTheme"/></param>
		/// <param name="embeddedThemesPath">The path to search your mod for
		/// embedded themes at. <seealso cref="EmbeddedThemesPath"/></param>
		/// <param name="assetPrefix">The asset prefix to use when
		/// the default theme is selected.
		/// <seealso cref="DefaultAssetPrefix"/></param>
		/// <param name="assetLoaderPrefix">A prefix prepended to all asset
		/// paths when redirecting asset loading through IAssetLoader. By
		/// default, this value is <c>Mods/{yourMod.UniqueId}/Themes</c>.
		/// <seealso cref="AssetLoaderPrefix"/></param>
		/// <param name="forceAssetRedirection">If set to true, we will use
		/// redirected asset loading even if Content Patcher is not
		/// present in the current environment.
		/// <seealso cref="UsingAssetRedirection"/></param>
		public ThemeManager(
			Mod mod,
			string selectedThemeId = "automatic",
			DataT defaultTheme = null,
			string embeddedThemesPath = "assets/themes",
			string assetPrefix = "assets",
			string assetLoaderPrefix = null,
			bool forceAssetRedirection = false
		)
		{
			// Store the basic initial values.
			Mod = mod;
			SelectedThemeId = selectedThemeId;
			_DefaultTheme = defaultTheme;
			DefaultAssetPrefix = assetPrefix;
			EmbeddedThemesPath = embeddedThemesPath;

			// Log our version.
			Log($"Using Theme Manager {Version}");

			// Detect Content Patcher
			bool hasCP = Mod.Helper.ModRegistry.IsLoaded(ContentPatcher_UniqueID);
			if (hasCP)
				Log("Content Patcher is present. Redirecting resource loading through game content to support patching.");
			else if (forceAssetRedirection)
				Log("Content Patcher is NOT present. However, we are still redirecting asset loading due to manual override.");

			UsingAssetRedirection = hasCP || forceAssetRedirection;

			// Always run the AssetLoaderPrefix through NormalizeAssetName,
			// otherwise we'll run into issues actually using our custom
			// asset loader.
			AssetLoaderPrefix = PathUtilities.NormalizeAssetName(
				string.IsNullOrEmpty(assetLoaderPrefix) ?
					Path.Join("Mods", mod.ModManifest.UniqueID, "Themes") :
					assetLoaderPrefix
				);

			// Register this class as an asset loader.
			Loader = new ThemeAssetLoader(this);
			Mod.Helper.Content.AssetLoaders.Add(Loader);
		}

		#endregion

		#region Properties

		/// <summary>
		/// An instance of <typeparamref name="DataT"/> for use when no
		/// specific theme is loaded. When assigning a new object to this
		/// property, a theme changed event may be emitted if the default
		/// theme is the active theme.
		/// </summary>
		public DataT DefaultTheme
		{
			get => _DefaultTheme;
			set
			{
				bool is_default = ActiveThemeId == "default";
				DataT oldData = Theme;
				_DefaultTheme = value;
				if (is_default)
					ThemeChanged?.Invoke(this, new ThemeChangedEventArgs<DataT>("default", oldData, "default", _DefaultTheme));
			}
		}

		/// <summary>
		/// The I18nPrefix is prepended to translation keys generated by
		/// ThemeManager. Currently, the only keys ThemeManager generates are
		/// for the Automatic and Default themes.
		/// </summary>
		public string I18nPrefix { get; set; } = "theme";

		/// <summary>
		/// The currently selected theme's ID. This value may be <c>automatic</c>,
		/// and thus should not be used for checking which theme is active.
		/// </summary>
		public string SelectedThemeId { get; private set; } = null;

		/// <summary>
		/// The currently active theme's ID. This value will never be
		/// <c>automatic</c>. It may be <c>default</c>, or the unique ID of a theme.
		/// </summary>
		public string ActiveThemeId { get; private set; } = null;

		/// <summary>
		/// The currently active theme's <typeparamref name="DataT"/> instance.
		/// If the active theme is <c>default</c>, then this will be the value
		/// of <see cref="DefaultTheme"/>. This value may be null if there is
		/// no default theme.
		/// </summary>
		public DataT Theme => BaseThemeData?.Item1 ?? _DefaultTheme;

		/// <summary>
		/// This event is fired whenever the currently active theme changes,
		/// which can happen either when themes are reloaded or when the user
		/// changes their selected theme.
		/// </summary>
		public event EventHandler<ThemeChangedEventArgs<DataT>> ThemeChanged;

		/// <summary>
		/// The DefaultAssetPrefix is prepended to asset paths when loading
		/// assets from the default theme. This prefix is not added to paths
		/// when loading assets from other themes, as themes have their own
		/// AssetPrefix to use.
		/// </summary>
		public string DefaultAssetPrefix { get; private set; }

		#endregion

		#region Logging

		private void Log(string message, LogLevel level = LogLevel.Trace, Exception ex = null, LogLevel? exLevel = null)
		{
			Mod.Monitor.Log($"[ThemeManager] {message}", level: level);
			if (ex != null)
				Mod.Monitor.Log($"[ThemeManager] Details:\n{ex}", level: exLevel ?? level);
		}

		#endregion

		#region Console Commands

		/// <summary>
		/// This method is designed for use as a console command. Using it
		/// invalidates all of our loaded resources and repeats theme
		/// discovery. A <see cref="ThemeChanged"/> event will be dispatched,
		/// and a message will be logged to the console notifying the user
		/// that the themes were reloaded.
		/// </summary>
		public void PerformReloadCommand()
		{
			Invalidate();
			Discover();
			Log($"Reloaded themes. You may need to reopen menus.", LogLevel.Info);
		}

		[SuppressMessage("Style", "IDE0060", Justification = "Provided for ease of use with SMAPI's API.")]
		public void PerformReloadCommand(string name, string[] args)
		{
			PerformReloadCommand();
		}

		/// <summary>
		/// This method is designed for use as a console command. This
		/// command logs the available, selected, and active themes as
		/// well as allowing you to easily change the current theme or
		/// to reload themes.
		/// </summary>
		/// <param name="args">Arguments passed to the console command</param>
		/// <returns>True if the <see cref="SelectedThemeId"/> changed as a
		/// result of this command. In such an event, you may wish to update
		/// the user's config with their selection.</returns>
		public bool PerformThemeCommand(string[] args)
		{
			if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
			{
				string key = args[0].Trim();

				// Check for reload first.
				if (key.Equals("reload", StringComparison.OrdinalIgnoreCase))
				{
					PerformReloadCommand();
					return false;
				}

				string selected = null;
				var themes = GetThemeChoices();

				// Check for unique ID matches first.
				foreach (var pair in themes)
				{
					if (pair.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
					{
						selected = pair.Key;
						break;
					}
				}

				// Then check for unique ID partial matches.
				if (selected == null)
					foreach (var pair in themes)
					{
						if (pair.Key.Contains(key, StringComparison.OrdinalIgnoreCase))
						{
							selected = pair.Key;
							break;
						}
					}

				// Then select for display name partial matches.
				if (selected == null)
					foreach (var pair in themes)
					{
						if (GetThemeName(pair.Key).Contains(key, StringComparison.OrdinalIgnoreCase))
						{
							selected = pair.Key;
							break;
						}
					}

				// If we've selected something, actually select it.
				if (selected != null)
				{
					SelectTheme(selected);
					Log($"Selected Theme: {selected}", LogLevel.Info);
					return true;
				}

				Log($"Unable to match theme: {key}", LogLevel.Warn);
				return false;
			}

			// If we've gotten here, we should log a list of all available
			// themes for the user's benefit.
			Log($"Available Themes:", LogLevel.Info);
			foreach (var pair in GetThemeChoices())
			{
				bool selected = pair.Key == SelectedThemeId;
				bool used = pair.Key == ActiveThemeId;

				string selection = selected ?
					(used ? "=>" : " >") :
					(used ? "= " : "  ");

				Log($" {selection} [{pair.Key}]: {pair.Value}", LogLevel.Info);
			}

			return false;
		}

		#endregion

		#region Theme Data Access

		/// <summary>
		/// Get the human readable name of a theme, optionally in a given
		/// locale. If no locale is given, the current locale will be read
		/// from your mod's translation helper.
		/// </summary>
		/// <param name="themeId">The ID of theme to get the name for</param>
		/// <param name="locale">The locale to get the name in, if it exists.</param>
		public string GetThemeName(string themeId, string locale = null)
		{
			// For the default theme, return a translation from the host mod's
			// translation layer.
			if (themeId.Equals("default", StringComparison.OrdinalIgnoreCase))
				return Mod.Helper.Translation.Get($"{I18nPrefix}.default").ToString();

			// Get the theme data. If the theme is the active theme, don't
			// bother with a dictionary lookp.
			Tuple<DataT, IContentPack> theme;
			if (themeId == ActiveThemeId)
				theme = BaseThemeData;
			else
			{
				lock ((Themes as ICollection).SyncRoot)
				{
					if (!Themes.TryGetValue(themeId, out theme))
						return themeId;
				}
			}

			// Check for the translation in our theme data.
			if (string.IsNullOrEmpty(locale))
				locale = Mod.Helper.Translation.Locale;

			if (theme.Item1.LocalizedNames?.TryGetValue(locale, out string name) ?? false)
				return name;

			// Manifest Name
			return theme.Item2.Manifest.Name;
		}

		/// <summary>
		/// Get an enumeration of the available themes, suitable for display in
		/// a configuration menu such as Generic Mod Config Menu. This list will
		/// always include an <c>automatic</c> and <c>default</c> entry.
		///
		/// The keys are theme IDs, and the values are human readable names in
		/// the current locale.
		/// </summary>
		/// <param name="locale">The locale to get names for</param>
		/// <returns>An enumeration of available themes</returns>
		public Dictionary<string, string> GetThemeChoices(string locale = null)
		{
			Dictionary<string, string> result = new();

			result.Add("automatic", Mod.Helper.Translation.Get($"{I18nPrefix}.automatic"));
			result.Add("default", Mod.Helper.Translation.Get($"{I18nPrefix}.default"));

			foreach (string theme in Themes.Keys)
				result.Add(theme, GetThemeName(theme, locale));

			return result;
		}

		/// <summary>
		/// Get an enumeration of the available themes, suitable for display in
		/// a configuration menu such as Generic Mod Config Menu. This list will
		/// always include an <c>automatic</c> and <c>default</c> entry.
		///
		/// The keys are theme IDs, and the values are methods that, when called,
		/// return a human readable name for the theme (in the current locale is
		/// a translation is available).
		/// </summary>
		/// <returns>An enumeration of available themes</returns>
		public Dictionary<string, Func<string>> GetThemeChoiceMethods()
		{
			Dictionary<string, Func<string>> result = new();

			result.Add("automatic", () => Mod.Helper.Translation.Get($"{I18nPrefix}.automatic"));
			result.Add("default", () => Mod.Helper.Translation.Get($"{I18nPrefix}.default"));

			foreach (string theme in Themes.Keys)
				result.Add(theme, () => GetThemeName(theme));

			return result;
		}

		#endregion

		#region Theme Discovery

		/// <summary>
		/// Perform theme discovery, reloading all theme data, and then update
		/// the active theme.
		/// </summary>
		/// <param name="checkOwned">Whether or not to load themes from owned content packs</param>
		/// <param name="checkEmbedded">Whether or not to load embedded themes.</param>
		/// <param name="checkExternal">Whether or not to load external themes from other mods.</param>
		/// <returns>The ThemeManager</returns>
		public ThemeManager<DataT> Discover(
			bool checkOwned = true,
			bool checkEmbedded = true,
			bool checkExternal = true
		)
		{
			lock ((Themes as ICollection).SyncRoot)
			{
				// Start by wiping the existing theme data.
				Themes.Clear();

				// We want to keep track of packs with custom IDs so that we
				// can use better IDs for embedded packs.
				Dictionary<string, Tuple<string, IContentPack>> packsWithIds = new();

				// If we haven't been forbidden, check for embedded themes and
				// add them to our packs. We do this first so that any content
				// pack themes can override our embedded themes, if they really
				// need to. They shouldn't, however.
				if (checkEmbedded)
				{
					var embedded = FindEmbeddedThemes();
					if (embedded != null)
						foreach (var cp in embedded)
							packsWithIds[cp.Key] = new(null, cp.Value);
				}

				// Now, check for your mod's owned content packs.
				if (checkOwned)
				{
					var owned = Mod.Helper.ContentPacks.GetOwned();
					if (owned != null)
						foreach (var cp in owned)
							packsWithIds[cp.Manifest.UniqueID] = new(null, cp);
				}

				// Finally, check for external mods that provide themes.
				if (checkExternal)
				{
					var external = FindExternalThemes();
					if (external != null)
						foreach (var cp in external)
							packsWithIds[cp.Key] = cp.Value;
				}

				// Now, load each of our packs.
				foreach (var cp in packsWithIds)
				{
					string file = string.IsNullOrEmpty(cp.Value.Item1) ? "theme.json" : cp.Value.Item1;
					if (!cp.Value.Item2.HasFile(file))
						continue;

					DataT data;
					try
					{
						data = cp.Value.Item2.ReadJsonFile<DataT>(file);
						if (data is null)
							throw new Exception("theme.json cannot be null");
					}
					catch (Exception ex)
					{
						Log($"The content pack {cp.Value.Item2.Manifest.Name} has an invalid theme json file and could not be loaded.", LogLevel.Warn, ex);
						continue;
					}

					Themes[cp.Key] = new(data, cp.Value.Item2);
				}
			}

			// Store our currently selected theme.
			string oldKey = SelectedThemeId;

			// Clear our data.
			BaseThemeData = null;
			SelectedThemeId = null;
			ActiveThemeId = null;

			// And select the new theme.
			SelectTheme(oldKey);
			return this;
		}

		/// <summary>
		/// Search for loose themes in your mod's embedded themes folder.
		/// </summary>
		/// <returns>A dictionary of temporary IContentPacks for each embedded theme.</returns>
		private Dictionary<string, IContentPack> FindEmbeddedThemes()
		{
			if (string.IsNullOrEmpty(EmbeddedThemesPath))
				return null;

			string path = Path.Join(Mod.Helper.DirectoryPath, PathUtilities.NormalizePath(EmbeddedThemesPath));
			if (!Directory.Exists(path))
				return null;

			Dictionary<string, IContentPack> results = new();
			int count = 0;

			// Start iterating subdirectories of our embedded themes folder.
			foreach (string dir in Directory.GetDirectories(path))
			{
				string man_path = Path.Join(dir, "manifest.json");
				string t_path = Path.Join(dir, "theme.json");

				// If the subdirectory has no theme.json, ignore it.
				if (!File.Exists(t_path))
					continue;

				string rel_path = Path.GetRelativePath(Mod.Helper.DirectoryPath, dir);
				string folder = Path.GetRelativePath(path, dir);

				Log($"Found Embedded Theme At: {dir}", LogLevel.Trace);

				SimpleManifest manifest = null;
				try
				{
					if (File.Exists(man_path))
						manifest = Mod.Helper.Data.ReadJsonFile<SimpleManifest>(
							Path.Join(rel_path, "manifest.json"));
					else
						manifest = Mod.Helper.Data.ReadJsonFile<SimpleManifest>(
							Path.Join(rel_path, "theme.json"));

					if (manifest is null)
						throw new Exception("manifest is empty or invalid");
				}
				catch (Exception ex)
				{
					Log($"Unable to read embedded theme manifest.", LogLevel.Warn, ex);
					continue;
				}

				// TODO: Slugify the folder name.

				var cp = Mod.Helper.ContentPacks.CreateTemporary(
					directoryPath: dir,
					id: manifest.UniqueID ?? $"{Mod.ModManifest.UniqueID}.theme.{folder}",
					name: manifest.Name,
					description: manifest.Description ?? $"{Mod.ModManifest.Name} Theme: {manifest.Name}",
					author: manifest.Author ?? Mod.ModManifest.Author,
					version: new SemanticVersion(manifest.Version ?? "1.0.0")
				);

				results[manifest.UniqueID ?? folder] = cp;
				count++;

				Log($"Found Embedded Theme: {cp.Manifest.Name} by {cp.Manifest.Author} ({cp.Manifest.UniqueID})", LogLevel.Trace);
			}

			Log($"Found {count} Embedded Themes.", LogLevel.Trace);
			return results;
		}

		/// <summary>
		/// Search for themes in other mods' manifests.
		/// </summary>
		/// <returns>A dictionary of temporary IContentPacks for each discovered theme.</returns>
		private Dictionary<string, Tuple<string, IContentPack>> FindExternalThemes()
		{
			Dictionary<string, Tuple<string, IContentPack>> results = new();
			int count = 0;

			string themeKey = $"{Mod.ModManifest.UniqueID}:theme";
			string themeFile = $"{Mod.ModManifest.UniqueID}.theme.json";

			foreach (var mod in Mod.Helper.ModRegistry.GetAll())
			{
				// For every mod, try reading a special value from its manifest.
				if (mod?.Manifest?.ExtraFields == null)
					continue;

				if (!mod.Manifest.ExtraFields.TryGetValue(themeKey, out object value))
					continue;

				string file = themeFile;

				// If the value is a boolean, and false for some reason,
				// just skip the mod. If it's true, assume the default
				// filename for our theme JSON.
				if (value is bool bv)
				{
					if (!bv)
						continue;

					// If the value is a string, use that string as a relative
					// filename for the theme.json file, relative to that mod's
					// directory root.
				}
				else if (value is string str)
				{
					file = str;

					// Display a warning for any other value, as we can only handle
					// strings and booleans.
				}
				else
				{
					Log($"Unknown or unsupported value for {themeKey} in mod {mod.Manifest.Name} ({mod.Manifest.UniqueID})", LogLevel.Warn);
					continue;
				}

				// We need to know the root path of this other mod.
				string root;
				try
				{
					if (mod.GetType().GetProperty("DirectoryPath", BindingFlags.Instance | BindingFlags.Public)?.GetValue(mod) is string str)
						root = str;
					else
						throw new ArgumentException("DirectoryPath");

				}
				catch (Exception)
				{
					Log("Unable to get mod directories from SMAPI internals. Disabling theme detection in other mods.", LogLevel.Warn);
					break;
				}

				// Get the full path to the theme file.
				string full_file = Path.Join(root, PathUtilities.NormalizePath(file));

				// Does the file exist?
				if (!File.Exists(full_file))
				{
					Log($"Unable to find {file} in mod {mod.Manifest.Name} ({mod.Manifest.UniqueID})", LogLevel.Warn);
					continue;
				}

				// ... and get the file again, from the joined path. We don't
				// want any of the directory part.
				file = Path.GetFileName(full_file);

				// Build a temporary content pack for just our theme within
				// this other mod.
				var cp = Mod.Helper.ContentPacks.CreateTemporary(
					directoryPath: Path.GetDirectoryName(full_file),
					id: $"{Mod.ModManifest.UniqueID}.theme.{mod.Manifest.UniqueID}",
					name: mod.Manifest.Name,
					description: mod.Manifest.Description,
					author: mod.Manifest.Author,
					version: mod.Manifest.Version
				);

				results[mod.Manifest.UniqueID] = new(file, cp);
				count++;
			}

			Log($"Found {count} External Themes");
			return results;
		}

		#endregion

		#region Select Theme

		/// <summary>
		/// Select a new theme, and possibly emit a <see cref="ThemeChanged"/>
		/// event if doing so has changed the active theme.
		/// </summary>
		/// <param name="themeId">The ID of the theme to select</param>
		public void SelectTheme(string themeId)
		{
			if (string.IsNullOrEmpty(themeId))
				themeId = "automatic";

			string old_active = ActiveThemeId;
			var old_data = BaseThemeData;

			// Deal with the default theme quickly.
			if (themeId.Equals("default", StringComparison.OrdinalIgnoreCase))
			{
				BaseThemeData = null;
				SelectedThemeId = "default";
				ActiveThemeId = "default";
			}

			// Does this string match something?
			else if (!themeId.Equals("automatic", StringComparison.OrdinalIgnoreCase) && Themes.TryGetValue(themeId, out var theme))
			{
				BaseThemeData = theme;
				SelectedThemeId = themeId;
				ActiveThemeId = themeId;
			}

			// Determine the best theme
			else
			{
				ActiveThemeId = "default";
				BaseThemeData = null;

				string[] ids = Themes.Keys.ToArray();
				for (int i = ids.Length - 1; i >= 0; i--)
				{
					if (!Themes.TryGetValue(ids[i], out var themeData))
						continue;

					if (themeData.Item1?.HasMatchingMod(Mod.Helper.ModRegistry) ?? false)
					{
						BaseThemeData = themeData;
						ActiveThemeId = ids[i];
						break;
					}
				}

				SelectedThemeId = "automatic";
			}

			Log($"Selected Theme: {SelectedThemeId} => {GetThemeName(ActiveThemeId)} ({ActiveThemeId})", LogLevel.Trace);

			// Did the active theme actually change?
			if (ActiveThemeId != old_active)
			{
				// Invalidate old resources to kick them out of memory when
				// we're no longer using that theme.
				if (old_active != null)
					Invalidate(old_active);

				// And emit our event.
				ThemeChanged?.Invoke(this, new(old_active, old_data?.Item1, ActiveThemeId, BaseThemeData?.Item1));
			}
		}

		#endregion

		#region Resource Loading

		/// <summary>
		/// Invalidate all content files that we provide via
		/// <see cref="IAssetLoader"/>.
		/// </summary>
		/// <param name="themeId">An optional theme ID to only clear tha
		/// theme's assets.</param>
		public void Invalidate(string themeId = null)
		{
			string key = AssetLoaderPrefix;
			if (!string.IsNullOrEmpty(themeId))
				key = PathUtilities.NormalizeAssetName(Path.Join(AssetLoaderPrefix, themeId));

			Mod.Helper.Content.InvalidateCache(info => info.AssetName.StartsWith(key));
		}

		/// <summary>
		/// Load content from a theme (if not already cached), and return it.
		/// Depending on the asset redirection status and whether or not the
		/// requested file is present in the active theme, this method may
		/// use <see cref="IContentPack.LoadAsset{T}(string)"/> or
		/// <see cref="IContentHelper.Load{T}(string, ContentSource)"/>.
		/// </summary>
		/// <typeparam name="T">The expected data type.</typeparam>
		/// <param name="path">The relative file path.</param>
		/// <exception cref="System.ArgumentException">The <paramref name="path"/> is empty or contains invalid characters.</exception>
		/// <exception cref="Microsoft.Xna.Framework.Content.ContentLoadException">The content asset couldn't be loaded (e.g. because it doesn't exist).</exception>
		public T Load<T>(string path)
		{
			// Just go straight to InternalLoad if we're not redirecting this
			// asset in some way. We only redirect if redirection is enabled
			// and this isn't the default theme.
			if (!UsingAssetRedirection || BaseThemeData == null)
				return InternalLoad<T>(path);

			path = PathUtilities.NormalizeAssetName(Path.Join(AssetLoaderPrefix, ActiveThemeId, path));
			return Mod.Helper.Content.Load<T>(path, ContentSource.GameContent);
		}

		/// <summary>
		/// Get whether a given file exists in the active theme or in the
		/// default theme (aka your mod's assets directory).
		/// </summary>
		/// <param name="path">The relative file path.</param>
		public bool HasFile(string path)
		{
			// Check the current active theme.
			if (BaseThemeData != null)
			{
				string lpath = path;
				if (!string.IsNullOrEmpty(BaseThemeData.Item1.AssetPrefix))
					lpath = Path.Join(BaseThemeData.Item1.AssetPrefix, lpath);

				if (BaseThemeData.Item2.HasFile(lpath))
					return true;
			}

			// Now check the default theme.
			if (!string.IsNullOrEmpty(DefaultAssetPrefix))
				path = Path.Join(DefaultAssetPrefix, path);

			string full = Path.Join(Mod.Helper.DirectoryPath, path);
			return File.Exists(full);
		}

		/// <summary>
		/// Actually load the asset, either from the active theme or from the
		/// default theme (aka your mod's assets directory).
		/// </summary>
		/// <typeparam name="T">The expected data type.</typeparam>
		/// <param name="path">The relative file path.</param>
		/// <exception cref="System.ArgumentException">The key is empty or contains invalid characters.</exception>
		/// <exception cref="Microsoft.Xna.Framework.Content.ContentLoadException">The content asset couldn't be loaded (e.g. because it doesn't exist).</exception>
		private T InternalLoad<T>(string path)
		{
			// Check the current active theme
			if (BaseThemeData != null)
			{
				string lpath = path;
				if (!string.IsNullOrEmpty(BaseThemeData.Item1.AssetPrefix))
					lpath = Path.Join(BaseThemeData.Item1.AssetPrefix, lpath);

				if (BaseThemeData.Item2.HasFile(lpath))
					try
					{
						return BaseThemeData.Item2.LoadAsset<T>(lpath);
					}
					catch (Exception ex)
					{
						Log($"Failed to load asset \"{path}\" from theme {ActiveThemeId}.", LogLevel.Warn, ex);
					}
			}

			// Now fallback to the default theme
			if (!string.IsNullOrEmpty(DefaultAssetPrefix))
				path = Path.Join(DefaultAssetPrefix, path);

			return Mod.Helper.Content.Load<T>(path);
		}

		#endregion

		#region IAssetLoader Implementation

		/// <summary>
		/// ThemeAssetLoader is a simple <see cref="IAssetLoader"/> used to
		/// load assets from a theme as a game asset, thus allowing
		/// Content Patcher to modify the file.
		/// </summary>
		public class ThemeAssetLoader : IAssetLoader
		{

			private readonly ThemeManager<DataT> Manager;

			public ThemeAssetLoader(ThemeManager<DataT> manager)
			{
				Manager = manager;
			}

			public bool CanLoad<T>(IAssetInfo asset)
			{
				// We can only load our own assets.
				if (!asset.AssetName.StartsWith(Manager.AssetLoaderPrefix))
					return false;

				string path = Path.GetRelativePath(Manager.AssetLoaderPrefix, asset.AssetName);

				// We only care about the currently used theme. There's no need to
				// load assets from inactive themes.
				if (!path.StartsWith(Manager.ActiveThemeId))
					return false;

				path = Path.GetRelativePath(Manager.ActiveThemeId, path);
				return Manager.HasFile(path);
			}

			public T Load<T>(IAssetInfo asset)
			{
				// We can only load our own assets.
				if (!asset.AssetName.StartsWith(Manager.AssetLoaderPrefix))
					return (T)(object)null;

				string path = Path.GetRelativePath(Manager.AssetLoaderPrefix, asset.AssetName);

				// We only care about the currently used theme. There's no need to
				// load assets from inactive themes.
				if (!path.StartsWith(Manager.ActiveThemeId))
					return (T)(object)null;

				path = Path.GetRelativePath(Manager.ActiveThemeId, path);
				return Manager.InternalLoad<T>(path);
			}
		}

		#endregion
	}
}
