﻿using AutoMapper;
using PowerStore.Domain.Blogs;
using PowerStore.Core.Mapper;
using PowerStore.Web.Areas.Admin.Extensions;
using PowerStore.Web.Areas.Admin.Models.Blogs;
using System.Collections.Generic;
using System.Linq;

namespace PowerStore.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class BlogCategoryProfile : Profile, IAutoMapperProfile
    {
        public BlogCategoryProfile()
        {
            CreateMap<BlogCategory, BlogCategoryModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore());

            CreateMap<BlogCategoryModel, BlogCategory>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()))
                .ForMember(dest => dest.BlogPosts, mo => mo.Ignore())
                .ForMember(dest => dest.Stores, mo => mo.MapFrom(x => x.SelectedStoreIds != null ? x.SelectedStoreIds.ToList() : new List<string>()));
        }

        public int Order => 0;
    }
}