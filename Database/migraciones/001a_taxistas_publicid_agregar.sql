-- =============================================================================
-- Migración 001a: Agregar identificador público aleatorio (PublicId) a Taxistas
-- =============================================================================
-- Cuándo ejecutar: ANTES de desplegar la versión nueva de la aplicación.
-- Compatibilidad: MySQL 8.0+ y MariaDB 10.x+.
-- Idempotencia: puede ejecutarse más de una vez sin fallar.
-- Importante: NO rellena filas con UUID() de MySQL/MariaDB porque UUID() genera
--             identificadores versión 1 (basados en tiempo, predecibles).
--             Las filas existentes quedan en NULL y la aplicación les asigna
--             Guid.NewGuid() (UUID v4 aleatorio) automáticamente al iniciar.
-- =============================================================================

USE QrProfileDB;

-- 1. Agregar la columna PublicId CHAR(36) NULL solo si todavía no existe.
SET @columna_existe := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Taxistas'
      AND COLUMN_NAME = 'PublicId'
);

SET @sql_agregar_columna := IF(
    @columna_existe = 0,
    'ALTER TABLE Taxistas ADD COLUMN PublicId CHAR(36) NULL AFTER Id',
    'SELECT ''La columna PublicId ya existe en Taxistas'' AS Info'
);

PREPARE stmt_columna FROM @sql_agregar_columna;
EXECUTE stmt_columna;
DEALLOCATE PREPARE stmt_columna;

-- 2. Crear el índice UNIQUE sobre PublicId solo si todavía no existe ningún
--    índice único sobre esa columna (en MySQL/MariaDB un índice UNIQUE permite
--    múltiples valores NULL mientras la aplicación aún no haya rellenado las filas).
SET @indice_existe := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Taxistas'
      AND COLUMN_NAME = 'PublicId'
      AND NON_UNIQUE = 0
);

SET @sql_crear_indice := IF(
    @indice_existe = 0,
    'CREATE UNIQUE INDEX UX_Taxistas_PublicId ON Taxistas (PublicId)',
    'SELECT ''Ya existe un índice UNIQUE sobre Taxistas.PublicId'' AS Info'
);

PREPARE stmt_indice FROM @sql_crear_indice;
EXECUTE stmt_indice;
DEALLOCATE PREPARE stmt_indice;
