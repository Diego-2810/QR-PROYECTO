# 03b - Generación exclusiva de PublicId con GUID aleatorio desde la aplicación

## Fecha y hora

2026-11-05 19:55:53 -03:00 (zona horaria del sistema: Argentina Standard Time). Dato obtenido con `Get-Date`.

## Rama y commit

- Rama de trabajo: `mejoras-taxiya` (partiendo del commit `8eb45d5 feat(ui): actualizar rutas, QR y descarga para usar PublicId y registrar paso 03`).
- Commits realizados en esta etapa:
  1. `feat(db): quitar UUID() de 001a/001b y agregar script opcional 001c` (`Database/migraciones/001a_taxistas_publicid_agregar.sql`, `Database/migraciones/001b_taxistas_publicid_exigir.sql`, `Database/migraciones/001c_regenerar_publicid.sql`).
  2. `feat(startup): rellenar PublicId faltantes con Guid.NewGuid() al iniciar la aplicación` (`QrProfile/Repositories/ITaxistaRepository.cs`, `QrProfile/Repositories/TaxistaRepository.cs`, `QrProfile/Program.cs`).
  3. `docs: actualizar orden de migración en README y registrar paso 03b` (`README.md` y este archivo `Historial/2026-11-05_19-55_03b-publicid-aleatorio.md`).

## Prompt

```text
CONTEXTO: Proyecto "Taxi Ya" (QrProfile): ASP.NET Core .NET 10, Dapper + MySql.Data, login con Google (cookie), frontend HTML/JS vanilla. En el paso anterior se agregó la columna Taxistas.PublicId (CHAR(36)) con los scripts Database/migraciones/001a_taxistas_publicid_agregar.sql y 001b_taxistas_publicid_exigir.sql, que rellenan filas existentes con LOWER(UUID()). Problema: UUID() de MySQL/MariaDB genera UUID versión 1 (basado en fecha y hora, predecible), no aleatorio, por lo que los identificadores de perfiles existentes podrían adivinarse. Los perfiles nuevos usan Guid.NewGuid(), que sí es aleatorio.

REGLAS (obligatorias):
1. Antes de modificar algo, leé el código real. Si algo de este prompt no coincide con lo que existe, NO lo inventes: anotalo en el registro y seguí con el resto.
2. Trabajá en la rama "mejoras-taxiya". Hacé commits chicos. No hagas push, merge ni deploy.
3. No ejecutes nada contra una base de producción ni uses credenciales reales. Los cambios de base de datos van como scripts SQL en Database/ para que una persona los ejecute a mano.
4. Nunca escribas secretos, contraseñas, cadenas de conexión, teléfonos, CBU ni emails reales en archivos, commits ni en el registro.
5. Al terminar, compilá (dotnet build) y reportá el resultado.

TAREA: que ningún PublicId se genere con UUID() de la base de datos.
1. 001a_taxistas_publicid_agregar.sql: dejá solo "agregar la columna PublicId CHAR(36) NULL si no existe" y "crear el índice UNIQUE si no existe". Quitá el relleno con UUID(); las filas existentes quedan en NULL.
2. Relleno desde la aplicación: creá un paso que se ejecute una vez al iniciar (por ejemplo un IHostedService o un bloque en Program.cs después de construir la app) y que, para cada taxista con PublicId NULL o vacío, asigne Guid.NewGuid().ToString("D") con un UPDATE parametrizado y condicionado (WHERE Id = @Id AND (PublicId IS NULL OR PublicId = '')), de modo que sea seguro si hay varias instancias o reinicios. Registrá en el log SOLO la cantidad de filas actualizadas, sin datos personales. Si la columna PublicId no existe, el arranque debe fallar con un mensaje claro en español que indique ejecutar primero 001a.
3. 001b_taxistas_publicid_exigir.sql: quitá el relleno con UUID(). Debe verificar que no queden filas con PublicId NULL o vacío (si quedan, abortar con un mensaje claro) y recién entonces ejecutar MODIFY COLUMN PublicId CHAR(36) NOT NULL.
4. Creá Database/migraciones/001c_regenerar_publicid.sql, para bases donde ya se hayan ejecutado las versiones anteriores de 001a/001b: pasa la columna a NULL (MODIFY) y pone PublicId = NULL en todas las filas, con una advertencia en comentarios de que esto invalida los links y QR actuales. Que no se ejecute solo; es manual y opcional.
5. Confirmá que EnsurePerfilAsync sigue usando Guid.NewGuid() y que ningún otro lugar del código o de los scripts usa UUID() para generar identificadores públicos.
6. Actualizá el README con el orden: respaldo → 001a → desplegar (el arranque asigna GUID aleatorios a los existentes) → verificar que no queden NULL → 001b.

AL FINALIZAR (obligatorio):
1. Obtené la fecha y hora reales del reloj del sistema (no las inventes), con zona horaria.
2. Creá el archivo Historial/AAAA-MM-DD_HH-mm_03b-publicid-aleatorio.md (carpeta Historial en la raíz del repo, al lado de QrProfile/; creala si no existe) con estas secciones: Fecha y hora; Rama y commit; Prompt (copiado textualmente, completo); Qué se hizo (archivos creados, modificados y eliminados, con una línea de explicación cada uno); Qué NO se hizo y por qué; Resultado de dotnet build y pruebas; Acciones manuales pendientes para las personas.
3. El registro no debe contener secretos ni datos reales (regla 4).
4. Incluí el archivo en el commit final.
```

## Qué se hizo

| Archivo | Acción | Explicación |
| --- | --- | --- |
| `Database/migraciones/001a_taxistas_publicid_agregar.sql` | Modificado | Se eliminó el `UPDATE` con `UUID()`; ahora únicamente agrega la columna `PublicId CHAR(36) NULL` si no existe y crea el índice único `UX_Taxistas_PublicId` si no existe, dejando las filas existentes en `NULL`. |
| `Database/migraciones/001b_taxistas_publicid_exigir.sql` | Modificado | Se eliminó el `UPDATE` con `UUID()`; mediante un procedimiento temporal verifica que `COUNT(*)` de filas con `PublicId IS NULL OR TRIM(PublicId) = ''` sea `0` (abortando con `SIGNAL SQLSTATE '45000'` y mensaje claro en español si quedan pendientes) y luego ejecuta `MODIFY COLUMN PublicId CHAR(36) NOT NULL`. |
| `Database/migraciones/001c_regenerar_publicid.sql` | Creado | Script manual y opcional para bases donde ya se hubieran corrido las versiones previas de `001a`/`001b`: pasa `PublicId` a `NULL` con `MODIFY COLUMN` y ejecuta `UPDATE Taxistas SET PublicId = NULL`, con advertencia de que invalida los enlaces y QR actuales. |
| `QrProfile/Repositories/ITaxistaRepository.cs` | Modificado | Se agregó el método `Task<int> RellenarPublicIdsFaltantesAsync()` a la interfaz del repositorio. |
| `QrProfile/Repositories/TaxistaRepository.cs` | Modificado | Se implementó `RellenarPublicIdsFaltantesAsync()`, que verifica en `INFORMATION_SCHEMA.COLUMNS` la existencia de `Taxistas.PublicId` (lanzando `InvalidOperationException` con mensaje claro en español si falta) y asigna `Guid.NewGuid().ToString("D")` por cada fila pendiente mediante `UPDATE Taxistas SET PublicId = @PublicId WHERE Id = @Id AND (PublicId IS NULL OR TRIM(PublicId) = '')`. Se confirmó que `EnsurePerfilAsync` sigue usando `Guid.NewGuid().ToString("D")`. |
| `QrProfile/Program.cs` | Modificado | Después de `builder.Build()` se ejecuta `RellenarPublicIdsFaltantesAsync()` dentro de un scope y se registra en el log únicamente la cantidad de filas actualizadas (`FilasActualizadas`), sin exponer datos personales. |
| `README.md` | Modificado | Se documentó la secuencia: respaldo → `001a` → desplegar (el arranque asigna GUID aleatorios) → verificar que no queden `NULL` → `001b`, además de mencionar `001c`. |
| `Historial/2026-11-05_19-55_03b-publicid-aleatorio.md` | Creado | Registro detallado de esta etapa. |

Verificación de ausencia de `UUID()` (punto 5):
- Se ejecutó `git grep -n "UUID()" -- Database/ QrProfile/` y se comprobó que no queda ninguna llamada ejecutable a `UUID()` en ningún script SQL ni archivo C# (solo se menciona en comentarios explicativos de por qué no se usa).

## Qué NO se hizo y por qué

- **No se ejecutaron los scripts SQL ni el paso de inicio contra una base de datos real:** en este entorno no hay una instancia MySQL/MariaDB de prueba configurada y la regla 3 prohíbe conectarse a bases de producción o usar credenciales reales.
- **No se ejecutó `001c_regenerar_publicid.sql` automáticamente:** es un script exclusivamente manual y opcional para entornos donde ya se hubiera corrido la versión previa de `001a`/`001b`.
- **No se hizo `git push`, `merge` ni deploy** (regla 2).

## Resultado de dotnet build y pruebas

- `dotnet build QrProfile/QrProfile.csproj -c Release`: **compilación correcta, 0 advertencias y 0 errores**.
- Pruebas automatizadas: no existen proyectos de pruebas en la solución.

## Acciones manuales pendientes para las personas

Orden recomendado para aplicar en producción o staging:
1. **Respaldar la base de datos** completa antes de ejecutar cambios de esquema.
2. *(Solo si en esa base ya se habían ejecutado las versiones anteriores de `001a`/`001b` con `UUID()`)*: decidir si se ejecuta manualmente `Database/migraciones/001c_regenerar_publicid.sql`, teniendo en cuenta que **invalida todos los links y códigos QR emitidos hasta ese momento**. Si nunca se ejecutaron las versiones anteriores, omitir `001c` y ejecutar `Database/migraciones/001a_taxistas_publicid_agregar.sql`.
3. **Desplegar e iniciar la nueva versión de la aplicación:** verificar en los logs de inicio el mensaje `"Verificación inicial de PublicId completada. Filas actualizadas: X."`.
4. **Verificar en la base de datos que no queden filas sin `PublicId`:**
   ```sql
   SELECT COUNT(*) FROM Taxistas WHERE PublicId IS NULL OR TRIM(PublicId) = '';
   ```
   (El resultado debe ser `0`).
5. **Ejecutar `Database/migraciones/001b_taxistas_publicid_exigir.sql`** para fijar la columna `PublicId CHAR(36) NOT NULL`.
