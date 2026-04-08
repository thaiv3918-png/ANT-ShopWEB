using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020740.DataLayers.Interfaces;
using SV22T1020740.Models.Sales;

namespace SV22T1020740.DataLayers.SQLServer
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối
        /// </summary>
        public ShoppingCartRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<List<ShoppingCartItem>> GetCartAsync(int customerId)
        {
            using var conn = new SqlConnection(_connectionString);

            string sql = @"
        SELECT c.ProductID, c.Quantity,
               p.ProductName, p.Photo, p.Price AS SalePrice
        FROM ShoppingCart c
        JOIN Products p ON c.ProductID = p.ProductID
        WHERE c.CustomerID = @CustomerID";

            return (await conn.QueryAsync<ShoppingCartItem>(sql, new { CustomerID = customerId })).ToList();
        }
        public async Task AddOrUpdateAsync(int customerId, int productId, int quantity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
        IF EXISTS (
            SELECT 1
            FROM ShoppingCart
            WHERE CustomerID = @CustomerID AND ProductID = @ProductID
        )
        BEGIN
            UPDATE ShoppingCart
            SET Quantity = Quantity + @Quantity
            WHERE CustomerID = @CustomerID AND ProductID = @ProductID
        END
        ELSE
        BEGIN
            INSERT INTO ShoppingCart(CustomerID, ProductID, Quantity)
            VALUES(@CustomerID, @ProductID, @Quantity)
        END";

                await connection.ExecuteAsync(sql, new
                {
                    CustomerID = customerId,
                    ProductID = productId,
                    Quantity = quantity
                });
            }
        }
        public async Task UpdateItemAsync(int customerId, int productId, int quantity)
        {
            using var conn = new SqlConnection(_connectionString);

            if (quantity <= 0)
            {
                string deleteSql = @"DELETE FROM ShoppingCart 
                             WHERE CustomerID = @CustomerID AND ProductID = @ProductID";

                await conn.ExecuteAsync(deleteSql, new
                {
                    CustomerID = customerId,
                    ProductID = productId
                });
                return;
            }

            string sql = @"
        UPDATE ShoppingCart
        SET Quantity = @Quantity
        WHERE CustomerID = @CustomerID AND ProductID = @ProductID";

            await conn.ExecuteAsync(sql, new
            {
                CustomerID = customerId,
                ProductID = productId,
                Quantity = quantity
            });
        }
        public async Task RemoveItemAsync(int customerId, int productId)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = "DELETE FROM ShoppingCart WHERE CustomerID = @CustomerID AND ProductID = @ProductID";
            await conn.ExecuteAsync(sql, new { CustomerID = customerId, ProductID = productId });
        }
        public async Task ClearCartAsync(int customerId)
        {
            using var conn = new SqlConnection(_connectionString);

            string sql = @"
        DELETE FROM ShoppingCart
        WHERE CustomerID = @CustomerID";

            await conn.ExecuteAsync(sql, new { CustomerID = customerId });
        }
    }
}
