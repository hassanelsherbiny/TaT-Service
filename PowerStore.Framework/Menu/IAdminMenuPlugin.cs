﻿using PowerStore.Core.Plugins;
using System.Threading.Tasks;

namespace PowerStore.Framework.Menu
{
    /// <summary>
    /// Interface for plugins which have some items in the admin area menu
    /// </summary>
    public interface IAdminMenuPlugin : IPlugin
    {
        /// <summary>
        /// Manage sitemap. You can use "SystemName" of menu items to manage existing sitemap or add a new menu item.
        /// </summary>
        /// <param name="rootNode">Root node of the sitemap.</param>
        Task ManageSiteMap(SiteMapNode rootNode);
    }
}
