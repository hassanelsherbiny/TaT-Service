﻿using PowerStore.Domain.Orders;
using PowerStore.Domain.Payments;
using PowerStore.Domain.Shipping;
using PowerStore.Framework.Components;
using PowerStore.Framework.Extensions;
using PowerStore.Services.Localization;
using PowerStore.Services.Security;
using PowerStore.Web.Areas.Admin.Models.Affiliates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PowerStore.Web.Areas.Admin.Components
{
    public class AffiliateViewComponent : BaseAdminViewComponent
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        public AffiliateViewComponent(ILocalizationService localizationService, IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string affiliateId)
        {
            if (!await _permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
                return Content("");

            if (String.IsNullOrEmpty(affiliateId))
                throw new Exception("Affliate ID cannot be empty");

            var model = new AffiliatedOrderListModel();
            model.AffliateId = affiliateId;

            //order statuses
            model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(HttpContext, false).ToList();
            model.AvailableOrderStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //payment statuses
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(HttpContext, false).ToList();
            model.AvailablePaymentStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            //shipping statuses
            model.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(HttpContext, false).ToList();
            model.AvailableShippingStatuses.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

            return View(model);
        }
    }
}