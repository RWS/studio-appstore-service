﻿using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    [Authorize]
    public class LogsController : Controller
    {
        private readonly ILoggingRepository _loggingRepository;
        private readonly IPluginRepository _pluginRepository;

        public LogsController(ILoggingRepository loggingRepository, IPluginRepository pluginRepository)
        {
            _loggingRepository = loggingRepository;
            _pluginRepository = pluginRepository;
        }

        [Route("/Plugins/Edit/{pluginId}/Logs")]
        public async Task<IActionResult> Index(int pluginId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var logs = await _loggingRepository.GetPluginLogs(pluginId);
            var fromDateFilter = string.IsNullOrEmpty(Request.Query["FromDate"].FirstOrDefault()) ? DateTime.MinValue : DateTime.Parse(Request.Query["FromDate"][0]);
            var toDateFilter = string.IsNullOrEmpty(Request.Query["ToDate"].FirstOrDefault()) ? DateTime.MaxValue : DateTime.Parse(Request.Query["ToDate"][0]);
            var extended = ExtendedPluginDetails.CopyFrom(plugin);

            logs = _loggingRepository.SearchLogs(logs, fromDateFilter, toDateFilter, Request.Query["Query"].FirstOrDefault());
            extended.Logs = logs;
            extended.IsEditMode = true;

            return View((extended, ApplyFilters()));
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ClearAll(int id)
        {
            await _loggingRepository.ClearLogs(id);
            return new EmptyResult();
        }

        private IEnumerable<FilterItem> ApplyFilters()
        {
            return new List<FilterItem>
            {
                new FilterItem
                {
                    Id = "FromDate",
                    Label = $"From {Request.Query["FromDate"]}",
                    Value = Request.Query["FromDate"],
                    IsSelected = !string.IsNullOrEmpty(Request.Query["FromDate"].FirstOrDefault())
                },
                new FilterItem
                {
                    Id = "ToDate",
                    Label = $"To {Request.Query["ToDate"]}",
                    Value = Request.Query["ToDate"],
                    IsSelected = !string.IsNullOrEmpty(Request.Query["ToDate"].FirstOrDefault())
                },
                new FilterItem
                {
                    Id = "Query",
                    Label = Request.Query["Query"],
                    Value = Request.Query["Query"],
                    IsSelected = !string.IsNullOrEmpty(Request.Query["Query"].FirstOrDefault())
                }
            };
        }
    }
}
