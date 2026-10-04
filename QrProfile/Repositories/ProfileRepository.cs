using System.Data;
using Dapper;
using MySql.Data.MySqlClient;
using QrProfile.Models;

namespace QrProfile.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly string _connectionString;

    public ProfileRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada.");
    }

    private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

    public async Task<ProfileData?> GetByLanguageCodeAsync(string languageCode)
    {
        const string sql = @"SELECT Id, LanguageCode, BusinessName, CoreValues, WhatsApp, Services 
                            FROM Profiles 
                            WHERE LanguageCode = @LanguageCode";

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<ProfileData>(sql, new { LanguageCode = languageCode });
    }
}
