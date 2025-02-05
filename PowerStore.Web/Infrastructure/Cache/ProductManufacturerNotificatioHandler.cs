﻿using PowerStore.Core.Caching;
using PowerStore.Domain.Catalog;
using PowerStore.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace PowerStore.Web.Infrastructure.Cache
{
    public class ProductManufacturerNotificatioHandler :
        INotificationHandler<EntityInserted<ProductManufacturer>>,
        INotificationHandler<EntityUpdated<ProductManufacturer>>,
        INotificationHandler<EntityDeleted<ProductManufacturer>>
    {

        private readonly ICacheBase _cacheBase;

        public ProductManufacturerNotificatioHandler(ICacheBase cacheManager)
        {
            _cacheBase = cacheManager;
        }

        public async Task Handle(EntityInserted<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_MANUFACTURERS_MODEL_PRODUCT_KEY, eventMessage.Entity.ProductId));
            await _cacheBase.RemoveByPrefix(string.Format(ModelCacheEventConst.MANUFACTURER_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.ManufacturerId));
        }
        public async Task Handle(EntityUpdated<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_MANUFACTURERS_MODEL_PRODUCT_KEY, eventMessage.Entity.ProductId));
            await _cacheBase.RemoveByPrefix(string.Format(ModelCacheEventConst.MANUFACTURER_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.ManufacturerId));
        }
        public async Task Handle(EntityDeleted<ProductManufacturer> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(string.Format(ModelCacheEventConst.PRODUCT_MANUFACTURERS_MODEL_PRODUCT_KEY, eventMessage.Entity.ProductId));
            await _cacheBase.RemoveByPrefix(string.Format(ModelCacheEventConst.MANUFACTURER_HAS_FEATURED_PRODUCTS_MODEL_KEY, eventMessage.Entity.ManufacturerId));
        }
    }
}