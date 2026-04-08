using Microsoft.AspNetCore.Mvc;
using SV22T1020740.BusinessLayers;
using SV22T1020740.Models.Catalog;
using SV22T1020740.Models.Common;
using SV22T1020740.Shop.Models;
using System.Diagnostics;

namespace SV22T1020740.Shop.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Cung cấp chức năng ghi log (nhật ký hoạt động) cho HomeController
        /// </summary>
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        /// <summary>
        // Hiển thị trang chủ, trong đó có danh sách sản phẩm mới nhất (theo ngày cập nhật)
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            /// Lấy danh sách sản phẩm mới nhất (theo ngày cập nhật) để hiển thị trên trang chủ
            var products = await CatalogDataService.ListProductsAsync(
                new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                });
            /// Lấy danh sách các loại sản phẩm để hiển thị trên menu điều hướng
            ViewBag.Categories = await CatalogDataService
                .ListCategoriesAsync(new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = 0
                });
            
            return View(products.DataItems);
        }
        /// <summary>
        /// hiển thị trang giới thiệu về cửa hàng, có thể là thông tin liên hệ, địa chỉ, sứ mệnh, tầm nhìn, v.v.
        /// </summary>
        /// <returns></returns>
        public IActionResult Privacy()
        {
            return View();
        }
        /// <summary>
        ///hiển thị trang lỗi khi có lỗi xảy ra trong quá trình xử lý yêu cầu, cung cấp thông tin về lỗi để người dùng có thể hiểu và phản hồi lại nếu cần thiết.
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
