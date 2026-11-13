using System.Data;
using Dapper;
using MySql.Data.MySqlClient;
using TarjeTaxi.Models;

namespace TarjeTaxi.Repositories;

public interface IViajeRepository
{
    Task<TaxistaReferencia?> ObtenerTaxistaAsync(string googleId);
    Task<List<Viaje>> ListarAsync(int taxistaId);
    Task<List<Viaje>?> ListarPublicoAsync(Guid publicId);
    Task<Viaje?> ObtenerAsync(int taxistaId, int viajeId);
    Task<int> ContarAsync(int taxistaId);
    Task<int> CrearAsync(int taxistaId, GuardarViajeRequest viaje);
    Task<bool> ActualizarAsync(int taxistaId, int viajeId, GuardarViajeRequest viaje);
    Task EliminarAsync(int taxistaId, int viajeId);
    Task<bool> ReordenarAsync(int taxistaId, IReadOnlyList<int> ids);
    Task<int> AgregarMedioAsync(int viajeId, string tipo, string url, string? cloudinaryPublicId);
    Task<ViajeMedio?> ObtenerMedioAsync(int taxistaId, int medioId);
    Task EliminarMedioAsync(int medioId);
    Task<bool> ReordenarMediosAsync(int taxistaId, int viajeId, IReadOnlyList<int> ids);
}

public class ViajeRepository : IViajeRepository
{
    private readonly string _connectionString;

    public ViajeRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada.");
    }

    private MySqlConnection CreateConnection() => new(_connectionString);

    /// <summary>Verifica al arrancar que existan las tablas, con un mensaje claro si faltan.</summary>
    public static async Task VerificarTablasAsync(string connectionString)
    {
        await using var connection = new MySqlConnection(connectionString);
        var tablas = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME IN ('Viajes', 'ViajeMedios')");
        if (tablas < 2)
        {
            throw new InvalidOperationException(
                "Faltan las tablas Viajes y ViajeMedios. Ejecutá Database/migraciones/003_viajes.sql antes de iniciar la aplicación.");
        }
    }

    public async Task<TaxistaReferencia?> ObtenerTaxistaAsync(string googleId)
    {
        using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<TaxistaReferencia>(
            @"SELECT t.Id, t.PublicId FROM Taxistas t
              INNER JOIN Usuarios u ON u.Id = t.UsuarioId
              WHERE u.GoogleId = @GoogleId",
            new { GoogleId = googleId });
    }

    public async Task<List<Viaje>> ListarAsync(int taxistaId)
    {
        using var connection = CreateConnection();
        var viajes = (await connection.QueryAsync<Viaje>(
            @"SELECT Id, TaxistaId, Titulo, Descripcion, Fecha, Orden FROM Viajes
              WHERE TaxistaId = @TaxistaId ORDER BY Orden, Id",
            new { TaxistaId = taxistaId })).AsList();
        if (viajes.Count == 0)
        {
            return viajes;
        }

        var medios = await connection.QueryAsync<ViajeMedio>(
            @"SELECT m.Id, m.ViajeId, m.Tipo, m.Url, m.CloudinaryPublicId, m.Orden FROM ViajeMedios m
              INNER JOIN Viajes v ON v.Id = m.ViajeId
              WHERE v.TaxistaId = @TaxistaId ORDER BY m.Orden, m.Id",
            new { TaxistaId = taxistaId });
        var porViaje = medios.ToLookup(m => m.ViajeId);
        foreach (var viaje in viajes)
        {
            viaje.Medios = porViaje[viaje.Id].ToList();
        }

        return viajes;
    }

    public async Task<List<Viaje>?> ListarPublicoAsync(Guid publicId)
    {
        int? taxistaId;
        using (var connection = CreateConnection())
        {
            taxistaId = await connection.QuerySingleOrDefaultAsync<int?>(
                "SELECT Id FROM Taxistas WHERE PublicId = @PublicId",
                new { PublicId = publicId.ToString("D") });
        }

        return taxistaId is null ? null : await ListarAsync(taxistaId.Value);
    }

    public async Task<Viaje?> ObtenerAsync(int taxistaId, int viajeId)
    {
        using var connection = CreateConnection();
        var viaje = await connection.QuerySingleOrDefaultAsync<Viaje>(
            @"SELECT Id, TaxistaId, Titulo, Descripcion, Fecha, Orden FROM Viajes
              WHERE Id = @ViajeId AND TaxistaId = @TaxistaId",
            new { ViajeId = viajeId, TaxistaId = taxistaId });
        if (viaje is null)
        {
            return null;
        }

        viaje.Medios = (await connection.QueryAsync<ViajeMedio>(
            @"SELECT Id, ViajeId, Tipo, Url, CloudinaryPublicId, Orden FROM ViajeMedios
              WHERE ViajeId = @ViajeId ORDER BY Orden, Id",
            new { ViajeId = viajeId })).AsList();
        return viaje;
    }

    public async Task<int> ContarAsync(int taxistaId)
    {
        using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Viajes WHERE TaxistaId = @TaxistaId", new { TaxistaId = taxistaId });
    }

    public async Task<int> CrearAsync(int taxistaId, GuardarViajeRequest viaje)
    {
        using var connection = CreateConnection();
        var orden = await connection.ExecuteScalarAsync<int>(
            "SELECT COALESCE(MAX(Orden), -1) + 1 FROM Viajes WHERE TaxistaId = @TaxistaId",
            new { TaxistaId = taxistaId });
        return await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO Viajes (TaxistaId, Titulo, Descripcion, Fecha, Orden)
              VALUES (@TaxistaId, @Titulo, @Descripcion, @Fecha, @Orden);
              SELECT LAST_INSERT_ID();",
            new { TaxistaId = taxistaId, viaje.Titulo, Descripcion = viaje.Descripcion ?? string.Empty, Fecha = viaje.Fecha?.Date, Orden = orden });
    }

    public async Task<bool> ActualizarAsync(int taxistaId, int viajeId, GuardarViajeRequest viaje)
    {
        using var connection = CreateConnection();
        var filas = await connection.ExecuteAsync(
            @"UPDATE Viajes SET Titulo = @Titulo, Descripcion = @Descripcion, Fecha = @Fecha
              WHERE Id = @ViajeId AND TaxistaId = @TaxistaId",
            new { viaje.Titulo, Descripcion = viaje.Descripcion ?? string.Empty, Fecha = viaje.Fecha?.Date, ViajeId = viajeId, TaxistaId = taxistaId });
        return filas > 0;
    }

    public async Task EliminarAsync(int taxistaId, int viajeId)
    {
        using var connection = CreateConnection();
        // ViajeMedios se borra en cascada (FK ON DELETE CASCADE).
        await connection.ExecuteAsync(
            "DELETE FROM Viajes WHERE Id = @ViajeId AND TaxistaId = @TaxistaId",
            new { ViajeId = viajeId, TaxistaId = taxistaId });
    }

    public async Task<bool> ReordenarAsync(int taxistaId, IReadOnlyList<int> ids)
    {
        using var connection = CreateConnection();
        var propios = (await connection.QueryAsync<int>(
            "SELECT Id FROM Viajes WHERE TaxistaId = @TaxistaId", new { TaxistaId = taxistaId })).ToHashSet();
        if (!MismoConjunto(propios, ids))
        {
            return false;
        }

        await GuardarOrdenAsync(connection, "UPDATE Viajes SET Orden = @Orden WHERE Id = @Id", ids);
        return true;
    }

    public async Task<int> AgregarMedioAsync(int viajeId, string tipo, string url, string? cloudinaryPublicId)
    {
        using var connection = CreateConnection();
        var orden = await connection.ExecuteScalarAsync<int>(
            "SELECT COALESCE(MAX(Orden), -1) + 1 FROM ViajeMedios WHERE ViajeId = @ViajeId", new { ViajeId = viajeId });
        return await connection.ExecuteScalarAsync<int>(
            @"INSERT INTO ViajeMedios (ViajeId, Tipo, Url, CloudinaryPublicId, Orden)
              VALUES (@ViajeId, @Tipo, @Url, @CloudinaryPublicId, @Orden);
              SELECT LAST_INSERT_ID();",
            new { ViajeId = viajeId, Tipo = tipo, Url = url, CloudinaryPublicId = cloudinaryPublicId, Orden = orden });
    }

    public async Task<ViajeMedio?> ObtenerMedioAsync(int taxistaId, int medioId)
    {
        using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ViajeMedio>(
            @"SELECT m.Id, m.ViajeId, m.Tipo, m.Url, m.CloudinaryPublicId, m.Orden FROM ViajeMedios m
              INNER JOIN Viajes v ON v.Id = m.ViajeId
              WHERE m.Id = @MedioId AND v.TaxistaId = @TaxistaId",
            new { MedioId = medioId, TaxistaId = taxistaId });
    }

    public async Task EliminarMedioAsync(int medioId)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync("DELETE FROM ViajeMedios WHERE Id = @MedioId", new { MedioId = medioId });
    }

    public async Task<bool> ReordenarMediosAsync(int taxistaId, int viajeId, IReadOnlyList<int> ids)
    {
        using var connection = CreateConnection();
        var propios = (await connection.QueryAsync<int>(
            @"SELECT m.Id FROM ViajeMedios m INNER JOIN Viajes v ON v.Id = m.ViajeId
              WHERE m.ViajeId = @ViajeId AND v.TaxistaId = @TaxistaId",
            new { ViajeId = viajeId, TaxistaId = taxistaId })).ToHashSet();
        if (!MismoConjunto(propios, ids))
        {
            return false;
        }

        await GuardarOrdenAsync(connection, "UPDATE ViajeMedios SET Orden = @Orden WHERE Id = @Id", ids);
        return true;
    }

    private static bool MismoConjunto(HashSet<int> propios, IReadOnlyList<int> ids) =>
        ids.Count == propios.Count && ids.Distinct().Count() == ids.Count && ids.All(propios.Contains);

    private static async Task GuardarOrdenAsync(MySqlConnection connection, string sql, IReadOnlyList<int> ids)
    {
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        await connection.ExecuteAsync(sql, ids.Select((id, indice) => new { Id = id, Orden = indice }), transaction);
        await transaction.CommitAsync();
    }
}
