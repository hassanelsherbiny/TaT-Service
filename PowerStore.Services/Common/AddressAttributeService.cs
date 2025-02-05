using PowerStore.Core.Caching;
using PowerStore.Core.Caching.Constants;
using PowerStore.Domain.Common;
using PowerStore.Domain.Data;
using PowerStore.Services.Events;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerStore.Services.Common
{
    /// <summary>
    /// Address attribute service
    /// </summary>
    public partial class AddressAttributeService : IAddressAttributeService
    {
        #region Fields

        private readonly IRepository<AddressAttribute> _addressAttributeRepository;
        private readonly IMediator _mediator;
        private readonly ICacheBase _cacheBase;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="addressAttributeRepository">Address attribute repository</param>
        /// <param name="mediator">Mediator</param>
        public AddressAttributeService(ICacheBase cacheManager,
            IRepository<AddressAttribute> addressAttributeRepository,
            IMediator mediator)
        {
            _cacheBase = cacheManager;
            _addressAttributeRepository = addressAttributeRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes an address attribute
        /// </summary>
        /// <param name="addressAttribute">Address attribute</param>
        public virtual async Task DeleteAddressAttribute(AddressAttribute addressAttribute)
        {
            if (addressAttribute == null)
                throw new ArgumentNullException("addressAttribute");

            await _addressAttributeRepository.DeleteAsync(addressAttribute);

            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTES_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(addressAttribute);
        }

        /// <summary>
        /// Gets all address attributes
        /// </summary>
        /// <returns>Address attributes</returns>
        public virtual async Task<IList<AddressAttribute>> GetAllAddressAttributes()
        {
            string key = CacheKey.ADDRESSATTRIBUTES_ALL_KEY;
            return await _cacheBase.GetAsync(key, () =>
            {
                var query = from aa in _addressAttributeRepository.Table
                            orderby aa.DisplayOrder
                            select aa;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Gets an address attribute 
        /// </summary>
        /// <param name="addressAttributeId">Address attribute identifier</param>
        /// <returns>Address attribute</returns>
        public virtual async Task<AddressAttribute> GetAddressAttributeById(string addressAttributeId)
        {
            if (String.IsNullOrEmpty(addressAttributeId))
                return null;

            string key = string.Format(CacheKey.ADDRESSATTRIBUTES_BY_ID_KEY, addressAttributeId);
            return await _cacheBase.GetAsync(key, () => _addressAttributeRepository.GetByIdAsync(addressAttributeId));
        }

        /// <summary>
        /// Inserts an address attribute
        /// </summary>
        /// <param name="addressAttribute">Address attribute</param>
        public virtual async Task InsertAddressAttribute(AddressAttribute addressAttribute)
        {
            if (addressAttribute == null)
                throw new ArgumentNullException("addressAttribute");

            await _addressAttributeRepository.InsertAsync(addressAttribute);

            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTES_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(addressAttribute);
        }

        /// <summary>
        /// Updates the address attribute
        /// </summary>
        /// <param name="addressAttribute">Address attribute</param>
        public virtual async Task UpdateAddressAttribute(AddressAttribute addressAttribute)
        {
            if (addressAttribute == null)
                throw new ArgumentNullException("addressAttribute");

            await _addressAttributeRepository.UpdateAsync(addressAttribute);

            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTES_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(addressAttribute);
        }

        /// <summary>
        /// Deletes an address attribute value
        /// </summary>
        /// <param name="addressAttributeValue">Address attribute value</param>
        public virtual async Task DeleteAddressAttributeValue(AddressAttributeValue addressAttributeValue)
        {
            if (addressAttributeValue == null)
                throw new ArgumentNullException("addressAttributeValue");

            var updatebuilder = Builders<AddressAttribute>.Update;
            var update = updatebuilder.Pull(p => p.AddressAttributeValues, addressAttributeValue);
            await _addressAttributeRepository.Collection.UpdateOneAsync(new BsonDocument("_id", addressAttributeValue.AddressAttributeId), update);

            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTES_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(addressAttributeValue);
        }

        /// <summary>
        /// Inserts an address attribute value
        /// </summary>
        /// <param name="addressAttributeValue">Address attribute value</param>
        public virtual async Task InsertAddressAttributeValue(AddressAttributeValue addressAttributeValue)
        {
            if (addressAttributeValue == null)
                throw new ArgumentNullException("addressAttributeValue");

            var updatebuilder = Builders<AddressAttribute>.Update;
            var update = updatebuilder.AddToSet(p => p.AddressAttributeValues, addressAttributeValue);
            await _addressAttributeRepository.Collection.UpdateOneAsync(new BsonDocument("_id", addressAttributeValue.AddressAttributeId), update);

            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTES_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(addressAttributeValue);
        }

        /// <summary>
        /// Updates the address attribute value
        /// </summary>
        /// <param name="addressAttributeValue">Address attribute value</param>
        public virtual async Task UpdateAddressAttributeValue(AddressAttributeValue addressAttributeValue)
        {
            if (addressAttributeValue == null)
                throw new ArgumentNullException("addressAttributeValue");

            var builder = Builders<AddressAttribute>.Filter;
            var filter = builder.Eq(x => x.Id, addressAttributeValue.AddressAttributeId);
            filter = filter & builder.ElemMatch(x => x.AddressAttributeValues, y => y.Id == addressAttributeValue.Id);
            var update = Builders<AddressAttribute>.Update
                .Set(x => x.AddressAttributeValues.ElementAt(-1).DisplayOrder, addressAttributeValue.DisplayOrder)
                .Set(x => x.AddressAttributeValues.ElementAt(-1).IsPreSelected, addressAttributeValue.IsPreSelected)
                .Set(x => x.AddressAttributeValues.ElementAt(-1).Locales, addressAttributeValue.Locales)
                .Set(x => x.AddressAttributeValues.ElementAt(-1).Name, addressAttributeValue.Name);

            await _addressAttributeRepository.Collection.UpdateManyAsync(filter, update);

            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTES_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(CacheKey.ADDRESSATTRIBUTEVALUES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(addressAttributeValue);
        }

        #endregion
    }
}
