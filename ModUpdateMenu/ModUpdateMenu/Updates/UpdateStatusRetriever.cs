using StardewModdingAPI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

            foreach (object modMetaData in (IEnumerable<object>) registry.GetType()
                .GetMethod("GetAll", BindingFlags.Public | BindingFlags.Instance)
                .Invoke(registry, new object[] {true, true}))
            {
                object updateCheckModel = GetInstanceProperty(modMetaData, "UpdateCheckData");

                if (updateCheckModel == null)
                    return false;

                IManifest modManifest = (IManifest) GetInstanceProperty(modMetaData, "Manifest");


                object latestModEntryVersionModel = GetInstanceProperty(updateCheckModel, "Main");
                object optionalModEntryVersionModel = GetInstanceProperty(updateCheckModel, "Optional");
                object unofficialModEntryVersionModel = GetInstanceProperty(updateCheckModel, "Unofficial");

                // parse versions
                ISemanticVersion localVersion = modManifest.Version;
                ISemanticVersion latestVersion =
                    (ISemanticVersion) GetInstanceProperty(latestModEntryVersionModel, "Version");
                ISemanticVersion optionalVersion =
                    (ISemanticVersion) GetInstanceProperty(optionalModEntryVersionModel, "Version");
                ISemanticVersion unofficialVersion =
                    (ISemanticVersion) GetInstanceProperty(unofficialModEntryVersionModel, "Version");

                UpdateStatus status = UpdateStatus.OutOfDate;
                object whichModel = null;
                ISemanticVersion updateVersion = null;
                string error = null;

                // show update alerts
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
                else if (IsValidUpdate(localVersion, unofficialVersion,
                    useBetaChannel: GetInstanceProperty(modMetaData, "Status") == GetInstanceProperty(modMetaData, "Status").GetType().GetEnumValues()
                                        .GetValue(1)))
                {
                    whichModel = unofficialModEntryVersionModel;
                    updateVersion = unofficialVersion;
                }
                else
                {
                    string[] errors = (string[])GetInstanceProperty(updateCheckModel, "Errors");
                    if (errors.Length > 0)
                    {
                        status = UpdateStatus.Error;
                        error = errors[0];
                        updateVersion = modManifest.Version;
                    }
                    else
                    {
                        updateVersion = modManifest.Version;
                        status = UpdateStatus.UpToDate;

                        if (latestVersion != null && localVersion.Equals(latestVersion))
                            whichModel = latestModEntryVersionModel;
                        else if (optionalVersion != null && localVersion.Equals(optionalVersion))
                            whichModel = optionalModEntryVersionModel;
                        else if (unofficialVersion != null && localVersion.Equals(unofficialVersion))
                            whichModel = unofficialVersion;
                        else
                            status = UpdateStatus.Skipped;
                    }
                }

                statuses.Add(new ModStatus(status, modManifest.UniqueID, modManifest.Name, modManifest.Author,
                    (string) GetInstanceProperty(whichModel, "Url") ?? "",
                    modManifest.Version.ToString(), updateVersion.ToString(), error));
            }

            return true;
        }

        private static object GetInstanceProperty(object obj, string name, bool isNonPublic = false)
        {
            return obj?.GetType().GetProperty(name,
                (isNonPublic ? BindingFlags.NonPublic : BindingFlags.Public) | BindingFlags.Instance).GetValue(obj);
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