using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020740.BusinessLayers;
using SV22T1020740.Models.Sales;

namespace SV22T1020740.Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private const string ORDER_SEARCH = "OrderSearchInput";
        private readonly ShoppingCartService _cartService;

        public OrderController(ShoppingCartService cartService)
        {
            _cartService = cartService;
        }
        //GIỎ HÀNG
        /// <summary>
        /// Hàm trả về trang giỏ hàng, hiển thị danh sách sản phẩm trong giỏ hàng và tiền. Nếu khách hàng chưa đăng nhập thì chuyển về trang đăng nhập
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Cart()
        {
            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(user.UserId);

            var cart = await _cartService.GetCartAsync(customerId);

            return View(cart);
        }
        /// <summary>
        /// Hàm trả về partial view hiển thị danh sách sản phẩm trong giỏ hàng. Hàm này được gọi khi khách hàng click vào icon giỏ hàng ở header để xem nhanh giỏ hàng mà không cần chuyển trang
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> ShowCart()
        {
            var user = User.GetUserData();
            if (user == null)
                return PartialView(new List<ShoppingCartItem>());

            int customerId = int.Parse(user.UserId);

            var cart = await _cartService.GetCartAsync(customerId);

            return PartialView(cart);
        }
        /// <summary>
        /// Hàm dùng để thêm một sản phẩm vào giỏ hàng. Nếu sản phẩm đã tồn tại trong giỏ hàng thì cập nhật lại số lượng. Nếu khách hàng chưa đăng nhập thì trả về lỗi yêu cầu đăng nhập
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AddCartItem(int productId, int quantity)
        {
            try
            {
                var user = User.GetUserData();
                if (user == null)
                    return Json(new { code = 0, message = "Vui lòng đăng nhập" });

                int customerId = int.Parse(user.UserId);

                await _cartService.AddItemAsync(customerId, productId, quantity);

                return Json(new { code = 1 });
            }
            catch (Exception ex)
            {
                return Json(new { code = 0, message = ex.Message }); // có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng, trả về lỗi
            }
        }
        /// <summary>
        /// Hàm trả về form xác nhận xóa một sản phẩm khỏi giỏ hàng. Nếu là POST thì thực hiện xóa sản phẩm khỏi giỏ hàng.
        /// Nếu khách hàng chưa đăng nhập thì trả về lỗi yêu cầu đăng nhập. Nếu productId không hợp lệ thì trả về lỗi dữ liệu không hợp lệ
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteCartItem(int productId = 0)
        {
            try
            {
                var user = User.GetUserData();
                if (user == null)
                    return Json(new ApiResult(0, "Vui lòng đăng nhập"));

                int customerId = int.Parse(user.UserId);

                // POST
                if (Request.Method == "POST")
                {
                    if (productId <= 0)
                        return Json(new ApiResult(0, "Dữ liệu không hợp lệ"));

                    await _cartService.RemoveItemAsync(customerId, productId);

                    return Json(new ApiResult(1));
                }

                // GET
                var cart = await _cartService.GetCartAsync(customerId);
                var item = cart.FirstOrDefault(x => x.ProductID == productId);

                return PartialView(item);
            }
            catch
            {
                return Json(new ApiResult(0, "Hệ thống lỗi"));
            }
        }
        /// <summary>
        /// Hàm dùng để xóa tất cả sản phẩm trong giỏ hàng. Nếu khách hàng chưa đăng nhập thì trả về lỗi yêu cầu đăng nhập
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var user = User.GetUserData();
            if (user == null)
                return Unauthorized();

            int customerId = int.Parse(user.UserId);

            await _cartService.ClearCartAsync(customerId);

            return Json(new ApiResult(1));
        }
        /// <summary>
        /// Hàm dùng để cập nhật số lượng của một sản phẩm trong giỏ hàng. Nếu khách hàng chưa đăng nhập thì trả về lỗi yêu cầu đăng nhập.
        /// Nếu productId hoặc quantity không hợp lệ thì trả về lỗi dữ liệu không hợp lệ
        /// </summary>
        /// <param name="productId">Mã sản phẩm cần cập nhật số lượng</param>
        /// <param name="quantity">Số lượng sản phẩm</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(int productId, int quantity)
        {
            try
            {
                var user = User.GetUserData();
                if (user == null)
                    return Unauthorized();

                int customerId = int.Parse(user.UserId);

                await _cartService.UpdateItemAsync(customerId, productId, quantity);

                return Json(new ApiResult(1));
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(0, ex.Message));
            }
        }
        /// <summary>
        /// Hàm trả về tổng số lượng sản phẩm trong giỏ hàng của khách hàng. Nếu khách hàng chưa đăng nhập thì trả về 0
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IActionResult> GetCartCount()
        {
            var user = User.GetUserData();
            if (user == null)
                return Json(0);

            int customerId = int.Parse(user.UserId);

            int total = await _cartService.GetCartCountAsync(customerId);

            return Json(total);
        }
        // ĐẶT HÀNG

        /// <summary>
        /// Hàm trả về form đặt hàng.
        /// <returns></returns>
        public async Task<IActionResult> Checkout()
        {
            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(user.UserId);

            var buyNowCart = HttpContext.Session.GetObject<List<ShoppingCartItem>>("BuyNowCart");

            if (buyNowCart != null)
            {
                ViewBag.Provinces = await SelectListHelper.ProvincesAsync();
                return View(buyNowCart);
            }

            var cart = await _cartService.GetCartAsync(customerId);

            ViewBag.Provinces = await SelectListHelper.ProvincesAsync();
            return View(cart);
        }
        /// <summary>
        /// Hàm dùng để tạo một đơn hàng mới từ giỏ hàng của khách hàng. Nếu khách hàng chưa đăng nhập thì trả về lỗi yêu cầu đăng nhập. 
        /// Nếu có lỗi xảy ra khi tạo đơn hàng thì trả về lỗi hệ thống
        /// </summary>
        /// <param name="province">Tỉnh thành của customer</param>
        /// <param name="address">Địa chỉ của customer</param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> CreateOrder(string province, string address)
        {
            try
            {
                var user = User.GetUserData();
                if (user == null)
                    return Json(new ApiResult(0, "Vui lòng đăng nhập"));

                int customerId = int.Parse(user.UserId);

                var buyNowCart = HttpContext.Session.GetObject<List<ShoppingCartItem>>("BuyNowCart");

                if (buyNowCart != null && buyNowCart.Count > 0)
                {
                    // xóa cart cũ, ko bị gộp
                    await _cartService.ClearCartAsync(customerId);

                    // add cart mới vào db
                    foreach (var item in buyNowCart)
                    {
                        await _cartService.AddItemAsync(customerId, item.ProductID, item.Quantity);
                    }

                    // xóa cart tạm
                    HttpContext.Session.Remove("BuyNowCart");
                }

                // tạo đơn hàng mới từ cart của customer
                int orderId = await SalesDataService.AddOrderFromCustomerAsync(
                    customerId,
                    province,
                    address
                );

                return Json(new ApiResult(orderId));
            }
            catch (Exception ex)
            {
                return Json(new ApiResult(0, ex.Message));
            }
        }
        // ĐƠN HÀNG (USER)

        /// <summary>
        /// Hàm trả về trang danh sách đơn hàng của khách hàng. Trang này hiển thị thông tin đơn hàng và trạng thái đơn hàng.
        /// Nếu khách hàng chưa đăng nhập thì chuyển về trang đăng nhập
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                var input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = 5,
                    SearchValue = "",
                    Status = 0
                };
                ViewBag.OrderStatuses = Enum.GetValues(typeof(OrderStatusEnum))
                                            .Cast<OrderStatusEnum>()
                                            .ToList();
                return View(input);
            }
            catch
            {
                return Json(new ApiResult(0, "Lỗi hệ thống"));
            }

        }
        /// <summary>
        /// Hàm trả về partial view hiển thị danh sách đơn hàng của khách hàng theo điều kiện tìm kiếm.
        /// Hàm này được gọi khi khách hàng click vào nút tìm kiếm hoặc phân trang ở trang danh sách đơn hàng.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            try
            {
                var userData = User.GetUserData();
                if (userData == null)
                    return Json(new ApiResult(0, "Vui lòng đăng nhập"));
                int customerID = int.Parse(userData.UserId);

                input.PageSize = 5;
                var result = await SalesDataService.ListOrdersByCustomerAsync(customerID, input);
                return PartialView(result);
            }
            catch
            {
                return Json(new ApiResult(0, "Lỗi hệ thống"));
            }

        }

        /// <summary>
        /// Hàm trả về trang chi tiết của một đơn hàng. Trang này hiển thị thông tin đơn hàng và danh sách sản phẩm trong đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng muốn xem chi tiết</param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userData = User.GetUserData();
                if (userData == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int userId = int.Parse(userData.UserId);
                // Lấy thông tin đơn hàng để kiểm tra quyền truy cập
                var order = await SalesDataService.GetOrderAsync(id);
                if (order == null || order.CustomerID != userId)
                    return RedirectToAction("Index");
                // Lấy danh sách sản phẩm trong đơn hàng để hiển thị ở trang chi tiết
                ViewBag.OrderDetails = await SalesDataService.ListDetailsAsync(id);

                return View(order);
            }
            catch
            {
                return Json(new ApiResult(0, "Lỗi hệ thống"));
            }

        }

        /// <summary>
        /// Theo dõi trạng thái
        /// </summary>
        /// <param name="id">Mã đơn hàng cần theo dõi</param>
        /// <returns></returns>
        public async Task<IActionResult> Status(int id)
        {
            try
            {
                /// Kiểm tra quyền truy cập
                var userData = User.GetUserData();
                if (userData == null)
                    return RedirectToAction("Login", "Account");
                /// Chỉ được xem trạng thái đơn hàng của chính mình

                int userId = int.Parse(userData.UserId);
                /// Lấy thông tin đơn hàng để kiểm tra quyền truy cập

                var order = await SalesDataService.GetOrderAsync(id);
                // Nếu đơn hàng không tồn tại hoặc không phải của khách hàng đang đăng nhập thì chuyển về trang danh sách đơn hàng

                if (order == null || order.CustomerID != userId)
                    return RedirectToAction("Index");

                return View(order);
            }
            catch
            {
                // Nếu có lỗi xảy ra thì chuyển về trang danh sách đơn hàng và hiển thị thông báo lỗi
                return Json(new ApiResult(0, "Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Hủy đơn (khách)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                /// Kiểm tra quyền truy cập
                var userData = User.GetUserData();
                if (userData == null)
                    return Json(new { code = 0, message = "Vui lòng đăng nhập" });

                int userId = int.Parse(userData.UserId);

                var order = await SalesDataService.GetOrderAsync(id);
                // Nếu đơn hàng không tồn tại hoặc không phải của khách hàng đang đăng nhập thì trả về lỗi không có quyền

                if (order == null || order.CustomerID != userId)
                    return Json(new { code = 0, message = "Không có quyền" });
                // Chỉ được hủy đơn hàng đang ở trạng thái mới, nếu đơn hàng đã được chấp nhận, đang giao hàng hoặc đã hoàn thành thì không được hủy

                if (order.Status != OrderStatusEnum.New)
                    return Json(new { code = 0, message = "Không thể hủy" });
                // Thực hiện hủy đơn hàng

                var ok = await SalesDataService.CancelOrderAsync(id);
                // Nếu hủy không thành công thì trả về lỗi

                if (!ok)
                    return Json(new { code = 0, message = "Không thể hủy" });

                // Hủy thành công thì trả về mã đơn hàng và thông báo đã hủy
                return Json(new { code = 1, orderId = id, message = "Đơn đã được hủy" });
            }
            catch
            {
                // Nếu có lỗi xảy ra thì trả về lỗi hệ thống
                return Json(new { code = 0, message = "Lỗi hệ thống" });
            }
        }
        public async Task<IActionResult> BuyNow(int productId)
        {
            /// Kiểm tra quyền truy cập
            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login", "Account");

            int customerId = int.Parse(user.UserId);

            var product = await CatalogDataService.GetProductAsync(productId);

            if (product == null)
                return RedirectToAction("Index", "Product");

            // 👉 tạo cart tạm chỉ chứa 1 sản phẩm để hiển thị ở trang Checkout, tránh bị gộp với cart cũ
            var tempCart = new List<ShoppingCartItem>
            {
                new ShoppingCartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Photo = product.Photo,
                    Quantity = 1,
                    SalePrice = product.Price
                }
            };

            // 👉 lưu cart tạm vào session để hiển thị ở trang Checkout
            HttpContext.Session.SetObject("BuyNowCart", tempCart);

            return RedirectToAction("Checkout");
        }
    }
}