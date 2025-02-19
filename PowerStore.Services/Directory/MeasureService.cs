using PowerStore.Core;
using PowerStore.Core.Caching;
using PowerStore.Core.Caching.Constants;
using PowerStore.Domain.Data;
using PowerStore.Domain.Directory;
using PowerStore.Services.Commands.Models.Catalog;
using PowerStore.Services.Events;
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
    /// Measure dimension service
    /// </summary>
    public partial class MeasureService : IMeasureService
    {
        

        #region Fields

        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly IRepository<MeasureUnit> _measureUnitRepository;
        private readonly ICacheBase _cacheBase;
        private readonly MeasureSettings _measureSettings;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="measureDimensionRepository">Dimension repository</param>
        /// <param name="measureWeightRepository">Weight repository</param>
        /// <param name="measureUnitRepository">Unit repository</param>
        /// <param name="measureSettings">Measure settings</param>
        /// <param name="mediator">Mediator</param>
        public MeasureService(ICacheBase cacheManager,
            IRepository<MeasureDimension> measureDimensionRepository,
            IRepository<MeasureWeight> measureWeightRepository,
            IRepository<MeasureUnit> measureUnitRepository,
            MeasureSettings measureSettings,
            IMediator mediator)
        {
            _cacheBase = cacheManager;
            _measureDimensionRepository = measureDimensionRepository;
            _measureWeightRepository = measureWeightRepository;
            _measureUnitRepository = measureUnitRepository;
            _measureSettings = measureSettings;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        #region Dimensions

        /// <summary>
        /// Deletes measure dimension
        /// </summary>
        /// <param name="measureDimension">Measure dimension</param>
        public virtual async Task DeleteMeasureDimension(MeasureDimension measureDimension)
        {
            if (measureDimension == null)
                throw new ArgumentNullException("measureDimension");

            await _measureDimensionRepository.DeleteAsync(measureDimension);

            await _cacheBase.RemoveByPrefix(CacheKey.MEASUREDIMENSIONS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(measureDimension);
        }

        /// <summary>
        /// Gets a measure dimension by identifier
        /// </summary>
        /// <param name="measureDimensionId">Measure dimension identifier</param>
        /// <returns>Measure dimension</returns>
        public virtual Task<MeasureDimension> GetMeasureDimensionById(string measureDimensionId)
        {
            string key = string.Format(CacheKey.MEASUREDIMENSIONS_BY_ID_KEY, measureDimensionId);
            return _cacheBase.GetAsync(key, () => _measureDimensionRepository.GetByIdAsync(measureDimensionId));
        }

        /// <summary>
        /// Gets a measure dimension by system keyword
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <returns>Measure dimension</returns>
        public virtual async Task<MeasureDimension> GetMeasureDimensionBySystemKeyword(string systemKeyword)
        {
            if (String.IsNullOrEmpty(systemKeyword))
                return null;

            var measureDimensions = await GetAllMeasureDimensions();
            foreach (var measureDimension in measureDimensions)
                if (measureDimension.SystemKeyword.ToLowerInvariant() == systemKeyword.ToLowerInvariant())
                    return measureDimension;
            return null;
        }

        /// <summary>
        /// Gets all measure dimensions
        /// </summary>
        /// <returns>Measure dimensions</returns>
        public virtual async Task<IList<MeasureDimension>> GetAllMeasureDimensions()
        {
            string key = CacheKey.MEASUREDIMENSIONS_ALL_KEY;
            return await _cacheBase.GetAsync(key, () =>
            {
                var query = from md in _measureDimensionRepository.Table
                            orderby md.DisplayOrder
                            select md;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Inserts a measure dimension
        /// </summary>
        /// <param name="measure">Measure dimension</param>
        public virtual async Task InsertMeasureDimension(MeasureDimension measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            await _measureDimensionRepository.InsertAsync(measure);

            await _cacheBase.RemoveByPrefix(CacheKey.MEASUREDIMENSIONS_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(measure);
        }

        /// <summary>
        /// Updates the measure dimension
        /// </summary>
        /// <param name="measure">Measure dimension</param>
        public virtual async Task UpdateMeasureDimension(MeasureDimension measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            await _measureDimensionRepository.UpdateAsync(measure);

            await _cacheBase.RemoveByPrefix(CacheKey.MEASUREDIMENSIONS_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(measure);
        }

        /// <summary>
        /// Converts dimension
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="sourceMeasureDimension">Source dimension</param>
        /// <param name="targetMeasureDimension">Target dimension</param>
        /// <param name="round">A value indicating whether a result should be rounded</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertDimension(decimal value,
            MeasureDimension sourceMeasureDimension, MeasureDimension targetMeasureDimension, bool round = true)
        {
            if (sourceMeasureDimension == null)
                throw new ArgumentNullException("sourceMeasureDimension");

            if (targetMeasureDimension == null)
                throw new ArgumentNullException("targetMeasureDimension");

            decimal result = value;
            if (result != decimal.Zero && sourceMeasureDimension.Id != targetMeasureDimension.Id)
            {
                result = await ConvertToPrimaryMeasureDimension(result, sourceMeasureDimension);
                result = await ConvertFromPrimaryMeasureDimension(result, targetMeasureDimension);
            }
            if (round)
                result = Math.Round(result, 2);
            return result;
        }

        /// <summary>
        /// Converts to primary measure dimension
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="sourceMeasureDimension">Source dimension</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertToPrimaryMeasureDimension(decimal value,
            MeasureDimension sourceMeasureDimension)
        {
            if (sourceMeasureDimension == null)
                throw new ArgumentNullException("sourceMeasureDimension");

            decimal result = value;
            var baseDimensionIn = await GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            if (result != decimal.Zero && sourceMeasureDimension.Id != baseDimensionIn.Id)
            {
                decimal exchangeRatio = sourceMeasureDimension.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new PowerStoreException(string.Format("Exchange ratio not set for dimension [{0}]", sourceMeasureDimension.Name));
                result = result / exchangeRatio;
            }
            return result;
        }

        /// <summary>
        /// Converts from primary dimension
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="targetMeasureDimension">Target dimension</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertFromPrimaryMeasureDimension(decimal value,
            MeasureDimension targetMeasureDimension)
        {
            if (targetMeasureDimension == null)
                throw new ArgumentNullException("targetMeasureDimension");

            decimal result = value;
            var baseDimensionIn = await GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            if (result != decimal.Zero && targetMeasureDimension.Id != baseDimensionIn.Id)
            {
                decimal exchangeRatio = targetMeasureDimension.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new PowerStoreException(string.Format("Exchange ratio not set for dimension [{0}]", targetMeasureDimension.Name));
                result = result * exchangeRatio;
            }
            return result;
        }

        #endregion

        #region Weights

        /// <summary>
        /// Deletes measure weight
        /// </summary>
        /// <param name="measureWeight">Measure weight</param>
        public virtual async Task DeleteMeasureWeight(MeasureWeight measureWeight)
        {
            if (measureWeight == null)
                throw new ArgumentNullException("measureWeight");

            await _measureWeightRepository.DeleteAsync(measureWeight);

            await _cacheBase.RemoveByPrefix(CacheKey.MEASUREWEIGHTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(measureWeight);
        }

        /// <summary>
        /// Gets a measure weight by identifier
        /// </summary>
        /// <param name="measureWeightId">Measure weight identifier</param>
        /// <returns>Measure weight</returns>
        public virtual Task<MeasureWeight> GetMeasureWeightById(string measureWeightId)
        {
            string key = string.Format(CacheKey.MEASUREWEIGHTS_BY_ID_KEY, measureWeightId);
            return _cacheBase.GetAsync(key, () => _measureWeightRepository.GetByIdAsync(measureWeightId));
        }

        /// <summary>
        /// Gets a measure weight by system keyword
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        /// <returns>Measure weight</returns>
        public virtual async Task<MeasureWeight> GetMeasureWeightBySystemKeyword(string systemKeyword)
        {
            if (String.IsNullOrEmpty(systemKeyword))
                return null;

            var measureWeights = await GetAllMeasureWeights();
            foreach (var measureWeight in measureWeights)
                if (measureWeight.SystemKeyword.ToLowerInvariant() == systemKeyword.ToLowerInvariant())
                    return measureWeight;
            return null;
        }

        /// <summary>
        /// Gets all measure weights
        /// </summary>
        /// <returns>Measure weights</returns>
        public virtual async Task<IList<MeasureWeight>> GetAllMeasureWeights()
        {
            string key = CacheKey.MEASUREWEIGHTS_ALL_KEY;
            return await _cacheBase.GetAsync(key, () =>
            {
                var query = from mw in _measureWeightRepository.Table
                            orderby mw.DisplayOrder
                            select mw;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Inserts a measure weight
        /// </summary>
        /// <param name="measure">Measure weight</param>
        public virtual async Task InsertMeasureWeight(MeasureWeight measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            await _measureWeightRepository.InsertAsync(measure);

            await _cacheBase.RemoveByPrefix(CacheKey.MEASUREWEIGHTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(measure);
        }

        /// <summary>
        /// Updates the measure weight
        /// </summary>
        /// <param name="measure">Measure weight</param>
        public virtual async Task UpdateMeasureWeight(MeasureWeight measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            await _measureWeightRepository.UpdateAsync(measure);

            await _cacheBase.RemoveByPrefix(CacheKey.MEASUREWEIGHTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(measure);
        }

        /// <summary>
        /// Converts weight
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="sourceMeasureWeight">Source weight</param>
        /// <param name="targetMeasureWeight">Target weight</param>
        /// <param name="round">A value indicating whether a result should be rounded</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertWeight(decimal value,
            MeasureWeight sourceMeasureWeight, MeasureWeight targetMeasureWeight, bool round = true)
        {
            if (sourceMeasureWeight == null)
                throw new ArgumentNullException("sourceMeasureWeight");

            if (targetMeasureWeight == null)
                throw new ArgumentNullException("targetMeasureWeight");

            decimal result = value;
            if (result != decimal.Zero && sourceMeasureWeight.Id != targetMeasureWeight.Id)
            {
                result = await ConvertToPrimaryMeasureWeight(result, sourceMeasureWeight);
                result = await ConvertFromPrimaryMeasureWeight(result, targetMeasureWeight);
            }
            if (round)
                result = Math.Round(result, 2);
            return result;
        }

        /// <summary>
        /// Converts to primary measure weight
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="sourceMeasureWeight">Source weight</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertToPrimaryMeasureWeight(decimal value, MeasureWeight sourceMeasureWeight)
        {
            if (sourceMeasureWeight == null)
                throw new ArgumentNullException("sourceMeasureWeight");

            decimal result = value;
            var baseWeightIn = await GetMeasureWeightById(_measureSettings.BaseWeightId);
            if (result != decimal.Zero && sourceMeasureWeight.Id != baseWeightIn.Id)
            {
                decimal exchangeRatio = sourceMeasureWeight.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new PowerStoreException(string.Format("Exchange ratio not set for weight [{0}]", sourceMeasureWeight.Name));
                result = result / exchangeRatio;
            }
            return result;
        }

        /// <summary>
        /// Converts from primary weight
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="targetMeasureWeight">Target weight</param>
        /// <returns>Converted value</returns>
        public virtual async Task<decimal> ConvertFromPrimaryMeasureWeight(decimal value,
            MeasureWeight targetMeasureWeight)
        {
            if (targetMeasureWeight == null)
                throw new ArgumentNullException("targetMeasureWeight");

            decimal result = value;
            var baseWeightIn = await GetMeasureWeightById(_measureSettings.BaseWeightId);
            if (result != decimal.Zero && targetMeasureWeight.Id != baseWeightIn.Id)
            {
                decimal exchangeRatio = targetMeasureWeight.Ratio;
                if (exchangeRatio == decimal.Zero)
                    throw new PowerStoreException(string.Format("Exchange ratio not set for weight [{0}]", targetMeasureWeight.Name));
                result = result * exchangeRatio;
            }
            return result;
        }

        #endregion

        #region MeasureUnit

        /// <summary>
        /// Deletes measure unit
        /// </summary>
        /// <param name="measureUnit">Measure unit</param>
        public virtual async Task DeleteMeasureUnit(MeasureUnit measureUnit)
        {
            if (measureUnit == null)
                throw new ArgumentNullException("measureUnit");

            //remove unit from products
            await _mediator.Send(new DeleteMeasureUnitOnProductCommand() { MeasureUnitId = measureUnit.Id });

            //delete
            await _measureUnitRepository.DeleteAsync(measureUnit);

            //clear cache
            await _cacheBase.RemoveByPrefix(CacheKey.MEASUREUNITS_PATTERN_KEY);
            await _cacheBase.RemoveByPrefix(CacheKey.PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(measureUnit);
        }

        /// <summary>
        /// Gets a measure unit by identifier
        /// </summary>
        /// <param name="measureUnitId">Measure unit identifier</param>
        /// <returns>Measure dimension</returns>
        public virtual Task<MeasureUnit> GetMeasureUnitById(string measureUnitId)
        {
            string key = string.Format(CacheKey.MEASUREUNITS_BY_ID_KEY, measureUnitId);
            return _cacheBase.GetAsync(key, () => _measureUnitRepository.GetByIdAsync(measureUnitId));
        }


        /// <summary>
        /// Gets all measure units
        /// </summary>
        /// <returns>Measure unit</returns>
        public virtual async Task<IList<MeasureUnit>> GetAllMeasureUnits()
        {
            string key = CacheKey.MEASUREUNITS_ALL_KEY;
            return await _cacheBase.GetAsync(key, () =>
            {
                var query = from md in _measureUnitRepository.Table
                            orderby md.DisplayOrder
                            select md;
                return query.ToListAsync();
            });
        }

        /// <summary>
        /// Inserts a measure unit
        /// </summary>
        /// <param name="measure">Measure unit</param>
        public virtual async Task InsertMeasureUnit(MeasureUnit measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            await _measureUnitRepository.InsertAsync(measure);

            await _cacheBase.RemoveByPrefix(CacheKey.MEASUREUNITS_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(measure);
        }

        /// <summary>
        /// Updates the measure unit
        /// </summary>
        /// <param name="measure">Measure unit</param>
        public virtual async Task UpdateMeasureUnit(MeasureUnit measure)
        {
            if (measure == null)
                throw new ArgumentNullException("measure");

            await _measureUnitRepository.UpdateAsync(measure);

            await _cacheBase.RemoveByPrefix(CacheKey.MEASUREUNITS_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(measure);
        }
        #endregion

        #endregion
    }
}