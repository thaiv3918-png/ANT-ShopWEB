using BCrypt.Net;
using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020740.DataLayers.Interfaces;
using SV22T1020740.Models.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020740.DataLayers.SQLServer
{
    public class CustomerAccountRepository : IUserAccountRepository
    {
        private readonly string _connectionString;

        public CustomerAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"
                    SELECT 
                        CustomerID AS UserId,
                        Email AS UserName,
                        CustomerName AS DisplayName,
                        Email,
                        Password,
                        'customer' AS RoleNames
                    FROM Customers
                    WHERE Email = @Email";

                var user = await connection.QueryFirstOrDefaultAsync<UserAccount>(sql, new
                {
                    Email = userName
                });

                if (user == null)
                    return null;

                // 🔐 verify password
                if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                    return null;

                return user;
            }
        }

        public async Task<bool> ValidatePasswordAsync(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT Password FROM Customers WHERE Email = @Username";

                var hashedPassword = await connection.ExecuteScalarAsync<string>(sql, new
                {
                    Username = username
                });

                if (string.IsNullOrEmpty(hashedPassword))
                    return false;

                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
        }

        public async Task<bool> ChangePasswordAsync(string username, string newPassword)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"
            UPDATE Customers
            SET Password = @Password
            WHERE Email = @Username";

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                var affectedRows = await connection.ExecuteAsync(sql, new
                {
                    Username = username,
                    Password = hashedPassword
                });

                return affectedRows > 0;
            }
        }
        public async Task<int> RegisterAsync(string email, string password, string customerName, string contactName, string province)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // 🔐 Hash password (thay vì lưu plain text)
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                string sql = @"
            INSERT INTO Customers (CustomerName, ContactName, Province, Email, Password, IsLocked)
            VALUES (@CustomerName, @ContactName, @Province, @Email, @Password, 0);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var id = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    CustomerName = customerName,
                    ContactName = contactName,
                    Province = province,
                    Email = email,
                    Password = hashedPassword
                });

                return id;
            }
        }
    }
}
