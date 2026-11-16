using System.Xml.Linq;
using Dapper;
using Microsoft.AspNetCore.DataProtection.Repositories;
using MySql.Data.MySqlClient;

namespace QrProfile.Repositories;

/// <summary>
/// Guarda en la base las claves de Data Protection (las que cifran la cookie de sesión).
/// Sin esto, en Render las claves viven dentro del contenedor y se pierden en cada reinicio
/// o despliegue, y todos los taxistas tienen que volver a iniciar sesión.
/// </summary>
public sealed class MySqlXmlRepository : IXmlRepository
{
    private readonly string _connectionString;

    public MySqlXmlRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // IXmlRepository es sincrónico por diseño; se llama pocas veces (al arrancar y al rotar claves).
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        using var connection = new MySqlConnection(_connectionString);
        return connection.Query<string>("SELECT `Xml` FROM DataProtectionKeys ORDER BY Id")
            .Select(XElement.Parse)
            .ToList()
            .AsReadOnly();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        using var connection = new MySqlConnection(_connectionString);
        connection.Execute(
            "INSERT INTO DataProtectionKeys (FriendlyName, `Xml`) VALUES (@FriendlyName, @Xml)",
            new { FriendlyName = friendlyName, Xml = element.ToString(SaveOptions.DisableFormatting) });
    }

    /// <summary>Verifica al arrancar que exista la tabla, con un mensaje claro si falta.</summary>
    public static async Task VerificarTablaAsync(string connectionString)
    {
        await using var connection = new MySqlConnection(connectionString);
        var existe = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'DataProtectionKeys'");
        if (existe == 0)
        {
            throw new InvalidOperationException(
                "Falta la tabla DataProtectionKeys. Ejecutá Database/migraciones/002_data_protection_keys.sql antes de iniciar la aplicación.");
        }
    }
}
