-- =============================================================================
-- Migración 005 (OPCIONAL): borrar las tablas viejas Languages y Profiles
-- =============================================================================
-- Eran del primer prototipo (un perfil fijo "Taxi Alexis" en 3 idiomas).
-- La aplicación ya no las usa: los idiomas de la tarjeta están en el navegador.
-- No hace falta ejecutarlo; la aplicación funciona igual con o sin estas tablas.
-- Las bases nuevas (init.sql e init-nube.sql) ya no las crean.
-- Borra datos de forma permanente: hacé una copia de seguridad antes si te interesa
-- conservar algo de esas tablas.
-- =============================================================================

DROP TABLE IF EXISTS Profiles;   -- primero Profiles: tiene una FK hacia Languages
DROP TABLE IF EXISTS Languages;
