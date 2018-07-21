using System;
using StardewModdingAPI;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using StardewModdingAPI.Toolkit.Framework.Clients.WebApi;

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

            foreach (object modMetaData in (IEnumerable<object>)registry.GetType()
                .GetMethod("GetAll", BindingFlags.Public | BindingFlags.Instance)
                .Invoke(registry, new object[] { true, true }))
            {
                ModEntryModel updateCheckModel = GetInstanceProperty<ModEntryModel>(modMetaData, "UpdateCheckData");

                if (updateCheckModel == null)
                    return false;

                IManifest modManifest = GetInstanceProperty<IManifest>(modMetaData, "Manifest");

                ModEntryVersionModel latestModEntryVersionModel = updateCheckModel.Main;
                ModEntryVersionModel optionalModEntryVersionModel = updateCheckModel.Optional;
                ModEntryVersionModel unofficialModEntryVersionModel = updateCheckModel.Unofficial;

                // get versions
                ISemanticVersion localVersion = modManifest.Version;
                ISemanticVersion latestVersion = latestModEntryVersionModel?.Version;
                ISemanticVersion optionalVersion = optionalModEntryVersionModel?.Version;
                ISemanticVersion unofficialVersion = unofficialModEntryVersionModel?.Version;

                UpdateStatus status = UpdateStatus.OutOfDate;
                ModEntryVersionModel whichModel = null;
                ISemanticVersion updateVersion;
                string error = null;

                // get update alerts
                if (IsValidUpdate(localVersion, latestVersion, useBetaChannel: true))
                {
                    whichModel = latestModEntryVersionModel;
                    updateVersion = latestVersion;
                }
                else if (IsValidUpdate(localVersion, optionalVersion, useBetaChannel: localVersion.IsPrerelease()))
                {
                    whichModel = optionalModEntryVersionModel;
                    updateVersion = optionalVersion;
                }
                else if (IsValidUpdate(localVersion, unofficialVersion, useBetaChannel: true))
                //Different from SMAPI: useBetaChannel is always true
                {
                    whichModel = unofficialModEntryVersionModel;
                    unofficialModEntryVersionModel.Url = $"https://stardewvalleywiki.com/Modding:SMAPI_compatibility#{GenerateAnchor(modManifest.Name)}";
                    updateVersion = unofficialVersion;
                }
                else
                {
                    if (updateCheckModel.Errors.Length > 0)
                    {
                        status = UpdateStatus.Error;
                        error = updateCheckModel.Errors[0];
                        updateVersion = modManifest.Version;
                    }
                    else
                    {
                        updateVersion = modManifest.Version;
                        status = UpdateStatus.UpToDate;

                        if (latestVersion != null && (latestVersion.Equals(localVersion) || IsValidUpdate(latestVersion, localVersion, true)))
                            whichModel = latestModEntryVersionModel;
                        else if (optionalVersion != null && (optionalVersion.Equals(localVersion) || IsValidUpdate(optionalVersion, localVersion, true)))
                            whichModel = optionalModEntryVersionModel;
                        else if (unofficialVersion != null && (unofficialVersion.Equals(localVersion) || IsValidUpdate(unofficialVersion, localVersion, true)))
                            whichModel = unofficialModEntryVersionModel;
                        else
                            status = UpdateStatus.Skipped;
                    }
                }

                statuses.Add(new ModStatus(status, modManifest.UniqueID, modManifest.Name, modManifest.Author,
                    whichModel?.Url ?? "",
                    modManifest.Version.ToString(), updateVersion?.ToString() ?? "", error));
            }

            return true;
        }

        private static string GenerateAnchor(string name)
        {
            name = name.Replace(' ', '_');
            StringBuilder builder = new StringBuilder();
            foreach (char c in name)
            {
                if (c != '_' && !IsLetterOrDigit(c, out int code))
                    builder.Append($".{code}");
                else
                    builder.Append(c);
            }

            return builder.ToString();
        }

        private static bool IsLetterOrDigit(char c, out int code)
        {
            code = Convert.ToInt32(c);
            if (code == -1)
                code = c;
            return (code >= 48 && code <= 57) || (code >= 65 && code <= 90) || (code >= 97 && code <= 122);
        }

        private static T GetInstanceProperty<T>(object obj, string name, bool isNonPublic = false)
        {
            return (T)(obj?.GetType().GetProperty(name,
                (isNonPublic ? BindingFlags.NonPublic : BindingFlags.Public) | BindingFlags.Instance).GetValue(obj));
        }


        ////Taken from https://github.com/Pathoschild/SMAPI/blob/develop/src/SMAPI/Program.cs#L709-L719
        /// <summary>Get whether a given version should be offered to the user as an update.</summary>
        /// <param name="currentVersion">The current semantic version.</param>
        /// <param name="newVersion">The target semantic version.</param>
        /// <param name="useBetaChannel">Whether the user enabled the beta channel and should be offered pre-release updates.</param>
        private static bool IsValidUpdate(ISemanticVersion currentVersion, ISemanticVersion newVersion, bool useBetaChannel)
        {
            return
                newVersion != null
                && newVersion.IsNewerThan(currentVersion)
                && (useBetaChannel || !newVersion.IsPrerelease());
        }
    }
}