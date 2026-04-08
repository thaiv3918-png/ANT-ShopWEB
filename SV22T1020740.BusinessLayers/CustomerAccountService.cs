using SV22T1020740.DataLayers.Interfaces;
using SV22T1020740.DataLayers.SQLServer;
using SV22T1020740.Models.Security;

namespace SV22T1020740.BusinessLayers
{
    public static class CustomerAccountService
    {
        private static readonly IUserAccountRepository customerDB;

        static CustomerAccountService()
        {
            customerDB = new CustomerAccountRepository(Configuration.ConnectionString);
        }

        public static async Task<UserAccount?> AuthorizeAsync(string username, string password)
        {
            return await customerDB.AuthorizeAsync(username, password);
        }

        public static async Task<bool> RegisterAsync(string email, string password,
                                             string customerName, string contactName, string province)
        {
            var id = await customerDB.RegisterAsync(email, password, customerName, contactName, province);
            return id > 0;
        }
        /// <summary>
        /// Kiểm tra mật khẩu hiện tại của tài khoản
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="password">Mật khẩu cần kiểm tra</param>
        /// <returns>True nếu mật khẩu đúng, False nếu sai</returns>
        public static async Task<bool> ValidatePasswordAsync(string username, string password)
        {
            if (customerDB == null)
                throw new Exception("UserAccountService chưa được khởi tạo");

            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    return false;

                return await customerDB.ValidatePasswordAsync(username, password);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ValidatePasswordAsync Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Đổi mật khẩu (yêu cầu nhập mật khẩu cũ)
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="currentPassword">Mật khẩu hiện tại</param>
        /// <param name="newPassword">Mật khẩu mới</param>
        /// <returns>True nếu đổi thành công, False nếu thất bại</returns>
        public static async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            if (customerDB == null)
                throw new Exception("CustomerAccountService chưa được khởi tạo");

            try
            {
                // Kiểm tra mật khẩu hiện tại
                var isValid = await ValidatePasswordAsync(username, currentPassword);
                if (!isValid)
                    return false;

                // Kiểm tra mật khẩu mới
                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                    return false;

                // Không cho phép đặt mật khẩu mới trùng với mật khẩu cũ
                var isSame = await ValidatePasswordAsync(username, newPassword);
                if (isSame)
                    return false;

                // Cập nhật mật khẩu mới
                return await customerDB.ChangePasswordAsync(username, newPassword);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ChangePasswordAsync Error: {ex.Message}");
                return false;
            }
        }
    }
}