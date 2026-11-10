-- =============================================================================
-- Migración 003: sección "Viajes" (álbumes de fotos y videos del taxista)
-- =============================================================================
-- Viajes:      cada álbum (ej. "Cataratas del Iguazú").
-- ViajeMedios: fotos y videos de cada álbum.
--   Tipo = 'imagen' | 'video'   -> archivo subido a Cloudinary (CloudinaryPublicId)
--   Tipo = 'youtube'            -> video de YouTube (se muestra dentro de la tarjeta)
--   Tipo = 'enlace'             -> Instagram, TikTok, Facebook o Vimeo (se abre aparte)
-- Cuándo ejecutar: en una base que ya existía, ANTES de iniciar esta versión.
--                  Las bases nuevas creadas con init.sql o init-nube.sql ya lo traen.
-- Compatibilidad: MySQL 8.0+ y MariaDB 10.x+. Se puede ejecutar más de una vez.
-- =============================================================================

CREATE TABLE IF NOT EXISTS Viajes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TaxistaId INT NOT NULL,
    Titulo VARCHAR(120) NOT NULL,
    Descripcion VARCHAR(1000) NOT NULL DEFAULT '',
    Fecha DATE NULL,
    Orden INT NOT NULL DEFAULT 0,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Viajes_Taxistas FOREIGN KEY (TaxistaId) REFERENCES Taxistas (Id) ON DELETE CASCADE,
    INDEX IX_Viajes_Taxista_Orden (TaxistaId, Orden)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ViajeMedios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ViajeId INT NOT NULL,
    Tipo VARCHAR(10) NOT NULL,
    Url VARCHAR(2048) NOT NULL,
    CloudinaryPublicId VARCHAR(255) NULL,
    Orden INT NOT NULL DEFAULT 0,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_ViajeMedios_Viajes FOREIGN KEY (ViajeId) REFERENCES Viajes (Id) ON DELETE CASCADE,
    INDEX IX_ViajeMedios_Viaje_Orden (ViajeId, Orden)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
