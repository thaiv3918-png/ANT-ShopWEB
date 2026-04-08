namespace SV22T1020740.Shop
{
    /// <summary>
    /// Lấy URL của website quản trị từ appsettings.json, ví dụ: https://admin.mywebsite.com/
    /// </summary>
    public static class AppSettings
    {
        public static string AdminBaseUrl =>
            ApplicationContext.GetConfigValue("AdminBaseUrl");
    }
}
