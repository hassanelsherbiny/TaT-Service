﻿using PowerStore.Domain.Catalog;
using PowerStore.Domain.Common;
using PowerStore.Domain.Media;
using PowerStore.Domain.Orders;
using PowerStore.Services.Catalog;
using PowerStore.Services.Directory;
using PowerStore.Services.Discounts;
using PowerStore.Services.Localization;
using PowerStore.Services.Media;
using PowerStore.Services.Security;
using PowerStore.Services.Tax;
using PowerStore.Web.Extensions;
using PowerStore.Web.Features.Models.Products;
using PowerStore.Web.Features.Models.ShoppingCart;
using PowerStore.Web.Models.Catalog;
using PowerStore.Web.Models.Media;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PowerStore.Web.Features.Handlers.Products
{
    public class GetProductDetailsAttributeChangeHandler : IRequestHandler<GetProductDetailsAttributeChange, ProductDetailsAttributeChangeModel>
    {
        private readonly IMediator _mediator;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IPermissionService _permissionService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly MediaSettings _mediaSettings;

        public GetProductDetailsAttributeChangeHandler(
            IMediator mediator,
            IProductAttributeParser productAttributeParser,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            ILocalizationService localizationService,
            IPictureService pictureService,
            ShoppingCartSettings shoppingCartSettings,
            MediaSettings mediaSettings)
        {
            _mediator = mediator;
            _productAttributeParser = productAttributeParser;
            _permissionService = permissionService;
            _priceCalculationService = priceCalculationService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _shoppingCartSettings = shoppingCartSettings;
            _mediaSettings = mediaSettings;
        }

        public async Task<ProductDetailsAttributeChangeModel> Handle(GetProductDetailsAttributeChange request, CancellationToken cancellationToken)
        {
            var model = new ProductDetailsAttributeChangeModel();

            var customAttributes = await _mediator.Send(new GetParseProductAttributes() { Product = request.Product, Form = request.Form });

            string warehouseId = _shoppingCartSettings.AllowToSelectWarehouse ?
               request.Form["WarehouseId"].ToString() :
               request.Product.UseMultipleWarehouses ? request.Store.DefaultWarehouseId :
               (string.IsNullOrEmpty(request.Store.DefaultWarehouseId) ? request.Product.WarehouseId : request.Store.DefaultWarehouseId);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (request.Product.ProductType == ProductType.Reservation)
            {
                request.Product.ParseReservationDates(request.Form, out rentalStartDate, out rentalEndDate);
            }

            model.Sku = request.Product.FormatSku(customAttributes, _productAttributeParser);
            model.Mpn = request.Product.FormatMpn(customAttributes, _productAttributeParser);
            model.Gtin = request.Product.FormatGtin(customAttributes, _productAttributeParser);

            if (await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices) && !request.Product.CustomerEntersPrice && request.Product.ProductType != ProductType.Auction)
            {
                //we do not calculate price of "customer enters price" option is enabled
                var unitprice = await _priceCalculationService.GetUnitPrice(request.Product,
                    request.Customer,
                    request.Currency,
                    ShoppingCartType.ShoppingCart,
                    1, customAttributes, (decimal?)default,
                    rentalStartDate, rentalEndDate,
                    true);

                decimal discountAmount = unitprice.discountAmount;
                List<AppliedDiscount> scDiscounts = unitprice.appliedDiscounts;
                decimal finalPrice = unitprice.unitprice;
                var productprice = await _taxService.GetProductPrice(request.Product, finalPrice);
                decimal finalPriceWithDiscount = productprice.productprice;
                decimal taxRate = productprice.taxRate;
                model.Price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
            }
            //stock
            model.StockAvailability = request.Product.FormatStockMessage(warehouseId, customAttributes, _localizationService, _productAttributeParser);

            //back in stock subscription
            if ((request.Product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes
                || request.Product.ManageInventoryMethod == ManageInventoryMethod.ManageStock) &&
                request.Product.BackorderMode == BackorderMode.NoBackorders &&
                request.Product.AllowBackInStockSubscriptions)
            {
                var combination = _productAttributeParser.FindProductAttributeCombination(request.Product, customAttributes);

                if (combination != null)
                    if (request.Product.GetTotalStockQuantityForCombination(combination, warehouseId: warehouseId) <= 0)
                        model.DisplayBackInStockSubscription = true;

                if (request.Product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                {
                    model.DisplayBackInStockSubscription = request.Product.AllowBackInStockSubscriptions;
                    customAttributes = new List<CustomAttribute>();
                }

                var subscription = await _backInStockSubscriptionService
                   .FindSubscription(request.Customer.Id,
                    request.Product.Id, customAttributes, request.Store.Id, warehouseId);

                if (subscription != null)
                    model.ButtonTextBackInStockSubscription = _localizationService.GetResource("BackInStockSubscriptions.DeleteNotifyWhenAvailable");
                else
                    model.ButtonTextBackInStockSubscription = _localizationService.GetResource("BackInStockSubscriptions.NotifyMeWhenAvailable");

            }


            //conditional attributes
            if (request.ValidateAttributeConditions)
            {
                var attributes = request.Product.ProductAttributeMappings;
                foreach (var attribute in attributes)
                {
                    var conditionMet = _productAttributeParser.IsConditionMet(request.Product, attribute, customAttributes);
                    if (conditionMet.HasValue)
                    {
                        if (conditionMet.Value)
                            model.EnabledAttributeMappingIds.Add(attribute.Id);
                        else
                            model.DisabledAttributeMappingids.Add(attribute.Id);
                    }
                }
            }
            //picture. used when we want to override a default product picture when some attribute is selected
            if (request.LoadPicture)
            {

                //first, try to get product attribute combination picture
                var pictureId = _productAttributeParser.FindProductAttributeCombination(request.Product, customAttributes)?.PictureId;
                //then, let's see whether we have attribute values with pictures
                if (string.IsNullOrEmpty(pictureId))
                {
                    pictureId = _productAttributeParser.ParseProductAttributeValues(request.Product, customAttributes)
                        .FirstOrDefault(attributeValue => !string.IsNullOrEmpty(attributeValue.PictureId))?.PictureId ?? "";
                }

                if (!string.IsNullOrEmpty(pictureId))
                {
                    var pictureModel = new PictureModel {
                        Id = pictureId,
                        FullSizeImageUrl = await _pictureService.GetPictureUrl(pictureId),
                        ImageUrl = await _pictureService.GetPictureUrl(pictureId, _mediaSettings.ProductDetailsPictureSize)
                    };
                    model.PictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                    model.PictureDefaultSizeUrl = pictureModel.ImageUrl;
                }
            }
            return model;
        }
    }
}
