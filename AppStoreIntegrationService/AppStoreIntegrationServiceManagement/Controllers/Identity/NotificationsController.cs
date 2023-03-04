﻿using AppStoreIntegrationServiceManagement.Model.DataBase;
using AppStoreIntegrationServiceManagement.Model.Identity;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using AppStoreIntegrationServiceManagement.Repository;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    [Authorize(Roles = "Administrator, Developer")]
    public class NotificationsController : Controller
    {
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly AccountsManager _accountsManager;
        private readonly INotificationCenter _notificationCenter;

        public NotificationsController
        (
            UserManager<IdentityUserExtended> userManager, 
            INotificationCenter notificationCenter,
            AccountsManager accountsManager)
        {
            _userManager = userManager;
            _notificationCenter = notificationCenter;
            _accountsManager = accountsManager;
        }

        [Route("Identity/Account/Notifications")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View("Notifications", (user.EmailNotificationsEnabled, user.PushNotificationsEnabled));
        }

        [HttpPost]
        public async Task<IActionResult> Update(bool emailNotificationsEnabled, bool pushNotificationsEnabled)
        {
            var user = await _userManager.GetUserAsync(User);
            user.EmailNotificationsEnabled = emailNotificationsEnabled;
            user.PushNotificationsEnabled = pushNotificationsEnabled;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Developer, Administrator")]
        public async Task<IActionResult> ChangeStatus(int? id, NotificationStatus status)
        {
            var user = await _userManager.GetUserAsync(User);
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            await _notificationCenter.ChangeStatus(User.IsInRole("Administrator") ? "Administrator" : account.AccountName, id, status);
            return new EmptyResult();
        }

        public static async Task<NotificationModel> PrepareNotifications(ClaimsPrincipal principal, IQueryCollection query, INotificationCenter notificationCenter)
        {
            var status = new List<string> { "Active", "Complete", "Acknowledged", "Inactive", "All" }.Select(x => new FilterItem
            {
                Id = "Status",
                Label = x,
                Value = $"{(int)Enum.Parse(typeof(NotificationStatus), x)}",
                IsSelected = query["Status"].Any(y => y == $"{(int)Enum.Parse(typeof(NotificationStatus), x)}")
            });
            var notifications = await notificationCenter.GetNotificationsForUser(principal);

            return new NotificationModel
            {
                Notifications = notificationCenter.FilterNotifications(notifications, (NotificationStatus)Enum.Parse(typeof(NotificationStatus), query["Status"].FirstOrDefault() ?? "Active"), query["Query"].FirstOrDefault()),
                StatusListItems = new SelectList(status, nameof(FilterItem.Value), nameof(FilterItem.Label), query["Status"].FirstOrDefault()),
                Filters = status.Append(new FilterItem
                {
                    Id = "Notification",
                    Label = query["Query"],
                    Value = query["Query"],
                    IsSelected = !string.IsNullOrEmpty(query["Query"].FirstOrDefault())
                })
            };
        }
    }
}
