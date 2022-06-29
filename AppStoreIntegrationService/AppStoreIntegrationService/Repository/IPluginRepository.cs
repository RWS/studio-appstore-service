﻿using AppStoreIntegrationService.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppStoreIntegrationService.Repository
{
	public interface IPluginRepository
	{
		Task<List<PluginDetails>> GetAll(string sortOrder);
		List<PluginDetails> SearchPlugins(List<PluginDetails>pluginsList,PluginFilter pluginFilter);
		Task UpdatePrivatePlugin(PrivatePlugin privatePlugin); 
		Task<PluginDetails> GetPluginById(int id);
		Task<List<CategoryDetails>> GetCategories();
		Task AddPrivatePlugin(PrivatePlugin plugin);
		Task RemovePlugin(int id);
		Task RemovePluginVersion(int pluginId, string versionId);
		Task<IActionResult> ImportFromFile(IFormFile file);
	}
}
