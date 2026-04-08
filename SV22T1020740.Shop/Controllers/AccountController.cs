using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SV22T1020740.BusinessLayers;
using SV22T1020740.Models.Partner;
using System.Threading.Tasks;

namespace SV22T1020740.Shop.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Login - Hiển thị trang đăng nhập
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Login - Xử lý đăng nhập khi người dùng form đăng nhập
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            //Validate input
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            //Authenticate
            var user = await CustomerAccountService.AuthorizeAsync(username, password);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            //Create claims
            var webUser = new WebUserData
            {
                UserId = user.UserId,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Photo = user.Photo,
                Roles = string.IsNullOrEmpty(user.RoleNames)
                ? new List<string>()
                : user.RoleNames.Split(',').ToList()
            };

            var principal = webUser.CreatePrincipal();

            await HttpContext.SignInAsync(
                "ShopScheme",
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });
            
            //Redirect
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Logout - Đăng xuất khỏi hệ thống
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Profile - Hiển thị thông tin tài khoản của người dùng đã đăng nhập
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userData = User.GetUserData();
            if (userData == null)
                return RedirectToAction("Login");

            int customerId = int.Parse(userData.UserId);
            var customer = await PartnerDataService.GetCustomerAsync(customerId);

            ViewBag.Provinces = await GetProvincesAsync(); // hoặc SelectListHelper.ProvincesAsync()
            return View("Profile", customer);
        }
        /// <summary>
        /// Profile - Hiển thị thông tin tài khoản của người dùng đã đăng nhập
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = User.GetUserData();
            if (user == null)
                return RedirectToAction("Login");

            int customerId = int.Parse(user.UserId);

            var customer = await PartnerDataService.GetCustomerAsync(customerId);

            ViewBag.Provinces = await SelectListHelper.ProvincesAsync();

            return View(customer);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(Customer model)
        {
            try
            {
                var user = User.GetUserData();
                if (user == null)
                    return RedirectToAction("Login");

                int customerId = int.Parse(user.UserId);
                var current = await PartnerDataService.GetCustomerAsync(customerId);

                if (current == null)
                    return RedirectToAction("Login");

                current.CustomerName = model.CustomerName;
                current.ContactName = model.ContactName;
                current.Province = model.Province;
                current.Address = model.Address;
                current.Phone = model.Phone;

                var result = await PartnerDataService.UpdateCustomerAsync(current);

                if (!result)
                {
                    ViewBag.Error = "Cập nhật thất bại";
                    ViewBag.Provinces = await GetProvincesAsync();
                    return View("Profile", model);
                }

                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Provinces = await GetProvincesAsync();
                return View("Profile", model);
            }
        }

        /// <summary>
        /// Trang AccessDenied - Hiển thị khi người dùng cố gắng truy cập vào một trang mà họ không có quyền
        /// </summary>
        /// <returns></returns>
        public IActionResult AccessDenied()
        {
            return View();
        }

        //REGISTER
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewBag.Provinces = await GetProvincesAsync();
            return View();
        }
        // Lấy danh sách các tỉnh từ bảng Provinces
        private async Task<List<string>> GetProvincesAsync()
        {
            using (var connection = new SqlConnection(Configuration.ConnectionString))
            {
                var query = "SELECT ProvinceName FROM Provinces";
                var provinces = await connection.QueryAsync<string>(query);
                return provinces.ToList();
            }
        }

        //REGISTER (POST) 
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword,
                                          string customerName, string contactName, string province)
        {
            ViewBag.Provinces = await GetProvincesAsync();
            // Validate
            if (string.IsNullOrWhiteSpace(customerName))
            {
                ViewBag.Error = "Vui lòng nhập tên khách hàng";
                return View();
            }

            if (string.IsNullOrWhiteSpace(contactName))
            {
                ViewBag.Error = "Vui lòng nhập tên liên hệ";
                return View();
            }
            if (string.IsNullOrWhiteSpace(province))
            {
                ViewBag.Error = "Vui lòng chọn tỉnh/thành";
                ViewBag.Provinces = await SelectListHelper.ProvincesAsync(); 
                return View();
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Vui lòng nhập email";
                return View();
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập mật khẩu";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Vui lòng nhập nhiều hơn 6 ký tự";
                return View();
            }

            var isValid = await PartnerDataService.ValidateCustomerEmailAsync(email);

            if (!isValid)
            {
                ViewBag.Error = "Email đã được sử dụng";
                return View();
            }

            //gọi repository
            var result = await CustomerAccountService.RegisterAsync(email, password, customerName, contactName, province);

            if (!result)
            {
                ViewBag.Error = "Đăng ký thất bại";
                return View();
            }

            TempData["Success"] = "Đăng ký thành công!";
            return RedirectToAction("Login");
        }
        /// <summary>
        /// Hiển thị form đổi mật khẩu (dành cho người dùng đã đăng nhập)
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewBag.Title = "Đổi mật khẩu";

            // Kiểm tra người dùng đăng nhập hay chưa
            var userData = User.GetUserData();
            if (userData == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        /// <summary>
        /// Xử lý đổi mật khẩu
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            ViewBag.Title = "Đổi mật khẩu";

            try
            {
                var userData = User.GetUserData();
                if (userData == null)
                {
                    return RedirectToAction("Login");
                }

                var username = userData?.Email;
                if (string.IsNullOrEmpty(username))
                {
                    ModelState.AddModelError("", "Không xác định được tài khoản");
                    return View();
                }

                bool hasError = false;

                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    ModelState.AddModelError("currentPassword", "Vui lòng nhập mật khẩu hiện tại");
                    hasError = true;
                }

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    ModelState.AddModelError("newPassword", "Vui lòng nhập mật khẩu mới");
                    hasError = true;
                }
                else if (newPassword.Length < 6)
                {
                    ModelState.AddModelError("newPassword", "Mật khẩu phải có ít nhất 6 ký tự");
                    hasError = true;
                }

                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp");
                    hasError = true;
                }

                if (hasError || !ModelState.IsValid)
                {
                    return View();
                }


                var result = await CustomerAccountService.ChangePasswordAsync(username, currentPassword, newPassword);

                if (!result)
                {
                    ModelState.AddModelError("", "Không thể đổi mật khẩu. Vui lòng kiểm tra lại mật khẩu hiện tại");
                    return View();
                }

                TempData["Success"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại!";
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ChangePassword Error: {ex.Message}");
                ModelState.AddModelError("", "Hệ thống đang bận, vui lòng thử lại sau");
                return View();
            }
        }
    }
}