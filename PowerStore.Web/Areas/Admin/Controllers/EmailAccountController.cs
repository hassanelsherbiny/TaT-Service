﻿using PowerStore.Core;
using PowerStore.Core.Caching;
using PowerStore.Domain.Messages;
using PowerStore.Framework.Controllers;
using PowerStore.Framework.Kendoui;
using PowerStore.Framework.Mvc.Filters;
using PowerStore.Framework.Security.Authorization;
using PowerStore.Services.Configuration;
using PowerStore.Services.Localization;
using PowerStore.Services.Messages;
using PowerStore.Services.Security;
using PowerStore.Web.Areas.Admin.Extensions;
using PowerStore.Web.Areas.Admin.Models.Messages;
using PowerStore.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PowerStore.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.EmailAccounts)]
    public partial class EmailAccountController : BaseAdminController
    {
        private readonly IEmailAccountViewModelService _emailAccountViewModelService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly ICacheBase _cacheBase;

        public EmailAccountController(IEmailAccountViewModelService emailAccountViewModelService, IEmailAccountService emailAccountService,
            ILocalizationService localizationService, ISettingService settingService,
            EmailAccountSettings emailAccountSettings, ICacheBase cacheBase)
        {
            _emailAccountViewModelService = emailAccountViewModelService;
            _emailAccountService = emailAccountService;
            _localizationService = localizationService;
            _emailAccountSettings = emailAccountSettings;
            _settingService = settingService;
            _cacheBase = cacheBase;
        }

        protected async Task ClearCache()
        {
            await _cacheBase.Clear();
        }
        public IActionResult List() => View();

        [HttpPost]
        [PermissionAuthorizeAction(PermissionActionName.List)]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var emailAccountModels = (await _emailAccountService.GetAllEmailAccounts())
                                    .Select(x => x.ToModel())
                                    .ToList();
            foreach (var eam in emailAccountModels)
                eam.IsDefaultEmailAccount = eam.Id == _emailAccountSettings.DefaultEmailAccountId;

            var gridModel = new DataSourceResult
            {
                Data = emailAccountModels,
                Total = emailAccountModels.Count()
            };

            return Json(gridModel);
        }
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> MarkAsDefaultEmail(string id)
        {
            var defaultEmailAccount = await _emailAccountService.GetEmailAccountById(id);
            if (defaultEmailAccount != null)
            {
                _emailAccountSettings.DefaultEmailAccountId = defaultEmailAccount.Id;
                await _settingService.SaveSetting(_emailAccountSettings);
            }

            //now clear cache
            await ClearCache();

            return RedirectToAction("List");
        }
        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public IActionResult Create()
        {
            var model = _emailAccountViewModelService.PrepareEmailAccountModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create(EmailAccountModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var emailAccount = await _emailAccountViewModelService.InsertEmailAccountModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = emailAccount.Id }) : RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            return View(model);
        }
        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var emailAccount = await _emailAccountService.GetEmailAccountById(id);
            if (emailAccount == null)
                //No email account found with the specified id
                return RedirectToAction("List");

            return View(emailAccount.ToModel());
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> Edit(EmailAccountModel model, bool continueEditing)
        {
            var emailAccount = await _emailAccountService.GetEmailAccountById(model.Id);
            if (emailAccount == null)
                //No email account found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                emailAccount = await _emailAccountViewModelService.UpdateEmailAccountModel(emailAccount, model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = emailAccount.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> ChangePassword(EmailAccountModel model)
        {
            var emailAccount = await _emailAccountService.GetEmailAccountById(model.Id);
            if (emailAccount == null)
                //No email account found with the specified id
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                //do not validate model
                await _emailAccountViewModelService.ChangePasswordEmailAccountModel(emailAccount, model);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Fields.Password.PasswordChanged"));
            }
            else
                ErrorNotification(ModelState);

            return RedirectToAction("Edit", new { id = emailAccount.Id });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("sendtestemail")]
        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> SendTestEmail(EmailAccountModel model)
        {
            var emailAccount = await _emailAccountService.GetEmailAccountById(model.Id);
            if (emailAccount == null)
                //No email account found with the specified id
                return RedirectToAction("List");
            try
            {
                if (String.IsNullOrWhiteSpace(model.SendTestEmailTo))
                    throw new PowerStoreException("Enter test email address");
                if (ModelState.IsValid)
                {
                    await _emailAccountViewModelService.SendTestEmail(emailAccount, model);
                    SuccessNotification(_localizationService.GetResource("Admin.Configuration.EmailAccounts.SendTestEmail.Success"), false);
                }
                else
                    ErrorNotification(ModelState);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message, false);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        public async Task<IActionResult> Delete(string id)
        {
            var emailAccount = await _emailAccountService.GetEmailAccountById(id);
            if (emailAccount == null)
                //No email account found with the specified id
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    await _emailAccountService.DeleteEmailAccount(emailAccount);
                    SuccessNotification(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Deleted"));
                }
                else
                    ErrorNotification(ModelState);

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = emailAccount.Id });
            }
        }
    }
}
