﻿using DotLiquid;
using PowerStore.Domain.Catalog;
using PowerStore.Domain.Localization;
using PowerStore.Domain.Stores;
using PowerStore.Services.Catalog;
using PowerStore.Services.Localization;
using PowerStore.Services.Seo;
using System.Collections.Generic;

namespace PowerStore.Services.Messages.DotLiquidDrops
{
    public partial class LiquidProduct : Drop
    {
        private Product _product;
        private Language _language;
        private Store _store;

        public LiquidProduct(Product product, Language language, Store store)
        {
            _product = product;
            _language = language;
            _store = store;
            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Id
        {
            get { return _product.Id.ToString(); }
        }

        public string Name
        {
            get { return _product.GetLocalized(x => x.Name, _language.Id); }
        }

        public string ShortDescription
        {
            get { return _product.GetLocalized(x => x.ShortDescription, _language.Id); }
        }

        public string SKU
        {
            get { return _product.Sku; }
        }

        public string StockQuantity
        {
            get { return _product.GetTotalStockQuantity().ToString(); }
        }

        public decimal Price
        {
            get { return _product.Price; }
        }

        public string ProductURLForCustomer
        {
            get { return string.Format("{0}{1}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _product.GetSeName(_language.Id)); }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}