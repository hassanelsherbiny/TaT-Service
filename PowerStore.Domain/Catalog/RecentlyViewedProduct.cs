﻿using System;

namespace PowerStore.Domain.Catalog
{
    public partial class RecentlyViewedProduct: BaseEntity
    {
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public DateTime CreatedOnUtc { get; set; }

    }
}
