﻿using PowerStore.Core;
using PowerStore.Core.Caching;
using PowerStore.Plugin.Widgets.Slider.Domain;
using PowerStore.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using PowerStore.Domain.Data;

namespace PowerStore.Plugin.Widgets.Slider.Services
{
    public partial class SliderService: ISliderService
    {
        #region Fields

        private readonly IRepository<PictureSlider> _reporistoryPictureSlider;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreContext _storeContext;
        private readonly ICacheBase _cacheBase;

        /// <summary>
        /// Key for sliders
        /// </summary>
        /// <remarks>
        /// {0} : Store id
        /// {1} : Slider type
        /// {2} : Object entry / categoryId || manufacturerId
        /// </remarks>
        public const string SLIDERS_MODEL_KEY = "PowerStore.slider-{0}-{1}-{2}";
        public const string SLIDERS_PATTERN_KEY = "PowerStore.slider";

        #endregion
        public SliderService(IRepository<PictureSlider> reporistoryPictureSlider,
            IStoreContext storeContext, IStoreMappingService storeMappingService,
            ICacheBase cacheManager)
        {
            _reporistoryPictureSlider = reporistoryPictureSlider;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _cacheBase = cacheManager;
        }
        /// <summary>
        /// Delete a slider
        /// </summary>
        /// <param name="slider">Slider</param>
        public virtual async Task DeleteSlider(PictureSlider slide)
        {
            if(slide==null)
                throw new ArgumentNullException("slide");

            //clear cache
            await _cacheBase.RemoveByPrefix(SLIDERS_PATTERN_KEY);

            await _reporistoryPictureSlider.DeleteAsync(slide);
        }

        /// <summary>
        /// Gets all 
        /// </summary>
        /// <returns>Picture Sliders</returns>
        public virtual async Task<IList<PictureSlider>> GetPictureSliders()
        {
            return await _reporistoryPictureSlider.Table.OrderBy(x => x.SliderTypeId).ThenBy(x => x.DisplayOrder).ToListAsync();
        }

        /// <summary>
        /// Gets by type 
        /// </summary>
        /// <returns>Picture Sliders</returns>
        public virtual async Task<IList<PictureSlider>> GetPictureSliders(SliderType sliderType, string objectEntry = "")
        {
            string cacheKey = string.Format(SLIDERS_MODEL_KEY, _storeContext.CurrentStore.Id, sliderType.ToString(), objectEntry);
            return await _cacheBase.GetAsync(cacheKey, async () =>
            {
                var query = from s in _reporistoryPictureSlider.Table
                            where s.SliderTypeId == (int)sliderType && s.Published
                            select s;

                if (!string.IsNullOrEmpty(objectEntry))
                    query = query.Where(x => x.ObjectEntry == objectEntry);

                var items = await query.ToListAsync();
                return items.Where(c => _storeMappingService.Authorize(c)).ToList();
            });
        }


        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="slideId">Slide identifier</param>
        /// <returns>Tax rate</returns>
        public virtual Task<PictureSlider> GetById(string slideId)
        {
            return _reporistoryPictureSlider.Table.FirstOrDefaultAsync(x => x.Id == slideId);
        }

        /// <summary>
        /// Inserts a slide
        /// </summary>
        /// <param name="slide">Picture Slider</param>
        public virtual async Task InsertPictureSlider(PictureSlider slide)
        {
            if (slide == null)
                throw new ArgumentNullException("slide");

            //clear cache
            await _cacheBase.RemoveByPrefix(SLIDERS_PATTERN_KEY);

            await _reporistoryPictureSlider.InsertAsync(slide);
        }

        /// <summary>
        /// Updates slide
        /// </summary>
        /// <param name="slide">Picture Slider</param>
        public virtual async Task UpdatePictureSlider(PictureSlider slide)
        {
            if (slide == null)
                throw new ArgumentNullException("slide");

            //clear cache
            await _cacheBase.RemoveByPrefix(SLIDERS_PATTERN_KEY);

            await _reporistoryPictureSlider.UpdateAsync(slide);
        }

        /// <summary>
        /// Delete slide
        /// </summary>
        /// <param name="slide">Picture Slider</param>
        public virtual async Task DeletePictureSlider(PictureSlider slide)
        {
            if (slide == null)
                throw new ArgumentNullException("slide");

            //clear cache
            await _cacheBase.RemoveByPrefix(SLIDERS_PATTERN_KEY);

            await _reporistoryPictureSlider.DeleteAsync(slide);
        }

    }
}
