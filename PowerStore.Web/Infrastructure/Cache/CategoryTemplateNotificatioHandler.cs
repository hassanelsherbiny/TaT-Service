﻿using PowerStore.Core.Caching;
using PowerStore.Domain.Catalog;
using PowerStore.Core.Events;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace PowerStore.Web.Infrastructure.Cache
{
    public partial class CategoryTemplateNotificatioHandler :
        INotificationHandler<EntityInserted<CategoryTemplate>>,
        INotificationHandler<EntityUpdated<CategoryTemplate>>,
        INotificationHandler<EntityDeleted<CategoryTemplate>>
        
    {
        private readonly ICacheBase _cacheBase;

        public CategoryTemplateNotificatioHandler(ICacheBase cacheManager)
        {
            _cacheBase = cacheManager;
        }

        public async Task Handle(EntityInserted<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityUpdated<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public async Task Handle(EntityDeleted<CategoryTemplate> eventMessage, CancellationToken cancellationToken)
        {
            await _cacheBase.RemoveByPrefix(ModelCacheEventConst.CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        
    }
}
