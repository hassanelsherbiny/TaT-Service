﻿using PowerStore.Core;
using PowerStore.Core.Caching;
using PowerStore.Domain.Blogs;
using PowerStore.Domain.Media;
using PowerStore.Services.Blogs;
using PowerStore.Services.Helpers;
using PowerStore.Services.Localization;
using PowerStore.Services.Media;
using PowerStore.Services.Seo;
using PowerStore.Web.Features.Models.Blogs;
using PowerStore.Web.Infrastructure.Cache;
using PowerStore.Web.Models.Blogs;
using PowerStore.Web.Models.Media;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PowerStore.Web.Features.Handlers.Blogs
{
    public class GetHomePageBlogHandler : IRequestHandler<GetHomePageBlog, HomePageBlogItemsModel>
    {
        private readonly IBlogService _blogService;
        private readonly ICacheBase _cacheBase;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;

        private readonly BlogSettings _blogSettings;
        private readonly MediaSettings _mediaSettings;

        public GetHomePageBlogHandler(IBlogService blogService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            ICacheBase cacheManager,
            BlogSettings blogSettings,
            MediaSettings mediaSettings)
        {
            _blogService = blogService;
            _workContext = workContext;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _cacheBase = cacheManager;

            _blogSettings = blogSettings;
            _mediaSettings = mediaSettings;
        }

        public async Task<HomePageBlogItemsModel> Handle(GetHomePageBlog request, CancellationToken cancellationToken)
        {
            var cacheKey = string.Format(ModelCacheEventConst.BLOG_HOMEPAGE_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                _storeContext.CurrentStore.Id);
            var cachedModel = await _cacheBase.GetAsync(cacheKey, async () =>
            {
                var model = new HomePageBlogItemsModel();

                var blogPosts = await _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id,
                        null, null, 0, _blogSettings.HomePageBlogCount);

                foreach (var post in blogPosts)
                {
                    var item = new HomePageBlogItemsModel.BlogItemModel();
                    var description = post.GetLocalized(x => x.BodyOverview, _workContext.WorkingLanguage.Id);
                    item.SeName = post.GetSeName(_workContext.WorkingLanguage.Id);
                    item.Title = post.GetLocalized(x => x.Title, _workContext.WorkingLanguage.Id);
                    item.Short = description?.Length > _blogSettings.MaxTextSizeHomePage ? description.Substring(0, _blogSettings.MaxTextSizeHomePage) : description;
                    item.CreatedOn = _dateTimeHelper.ConvertToUserTime(post.StartDateUtc ?? post.CreatedOnUtc, DateTimeKind.Utc);
                    item.GenericAttributes = post.GenericAttributes;
                    item.Category = (await _blogService.GetBlogCategoryByPostId(post.Id)).FirstOrDefault()?.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);

                    //prepare picture model
                    if (!string.IsNullOrEmpty(post.PictureId))
                    {
                        var pictureModel = new PictureModel {
                            Id = post.PictureId,
                            FullSizeImageUrl = await _pictureService.GetPictureUrl(post.PictureId),
                            ImageUrl = await _pictureService.GetPictureUrl(post.PictureId, _mediaSettings.BlogThumbPictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Blog.ImageLinkTitleFormat"), post.Title),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Blog.ImageAlternateTextFormat"), post.Title)
                        };
                        item.PictureModel = pictureModel;
                    }
                    model.Items.Add(item);
                }
                return model;
            });

            return cachedModel;
        }
    }
}
