﻿using AutoMapper;
using PowerStore.Domain.Customers;
using PowerStore.Core.Mapper;
using PowerStore.Web.Areas.Admin.Models.Customers;

namespace PowerStore.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class CustomerReminderProfile : Profile, IAutoMapperProfile
    {
        public CustomerReminderProfile()
        {
            CreateMap<CustomerReminderModel, CustomerReminder>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());

            CreateMap<CustomerReminder, CustomerReminderModel>();

            CreateMap<CustomerReminder.ReminderLevel, CustomerReminderModel.ReminderLevelModel>()
                .ForMember(dest => dest.EmailAccounts, mo => mo.Ignore());
            CreateMap<CustomerReminderModel.ReminderLevelModel, CustomerReminder.ReminderLevel>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());

            CreateMap<CustomerReminder.ReminderCondition, CustomerReminderModel.ConditionModel>()
                .ForMember(dest => dest.ConditionType, mo => mo.Ignore());
            CreateMap<CustomerReminderModel.ConditionModel, CustomerReminder.ReminderCondition>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.ConditionType, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}