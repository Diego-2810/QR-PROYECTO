using System.Data;
using Dapper;
using MySql.Data.MySqlClient;
using QrProfile.Models;

namespace QrProfile.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada.");
    }

    private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

    public async Task<bool> CreateUserAsync(User user)
    {
        const string sql = @"
            INSERT INTO Users (Email, PasswordHash, Slug)
            VALUES (@Email, @PasswordHash, @Slug)";
            
        using var connection = CreateConnection();
        var result = await connection.ExecuteAsync(sql, new { user.Email, user.PasswordHash, user.Slug });
        return result > 0;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        const string sql = "SELECT * FROM Users WHERE Email = @Email";
        
        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }
}
