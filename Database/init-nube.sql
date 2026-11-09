-- =============================================================================
-- Base nueva en la nube (Aiven, TiDB, Clever Cloud u otro MySQL 8 / MariaDB)
-- =============================================================================
-- Diferencias con init.sql:
--   * No crea la base ni usa "USE": el proveedor ya te da una base (en Aiven suele
--     llamarse "defaultdb"). Conectate a esa base y ejecutá este archivo completo.
--   * No incluye las tablas viejas Languages/Profiles ni sus datos de ejemplo.
-- Los nombres de tabla respetan mayúsculas (Taxistas, Usuarios): en Linux MySQL
-- distingue mayúsculas y la aplicación los usa así.
-- Se puede ejecutar más de una vez sin romper nada.
-- =============================================================================

CREATE TABLE IF NOT EXISTS Usuarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GoogleId VARCHAR(255) NULL UNIQUE,
    Nombre VARCHAR(255) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    FotoUrl VARCHAR(2048) NULL,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Taxistas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PublicId CHAR(36) NOT NULL UNIQUE,
    UsuarioId INT NOT NULL UNIQUE,
    NombreServicio VARCHAR(120) NOT NULL,
    NombreCompleto VARCHAR(120) NOT NULL DEFAULT '',
    Telefono VARCHAR(40) NOT NULL DEFAULT '',
    FotoUrl VARCHAR(2048) NULL,
    Vehiculo VARCHAR(120) NOT NULL DEFAULT '',
    Patente VARCHAR(30) NOT NULL DEFAULT '',
    DatosPago VARCHAR(250) NOT NULL DEFAULT '',
    Servicios TEXT NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Taxistas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios (Id) ON DELETE CASCADE
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS DataProtectionKeys (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FriendlyName VARCHAR(255) NULL,
    `Xml` TEXT NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
