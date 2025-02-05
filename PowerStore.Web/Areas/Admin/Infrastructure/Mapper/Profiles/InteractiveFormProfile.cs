﻿using AutoMapper;
using PowerStore.Domain.Messages;
using PowerStore.Core.Mapper;
using PowerStore.Web.Areas.Admin.Extensions;
using PowerStore.Web.Areas.Admin.Models.Messages;

namespace PowerStore.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class InteractiveFormProfile : Profile, IAutoMapperProfile
    {
        public InteractiveFormProfile()
        {
            CreateMap<InteractiveForm, InteractiveFormModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableEmailAccounts, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableTokens, mo => mo.Ignore());

            CreateMap<InteractiveFormModel, InteractiveForm>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.FormAttributes, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));

            CreateMap<InteractiveForm.FormAttribute, InteractiveFormAttributeModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            CreateMap<InteractiveFormAttributeModel, InteractiveForm.FormAttribute>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));

            CreateMap<InteractiveForm.FormAttributeValue, InteractiveFormAttributeValueModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            CreateMap<InteractiveFormAttributeValueModel, InteractiveForm.FormAttributeValue>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()));
        }

        public int Order => 0;
    }
}