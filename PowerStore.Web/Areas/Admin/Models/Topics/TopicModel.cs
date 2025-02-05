﻿using PowerStore.Framework.Localization;
using PowerStore.Framework.Mapping;
using PowerStore.Core.ModelBinding;
using PowerStore.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using PowerStore.Framework.Mvc.Models;
using System.ComponentModel.DataAnnotations;
using System;

namespace PowerStore.Web.Areas.Admin.Models.Topics
{
    public partial class TopicModel : BaseEntityModel, ILocalizedModel<TopicLocalizedModel>, IAclMappingModel, IStoreMappingModel
    {
        public TopicModel()
        {
            AvailableTopicTemplates = new List<SelectListItem>();
            Locales = new List<TopicLocalizedModel>();
            AvailableStores = new List<StoreModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
        }

        //Store mapping
        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.LimitedToStores")]
        public bool LimitedToStores { get; set; }
        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.AvailableStores")]
        public List<StoreModel> AvailableStores { get; set; }
        public string[] SelectedStoreIds { get; set; }


        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.SystemName")]

        public string SystemName { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.IncludeInSitemap")]
        public bool IncludeInSitemap { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.IncludeInTopMenu")]
        public bool IncludeInTopMenu { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.IncludeInFooterRow1")]
        public bool IncludeInFooterRow1 { get; set; }
        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.IncludeInFooterRow2")]
        public bool IncludeInFooterRow2 { get; set; }
        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.IncludeInFooterRow3")]
        public bool IncludeInFooterRow3 { get; set; }
        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.AccessibleWhenStoreClosed")]
        public bool AccessibleWhenStoreClosed { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.IsPasswordProtected")]
        public bool IsPasswordProtected { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.Published")]
        public bool Published { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.URL")]

        public string Url { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.Title")]

        public string Title { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.Body")]

        public string Body { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.TopicTemplate")]
        public string TopicTemplateId { get; set; }
        public IList<SelectListItem> AvailableTopicTemplates { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaKeywords")]

        public string MetaKeywords { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaDescription")]

        public string MetaDescription { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaTitle")]

        public string MetaTitle { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.SeName")]

        public string SeName { get; set; }

        public IList<TopicLocalizedModel> Locales { get; set; }
        //ACL
        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public string[] SelectedCustomerRoleIds { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.StartDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartDateUtc { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.EndDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndDateUtc { get; set; }
    }

    public partial class TopicLocalizedModel : ILocalizedModelLocal, ISlugModelLocal
    {
        public string LanguageId { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.Title")]

        public string Title { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.Body")]

        public string Body { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaKeywords")]

        public string MetaKeywords { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaDescription")]

        public string MetaDescription { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.MetaTitle")]

        public string MetaTitle { get; set; }

        [PowerStoreResourceDisplayName("Admin.ContentManagement.Topics.Fields.SeName")]

        public string SeName { get; set; }

    }
}