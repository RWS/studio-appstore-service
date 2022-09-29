﻿using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class PluginDetailsModel
    {
        public PrivatePlugin PrivatePlugin { get; set; }

        public List<CategoryDetails> Categories { get; set; }

        public SelectList CategoryListItems { get; set; }

        public List<int> SelectedCategories { get; set; } = new List<int>();

        public string SelectedVersionId { get; set; }
    }
}
