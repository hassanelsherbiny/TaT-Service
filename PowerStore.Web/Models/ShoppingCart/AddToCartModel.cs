﻿using PowerStore.Domain.Orders;
using PowerStore.Core.Models;
using PowerStore.Web.Models.Media;
using System;

namespace PowerStore.Web.Models.ShoppingCart
{
    public partial class AddToCartModel : BaseModel
    {
        public AddToCartModel()
        {
            Picture = new PictureModel();
        }

        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSeName { get; set; }
        public string AttributeDescription { get; set; }
        public string ReservationInfo { get; set; }
        public PictureModel Picture { get; set; }
        public int Quantity { get; set; }
        public int ItemQuantity { get; set; }
        public string Price { get; set; }
        public decimal DecimalPrice { get; set; }
        public string TotalPrice { get; set; }
        public ShoppingCartType CartType { get; set; }

        public int TotalItems { get; set; }
        public string SubTotal { get; set; }
        public string SubTotalDiscount { get; set; }
        public decimal DecimalSubTotal { get; set; }

        public bool IsAuction { get; set; }
        public string HighestBid { get; set; }
        public decimal HighestBidValue { get; set; }
        public DateTime? EndTime { get; set; }
    }
}