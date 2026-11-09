-- =============================================================================
-- Migración 002: tabla DataProtectionKeys
-- =============================================================================
-- Para qué: guarda las claves que cifran la cookie de sesión de los taxistas.
--           Así no se pierden cuando Render reinicia o vuelve a desplegar la app
--           (si se pierden, todos tienen que volver a iniciar sesión).
-- Cuándo ejecutar: en una base que ya existía, ANTES de iniciar la versión de la
--                  aplicación que incluye esta migración. Las bases nuevas creadas
--                  con init.sql o init-nube.sql ya traen la tabla.
-- Compatibilidad: MySQL 8.0+ y MariaDB 10.x+. Se puede ejecutar más de una vez.
-- Nota: en la base en la nube no hace falta "USE"; conectate a la base correcta
--       y ejecutá solo el CREATE TABLE.
-- =============================================================================

CREATE TABLE IF NOT EXISTS DataProtectionKeys (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FriendlyName VARCHAR(255) NULL,
    `Xml` TEXT NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
