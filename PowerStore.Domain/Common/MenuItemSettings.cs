﻿using PowerStore.Domain.Configuration;

namespace PowerStore.Domain.Common
{
    public class MenuItemSettings: ISettings
    {
        public bool DisplayHomePageMenu { get; set; }

        public bool DisplayNewProductsMenu { get; set; }

        public bool DisplaySearchMenu { get; set; }

        public bool DisplayCustomerMenu { get; set; }

        public bool DisplayBlogMenu { get; set; }

        public bool DisplayContactUsMenu { get; set; }
    }
}
