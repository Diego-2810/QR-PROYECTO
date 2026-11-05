-- =============================================================================
-- Migración 001a: Agregar identificador público aleatorio (PublicId) a Taxistas
-- =============================================================================
-- Cuándo ejecutar: ANTES de desplegar la versión nueva de la aplicación.
-- Compatibilidad: MySQL 8.0+ y MariaDB 10.x+.
-- Idempotencia: puede ejecutarse más de una vez sin fallar ni sobrescribir
--               los PublicId ya generados.
-- Seguridad para el código actual: la columna se crea como NULL para que las
--               inserciones del código viejo (que todavía no envía PublicId)
--               sigan funcionando durante la ventana de despliegue.
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

-- 2. Rellenar con UUID() las filas existentes que tengan PublicId en NULL o vacío.
UPDATE Taxistas
SET PublicId = LOWER(UUID())
WHERE PublicId IS NULL OR TRIM(PublicId) = '';

-- 3. Crear el índice UNIQUE sobre PublicId solo si todavía no existe ningún
--    índice único sobre esa columna (por ejemplo si init.sql ya creó uno).
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
