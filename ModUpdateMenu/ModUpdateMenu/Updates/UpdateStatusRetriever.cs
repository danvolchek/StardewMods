using System;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using StardewModdingAPI.Toolkit.Framework.Clients.WebApi;
using StardewModdingAPI.Toolkit.Framework.ModData;

namespace ModUpdateMenu.Updates
{
    class UpdateStatusRetriever : IUpdateStatusRetriever
    {
        private readonly IModHelper helper;

        public UpdateStatusRetriever(IModHelper helper)
        {
            this.helper = helper;
        }

        public bool GetUpdateStatuses(out IList<ModStatus> statuses)
        {
            statuses = new List<ModStatus>();


            object registry = this.helper.ModRegistry.GetType()
                .GetField("Registry", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this.helper.ModRegistry);

            bool addedNonSkippedStatus = false;

            foreach (object modMetaData in (IEnumerable<object>)registry.GetType().GetField("Mods", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(registry))
            {
                ModEntryModel result = GetInstanceProperty<ModEntryModel>(modMetaData, "UpdateCheckData");
                IManifest manifest = GetInstanceProperty<IManifest>(modMetaData, "Manifest");

                if (manifest.UpdateKeys == null || !manifest.UpdateKeys.Any())
                {
                    statuses.Add(new ModStatus(UpdateStatus.Skipped, manifest, "", null, "Mod has no update keys"));
                    continue;
                }
                else if (result == null)
                {
                    statuses.Add(new ModStatus(UpdateStatus.Skipped, manifest, "", null, "SMAPI didn't check for an update"));
                    continue;
                }

                ModDataRecordVersionedFields dataRecord =
                    GetInstanceProperty<ModDataRecordVersionedFields>(modMetaData, "DataRecord");

                bool useBetaInfo = result.HasBetaInfo && Constants.ApiVersion.IsPrerelease();
                ISemanticVersion localVersion = dataRecord?.GetLocalVersionForUpdateChecks(manifest.Version) ?? manifest.Version;
                ISemanticVersion latestVersion = dataRecord?.GetRemoteVersionForUpdateChecks(result.Main?.Version) ?? result.Main?.Version;
                ISemanticVersion optionalVersion = dataRecord?.GetRemoteVersionForUpdateChecks(result.Optional?.Version) ?? result.Optional?.Version;
                ISemanticVersion unofficialVersion = useBetaInfo ? result.UnofficialForBeta?.Version : result.Unofficial?.Version;

                if (this.IsValidUpdate(localVersion, latestVersion, useBetaChannel: true))
                    statuses.Add(new ModStatus(UpdateStatus.OutOfDate, manifest, result.Main?.Url, latestVersion.ToString()));
                else if (this.IsValidUpdate(localVersion, optionalVersion, useBetaChannel: localVersion.IsPrerelease()))
                    statuses.Add(new ModStatus(UpdateStatus.OutOfDate, manifest, result.Optional?.Url, optionalVersion.ToString()));
                else if (this.IsValidUpdate(localVersion, unofficialVersion, useBetaChannel: GetEnumName(modMetaData, "Status") == "Failed"))
                    statuses.Add(new ModStatus(UpdateStatus.OutOfDate, manifest, useBetaInfo ? result.UnofficialForBeta?.Url : result.Unofficial?.Url, unofficialVersion.ToString()));
                else if (result.Errors != null && result.Errors.Any())
                    statuses.Add(new ModStatus(UpdateStatus.Error, manifest, "", "", result.Errors[0]));
                else
                {
                    string updateURL = "";
                    UpdateStatus updateStatus = UpdateStatus.UpToDate;
                    if (localVersion.Equals(latestVersion))
                        updateURL = result.Main?.Url;
                    else if (localVersion.Equals(optionalVersion))
                        updateURL = result.Optional?.Url;
                    else if (localVersion.Equals(unofficialVersion))
                        updateURL = useBetaInfo ? result.UnofficialForBeta?.Url : result.Unofficial?.Url;
                    else
                    {
                        if (this.IsValidUpdate(latestVersion, localVersion, useBetaChannel: true))
                        {
                            updateURL = result.Main?.Url;
                            updateStatus = UpdateStatus.VeryNew;
                        }
                        else if (this.IsValidUpdate(optionalVersion, localVersion, useBetaChannel: localVersion.IsPrerelease()))
                        {
                            updateURL = result.Optional?.Url;
                            updateStatus = UpdateStatus.VeryNew;
                        }
                        else if (this.IsValidUpdate(unofficialVersion, localVersion, useBetaChannel: GetEnumName(modMetaData, "Status") == "Failed"))
                        {
                            updateURL = useBetaInfo ? result.UnofficialForBeta?.Url : result.Unofficial?.Url;
                            updateStatus = UpdateStatus.VeryNew;
                        }
                    }
                    statuses.Add(new ModStatus(updateStatus, manifest, updateURL));
                }

                addedNonSkippedStatus = true;
            }

            return addedNonSkippedStatus;
        }

        private static T GetInstanceProperty<T>(object obj, string name, bool isNonPublic = false)
        {
            return (T)(obj?.GetType().GetProperty(name,
                (isNonPublic ? BindingFlags.NonPublic : BindingFlags.Public) | BindingFlags.Instance).GetValue(obj));
        }

        private static string GetEnumName(object obj, string name, bool isNonPublic = false)
        {
            object foundEnum = GetInstanceProperty<object>(obj, name, isNonPublic);
            if (foundEnum == null)
                return null;
            return Enum.GetName(foundEnum.GetType(), foundEnum);
        }


        ////Taken from https://github.com/Pathoschild/SMAPI/blob/develop/src/SMAPI/Program.cs#L709-L719
        /// <summary>Get whether a given version should be offered to the user as an update.</summary>
        /// <param name="currentVersion">The current semantic version.</param>
        /// <param name="newVersion">The target semantic version.</param>
        /// <param name="useBetaChannel">Whether the user enabled the beta channel and should be offered pre-release updates.</param>
        private bool IsValidUpdate(ISemanticVersion currentVersion, ISemanticVersion newVersion, bool useBetaChannel)
        {
            return
                newVersion != null
                && newVersion.IsNewerThan(currentVersion)
                && (useBetaChannel || !newVersion.IsPrerelease());
        }
    }
}