-- =============================================================================
-- Migración 001b: Exigir PublicId NOT NULL en Taxistas
-- =============================================================================
-- Cuándo ejecutar: DESPUÉS de desplegar la versión nueva de la aplicación
--                  (que ya inserta PublicId en cada alta de taxista).
-- Compatibilidad: MySQL 8.0+ y MariaDB 10.x+.
-- Idempotencia: puede ejecutarse más de una vez sin romper el esquema ni el
--               índice único creado en 001a.
-- =============================================================================

USE QrProfileDB;

-- 1. Rellenar con UUID() cualquier fila creada durante la ventana de despliegue
--    que haya quedado con PublicId en NULL o vacío.
UPDATE Taxistas
SET PublicId = LOWER(UUID())
WHERE PublicId IS NULL OR TRIM(PublicId) = '';

-- 2. Pasar la columna PublicId a NOT NULL (sin repetir UNIQUE para no crear
--    índices duplicados en reejecuciones).
ALTER TABLE Taxistas
    MODIFY COLUMN PublicId CHAR(36) NOT NULL;
