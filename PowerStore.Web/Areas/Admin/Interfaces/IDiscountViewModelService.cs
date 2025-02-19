﻿using PowerStore.Domain.Catalog;
using PowerStore.Domain.Discounts;
using PowerStore.Domain.Stores;
using PowerStore.Domain.Vendors;
using PowerStore.Services.Discounts;
using PowerStore.Web.Areas.Admin.Models.Catalog;
using PowerStore.Web.Areas.Admin.Models.Discounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PowerStore.Web.Areas.Admin.Interfaces
{
    public interface IDiscountViewModelService
    {
        DiscountListModel PrepareDiscountListModel();
        Task<(IEnumerable<DiscountModel> discountModel, int totalCount)> PrepareDiscountModel(DiscountListModel model, int pageIndex, int pageSize);
        Task PrepareDiscountModel(DiscountModel model, Discount discount);
        Task<Discount> InsertDiscountModel(DiscountModel model);
        Task<Discount> UpdateDiscountModel(Discount discount, DiscountModel model);
        Task DeleteDiscount(Discount discount);
        Task InsertCouponCode(string discountId, string couponCode);
        string GetRequirementUrlInternal(IDiscountRequirementRule discountRequirementRule, Discount discount, string discountRequirementId);
        Task DeleteDiscountRequirement(DiscountRequirement discountRequirement, Discount discount);
        Task<DiscountModel.AddProductToDiscountModel> PrepareProductToDiscountModel();
        Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(DiscountModel.AddProductToDiscountModel model, int pageIndex, int pageSize);
        Task InsertProductToDiscountModel(DiscountModel.AddProductToDiscountModel model);
        Task DeleteProduct(Discount discount, Product product);
        Task DeleteCategory(Discount discount, Category category);
        Task DeleteVendor(Discount discount, Vendor vendor);
        Task DeleteManufacturer(Discount discount, Manufacturer manufacturer);
        Task InsertCategoryToDiscountModel(DiscountModel.AddCategoryToDiscountModel model);
        Task InsertManufacturerToDiscountModel(DiscountModel.AddManufacturerToDiscountModel model);
        Task InsertVendorToDiscountModel(DiscountModel.AddVendorToDiscountModel model);
        Task<(IEnumerable<DiscountModel.DiscountUsageHistoryModel> usageHistoryModels, int totalCount)> PrepareDiscountUsageHistoryModel(Discount discount, int pageIndex, int pageSize);

    }
}
