﻿using PowerStore.Core;
using PowerStore.Domain;
using System;

namespace PowerStore.Web.Areas.Admin.Models.PushNotifications
{
    public class PushRegistrationGridModel : BaseEntity
    {
        public string CustomerId { get; set; }

        public bool Allowed { get; set; }

        public string Token { get; set; }

        public DateTime RegisteredOn { get; set; }

        public string CustomerEmail { get; set; }
    }
}