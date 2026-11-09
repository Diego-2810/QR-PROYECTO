-- =============================================================================
-- Migración 001c (MANUAL Y OPCIONAL): Regenerar todos los PublicId
-- =============================================================================
-- Cuándo usar: SOLO en bases de datos donde ya se hayan ejecutado las versiones
--              anteriores de 001a/001b que rellenaban PublicId con UUID() de
--              MySQL/MariaDB (UUID v1 basado en tiempo).
--
-- ADVERTENCIA IMPORTANTE:
--   Ejecutar este script borra todos los PublicId actuales (los pone en NULL),
--   por lo que INVALIDA TODOS LOS LINKS PÚBLICOS (/perfil/{guid}) Y CÓDIGOS QR
--   ACTUALES. No debe ejecutarse de forma automática.
--
-- Pasos posteriores a ejecutar 001c:
--   1. Iniciar o reiniciar la aplicación para que asigne nuevos Guid.NewGuid()
--      aleatorios (UUID v4) a todas las filas en NULL.
--   2. Verificar que no queden filas con PublicId IS NULL.
--   3. Ejecutar 001b_taxistas_publicid_exigir.sql para volver a fijar NOT NULL.
-- =============================================================================

USE QrProfileDB;

-- 1. Permitir temporalmente NULL en la columna PublicId.
ALTER TABLE Taxistas
    MODIFY COLUMN PublicId CHAR(36) NULL;

-- 2. Limpiar todos los PublicId existentes para que la aplicación los regenere
--    con Guid.NewGuid() al iniciar.
UPDATE Taxistas
SET PublicId = NULL;

