using PowerStore.Core;
using PowerStore.Core.Caching;
using PowerStore.Domain.Data;
using PowerStore.Domain.Catalog;
using PowerStore.Domain.Orders;
using PowerStore.Services.Customers;
using PowerStore.Services.Events;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PowerStore.Core.Caching.Constants;

namespace PowerStore.Services.Orders
{
    /// <summary>
    /// Checkout attribute service
    /// </summary>
    public partial class CheckoutAttributeService : ICheckoutAttributeService
    {
        #region Fields

        private readonly IRepository<CheckoutAttribute> _checkoutAttributeRepository;
        private readonly IMediator _mediator;
        private readonly ICacheBase _cacheBase;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="checkoutAttributeRepository">Checkout attribute repository</param>
        /// <param name="mediator">Mediator</param>
        public CheckoutAttributeService(ICacheBase cacheManager,
            IRepository<CheckoutAttribute> checkoutAttributeRepository,
            IMediator mediator,
            IWorkContext workContext,
            CatalogSettings catalogSettings)
        {
            _cacheBase = cacheManager;
            _checkoutAttributeRepository = checkoutAttributeRepository;
            _mediator = mediator;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        public virtual async Task DeleteCheckoutAttribute(CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                throw new ArgumentNullException("checkoutAttribute");

            await _checkoutAttributeRepository.DeleteAsync(checkoutAttribute);

            await _cacheBase.RemoveByPrefix(CacheKey.CHECKOUTATTRIBUTES_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(CacheKey.CHECKOUTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(checkoutAttribute);
        }

        /// <summary>
        /// Gets all checkout attributes
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="excludeShippableAttributes">A value indicating whether we should exlude shippable attributes</param>
        /// <returns>Checkout attributes</returns>
        public virtual async Task<IList<CheckoutAttribute>> GetAllCheckoutAttributes(string storeId = "", bool excludeShippableAttributes = false, bool ignorAcl = false)
        {
            string key = string.Format(CacheKey.CHECKOUTATTRIBUTES_ALL_KEY, storeId, excludeShippableAttributes, ignorAcl);
            return await _cacheBase.GetAsync(key, () =>
            {
                var query = _checkoutAttributeRepository.Table;
                query = query.OrderBy(c => c.DisplayOrder);

                if ((!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations) ||
                    (!ignorAcl && !_catalogSettings.IgnoreAcl))
                {
                    if (!ignorAcl && !_catalogSettings.IgnoreAcl)
                    {
                        var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                        query = from p in query
                                where !p.SubjectToAcl || allowedCustomerRolesIds.Any(x => p.CustomerRoles.Contains(x))
                                select p;
                    }
                    //Store mapping
                    if (!String.IsNullOrEmpty(storeId) && !_catalogSettings.IgnoreStoreLimitations)
                    {
                        query = from p in query
                                where !p.LimitedToStores || p.Stores.Contains(storeId)
                                select p; 
                    }
                }
                if (excludeShippableAttributes)
                {
                    query = query.Where(x => !x.ShippableProductRequired);
                }
                return query.ToListAsync();

            });
        }

        /// <summary>
        /// Gets a checkout attribute 
        /// </summary>
        /// <param name="checkoutAttributeId">Checkout attribute identifier</param>
        /// <returns>Checkout attribute</returns>
        public virtual Task<CheckoutAttribute> GetCheckoutAttributeById(string checkoutAttributeId)
        {
            string key = string.Format(CacheKey.CHECKOUTATTRIBUTES_BY_ID_KEY, checkoutAttributeId);
            return _cacheBase.GetAsync(key, () => _checkoutAttributeRepository.GetByIdAsync(checkoutAttributeId));
        }

        /// <summary>
        /// Inserts a checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        public virtual async Task InsertCheckoutAttribute(CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                throw new ArgumentNullException("checkoutAttribute");

            await _checkoutAttributeRepository.InsertAsync(checkoutAttribute);

            await _cacheBase.RemoveByPrefix(CacheKey.CHECKOUTATTRIBUTES_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(CacheKey.CHECKOUTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(checkoutAttribute);
        }

        /// <summary>
        /// Updates the checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        public virtual async Task UpdateCheckoutAttribute(CheckoutAttribute checkoutAttribute)
        {
            if (checkoutAttribute == null)
                throw new ArgumentNullException("checkoutAttribute");

            await _checkoutAttributeRepository.UpdateAsync(checkoutAttribute);

            await _cacheBase.RemoveByPrefix(CacheKey.CHECKOUTATTRIBUTES_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(CacheKey.CHECKOUTATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(checkoutAttribute);
        }

        #endregion
    }
}
