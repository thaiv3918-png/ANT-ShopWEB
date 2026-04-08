using SV22T1020740.Models.Sales;

public static class EnumExtensions
{
    public static string ToDisplayName(this OrderStatusEnum status)
    {
        return status switch
        {
            OrderStatusEnum.New => "Đang chờ duyệt",
            OrderStatusEnum.Accepted => "Đã duyệt",
            OrderStatusEnum.Shipping => "Đang giao",
            OrderStatusEnum.Completed => "Hoàn tất",
            OrderStatusEnum.Cancelled => "Đã hủy",
            OrderStatusEnum.Rejected => "Bị từ chối",
            _ => ""
        };
    }
    public static string ToBadgeClass(this OrderStatusEnum status)
    {
        return status switch
        {
            OrderStatusEnum.New => "bg-warning text-dark",     
            OrderStatusEnum.Accepted => "bg-primary",          
            OrderStatusEnum.Shipping => "bg-info text-dark",   
            OrderStatusEnum.Completed => "bg-success",         
            OrderStatusEnum.Cancelled => "bg-danger",          
            OrderStatusEnum.Rejected => "bg-dark",             
            _ => "bg-secondary"
        };
    }
}