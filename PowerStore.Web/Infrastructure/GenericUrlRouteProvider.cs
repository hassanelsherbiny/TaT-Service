﻿using PowerStore.Core.Configuration;
using PowerStore.Core.Data;
using PowerStore.Core.Routing;
using PowerStore.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace PowerStore.Web.Infrastructure
{
    public partial class GenericUrlRouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            var pattern = "{SeName}";
            if (DataSettingsHelper.DatabaseIsInstalled())
            {
                var config = routeBuilder.ServiceProvider.GetRequiredService<PowerStoreConfig>();
                if (config.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    pattern = $"{{language:lang={config.SeoFriendlyUrlsDefaultCode}}}/{{**SeName}}";
                }

            }
            routeBuilder.MapDynamicControllerRoute<SlugRouteTransformer>(pattern);
            
            //and default one
            routeBuilder.MapControllerRoute(
                name: "Default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            //generic URLs
            routeBuilder.MapControllerRoute(
                name: "GenericUrl",
                pattern: "{GenericSeName}",
                new { controller = "Common", action = "GenericUrl" });

            //define this routes to use in UI views (in case if you want to customize some of them later)
            routeBuilder.MapControllerRoute(
                name: "Product",
                pattern: pattern,
                new { controller = "Product", action = "ProductDetails" });

            routeBuilder.MapControllerRoute(
                name: "Category",
                pattern: pattern,
                new { controller = "Catalog", action = "Category" });

            routeBuilder.MapControllerRoute(
                name: "Manufacturer",
                pattern: pattern,
                new { controller = "Catalog", action = "Manufacturer" });

            routeBuilder.MapControllerRoute(
                name: "Vendor",
                pattern: pattern,
                new { controller = "Catalog", action = "Vendor" });

            routeBuilder.MapControllerRoute(
                name: "NewsItem",
                pattern: pattern,
                new { controller = "News", action = "NewsItem" });

            routeBuilder.MapControllerRoute(
                name: "BlogPost",
                pattern: pattern,
                new { controller = "Blog", action = "BlogPost" });

            routeBuilder.MapControllerRoute(
                name: "Topic",
                pattern: pattern,
                new { controller = "Topic", action = "TopicDetails" });

            routeBuilder.MapControllerRoute(
                name: "KnowledgebaseArticle",
                pattern: pattern,
                new { controller = "Knowledgebase", action = "KnowledgebaseArticle" });

            routeBuilder.MapControllerRoute(
                name: "KnowledgebaseCategory",
                pattern: pattern,
                new { controller = "Knowledgebase", action = "ArticlesByCategory" });

            routeBuilder.MapControllerRoute(
                name: "Course",
                pattern: pattern,
                new { controller = "Course", action = "Details" });
            
        }

        public int Priority {
            get {
                //it should be the last route
                //we do not set it to -int.MaxValue so it could be overridden (if required)
                return -1000000;
            }
        }
    }
}
