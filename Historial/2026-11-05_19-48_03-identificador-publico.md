# 03 - Identificador público aleatorio (GUID / PublicId)

## Fecha y hora

2026-11-05 19:48:34 -03:00 (zona horaria del sistema: Argentina Standard Time). Dato obtenido con `Get-Date`.

## Rama y commit

- Rama de trabajo: `mejoras-taxiya` (partiendo del commit `46acc72 docs: actualizar README con Cloudinary y variables de entorno, y registrar paso 02`).
- Commits realizados en esta etapa:
  1. `feat(db): agregar columna PublicId en init.sql y scripts de migración 001a/001b` (`Database/init.sql`, `Database/migraciones/001a_taxistas_publicid_agregar.sql`, `Database/migraciones/001b_taxistas_publicid_exigir.sql`).
  2. `feat(api): usar PublicId (GUID) en consulta pública de taxistas y ocultar Id numérico` (`QrProfile/Models/TaxistaPerfil.cs`, `QrProfile/Models/TaxistaPublicoDto.cs`, `QrProfile/Repositories/ITaxistaRepository.cs`, `QrProfile/Repositories/TaxistaRepository.cs`, `QrProfile/Controllers/TaxistasController.cs`, `QrProfile/Program.cs`).
  3. `feat(ui): actualizar rutas, QR y descarga para usar PublicId y registrar paso 03` (`QrProfile/wwwroot/index.html`, `QrProfile/Views/administracion.html`, `README.md` y este archivo `Historial/2026-11-05_19-48_03-identificador-publico.md`).

## Prompt

```text
CONTEXTO: Proyecto "Taxi Ya" (QrProfile): ASP.NET Core .NET 10, Dapper + MySql.Data, login con Google (cookie), fotos en Cloudinary, frontend HTML/JS vanilla en QrProfile/Views y QrProfile/wwwroot. La tarjeta pública está en /perfil/{taxistaId:int} (Program.cs) y llama a GET /api/taxistas/{taxistaId:int} (TaxistasController), con IDs numéricos secuenciales, por lo que cualquiera puede recorrerlos y descargar los datos de todos los taxistas. El panel (Views/administracion.html) arma el link y el QR con /perfil/{id}; wwwroot/index.html lee el id con una expresión regular numérica.

REGLAS (obligatorias):
1. Antes de modificar algo, leé el código real. Si algo de este prompt no coincide con lo que existe, NO lo inventes: anotalo en el registro y seguí con el resto.
2. Trabajá en la rama "mejoras-taxiya". Hacé commits chicos. No hagas push, merge ni deploy.
3. No ejecutes nada contra una base de producción ni uses credenciales reales. Los cambios de base de datos van como scripts SQL en Database/ para que una persona los ejecute a mano.
4. Nunca escribas secretos, contraseñas, cadenas de conexión, teléfonos, CBU ni emails reales en archivos, commits ni en el registro.
5. Al terminar, compilá (dotnet build) y reportá el resultado.

TAREA: reemplazar el ID numérico público por un identificador aleatorio (GUID).
1. Base de datos, en DOS scripts dentro de Database/migraciones/:
   a) 001a_taxistas_publicid_agregar.sql (seguro de ejecutar ANTES de desplegar el código nuevo): agregá la columna PublicId CHAR(36) NULL a Taxistas, rellenala con UUID() donde sea NULL y creá un índice UNIQUE. Debe poder ejecutarse más de una vez sin romper nada. El código viejo debe seguir funcionando con este script aplicado.
   b) 001b_taxistas_publicid_exigir.sql (para ejecutar DESPUÉS de desplegar): rellená con UUID() los PublicId que hayan quedado NULL y pasá la columna a NOT NULL.
   Ambos con comentarios claros en español, compatibles con MySQL 8 y MariaDB; indicá cómo lo verificaste o qué no pudiste verificar. Actualizá también Database/init.sql para instalaciones nuevas (PublicId CHAR(36) NOT NULL UNIQUE).
2. Modelos: agregá PublicId a la entidad. Creá un DTO público (por ejemplo TaxistaPublicoDto) que NO incluya el Id numérico. El panel propio (GetPropio) debe devolver el PublicId para armar el link y el QR.
3. Repositorio: GetPublicoAsync debe buscar por PublicId. EnsurePerfilAsync debe generar Guid.NewGuid() al crear un perfil nuevo.
4. API y rutas: GET /api/taxistas/{publicId:guid} y GET /perfil/{publicId:guid}. ELIMINÁ por completo las rutas con id numérico (no dejes redirecciones, porque revelarían el GUID de cualquier id). Un id con formato inválido o inexistente devuelve 404.
5. Frontend: en wwwroot/index.html reemplazá la expresión regular del id numérico por una de GUID. En Views/administracion.html, el link público, el QR y el nombre del archivo de descarga deben usar el PublicId (sin mostrar el id numérico en ningún lado).
6. Revisá la respuesta del POST /api/taxistas y cualquier otro lugar del código o del frontend que use el id numérico en una URL pública o lo exponga en una respuesta anónima.

En "Acciones manuales pendientes" indicá, en este orden: respaldar la base; ejecutar 001a en producción; desplegar el código; ejecutar 001b. Aclará que los QR ya impresos con /perfil/{número} dejarán de funcionar. Si no podés probar con una base local, listá los pasos para que una persona pruebe manualmente: crear un perfil, abrir /perfil/{GUID}, verificar que /perfil/1 y /api/taxistas/1 den 404.

AL FINALIZAR (obligatorio):
1. Obtené la fecha y hora reales del reloj del sistema (no las inventes), con zona horaria.
2. Creá el archivo Historial/AAAA-MM-DD_HH-mm_03-identificador-publico.md (carpeta Historial en la raíz del repo, al lado de QrProfile/; creala si no existe) con estas secciones: Fecha y hora; Rama y commit; Prompt (copiado textualmente, completo); Qué se hizo (archivos creados, modificados y eliminados, con una línea de explicación cada uno); Qué NO se hizo y por qué; Resultado de dotnet build y pruebas; Acciones manuales pendientes para las personas.
3. El registro no debe contener secretos ni datos reales (regla 4).
4. Incluí el archivo en el commit final.
```

## Qué se hizo

| Archivo | Acción | Explicación |
| --- | --- | --- |
| `Database/init.sql` | Modificado | Se agregó la columna `PublicId CHAR(36) NOT NULL UNIQUE` a la tabla `Taxistas` para instalaciones nuevas. |
| `Database/migraciones/001a_taxistas_publicid_agregar.sql` | Creado | Script idempotente (previo al deploy) que agrega `PublicId CHAR(36) NULL` si no existe, rellena filas existentes con `LOWER(UUID())` y crea el índice único `UX_Taxistas_PublicId` si no existe, consultando `INFORMATION_SCHEMA` con SQL dinámico compatible tanto con MySQL 8 como con MariaDB. |
| `Database/migraciones/001b_taxistas_publicid_exigir.sql` | Creado | Script idempotente (posterior al deploy) que rellena con `LOWER(UUID())` cualquier fila con `PublicId` nulo o vacío y ejecuta `ALTER TABLE Taxistas MODIFY COLUMN PublicId CHAR(36) NOT NULL`. |
| `QrProfile/Models/TaxistaPerfil.cs` | Modificado | Se agregó la propiedad `PublicId` al modelo de perfil del taxista para que el panel autenticado la reciba en `GetPropioAsync`. |
| `QrProfile/Models/TaxistaPublicoDto.cs` | Creado | Nuevo DTO para la consulta pública de una tarjeta que incluye `PublicId` y los datos públicos del taxista, sin la propiedad `Id` numérica. |
| `QrProfile/Repositories/ITaxistaRepository.cs` | Modificado | Se cambió la firma `GetPublicoAsync(int taxistaId)` por `Task<TaxistaPublicoDto?> GetPublicoAsync(Guid publicId)`. |
| `QrProfile/Repositories/TaxistaRepository.cs` | Modificado | `EnsurePerfilAsync` ahora genera `Guid.NewGuid().ToString("D")` para `PublicId` (y rellena `PublicId` en `ON DUPLICATE KEY UPDATE` si estaba vacío); `GetPublicoAsync` filtra por `PublicId = @PublicId` devolviendo `TaxistaPublicoDto` sin `Id`; `GetPropioAsync` incluye `t.PublicId`. |
| `QrProfile/Controllers/TaxistasController.cs` | Modificado | Se reemplazó `[HttpGet("{taxistaId:int}")]` por `[HttpGet("{publicId:guid}")]` y se revisó `POST /api/taxistas` (devuelve `ActualizarTaxistaRequest`, que no contiene `Id`). |
| `QrProfile/Program.cs` | Modificado | Se reemplazó la ruta `/perfil/{taxistaId:int}` por `/perfil/{publicId:guid}`, eliminando la ruta numérica sin redirección. |
| `QrProfile/wwwroot/index.html` | Modificado | Se reemplazó la expresión regular `/^\/perfil\/(\d+)\/?$/` por `/^\/perfil\/([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})\/?$/i`. |
| `QrProfile/Views/administracion.html` | Modificado | El enlace público, el código QR (normal y ampliado) y el nombre del archivo PNG descargado usan `currentProfile.publicId` en lugar de `currentProfile.id`. |
| `README.md` | Modificado | Se actualizó la tabla de rutas y descripción del QR para reflejar `/perfil/{publicId}` y `GET /api/taxistas/{publicId}`. |
| `Historial/2026-11-05_19-48_03-identificador-publico.md` | Creado | Registro detallado de esta etapa. |

Revisión de exposición de ID numérico (punto 6):
- `GET /api/taxistas/{publicId:guid}` es el único endpoint anónimo de taxistas y devuelve `TaxistaPublicoDto`, que no posee la propiedad `Id`.
- `POST /api/taxistas` requiere sesión autenticada de Google y devuelve `ActualizarTaxistaRequest` (tampoco incluye `Id`).
- Con `git grep -n "taxistaId\|currentProfile\.id" -- QrProfile/` se verificó que no quedan referencias al ID numérico en rutas ni en el frontend.

Sobre compatibilidad y verificación de los scripts SQL (punto 1):
- En MySQL 8.0 no existe la sintaxis `ADD COLUMN IF NOT EXISTS` ni `CREATE UNIQUE INDEX IF NOT EXISTS` (exclusiva de MariaDB). Por eso en `001a_taxistas_publicid_agregar.sql` se verificó la existencia de la columna y del índice consultando `INFORMATION_SCHEMA.COLUMNS` e `INFORMATION_SCHEMA.STATISTICS` con `PREPARE`/`EXECUTE`, que es estándar tanto en MySQL 8 como en MariaDB.
- En `001b_taxistas_publicid_exigir.sql` se usa `ALTER TABLE Taxistas MODIFY COLUMN PublicId CHAR(36) NOT NULL` (sin repetir la palabra `UNIQUE` para no duplicar el índice en reejecuciones).

## Qué NO se hizo y por qué

- **No se ejecutaron los scripts SQL contra un motor MariaDB/MySQL en vivo:** en este entorno no se dispone de una base de datos de prueba configurada y la regla 3 prohíbe usar bases de producción o credenciales reales. La compatibilidad con MySQL 8 y MariaDB se verificó por inspección sintáctica del estándar SQL/INFORMATION_SCHEMA.
- **No se dejaron redirecciones desde `/perfil/{id:int}` ni `/api/taxistas/{id:int}`:** se eliminaron por completo tal como pide el punto 4 para impedir que alguien enumere IDs secuenciales y descubra los GUID correspondientes.
- **No se hizo `git push`, `merge` ni deploy** (regla 2).

## Resultado de dotnet build y pruebas

- `dotnet build QrProfile/QrProfile.csproj -c Release`: **compilación correcta, 0 advertencias y 0 errores**.
- Pruebas automatizadas: no existen proyectos de pruebas en la solución.

## Acciones manuales pendientes para las personas

Orden obligatorio para el despliegue en producción:
1. **Respaldar la base de datos** de producción antes de cualquier cambio de esquema.
2. **Ejecutar `Database/migraciones/001a_taxistas_publicid_agregar.sql` en producción** (antes de desplegar el código nuevo). Esto crea la columna `PublicId` anulable, asigna un UUID a cada taxista existente y crea el índice único sin afectar al código actualmente en ejecución.
3. **Desplegar el código nuevo** de la aplicación.
4. **Ejecutar `Database/migraciones/001b_taxistas_publicid_exigir.sql` en producción** (después de desplegar el código nuevo) para asegurar que ninguna fila haya quedado en `NULL` y fijar `PublicId CHAR(36) NOT NULL`.

> **Importante:** Todos los códigos QR ya impresos o compartidos anteriormente que apunten a `/perfil/{número}` (por ejemplo `/perfil/1`) **dejarán de funcionar** y devolverán `404 Not Found`. Cada taxista deberá descargar e imprimir su nuevo código QR desde `/admin`.

Pasos para probar manualmente en un entorno local o de staging:
1. Aplicar `Database/init.sql` (en una base nueva) o `001a` + `001b` (en una base existente de prueba).
2. Iniciar la aplicación, autenticarse con Google y entrar a `/admin` para crear o cargar el perfil de un taxista.
3. Verificar que el enlace "Ver tarjeta", el texto debajo del QR y la descarga del PNG usen `/perfil/{GUID}` (formato `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`) y no muestren el ID numérico.
4. Abrir `/perfil/{GUID}` en una ventana de incógnito (sin sesión) y comprobar que la tarjeta pública carga los datos y que la respuesta JSON de `GET /api/taxistas/{GUID}` no incluye el campo `id`.
5. Verificar que abrir `/perfil/1` y consultar `GET /api/taxistas/1` (o cualquier identificador que no sea un GUID válido o no exista) devuelvan **404 Not Found**.

