﻿using System.Collections.Generic;
using System.Linq;

namespace AppStoreIntegrationService.Model
{
    public static class PrivatePluginExtention
    {
        public static PluginDetails ConvertToPluginDetails(this PrivatePlugin privateDetails, PluginDetails foundDetails, PluginVersion selectedVersionDetails)
        {
            return new PluginDetails
            {
                Name = privateDetails.Name,
                Icon = new IconDetails { MediaUrl = privateDetails.IconUrl },
                Description = privateDetails.Description,
                PaidFor = privateDetails.PaidFor,
                Categories = privateDetails.Categories,
                DownloadUrl = foundDetails.DownloadUrl,
                Versions = PrepareVersions(foundDetails.Versions, selectedVersionDetails)
            };
        }

        private static List<PluginVersion> PrepareVersions(List<PluginVersion> versions, PluginVersion selectedVersionDetails)
        {
            var newVersionList = new List<PluginVersion>(versions);
            var existingVersion = newVersionList.FirstOrDefault(v => v.Id.Equals(selectedVersionDetails.Id));
            if (existingVersion != null)
            {
                newVersionList[newVersionList.IndexOf(existingVersion)] = selectedVersionDetails;
                return newVersionList;
            }

            newVersionList.Add(selectedVersionDetails);
            return newVersionList;
        }
    }
}