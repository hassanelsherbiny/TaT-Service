using PowerStore.Core;
using PowerStore.Domain.Catalog;
using PowerStore.Domain.Customers;
using PowerStore.Domain.Orders;
using PowerStore.Services.Common;
using PowerStore.Services.Directory;
using PowerStore.Services.Localization;
using PowerStore.Services.Media;
using PowerStore.Services.Seo;
using PowerStore.Services.Tax;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using PowerStore.Domain.Common;

namespace PowerStore.Services.Catalog
{
    /// <summary>
    /// Product attribute formatter
    /// </summary>
    public partial class ProductAttributeFormatter : IProductAttributeFormatter
    {
        private readonly IWorkContext _workContext;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDownloadService _downloadService;
        private readonly IWebHelper _webHelper;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductService _productService;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public ProductAttributeFormatter(IWorkContext workContext,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            ITaxService taxService,
            IPriceFormatter priceFormatter,
            IDownloadService downloadService,
            IWebHelper webHelper,
            IPriceCalculationService priceCalculationService,
            IProductService productService,
            ShoppingCartSettings shoppingCartSettings)
        {
            _workContext = workContext;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _taxService = taxService;
            _priceFormatter = priceFormatter;
            _downloadService = downloadService;
            _webHelper = webHelper;
            _priceCalculationService = priceCalculationService;
            _productService = productService;
            _shoppingCartSettings = shoppingCartSettings;
        }

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customAttributes">Attributes</param>
        /// <returns>Attributes</returns>
        public virtual Task<string> FormatAttributes(Product product, IList<CustomAttribute> customAttributes)
        {
            var customer = _workContext.CurrentCustomer;
            return FormatAttributes(product, customAttributes, customer);
        }

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customAttributes">Attributes</param>
        /// <param name="customer">Customer</param>
        /// <param name="serapator">Serapator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <param name="renderPrices">A value indicating whether to render prices</param>
        /// <param name="renderProductAttributes">A value indicating whether to render product attributes</param>
        /// <param name="renderGiftCardAttributes">A value indicating whether to render gift card attributes</param>
        /// <param name="allowHyperlinks">A value indicating whether to HTML hyperink tags could be rendered (if required)</param>
        /// <returns>Attributes</returns>
        public virtual async Task<string> FormatAttributes(Product product, IList<CustomAttribute> customAttributes,
            Customer customer, string serapator = "<br />", bool htmlEncode = true, bool renderPrices = true,
            bool renderProductAttributes = true, bool renderGiftCardAttributes = true,
            bool allowHyperlinks = true, bool showInAdmin = false)
        {

            var result = new StringBuilder();

            if (customAttributes == null || !customAttributes.Any())
                return result.ToString();

            var langId = string.Empty;

            if (_workContext.WorkingLanguage != null)
                langId = _workContext.WorkingLanguage.Id;
            else
                langId = customer?.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.LanguageId);

            if (string.IsNullOrEmpty(langId))
                langId = "";

            //attributes
            if (renderProductAttributes)
            {
                result = await PrepareFormattedAttribute(product, customAttributes, langId, serapator, htmlEncode,
                    renderPrices, allowHyperlinks, showInAdmin);

                if (product.ProductType == ProductType.BundledProduct)
                {
                    int i = 0;
                    foreach (var bundle in product.BundleProducts)
                    {
                        var p1 = await _productService.GetProductById(bundle.ProductId);
                        if (p1 != null)
                        {
                            if (i > 0)
                                result.Append(serapator);

                            result.Append($"<a href=\"{p1.GetSeName(langId)}\"> {p1.GetLocalized(x => x.Name, langId)} </a>");
                            var formattedAttribute = await PrepareFormattedAttribute(p1, customAttributes, langId, serapator, htmlEncode,
                            renderPrices, allowHyperlinks, showInAdmin);
                            if (formattedAttribute.Length > 0)
                            {
                                result.Append(serapator);
                                result.Append(formattedAttribute);
                            }
                            i++;
                        }
                    }
                }

            }

            //gift cards
            if (renderGiftCardAttributes)
            {
                if (product.IsGiftCard)
                {
                    string giftCardRecipientName;
                    string giftCardRecipientEmail;
                    string giftCardSenderName;
                    string giftCardSenderEmail;
                    string giftCardMessage;
                    _productAttributeParser.GetGiftCardAttribute(customAttributes, out giftCardRecipientName, out giftCardRecipientEmail,
                        out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                    //sender
                    var giftCardFrom = product.GiftCardType == GiftCardType.Virtual ?
                        string.Format(_localizationService.GetResource("GiftCardAttribute.From.Virtual"), giftCardSenderName, giftCardSenderEmail) :
                        string.Format(_localizationService.GetResource("GiftCardAttribute.From.Physical"), giftCardSenderName);
                    //recipient
                    var giftCardFor = product.GiftCardType == GiftCardType.Virtual ?
                        string.Format(_localizationService.GetResource("GiftCardAttribute.For.Virtual"), giftCardRecipientName, giftCardRecipientEmail) :
                        string.Format(_localizationService.GetResource("GiftCardAttribute.For.Physical"), giftCardRecipientName);

                    //encode (if required)
                    if (htmlEncode)
                    {
                        giftCardFrom = WebUtility.HtmlEncode(giftCardFrom);
                        giftCardFor = WebUtility.HtmlEncode(giftCardFor);
                    }

                    if (!String.IsNullOrEmpty(result.ToString()))
                    {
                        result.Append(serapator);
                    }
                    result.Append(giftCardFrom);
                    result.Append(serapator);
                    result.Append(giftCardFor);
                }
            }
            return result.ToString();
        }

        private async Task<StringBuilder> PrepareFormattedAttribute(Product product, IList<CustomAttribute> customAttributes, string langId,
            string serapator, bool htmlEncode, bool renderPrices,
            bool allowHyperlinks, bool showInAdmin)
        {
            var result = new StringBuilder();
            var attributes = _productAttributeParser.ParseProductAttributeMappings(product, customAttributes);
            for (int i = 0; i < attributes.Count; i++)
            {
                var productAttribute = await _productAttributeService.GetProductAttributeById(attributes[i].ProductAttributeId);
                var attribute = attributes[i];
                var valuesStr = _productAttributeParser.ParseValues(customAttributes, attribute.Id);
                for (int j = 0; j < valuesStr.Count; j++)
                {
                    string valueStr = valuesStr[j];
                    string formattedAttribute = string.Empty;
                    if (!attribute.ShouldHaveValues())
                    {
                        //no values
                        if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                        {
                            //multiline textbox
                            var attributeName = productAttribute.GetLocalized(a => a.Name, langId);
                            //encode (if required)
                            if (htmlEncode)
                                attributeName = WebUtility.HtmlEncode(attributeName);
                            formattedAttribute = string.Format("{0}: {1}", attributeName, FormatText.ConvertText(valueStr));
                            //we never encode multiline textbox input
                        }
                        else if (attribute.AttributeControlType == AttributeControlType.FileUpload)
                        {
                            //file upload
                            Guid downloadGuid;
                            Guid.TryParse(valueStr, out downloadGuid);
                            var download = await _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                                string attributeText = "";
                                var fileName = string.Format("{0}{1}",
                                    download.Filename ?? download.DownloadGuid.ToString(),
                                    download.Extension);
                                //encode (if required)
                                if (htmlEncode)
                                    fileName = WebUtility.HtmlEncode(fileName);
                                if (allowHyperlinks)
                                {
                                    //hyperlinks are allowed
                                    var downloadLink = string.Format("{0}download/getfileupload/?downloadId={1}", _webHelper.GetStoreLocation(false), download.DownloadGuid);
                                    attributeText = string.Format("<a href=\"{0}\" class=\"fileuploadattribute\">{1}</a>", downloadLink, fileName);
                                }
                                else
                                {
                                    //hyperlinks aren't allowed
                                    attributeText = fileName;
                                }
                                var attributeName = productAttribute.GetLocalized(a => a.Name, langId);
                                //encode (if required)
                                if (htmlEncode)
                                    attributeName = WebUtility.HtmlEncode(attributeName);
                                formattedAttribute = string.Format("{0}: {1}", attributeName, attributeText);
                            }
                        }
                        else
                        {
                            //other attributes (textbox, datepicker)
                            formattedAttribute = string.Format("{0}: {1}", productAttribute.GetLocalized(a => a.Name, langId), valueStr);
                            //encode (if required)
                            if (htmlEncode)
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                        }
                    }
                    else
                    {
                        //attributes with values
                        if (product.ProductAttributeMappings.Where(x => x.Id == attributes[i].Id).FirstOrDefault() != null)
                        {

                            var attributeValue = product.ProductAttributeMappings.Where(x => x.Id == attributes[i].Id).FirstOrDefault().ProductAttributeValues.Where(x => x.Id == valueStr).FirstOrDefault();
                            if (attributeValue != null)
                            {
                                formattedAttribute = string.Format("{0}: {1}", productAttribute.GetLocalized(a => a.Name, langId), attributeValue.GetLocalized(a => a.Name, langId));

                                if (renderPrices)
                                {
                                    decimal attributeValuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustment(attributeValue);
                                    var prices = await _taxService.GetProductPrice(product, attributeValuePriceAdjustment, _workContext.CurrentCustomer);
                                    decimal priceAdjustmentBase = prices.productprice;
                                    decimal taxRate = prices.taxRate;
                                    decimal priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                                    if (priceAdjustmentBase > 0)
                                    {
                                        string priceAdjustmentStr = _priceFormatter.FormatPrice(priceAdjustment, false, false);
                                        formattedAttribute += string.Format(" [+{0}]", priceAdjustmentStr);
                                    }
                                    else if (priceAdjustmentBase < decimal.Zero)
                                    {
                                        string priceAdjustmentStr = _priceFormatter.FormatPrice(-priceAdjustment, false, false);
                                        formattedAttribute += string.Format(" [-{0}]", priceAdjustmentStr);
                                    }
                                }

                                //display quantity
                                if (_shoppingCartSettings.RenderAssociatedAttributeValueQuantity &&
                                    attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                                {
                                    //render only when more than 1
                                    if (attributeValue.Quantity > 1)
                                    {
                                        //TODO localize resource
                                        formattedAttribute += string.Format(" - qty {0}", attributeValue.Quantity);
                                    }
                                }
                            }
                            else
                            {
                                if (showInAdmin)
                                    formattedAttribute += string.Format("{0}: {1}", productAttribute.GetLocalized(a => a.Name, langId), "");
                            }

                            //encode (if required)
                            if (htmlEncode)
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                        }
                    }

                    if (!string.IsNullOrEmpty(formattedAttribute))
                    {
                        if (i != 0 || j != 0)
                            result.Append(serapator);
                        result.Append(formattedAttribute);
                    }
                }
            }
            return result;
        }
    }
}
