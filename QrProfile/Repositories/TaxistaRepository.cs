using System.Data;
using Dapper;
using MySql.Data.MySqlClient;
using QrProfile.Models;

namespace QrProfile.Repositories;

public class TaxistaRepository : ITaxistaRepository
{
    private readonly string _connectionString;

    public TaxistaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada.");
    }

    private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

    public async Task EnsurePerfilAsync(Usuario usuario)
    {
        using var connection = CreateConnection();
        const string sql = @"INSERT INTO Taxistas
                                 (UsuarioId, NombreServicio, NombreCompleto, Telefono, FotoUrl, Vehiculo, Patente, DatosPago, Servicios)
                             VALUES
                                 (@UsuarioId, @NombreServicio, @NombreCompleto, '', @FotoUrl, '', '', '', @Servicios)
                             ON DUPLICATE KEY UPDATE UsuarioId = VALUES(UsuarioId)";
        await connection.ExecuteAsync(sql, new
        {
            UsuarioId = usuario.Id,
            NombreServicio = $"{usuario.Nombre} Taxi",
            NombreCompleto = usuario.Nombre,
            FotoUrl = usuario.FotoUrl,
            Servicios = "Traslados particulares, Aeropuertos, Viajes programados"
        });
    }

    public async Task<TaxistaPerfil?> GetPublicoAsync(int taxistaId)
    {
        using var connection = CreateConnection();
        const string sql = @"SELECT Id, NombreCompleto, Telefono, FotoUrl, Vehiculo, Patente, DatosPago, Servicios
                             FROM Taxistas WHERE Id = @TaxistaId";
        return await connection.QuerySingleOrDefaultAsync<TaxistaPerfil>(sql, new { TaxistaId = taxistaId });
    }

    public async Task<TaxistaPerfil?> GetPropioAsync(string googleId)
    {
        using var connection = CreateConnection();
        const string sql = @"SELECT t.Id, t.NombreCompleto, t.Telefono,
                                    COALESCE(NULLIF(t.FotoUrl, ''), u.FotoUrl) AS FotoUrl,
                                    t.Vehiculo, t.Patente, t.DatosPago, t.Servicios
                             FROM Taxistas t
                             INNER JOIN Usuarios u ON u.Id = t.UsuarioId
                             WHERE u.GoogleId = @GoogleId";
        return await connection.QuerySingleOrDefaultAsync<TaxistaPerfil>(sql, new { GoogleId = googleId });
    }

    public async Task<bool> ActualizarPropioAsync(string googleId, ActualizarTaxistaRequest perfil)
    {
        using var connection = CreateConnection();
        const string existsSql = @"SELECT EXISTS(
                                       SELECT 1
                                       FROM Taxistas t
                                       INNER JOIN Usuarios u ON u.Id = t.UsuarioId
                                       WHERE u.GoogleId = @GoogleId)";
        var existe = await connection.ExecuteScalarAsync<bool>(existsSql, new { GoogleId = googleId });
        if (!existe)
        {
            return false;
        }

        const string sql = @"UPDATE Taxistas t
                             INNER JOIN Usuarios u ON u.Id = t.UsuarioId
                             SET t.NombreServicio = CONCAT(@NombreCompleto, ' Taxi'),
                                 t.NombreCompleto = @NombreCompleto,
                                 t.Telefono = @Telefono,
                                 t.FotoUrl = @FotoUrl,
                                 t.Vehiculo = @Vehiculo,
                                 t.Patente = @Patente,
                                 t.DatosPago = @DatosPago,
                                 t.Servicios = @Servicios
                             WHERE u.GoogleId = @GoogleId";
        await connection.ExecuteAsync(sql, new
        {
            GoogleId = googleId,
            perfil.NombreCompleto,
            perfil.Telefono,
            perfil.FotoUrl,
            perfil.Vehiculo,
            perfil.Patente,
            perfil.DatosPago,
            perfil.Servicios
        });
        return true;
    }
}