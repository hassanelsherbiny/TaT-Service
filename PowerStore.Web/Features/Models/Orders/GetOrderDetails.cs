﻿using PowerStore.Domain.Localization;
using PowerStore.Domain.Orders;
using PowerStore.Web.Models.Orders;
using MediatR;

namespace PowerStore.Web.Features.Models.Orders
{
    public class GetOrderDetails : IRequest<OrderDetailsModel>
    {
        public Order Order { get; set; }
        public Language Language { get; set; }
    }
}
