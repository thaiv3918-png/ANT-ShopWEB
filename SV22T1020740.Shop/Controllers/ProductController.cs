using Microsoft.AspNetCore.Mvc;
using SV22T1020740.BusinessLayers;
using SV22T1020740.Models.Catalog;
using SV22T1020740.Models.Common;

namespace SV22T1020740.Shop.Controllers
{
    /// <summary>
    /// Các chức năng mặt hàng (Shop)
    /// </summary>
    public class ProductController : Controller
    {
        private const string PRODUCT_SEARCH = "ProductSearchInput";
        /// <summary>
        ///Nhập tìm kiếm, hiển thị kết quả tìm kiếm
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index(int CategoryID = 0)
        {
            // load session
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);

            // nếu chưa có session → tạo mới
            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = 12,
                    SearchValue = "",
                    CategoryID = CategoryID, // nếu có CategoryID từ URL → override
                    MinPrice = 0,
                    MaxPrice = 0
                };
            }
            else
            {
                input.PageSize = 12;

                // nếu có CategoryID từ URL → override
                if (CategoryID > 0)
                {
                    input.CategoryID = CategoryID;
                    input.Page = 1;
                }
            }
            // load combobox
            ViewBag.Categories = await CatalogDataService
                .ListCategoriesAsync(new PaginationSearchInput() { Page = 1, PageSize = 0 });

            return View(input);
        }
        /// <summary>
        /// Tìm kiếm và trả về kết quả
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);

            // lưu session
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);

            // load combobox
            ViewBag.Categories = await CatalogDataService
                .ListCategoriesAsync(new PaginationSearchInput() { Page = 1, PageSize = 0 });

            return PartialView("Search", result);
        }
        /// <summary>
        /// Xem chi tiết sản phẩm
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return RedirectToAction("Index");

            // load thêm attribute + ảnh (giống admin nhưng chỉ hiển thị)
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);
            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);

            return View(product);
        }
    }
}