using PowerStore.Core.Caching;
using PowerStore.Core.Caching.Constants;
using PowerStore.Domain.Data;
using PowerStore.Domain.Directory;
using PowerStore.Services.Events;
using PowerStore.Services.Localization;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerStore.Services.Directory
{
    /// <summary>
    /// State province service
    /// </summary>
    public partial class StateProvinceService : IStateProvinceService
    {
        

        #region Fields

        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IMediator _mediator;
        private readonly ICacheBase _cacheBase;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="stateProvinceRepository">State/province repository</param>
        /// <param name="mediator">Mediator</param>
        public StateProvinceService(ICacheBase cacheManager,
            IRepository<StateProvince> stateProvinceRepository,
            IMediator mediator)
        {
            _cacheBase = cacheManager;
            _stateProvinceRepository = stateProvinceRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Deletes a state/province
        /// </summary>
        /// <param name="stateProvince">The state/province</param>
        public virtual async Task DeleteStateProvince(StateProvince stateProvince)
        {
            if (stateProvince == null)
                throw new ArgumentNullException("stateProvince");

            await _stateProvinceRepository.DeleteAsync(stateProvince);

            await _cacheBase.RemoveByPrefix(CacheKey.STATEPROVINCES_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(stateProvince);
        }

        /// <summary>
        /// Gets a state/province
        /// </summary>
        /// <param name="stateProvinceId">The state/province identifier</param>
        /// <returns>State/province</returns>
        public virtual async Task<StateProvince> GetStateProvinceById(string stateProvinceId)
        {
            if (string.IsNullOrEmpty(stateProvinceId))
                return null;

            var key = string.Format(CacheKey.STATEPROVINCES_BY_KEY, stateProvinceId);
            return await _cacheBase.GetAsync(key, () => _stateProvinceRepository.GetByIdAsync(stateProvinceId));

        }

        /// <summary>
        /// Gets a state/province collection by country identifier
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <param name="languageId">Language identifier. It's used to sort states by localized names (if specified); pass 0 to skip it</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>States</returns>
        public virtual async Task<IList<StateProvince>> GetStateProvincesByCountryId(string countryId, string languageId = "", bool showHidden = false)
        {
            string key = string.Format(CacheKey.STATEPROVINCES_ALL_KEY, countryId, languageId, showHidden);
            return await _cacheBase.GetAsync(key, async () =>
            {
                var query = from sp in _stateProvinceRepository.Table
                            orderby sp.DisplayOrder, sp.Name
                            where sp.CountryId == countryId &&
                            (showHidden || sp.Published)
                            select sp;
                var stateProvinces = await query.ToListAsync();

                if (!String.IsNullOrEmpty(languageId))
                {
                    //we should sort states by localized names when they have the same display order
                    stateProvinces = stateProvinces
                        .OrderBy(c => c.DisplayOrder)
                        .ThenBy(c => c.GetLocalized(x => x.Name, languageId))
                        .ToList();
                }
                return stateProvinces;
            });
        }

        /// <summary>
        /// Gets all states/provinces
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>State/province collection</returns>
        public virtual async Task<IList<StateProvince>> GetStateProvinces(bool showHidden = false)
        {
            var query = from sp in _stateProvinceRepository.Table
                        orderby sp.CountryId, sp.DisplayOrder, sp.Name
                        where showHidden || sp.Published
                        select sp;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Inserts a state/province
        /// </summary>
        /// <param name="stateProvince">State/province</param>
        public virtual async Task InsertStateProvince(StateProvince stateProvince)
        {
            if (stateProvince == null)
                throw new ArgumentNullException("stateProvince");

            await _stateProvinceRepository.InsertAsync(stateProvince);

            await _cacheBase.RemoveByPrefix(CacheKey.STATEPROVINCES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(stateProvince);
        }

        /// <summary>
        /// Updates a state/province
        /// </summary>
        /// <param name="stateProvince">State/province</param>
        public virtual async Task UpdateStateProvince(StateProvince stateProvince)
        {
            if (stateProvince == null)
                throw new ArgumentNullException("stateProvince");

            await _stateProvinceRepository.UpdateAsync(stateProvince);

            await _cacheBase.RemoveByPrefix(CacheKey.STATEPROVINCES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(stateProvince);
        }

        #endregion
    }
}
