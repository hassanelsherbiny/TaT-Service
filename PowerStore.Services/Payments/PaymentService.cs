using PowerStore.Core;
using PowerStore.Domain.Customers;
using PowerStore.Domain.Orders;
using PowerStore.Domain.Payments;
using PowerStore.Domain.Shipping;
using PowerStore.Core.Plugins;
using PowerStore.Services.Catalog;
using PowerStore.Services.Common;
using PowerStore.Services.Configuration;
using PowerStore.Services.Directory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerStore.Services.Payments
{
    /// <summary>
    /// Payment service
    /// </summary>
    public partial class PaymentService : IPaymentService
    {
        #region Fields

        private readonly PaymentSettings _paymentSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="paymentSettings">Payment settings</param>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="settingService">Setting service</param>
        /// <param name="shoppingCartSettings">Shopping cart settings</param>
        public PaymentService(PaymentSettings paymentSettings,
            IPluginFinder pluginFinder,
            ISettingService settingService,
            ICurrencyService currencyService,
            ShoppingCartSettings shoppingCartSettings)
        {
            _paymentSettings = paymentSettings;
            _pluginFinder = pluginFinder;
            _settingService = settingService;
            _currencyService = currencyService;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load active payment methods
        /// </summary>
        /// <param name="filterByCustomerId">Filter payment methods by customer; null to load all records</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <param name="filterByCountryId">Load records allowed only in a specified country; pass "" to load all records</param>
        /// <returns>Payment methods</returns>
        public virtual async Task<IList<IPaymentMethod>> LoadActivePaymentMethods(Customer filterByCustomer = null, string storeId = "", string filterByCountryId = "")
        {
            var pm = LoadAllPaymentMethods(storeId, filterByCountryId)
                   .Where(provider => _paymentSettings.ActivePaymentMethodSystemNames.Contains(provider.PluginDescriptor.SystemName, StringComparer.OrdinalIgnoreCase))
                   .ToList();

            if (filterByCustomer != null)
            {

                var selectedShippingOption = filterByCustomer.GetAttributeFromEntity<ShippingOption>(
                       SystemCustomerAttributeNames.SelectedShippingOption, storeId);

                for (int i = pm.Count - 1; i >= 0; i--)
                {
                    var restictedRoleIds = GetRestrictedRoleIds(pm[i]);
                    if (filterByCustomer.CustomerRoles.Where(x => restictedRoleIds.Contains(x.Id)).Count() > 0)
                    {
                        pm.Remove(pm[i]);
                    }
                }

                if (selectedShippingOption != null)
                {
                    for (int i = pm.Count - 1; i >= 0; i--)
                    {
                        var restictedRoleIds = GetRestrictedShippingIds(pm[i]);
                        if (restictedRoleIds.Contains(selectedShippingOption.Name))
                        {
                            pm.Remove(pm[i]);
                        }
                    }
                }

            }
            return await Task.FromResult(pm);
        }

        /// <summary>
        /// Load payment provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found payment provider</returns>
        public virtual IPaymentMethod LoadPaymentMethodBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IPaymentMethod>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IPaymentMethod>(_pluginFinder.ServiceProvider);

            return null;
        }

        /// <summary>
        /// Load all payment providers
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <param name="filterByCountryId">Load records allowed only in a specified country; pass "" to load all records</param>
        /// <returns>Payment providers</returns>
        public virtual IList<IPaymentMethod> LoadAllPaymentMethods(string storeId = "", string filterByCountryId = "")
        {
            var paymentMethods = _pluginFinder.GetPlugins<IPaymentMethod>(storeId: storeId).ToList();
            if (String.IsNullOrEmpty(filterByCountryId))
                return paymentMethods;

            //filter by country
            var paymentMetodsByCountry = new List<IPaymentMethod>();
            foreach (var pm in paymentMethods)
            {
                var restictedCountryIds = GetRestrictedCountryIds(pm);
                if (!restictedCountryIds.Contains(filterByCountryId))
                {
                    paymentMetodsByCountry.Add(pm);
                }
            }
            return paymentMetodsByCountry;
        }

        /// <summary>
        /// Gets a list of coutnry identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <returns>A list of country identifiers</returns>
        public virtual IList<string> GetRestrictedCountryIds(IPaymentMethod paymentMethod)
        {
            if (paymentMethod == null)
                throw new ArgumentNullException("paymentMethod");

            var settingKey = string.Format("PaymentMethodRestictions.{0}", paymentMethod.PluginDescriptor.SystemName);
            var restictedCountryIds = _settingService.GetSettingByKey<List<string>>(settingKey);
            if (restictedCountryIds == null)
                restictedCountryIds = new List<string>();
            return restictedCountryIds;
        }

        /// <summary>
        /// Gets a list of role identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <returns>A list of role identifiers</returns>
        public virtual IList<string> GetRestrictedRoleIds(IPaymentMethod paymentMethod)
        {
            if (paymentMethod == null)
                throw new ArgumentNullException("paymentMethod");

            var settingKey = string.Format("PaymentMethodRestictionsRole.{0}", paymentMethod.PluginDescriptor.SystemName);
            var restictedRoleIds = _settingService.GetSettingByKey<List<string>>(settingKey);
            if (restictedRoleIds == null)
                restictedRoleIds = new List<string>();
            return restictedRoleIds;
        }

        /// <summary>
        /// Gets a list of role identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <returns>A list of role identifiers</returns>
        public virtual IList<string> GetRestrictedShippingIds(IPaymentMethod paymentMethod)
        {
            if (paymentMethod == null)
                throw new ArgumentNullException("paymentMethod");

            var settingKey = string.Format("PaymentMethodRestictionsShipping.{0}", paymentMethod.PluginDescriptor.SystemName);
            var restictedShippingIds = _settingService.GetSettingByKey<List<string>>(settingKey);
            if (restictedShippingIds == null)
                restictedShippingIds = new List<string>();
            return restictedShippingIds;
        }
        /// <summary>
        /// Saves a list of country identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <param name="countryIds">A list of country identifiers</param>
        public virtual async Task SaveRestictedCountryIds(IPaymentMethod paymentMethod, List<string> countryIds)
        {
            if (paymentMethod == null)
                throw new ArgumentNullException("paymentMethod");

            var settingKey = string.Format("PaymentMethodRestictions.{0}", paymentMethod.PluginDescriptor.SystemName);
            await _settingService.SetSetting(settingKey, countryIds);
        }

        /// <summary>
        /// Saves a list of role identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <param name="countryIds">A list of country identifiers</param>
        public virtual async Task SaveRestictedRoleIds(IPaymentMethod paymentMethod, List<string> roleIds)
        {
            if (paymentMethod == null)
                throw new ArgumentNullException("paymentMethod");

            var settingKey = string.Format("PaymentMethodRestictionsRole.{0}", paymentMethod.PluginDescriptor.SystemName);
            await _settingService.SetSetting(settingKey, roleIds);
        }

        /// <summary>
        /// Saves a list of shipping identifiers in which a certain payment method is now allowed
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <param name="shippingIds">A list of country identifiers</param>
        public virtual async Task SaveRestictedShippingIds(IPaymentMethod paymentMethod, List<string> shippingIds)
        {
            if (paymentMethod == null)
                throw new ArgumentNullException("paymentMethod");

            var settingKey = string.Format("PaymentMethodRestictionsShipping.{0}", paymentMethod.PluginDescriptor.SystemName);
            await _settingService.SetSetting(settingKey, shippingIds);
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public virtual async Task<ProcessPaymentResult> ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest.OrderTotal == decimal.Zero)
            {
                var result = new ProcessPaymentResult
                {
                    NewPaymentStatus = PaymentStatus.Paid
                };
                return result;
            }

            //We should strip out any white space or dash in the CC number entered.
            if (!String.IsNullOrWhiteSpace(processPaymentRequest.CreditCardNumber))
            {
                processPaymentRequest.CreditCardNumber = processPaymentRequest.CreditCardNumber.Replace(" ", "");
                processPaymentRequest.CreditCardNumber = processPaymentRequest.CreditCardNumber.Replace("-", "");
            }
            var paymentMethod = LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new PowerStoreException("Payment method couldn't be loaded");
            return await paymentMethod.ProcessPayment(processPaymentRequest);
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public virtual async Task PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //already paid or order.OrderTotal == decimal.Zero
            if (postProcessPaymentRequest.Order.PaymentStatus == PaymentStatus.Paid)
                return;

            var paymentMethod = LoadPaymentMethodBySystemName(postProcessPaymentRequest.Order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new PowerStoreException("Payment method couldn't be loaded");
            await paymentMethod.PostProcessPayment(postProcessPaymentRequest);
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public virtual async Task<bool> CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (!_paymentSettings.AllowRePostingPayments)
                return false;

            var paymentMethod = LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            if (paymentMethod == null)
                return false; //Payment method couldn't be loaded (for example, was uninstalled)

            if (paymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
                return false;   //this option is available only for redirection payment methods

            if (order.Deleted)
                return false;  //do not allow for deleted orders

            if (order.OrderStatus == OrderStatus.Cancelled)
                return false;  //do not allow for cancelled orders

            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;  //payment status should be Pending

            return await paymentMethod.CanRePostProcessPayment(order);
        }



        /// <summary>
        /// Gets an additional handling fee of a payment method
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Additional handling fee</returns>
        public virtual async Task<decimal> GetAdditionalHandlingFee(IList<ShoppingCartItem> cart, string paymentMethodSystemName)
        {
            if (String.IsNullOrEmpty(paymentMethodSystemName))
                return decimal.Zero;

            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return decimal.Zero;

            decimal result = await paymentMethod.GetAdditionalHandlingFee(cart);
            if (result < decimal.Zero)
                result = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
            {
                var currency = await _currencyService.GetPrimaryExchangeRateCurrency();
                result = RoundingHelper.RoundPrice(result, currency);
            }
            return result;
        }



        /// <summary>
        /// Gets a value indicating whether capture is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether capture is supported</returns>
        public virtual async Task<bool> SupportCapture(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return false;
            return await paymentMethod.SupportCapture();
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public virtual async Task<CapturePaymentResult> Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(capturePaymentRequest.Order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new PowerStoreException("Payment method couldn't be loaded");
            return await paymentMethod.Capture(capturePaymentRequest);
        }



        /// <summary>
        /// Gets a value indicating whether partial refund is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether partial refund is supported</returns>
        public virtual async Task<bool> SupportPartiallyRefund(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return false;
            return await paymentMethod.SupportPartiallyRefund();
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether refund is supported</returns>
        public virtual async Task<bool> SupportRefund(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return false;
            return await paymentMethod.SupportRefund();
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public virtual async Task<RefundPaymentResult> Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(refundPaymentRequest.Order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new PowerStoreException("Payment method couldn't be loaded");
            return await paymentMethod.Refund(refundPaymentRequest);
        }



        /// <summary>
        /// Gets a value indicating whether void is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether void is supported</returns>
        public virtual async Task<bool> SupportVoid(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return false;
            return await paymentMethod.SupportVoid();
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public virtual async Task<VoidPaymentResult> Void(VoidPaymentRequest voidPaymentRequest)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(voidPaymentRequest.Order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new PowerStoreException("Payment method couldn't be loaded");
            return await paymentMethod.Void(voidPaymentRequest);
        }



        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A recurring payment type of payment method</returns>
        public virtual RecurringPaymentType GetRecurringPaymentType(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return RecurringPaymentType.NotSupported;
            return paymentMethod.RecurringPaymentType;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public virtual async Task<ProcessPaymentResult> ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest.OrderTotal == decimal.Zero)
            {
                var result = new ProcessPaymentResult
                {
                    NewPaymentStatus = PaymentStatus.Paid
                };
                return result;
            }

            var paymentMethod = LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new PowerStoreException("Payment method couldn't be loaded");
            return await paymentMethod.ProcessRecurringPayment(processPaymentRequest);
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public virtual async Task<CancelRecurringPaymentResult> CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            if (cancelPaymentRequest.Order.OrderTotal == decimal.Zero)
                return new CancelRecurringPaymentResult();

            var paymentMethod = LoadPaymentMethodBySystemName(cancelPaymentRequest.Order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new PowerStoreException("Payment method couldn't be loaded");
            return await paymentMethod.CancelRecurringPayment(cancelPaymentRequest);
        }



        /// <summary>
        /// Gets a payment method type
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A payment method type</returns>
        public virtual PaymentMethodType GetPaymentMethodType(string paymentMethodSystemName)
        {
            var paymentMethod = LoadPaymentMethodBySystemName(paymentMethodSystemName);
            if (paymentMethod == null)
                return PaymentMethodType.Unknown;
            return paymentMethod.PaymentMethodType;
        }

        /// <summary>
        /// Gets masked credit card number
        /// </summary>
        /// <param name="creditCardNumber">Credit card number</param>
        /// <returns>Masked credit card number</returns>
        public virtual string GetMaskedCreditCardNumber(string creditCardNumber)
        {
            if (String.IsNullOrEmpty(creditCardNumber))
                return string.Empty;

            if (creditCardNumber.Length <= 4)
                return creditCardNumber;

            string last4 = creditCardNumber.Substring(creditCardNumber.Length - 4, 4);
            string maskedChars = string.Empty;
            for (int i = 0; i < creditCardNumber.Length - 4; i++)
            {
                maskedChars += "*";
            }
            return maskedChars + last4;
        }

        #endregion
    }
}
