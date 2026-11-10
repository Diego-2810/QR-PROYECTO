-- =============================================================================
-- Migración 004: Nombre / Apellido y Marca / Modelo en Taxistas
-- =============================================================================
-- Agrega cuatro columnas:
--   Nombre   (obligatorio en el panel; puede ser un nombre comercial, ej. "Diego Ya")
--   Apellido (opcional)
--   Marca y Modelo del vehículo (opcionales; reemplazan al campo libre "Vehículo")
-- NombreCompleto y Vehiculo se mantienen: la aplicación los arma sola al guardar
-- (Nombre + Apellido, Marca + Modelo), así nada de lo anterior se rompe.
--
-- Datos existentes:
--   * Nombre   = el NombreCompleto actual entero (Apellido queda vacío). No se corta
--                por espacios para no partir mal nombres como "María José".
--   * Marca    = primera palabra de Vehiculo; Modelo = el resto. Ej. "Toyota Etios".
--   El taxista puede corregirlos desde el panel.
--
-- Cuándo ejecutar: en una base que ya existía, ANTES de iniciar esta versión.
--                  Las bases nuevas creadas con init.sql o init-nube.sql ya lo traen.
-- Compatibilidad: MySQL 8.0+ y MariaDB 10.x+. Se puede ejecutar más de una vez.
-- Conectate a la base de la aplicación antes de ejecutarlo (no usa "USE").
-- =============================================================================

-- 1. Agregar las columnas solo si todavía no existen.
SET @columnas_existen := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'Taxistas'
      AND COLUMN_NAME = 'Nombre'
);

SET @sql_agregar := IF(
    @columnas_existen = 0,
    'ALTER TABLE Taxistas
        ADD COLUMN Nombre VARCHAR(60) NOT NULL DEFAULT '''' AFTER NombreCompleto,
        ADD COLUMN Apellido VARCHAR(59) NOT NULL DEFAULT '''' AFTER Nombre,
        ADD COLUMN Marca VARCHAR(50) NOT NULL DEFAULT '''' AFTER Vehiculo,
        ADD COLUMN Modelo VARCHAR(60) NOT NULL DEFAULT '''' AFTER Marca',
    'SELECT ''Las columnas Nombre, Apellido, Marca y Modelo ya existen'' AS Info'
);

PREPARE stmt_agregar FROM @sql_agregar;
EXECUTE stmt_agregar;
DEALLOCATE PREPARE stmt_agregar;

-- 2. Completar con los datos que ya había (solo filas sin completar).
UPDATE Taxistas
   SET Nombre = LEFT(TRIM(NombreCompleto), 60)
 WHERE Nombre = '' AND TRIM(NombreCompleto) <> '';

UPDATE Taxistas
   SET Marca  = LEFT(SUBSTRING_INDEX(TRIM(Vehiculo), ' ', 1), 50),
       Modelo = LEFT(TRIM(SUBSTRING(TRIM(Vehiculo), CHAR_LENGTH(SUBSTRING_INDEX(TRIM(Vehiculo), ' ', 1)) + 1)), 60)
 WHERE Marca = '' AND Modelo = '' AND TRIM(Vehiculo) <> '';
