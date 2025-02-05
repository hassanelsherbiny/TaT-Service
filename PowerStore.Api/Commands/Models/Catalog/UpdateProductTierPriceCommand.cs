﻿using PowerStore.Api.DTOs.Catalog;
using MediatR;

namespace PowerStore.Api.Commands.Models.Catalog
{
    public class UpdateProductTierPriceCommand : IRequest<bool>
    {
        public ProductDto Product { get; set; }
        public ProductTierPriceDto Model { get; set; }
    }
}
