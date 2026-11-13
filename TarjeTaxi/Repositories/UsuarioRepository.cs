using System.Data;
using Dapper;
using MySql.Data.MySqlClient;
using TarjeTaxi.Models;

namespace TarjeTaxi.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Cadena de conexión 'DefaultConnection' no encontrada.");
    }

    private IDbConnection CreateConnection() => new MySqlConnection(_connectionString);

    public async Task<Usuario> RegistrarOBuscarUsuarioGoogleAsync(Usuario usuario)
    {
        using var connection = CreateConnection();
        const string findSql = @"SELECT Id, GoogleId, Nombre, Email, FotoUrl, FechaCreacion
                                 FROM Usuarios
                                 WHERE Email = @Email OR GoogleId = @GoogleId";

        var coincidencias = (await connection.QueryAsync<Usuario>(findSql, usuario)).AsList();
        if (coincidencias.Select(u => u.Id).Distinct().Skip(1).Any())
        {
            throw new InvalidOperationException("El email y el identificador de Google pertenecen a usuarios diferentes.");
        }

        if (coincidencias.Count > 0)
        {
            var existente = coincidencias[0];
            if (!string.IsNullOrWhiteSpace(existente.GoogleId) && existente.GoogleId != usuario.GoogleId)
            {
                throw new InvalidOperationException("El email ya está vinculado a otra cuenta de Google.");
            }

            const string updateSql = @"UPDATE Usuarios
                                       SET GoogleId = @GoogleId,
                                           Nombre = @Nombre,
                                           Email = @Email,
                                           FotoUrl = @FotoUrl
                                       WHERE Id = @Id";
            await connection.ExecuteAsync(updateSql, new
            {
                usuario.GoogleId,
                usuario.Nombre,
                usuario.Email,
                usuario.FotoUrl,
                existente.Id
            });

            const string findExistingByIdSql = @"SELECT Id, GoogleId, Nombre, Email, FotoUrl, FechaCreacion
                                                 FROM Usuarios
                                                 WHERE Id = @Id";
            return await connection.QuerySingleAsync<Usuario>(findExistingByIdSql, new { existente.Id });
        }

        const string insertSql = @"INSERT INTO Usuarios (GoogleId, Nombre, Email, FotoUrl)
                                   VALUES (@GoogleId, @Nombre, @Email, @FotoUrl)
                                   ON DUPLICATE KEY UPDATE Id = LAST_INSERT_ID(Id)";
        await connection.ExecuteAsync(insertSql, usuario);

        var id = await connection.ExecuteScalarAsync<int>("SELECT LAST_INSERT_ID();");
        const string findByIdSql = @"SELECT Id, GoogleId, Nombre, Email, FotoUrl, FechaCreacion
                         FROM Usuarios
                         WHERE Id = @Id";

        return await connection.QuerySingleAsync<Usuario>(findByIdSql, new { Id = id });
    }
}