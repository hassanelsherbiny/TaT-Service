﻿using PowerStore.Domain.Messages;
using PowerStore.Web.Areas.Admin.Models.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PowerStore.Web.Areas.Admin.Interfaces
{
    public interface ICampaignViewModelService
    {
        Task<CampaignModel> PrepareCampaignModel();
        Task<CampaignModel> PrepareCampaignModel(CampaignModel model);
        Task<CampaignModel> PrepareCampaignModel(Campaign campaign);
        Task<(IEnumerable<CampaignModel> campaignModels, int totalCount)> PrepareCampaignModels();
        Task<Campaign> InsertCampaignModel(CampaignModel model);
        Task<Campaign> UpdateCampaignModel(Campaign campaign, CampaignModel model);
    }
}
