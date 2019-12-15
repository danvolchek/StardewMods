using StardewModdingAPI;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Toolkit.Framework.Clients.WebApi;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SemanticVersion = StardewModdingAPI.SemanticVersion;

namespace ModUpdateMenu.Updates
{
    /// <summary>Retrieves update statuses.</summary>
    internal class UpdateStatusRetriever
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod helper.</summary>
        private readonly IModHelper helper;

        private readonly ModToolkit toolkit;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="helper">The mod helper.</param>
        public UpdateStatusRetriever(IModHelper helper)
        {
            this.helper = helper;
            this.toolkit = new ModToolkit();
        }

        /// <summary>Gets the SMAPI update version.</summary>
        /// <returns>The SMAPI update version.</returns>
        public ISemanticVersion GetSMAPIUpdateVersion()
        {
            string updateMarker = (string)typeof(Constants).GetProperty("UpdateMarker", BindingFlags.Static | BindingFlags.NonPublic).GetValue(typeof(Constants));

            //If there's no update marker, SMAPI is up to date
            //(Or there was an error, but we'd need to intercept console output to determine that)
            if (!File.Exists(updateMarker))
                return Constants.ApiVersion;

            //If there is an update marker, there is a SMAPI update
            string rawUpdate = File.ReadAllText(updateMarker);

            return SemanticVersion.TryParse(rawUpdate, out ISemanticVersion updateFound) ? updateFound : null;
        }

        /// <summary>Gets the update status of all mods.</summary>
        /// <param name="statuses">All mod statuses.</param>
        /// <returns>Whether any non skipped statuses were added.</returns>
        public bool GetUpdateStatuses(out IList<ModStatus> statuses)
        {
            statuses = new List<ModStatus>();

            bool addedNonSkippedStatus = false;

            foreach (object modMetaData in GetInstanceField<IEnumerable<object>>(GetInstanceField<object>(this.helper.ModRegistry, "Registry"), "Mods"))
            {
                ModEntryModel result = GetInstanceProperty<ModEntryModel>(modMetaData, "UpdateCheckData");
                IManifest manifest = GetInstanceProperty<IManifest>(modMetaData, "Manifest");

                string fallbackURL = manifest.UpdateKeys?.Select(this.toolkit.GetUpdateUrl).FirstOrDefault(p => p != null) ?? "";

                if (result == null)
                {
                    statuses.Add(new ModStatus(UpdateStatus.Skipped, manifest, fallbackURL, null, "SMAPI didn't check for an update"));
                    continue;
                }

                if (result.SuggestedUpdate == null)
                {
                    if (result.Errors.Length != 0)
                    {
                        // Return the first error. That's not perfect, but generally users don't care why each different update failed, they just want to know there was an error.
                        statuses.Add(new ModStatus(UpdateStatus.Error, manifest, fallbackURL, null, result.Errors[0]));
                    }
                    else
                    {
                        statuses.Add(new ModStatus(UpdateStatus.UpToDate, manifest, fallbackURL));
                    }
                }
                else
                {
                    statuses.Add(new ModStatus(UpdateStatus.OutOfDate, manifest, result.SuggestedUpdate.Url, result.SuggestedUpdate.Version.ToString()));
                }

                addedNonSkippedStatus = true;
            }

            return addedNonSkippedStatus;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Gets an instance property value from an object.</summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="obj">The object to get the value from.</param>
        /// <param name="name">The name of the property</param>
        /// <returns>The property value.</returns>
        private static T GetInstanceProperty<T>(object obj, string name)
        {
            return (T)(obj?.GetType().GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(obj));
        }

        /// <summary>Gets an instance field value from an object.</summary>
        /// <typeparam name="T">The type of the field value.</typeparam>
        /// <param name="obj">The object to get the value from.</param>
        /// <param name="name">The name of the field</param>
        /// <returns>The field value.</returns>
        private static T GetInstanceField<T>(object obj, string name)
        {
            return (T)(obj?.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(obj));
        }
    }
}
