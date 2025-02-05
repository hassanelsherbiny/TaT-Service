﻿using AutoMapper;
using PowerStore.Domain.Polls;
using PowerStore.Core.Mapper;
using PowerStore.Web.Areas.Admin.Extensions;
using PowerStore.Web.Areas.Admin.Models.Polls;
using System.Collections.Generic;
using System.Linq;

namespace PowerStore.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class PollProfile : Profile, IAutoMapperProfile
    {
        public PollProfile()
        {
            CreateMap<Poll, PollModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedStoreIds, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore());

            CreateMap<PollModel, Poll>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.PollAnswers, mo => mo.Ignore())
                .ForMember(dest => dest.Stores, mo => mo.MapFrom(model => model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>()))
                .ForMember(dest => dest.CustomerRoles, mo => mo.MapFrom(model => model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>()))
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));

            CreateMap<PollAnswer, PollAnswerModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());
            CreateMap<PollAnswerModel, PollAnswer>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));
        }

        public int Order => 0;
    }
}