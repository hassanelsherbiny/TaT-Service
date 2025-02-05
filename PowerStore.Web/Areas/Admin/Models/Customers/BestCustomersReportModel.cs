﻿using PowerStore.Core.ModelBinding;
using PowerStore.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PowerStore.Web.Areas.Admin.Models.Customers
{
    public partial class BestCustomersReportModel : BaseModel
    {
        public BestCustomersReportModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableShippingStatuses = new List<SelectListItem>();
        }

        [PowerStoreResourceDisplayName("Admin.Reports.Customers.BestBy.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [PowerStoreResourceDisplayName("Admin.Reports.Customers.BestBy.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [PowerStoreResourceDisplayName("Admin.Reports.Customers.BestBy.OrderStatus")]
        public int OrderStatusId { get; set; }
        [PowerStoreResourceDisplayName("Admin.Reports.Customers.BestBy.PaymentStatus")]
        public int PaymentStatusId { get; set; }
        [PowerStoreResourceDisplayName("Admin.Reports.Customers.BestBy.ShippingStatus")]
        public int ShippingStatusId { get; set; }

        public string StoreId { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
        public IList<SelectListItem> AvailableShippingStatuses { get; set; }
    }
}