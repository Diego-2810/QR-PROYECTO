-- =============================================================================
-- Migración 001b: Exigir PublicId NOT NULL en Taxistas
-- =============================================================================
-- Cuándo ejecutar: DESPUÉS de desplegar e iniciar la versión nueva de la
--                  aplicación (que rellena al arrancar los PublicId en NULL con
--                  Guid.NewGuid() aleatorio).
-- Compatibilidad: MySQL 8.0+ y MariaDB 10.x+.
-- Importante: NO usa UUID() de la base de datos. Si todavía quedan filas con
--             PublicId en NULL o vacío, aborta con un error claro para que se
--             inicie primero la aplicación antes de volver a correr este script.
-- =============================================================================

USE QrProfileDB;

DROP PROCEDURE IF EXISTS sp_001b_exigir_publicid;

DELIMITER $$

CREATE PROCEDURE sp_001b_exigir_publicid()
BEGIN
    DECLARE v_pendientes INT DEFAULT 0;

    SELECT COUNT(*)
      INTO v_pendientes
      FROM Taxistas
     WHERE PublicId IS NULL OR TRIM(PublicId) = '';

    IF v_pendientes > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Abortado: quedan filas en Taxistas con PublicId NULL o vacío. Iniciá primero la aplicación para que asigne GUID aleatorios antes de ejecutar 001b.';
    END IF;

    ALTER TABLE Taxistas
        MODIFY COLUMN PublicId CHAR(36) NOT NULL;
END$$

DELIMITER ;

CALL sp_001b_exigir_publicid();
DROP PROCEDURE IF EXISTS sp_001b_exigir_publicid;
