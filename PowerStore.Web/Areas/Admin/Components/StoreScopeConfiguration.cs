﻿using PowerStore.Core;
using PowerStore.Domain.Customers;
using PowerStore.Framework.Components;
using PowerStore.Services.Common;
using PowerStore.Services.Stores;
using PowerStore.Web.Areas.Admin.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PowerStore.Web.Areas.Admin.Components
{
    public class StoreScopeConfigurationViewComponent : BaseAdminViewComponent
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Constructors

        public StoreScopeConfigurationViewComponent(IStoreService storeService, IWorkContext workContext)
        {
            _storeService = storeService;
            _workContext = workContext;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var allStores = await _storeService.GetAllStores();
            if (allStores.Count < 2)
                return Content("");

            var model = new StoreScopeConfigurationModel();
            foreach (var s in allStores)
            {
                model.Stores.Add(new Framework.Mvc.Models.StoreModel
                {
                    Id = s.Id,
                    Name = s.Shortcut
                });
            }
            model.StoreId = await GetActiveStoreScopeConfiguration(_storeService, _workContext);
            return View(model);
        }

        #endregion

        #region Methods

        private async Task<string> GetActiveStoreScopeConfiguration(IStoreService storeService, IWorkContext workContext)
        {
            //ensure that we have 2 (or more) stores
            if ((await storeService.GetAllStores()).Count < 2)
                return string.Empty;

            var storeId = workContext.CurrentCustomer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration);
            var store = await storeService.GetStoreById(storeId);

            return store != null ? store.Id : "";
        }

        #endregion
    }
}