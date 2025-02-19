﻿using DotLiquid;
using PowerStore.Domain.Catalog;
using PowerStore.Domain.Customers;
using System.Collections.Generic;

namespace PowerStore.Services.Messages.DotLiquidDrops
{
    public partial class LiquidAttributeCombination : Drop
    {
        private ProductAttributeCombination _combination;

        public LiquidAttributeCombination(ProductAttributeCombination combination)
        {
            _combination = combination;
            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Formatted { get; set; }

        public string SKU { get; set; }

        public string StockQuantity
        {
            get { return _combination.StockQuantity.ToString(); }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}
