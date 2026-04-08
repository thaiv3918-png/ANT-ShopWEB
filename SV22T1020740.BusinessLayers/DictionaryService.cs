using SV22T1020740.DataLayers.Interfaces;
using SV22T1020740.DataLayers.SQLServer;
using SV22T1020740.Models.DataDictionary;

namespace SV22T1020740.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu từ điển của hệ thống
    /// (Data Dictionary)
    ///
    /// Hiện tại bao gồm:
    /// - Provinces (Danh sách tỉnh/thành)
    ///
    /// Các dữ liệu từ điển thường chỉ phục vụ mục đích tra cứu
    /// để hiển thị trong các combobox, dropdown list,... trong giao diện
    /// </summary>
    public static class DictionaryService
    {
        private static readonly IDataDictionaryRepository<Province> provinceDB;

        /// <summary>
        /// Constructor
        /// Khởi tạo repository dùng để truy xuất dữ liệu tỉnh/thành
        /// </summary>
        static DictionaryService()
        {
            provinceDB = new ProvinceRepository(Configuration.ConnectionString);
        }

        /// <summary>
        /// Lấy danh sách toàn bộ tỉnh/thành trong hệ thống
        /// </summary>
        /// <returns>
        /// Danh sách các tỉnh/thành
        /// </returns>
        public static async Task<List<Province>> ListProvincesAsync()
        {
            //TODO: Trong tương lai có thể bổ sung cache để giảm truy vấn database vì dữ liệu tỉnh/thành hầu như không thay đổi

            return await provinceDB.ListAsync();
        }
    }

}