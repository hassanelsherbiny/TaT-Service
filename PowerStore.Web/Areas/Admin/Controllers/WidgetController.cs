﻿using PowerStore.Core.Caching;
using PowerStore.Domain.Cms;
using PowerStore.Core.Plugins;
using PowerStore.Framework.Kendoui;
using PowerStore.Framework.Mvc;
using PowerStore.Framework.Security.Authorization;
using PowerStore.Services.Cms;
using PowerStore.Services.Configuration;
using PowerStore.Services.Security;
using PowerStore.Web.Areas.Admin.Extensions;
using PowerStore.Web.Areas.Admin.Models.Cms;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerStore.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Widgets)]
    public partial class WidgetController : BaseAdminController
	{
		#region Fields
        private readonly IWidgetService _widgetService;
        private readonly ISettingService _settingService;
	    private readonly IPluginFinder _pluginFinder;
        private readonly ICacheBase _cacheBase;
        private readonly WidgetSettings _widgetSettings;
        #endregion

        #region Constructors

        public WidgetController(IWidgetService widgetService,
            ISettingService settingService,
            IPluginFinder pluginFinder,
            ICacheBase cacheManager,
            WidgetSettings widgetSettings)
		{
            _widgetService = widgetService;
            _widgetSettings = widgetSettings;
            _pluginFinder = pluginFinder;
            _cacheBase = cacheManager;
            _settingService = settingService;
        }

		#endregion 
        
        #region Methods
        
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var widgetsModel = new List<WidgetModel>();
            var widgets = _widgetService.LoadAllWidgets();
            foreach (var widget in widgets)
            {
                var tmp1 = widget.ToModel();
                tmp1.IsActive = widget.IsWidgetActive(_widgetSettings);
                tmp1.ConfigurationUrl = widget.GetConfigurationPageUrl();
                widgetsModel.Add(tmp1);
            }
            widgetsModel = widgetsModel.ToList();
            var gridModel = new DataSourceResult
            {
                Data = widgetsModel,
                Total = widgetsModel.Count()
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> WidgetUpdate(WidgetModel model)
        {
            var widget = _widgetService.LoadWidgetBySystemName(model.SystemName);
            if (widget.IsWidgetActive(_widgetSettings))
            {
                if (!model.IsActive)
                {
                    //mark as disabled
                    _widgetSettings.ActiveWidgetSystemNames.Remove(widget.PluginDescriptor.SystemName);
                    await _settingService.SaveSetting(_widgetSettings);
                }
            }
            else
            {
                if (model.IsActive)
                {
                    //mark as active
                    _widgetSettings.ActiveWidgetSystemNames.Add(widget.PluginDescriptor.SystemName);
                    await _settingService.SaveSetting(_widgetSettings);
                }
            }
            await _cacheBase.Clear();
            var pluginDescriptor = widget.PluginDescriptor;
            //display order
            pluginDescriptor.DisplayOrder = model.DisplayOrder;
            PluginFileParser.SavePluginConfigFile(pluginDescriptor);
            //reset plugin cache
            _pluginFinder.ReloadPlugins();

            return new NullJsonResult();
        }

        #endregion
    }
}
