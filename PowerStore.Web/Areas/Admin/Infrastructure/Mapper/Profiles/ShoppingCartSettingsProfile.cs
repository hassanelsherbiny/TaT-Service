﻿using AutoMapper;
using PowerStore.Domain.Orders;
using PowerStore.Core.Mapper;
using PowerStore.Web.Areas.Admin.Models.Settings;

namespace PowerStore.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class ShoppingCartSettingsProfile : Profile, IAutoMapperProfile
    {
        public ShoppingCartSettingsProfile()
        {
            CreateMap<ShoppingCartSettings, ShoppingCartSettingsModel>()
                .ForMember(dest => dest.ActiveStoreScopeConfiguration, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayCartAfterAddingProduct_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.DisplayWishlistAfterAddingProduct_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MaximumShoppingCartItems_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MaximumWishlistItems_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowOutOfStockItemsToBeAddedToWishlist_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MoveItemsFromWishlistToCart_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowProductImagesOnShoppingCart_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowProductImagesOnWishList_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowDiscountBox_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowGiftCardBox_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CrossSellsNumber_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.EmailWishlistEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowAnonymousUsersToEmailWishlist_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MiniShoppingCartEnabled_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.ShowProductImagesInMiniShoppingCart_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.MiniShoppingCartProductNumber_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.AllowCartItemEditing_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.CartsSharedBetweenStores_OverrideForStore, mo => mo.Ignore())
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<ShoppingCartSettingsModel, ShoppingCartSettings>()
                .ForMember(dest => dest.RoundPricesDuringCalculation, mo => mo.Ignore())
                .ForMember(dest => dest.GroupTierPricesForDistinctShoppingCartItems, mo => mo.Ignore())
                .ForMember(dest => dest.RenderAssociatedAttributeValueQuantity, mo => mo.Ignore())
                .ForMember(dest => dest.ReservationDateFormat, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}