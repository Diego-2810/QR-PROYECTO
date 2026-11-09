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

    public async Task<int> RellenarPublicIdsFaltantesAsync()
    {
        using var connection = CreateConnection();
        const string checkColumnSql = @"SELECT COUNT(*)
                                        FROM INFORMATION_SCHEMA.COLUMNS
                                        WHERE TABLE_SCHEMA = DATABASE()
                                          AND TABLE_NAME = 'Taxistas'
                                          AND COLUMN_NAME = 'PublicId'";
        var columnaExiste = await connection.ExecuteScalarAsync<int>(checkColumnSql);
        if (columnaExiste == 0)
        {
            throw new InvalidOperationException(
                "Falta la columna PublicId en la tabla Taxistas. Ejecutá primero el script Database/migraciones/001a_taxistas_publicid_agregar.sql antes de iniciar la aplicación.");
        }

        const string selectMissingSql = @"SELECT Id
                                          FROM Taxistas
                                          WHERE PublicId IS NULL OR TRIM(PublicId) = ''";
        var idsSinPublicId = await connection.QueryAsync<int>(selectMissingSql);

        const string updateSql = @"UPDATE Taxistas
                                   SET PublicId = @PublicId
                                   WHERE Id = @Id AND (PublicId IS NULL OR TRIM(PublicId) = '')";

        var actualizadas = 0;
        foreach (var id in idsSinPublicId)
        {
            var filas = await connection.ExecuteAsync(updateSql, new
            {
                Id = id,
                PublicId = Guid.NewGuid().ToString("D")
            });
            actualizadas += filas;
        }

        return actualizadas;
    }

    public async Task EnsurePerfilAsync(Usuario usuario)
    {
        using var connection = CreateConnection();
        const string sql = @"INSERT INTO Taxistas
                                 (PublicId, UsuarioId, NombreServicio, NombreCompleto, Telefono, FotoUrl, Vehiculo, Patente, DatosPago, Servicios)
                             VALUES
                                 (@PublicId, @UsuarioId, @NombreServicio, @NombreCompleto, '', @FotoUrl, '', '', '', @Servicios)
                             ON DUPLICATE KEY UPDATE
                                 UsuarioId = VALUES(UsuarioId),
                                 PublicId = COALESCE(NULLIF(PublicId, ''), VALUES(PublicId))";
        await connection.ExecuteAsync(sql, new
        {
            PublicId = Guid.NewGuid().ToString("D"),
            UsuarioId = usuario.Id,
            NombreServicio = $"{usuario.Nombre} Taxi",
            NombreCompleto = usuario.Nombre,
            FotoUrl = usuario.FotoUrl,
            Servicios = "Traslados particulares, Aeropuertos, Viajes programados"
        });
    }

    public async Task<TaxistaPublicoDto?> GetPublicoAsync(Guid publicId)
    {
        using var connection = CreateConnection();
        const string sql = @"SELECT PublicId, NombreCompleto, Telefono, FotoUrl, Vehiculo, Patente, DatosPago, Servicios
                             FROM Taxistas WHERE PublicId = @PublicId";
        return await connection.QuerySingleOrDefaultAsync<TaxistaPublicoDto>(sql, new { PublicId = publicId.ToString("D") });
    }

    public async Task<TaxistaPerfil?> GetPropioAsync(string googleId)
    {
        using var connection = CreateConnection();
        const string sql = @"SELECT t.Id, t.PublicId, t.NombreCompleto, t.Telefono,
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