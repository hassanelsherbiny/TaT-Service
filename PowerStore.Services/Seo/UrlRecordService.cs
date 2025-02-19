using PowerStore.Domain;
using PowerStore.Core.Caching;
using PowerStore.Domain.Configuration;
using PowerStore.Domain.Data;
using PowerStore.Domain.Seo;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PowerStore.Core.Configuration;
using PowerStore.Core.Caching.Constants;

namespace PowerStore.Services.Seo
{
    /// <summary>
    /// Provides information about URL records
    /// </summary>
    public partial class UrlRecordService : IUrlRecordService
    {
        #region Fields

        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly ICacheBase _cacheBase;
        private readonly PowerStoreConfig _config;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="urlRecordRepository">URL record repository</param>
        /// <param name="localizationSettings">Localization settings</param>
        public UrlRecordService(ICacheBase cacheManager,
            IRepository<UrlRecord> urlRecordRepository,
            PowerStoreConfig config)
        {
            _cacheBase = cacheManager;
            _urlRecordRepository = urlRecordRepository;
            _config = config;
        }

        #endregion

        #region Utilities

        protected UrlRecordForCaching Map(UrlRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("record");

            var urlRecordForCaching = new UrlRecordForCaching {
                Id = record.Id,
                EntityId = record.EntityId,
                EntityName = record.EntityName,
                Slug = record.Slug,
                IsActive = record.IsActive,
                LanguageId = record.LanguageId
            };
            return urlRecordForCaching;
        }

        /// <summary>
        /// Gets all cached URL records
        /// </summary>
        /// <returns>cached URL records</returns>
        protected virtual async Task<IList<UrlRecordForCaching>> GetAllUrlRecordsCached()
        {
            //cache
            string key = string.Format(CacheKey.URLRECORD_ALL_KEY);
            return await _cacheBase.GetAsync(key, async () =>
            {
                var query = _urlRecordRepository.Table;
                var urlRecords = await query.ToListAsync();
                var list = new List<UrlRecordForCaching>();
                foreach (var ur in urlRecords)
                {
                    var urlRecordForCaching = Map(ur);
                    list.Add(urlRecordForCaching);
                }
                return list;
            });
        }

        #endregion

        #region Nested classes

        [Serializable]
        public class UrlRecordForCaching
        {
            public string Id { get; set; }
            public string EntityId { get; set; }
            public string EntityName { get; set; }
            public string Slug { get; set; }
            public bool IsActive { get; set; }
            public string LanguageId { get; set; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes an URL record
        /// </summary>
        /// <param name="urlRecord">URL record</param>
        public virtual async Task DeleteUrlRecord(UrlRecord urlRecord)
        {
            if (urlRecord == null)
                throw new ArgumentNullException("urlRecord");

            await _urlRecordRepository.DeleteAsync(urlRecord);

            //cache
            await _cacheBase.RemoveByPrefix(CacheKey.URLRECORD_PATTERN_KEY);
        }

        /// <summary>
        /// Gets an URL record
        /// </summary>
        /// <param name="urlRecordId">URL record identifier</param>
        /// <returns>URL record</returns>
        public virtual Task<UrlRecord> GetUrlRecordById(string urlRecordId)
        {
            return _urlRecordRepository.GetByIdAsync(urlRecordId);
        }

        /// <summary>
        /// Inserts an URL record
        /// </summary>
        /// <param name="urlRecord">URL record</param>
        public virtual async Task InsertUrlRecord(UrlRecord urlRecord)
        {
            if (urlRecord == null)
                throw new ArgumentNullException("urlRecord");

            await _urlRecordRepository.InsertAsync(urlRecord);

            //cache
            await _cacheBase.RemoveByPrefix(CacheKey.URLRECORD_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the URL record
        /// </summary>
        /// <param name="urlRecord">URL record</param>
        public virtual async Task UpdateUrlRecord(UrlRecord urlRecord)
        {
            if (urlRecord == null)
                throw new ArgumentNullException("urlRecord");

            await _urlRecordRepository.UpdateAsync(urlRecord);

            //cache
            await _cacheBase.RemoveByPrefix(CacheKey.URLRECORD_PATTERN_KEY);
        }

        /// <summary>
        /// Find URL record
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <returns>Found URL record</returns>
        public virtual async Task<UrlRecord> GetBySlug(string slug)
        {
            if (String.IsNullOrEmpty(slug))
                return null;

            slug = slug.ToLowerInvariant();

            var query = from ur in _urlRecordRepository.Table
                        where ur.Slug == slug
                        orderby ur.IsActive
                        select ur;
            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Find URL record (cached version).
        /// This method works absolutely the same way as "GetBySlug" one but caches the results.
        /// Hence, it's used only for performance optimization in public store
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <returns>Found URL record</returns>
        public virtual async Task<UrlRecordForCaching> GetBySlugCached(string slug)
        {
            if (String.IsNullOrEmpty(slug))
                return null;

            slug = slug.ToLowerInvariant();

            if (_config.LoadAllUrlRecordsOnStartup)
            {
                //load all records (we know they are cached)
                var source = await GetAllUrlRecordsCached();
                var query = from ur in source
                            where ur.Slug != null && ur.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)
                            orderby ur.IsActive
                            select ur;
                var urlRecordForCaching = query.FirstOrDefault();
                return urlRecordForCaching;
            }

            //gradual loading
            string key = string.Format(CacheKey.URLRECORD_BY_SLUG_KEY, slug);
            return await _cacheBase.GetAsync(key, async () =>
            {
                var urlRecord = await GetBySlug(slug);
                if (urlRecord == null)
                    return null;

                var urlRecordForCaching = Map(urlRecord);
                return urlRecordForCaching;
            });
        }

        /// <summary>
        /// Gets all URL records
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <param name="active">Active</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>URL records</returns>
        public virtual async Task<IPagedList<UrlRecord>> GetAllUrlRecords(string slug = "", bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {

            var query = _urlRecordRepository.Table;

            if (!string.IsNullOrWhiteSpace(slug))
                query = query.Where(ur => ur.Slug.Contains(slug.ToLowerInvariant()));

            if (active.HasValue)
                query = query.Where(ur => ur.IsActive == active.Value);

            query = query.OrderBy(ur => ur.Slug);
            return await PagedList<UrlRecord>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Find slug
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="entityName">Entity name</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Found slug</returns>
        public virtual async Task<string> GetActiveSlug(string entityId, string entityName, string languageId)
        {
            if (_config.LoadAllUrlRecordsOnStartup)
            {
                string key = string.Format(CacheKey.URLRECORD_ACTIVE_BY_ID_NAME_LANGUAGE_KEY, entityId, entityName, languageId);
                return await _cacheBase.GetAsync(key, async () =>
                {
                    //load all records (we know they are cached)
                    var source = await GetAllUrlRecordsCached();
                    var query = from ur in source
                                where ur.EntityId == entityId &&
                                ur.EntityName == entityName &&
                                ur.LanguageId == languageId &&
                                ur.IsActive
                                select ur.Slug;
                    var slug = query.FirstOrDefault();
                    //little hack here. nulls aren't cacheable so set it to ""
                    if (slug == null)
                        slug = "";
                    return slug;
                });
            }
            else
            {
                //gradual loading
                string key = string.Format(CacheKey.URLRECORD_ACTIVE_BY_ID_NAME_LANGUAGE_KEY, entityId, entityName, languageId);
                return await _cacheBase.GetAsync(key, async () =>
                {

                    var source = _urlRecordRepository.Table;
                    var query = from ur in source
                                where ur.EntityId == entityId &&
                                ur.EntityName == entityName &&
                                ur.LanguageId == languageId &&
                                ur.IsActive
                                select ur.Slug;
                    var slug = await query.FirstOrDefaultAsync();
                    //little hack here. nulls aren't cacheable so set it to ""
                    if (slug == null)
                        slug = "";
                    return slug;
                });
            }
        }

        /// <summary>
        /// Save slug
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="slug">Slug</param>
        /// <param name="languageId">Language ID</param>
        public virtual async Task SaveSlug<T>(T entity, string slug, string languageId) where T : BaseEntity, ISlugSupported
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            string entityId = entity.Id;
            string entityName = typeof(T).Name;

            var query = from ur in _urlRecordRepository.Table
                        where ur.EntityId == entityId &&
                        ur.EntityName == entityName &&
                        ur.LanguageId == languageId
                        select ur;

            var allUrlRecords = await query.ToListAsync();
            var activeUrlRecord = allUrlRecords.FirstOrDefault(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(slug))
                slug = slug.ToLowerInvariant();

            if (activeUrlRecord == null && !string.IsNullOrWhiteSpace(slug))
            {
                //find in non-active records with the specified slug
                var nonActiveRecordWithSpecifiedSlug = allUrlRecords
                    .FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) && !x.IsActive);
                if (nonActiveRecordWithSpecifiedSlug != null)
                {
                    //mark non-active record as active
                    nonActiveRecordWithSpecifiedSlug.IsActive = true;
                    await UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);
                }
                else
                {
                    //new record
                    var urlRecord = new UrlRecord {
                        EntityId = entityId,
                        EntityName = entityName,
                        Slug = slug,
                        LanguageId = languageId,
                        IsActive = true,
                    };
                    await InsertUrlRecord(urlRecord);
                }
            }

            if (activeUrlRecord != null && string.IsNullOrWhiteSpace(slug))
            {
                //disable the previous active URL record
                activeUrlRecord.IsActive = false;
                await UpdateUrlRecord(activeUrlRecord);
            }

            if (activeUrlRecord != null && !string.IsNullOrWhiteSpace(slug))
            {
                //it should not be the same slug as in active URL record
                if (!activeUrlRecord.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase))
                {
                    //find in non-active records with the specified slug
                    var nonActiveRecordWithSpecifiedSlug = allUrlRecords
                        .FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) && !x.IsActive);
                    if (nonActiveRecordWithSpecifiedSlug != null)
                    {
                        //mark non-active record as active
                        nonActiveRecordWithSpecifiedSlug.IsActive = true;
                        await UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);

                        //disable the previous active URL record
                        activeUrlRecord.IsActive = false;
                        await UpdateUrlRecord(activeUrlRecord);
                    }
                    else
                    {
                        //insert new record
                        //we do not update the existing record because we should track all previously entered slugs
                        //to ensure that URLs will work fine
                        var urlRecord = new UrlRecord {
                            EntityId = entityId,
                            EntityName = entityName,
                            Slug = slug,
                            LanguageId = languageId,
                            IsActive = true,
                        };
                        await InsertUrlRecord(urlRecord);

                        //disable the previous active URL record
                        activeUrlRecord.IsActive = false;
                        await UpdateUrlRecord(activeUrlRecord);
                    }

                }
            }
        }

        #endregion
    }
}