# 02 - Secretos fuera del repositorio e infraestructura alineada

## Fecha y hora

2026-11-05 19:42:43 -03:00 (zona horaria del sistema: Argentina Standard Time). Dato obtenido con `Get-Date`.

## Rama y commit

- Rama de trabajo: `mejoras-taxiya` (partiendo del commit `59a61f0 docs: diagnóstico inicial`).
- Commits realizados en esta etapa:
  1. `chore(security): vaciar secretos de appsettings.json y limpiar archivos locales` (`QrProfile/appsettings.json`, `.gitignore`, eliminación de `Taxi Ya Local.session.sql`).
  2. `build(docker): actualizar Dockerfile a .NET 10 y agregar .dockerignore` (`Dockerfile`, `.dockerignore`).
  3. `docs: actualizar README con Cloudinary y variables de entorno, y registrar paso 02` (`README.md` y este archivo `Historial/2026-11-05_19-42_02-secretos-infraestructura.md`).

## Prompt

```text
CONTEXTO: Proyecto "Taxi Ya" (QrProfile): ASP.NET Core .NET 10, Dapper + MySql.Data, login con Google (cookie), fotos en Cloudinary, frontend HTML/JS vanilla en QrProfile/Views y QrProfile/wwwroot.

REGLAS (obligatorias):
1. Antes de modificar algo, leé el código real. Si algo de este prompt no coincide con lo que existe, NO lo inventes: anotalo en el registro y seguí con el resto.
2. Trabajá en la rama "mejoras-taxiya". Hacé commits chicos. No hagas push, merge ni deploy.
3. No ejecutes nada contra una base de producción ni uses credenciales reales. Los cambios de base de datos van como scripts SQL en Database/ para que una persona los ejecute a mano.
4. Nunca escribas secretos, contraseñas, cadenas de conexión, teléfonos, CBU ni emails reales en archivos, commits ni en el registro.
5. Al terminar, compilá (dotnet build) y reportá el resultado.

TAREA: sacar los secretos del repositorio actual y alinear la infraestructura.
1. QrProfile/appsettings.json: vaciá los valores de ConnectionStrings:DefaultConnection, Authentication:Google:ClientId y Authentication:Google:ClientSecret (dejá las claves con "" como valor). Antes de borrar la sección "Email", verificá con una búsqueda que ningún código C# la lea; si nadie la usa, eliminala. Agregá una sección "Cloudinary" con CloudName, ApiKey y ApiSecret vacíos, para documentar las claves requeridas. No copies los valores viejos a ningún otro archivo, commit ni al registro. Dejá intactas las claves de Logging.
2. Verificá que, sin configuración, el arranque falle con un mensaje claro (Program.cs ya valida la cadena y las credenciales de Google). NO ejecutes la aplicación con credenciales reales.
3. .gitignore: agregá appsettings.*.local.json, appsettings.Local.json, secrets.json, .env, .env.* y *.session.sql. No ignores appsettings.Development.json (ya está versionado y no tiene datos sensibles) ni la carpeta Historial. Eliminá del repositorio (git rm) el archivo vacío "Taxi Ya Local.session.sql".
4. Dockerfile: cambiá las imágenes base a las de .NET 10 (sdk:10.0 y aspnet:10.0) para que coincidan con el TargetFramework net10.0. Creá o actualizá .dockerignore (bin, obj, .git, .vs, appsettings.Development.json, Historial). Si Docker está disponible, probá "docker build" sin credenciales reales; si no, indicalo en el registro.
5. QrProfile.csproj: no lo cambies salvo que encuentres una incoherencia real; si no la hay, anotalo.
6. README.md: corregilo (la aplicación SÍ permite subir fotos, con Cloudinary) y agregá una sección de configuración con una tabla de variables SOLO con nombres y ejemplos ficticios: ConnectionStrings__DefaultConnection, Authentication__Google__ClientId, Authentication__Google__ClientSecret, ForwardedHeaders__TrustProxyHeaders, Cloudinary__CloudName, Cloudinary__ApiKey, Cloudinary__ApiSecret. Incluí los comandos "dotnet user-secrets set" con valores ficticios para desarrollo local.
7. NO reescribas el historial de git ni hagas push.

En "Acciones manuales pendientes" del registro, listá para las personas: rotar el secreto de Google y la contraseña de la base (creando un usuario de base con permisos limitados), cargar las variables en Render, revisar los logs del último deploy de Render, y confirmar qué repositorio y rama despliega Render.

AL FINALIZAR (obligatorio):
1. Obtené la fecha y hora reales del reloj del sistema (no las inventes), con zona horaria.
2. Creá el archivo Historial/AAAA-MM-DD_HH-mm_02-secretos-infraestructura.md (carpeta Historial en la raíz del repo, al lado de QrProfile/; creala si no existe) con estas secciones: Fecha y hora; Rama y commit; Prompt (copiado textualmente, completo); Qué se hizo (archivos creados, modificados y eliminados, con una línea de explicación cada uno); Qué NO se hizo y por qué; Resultado de dotnet build y pruebas; Acciones manuales pendientes para las personas.
3. El registro no debe contener secretos ni datos reales (regla 4).
4. Incluí el archivo en el commit final.
```

## Qué se hizo

| Archivo | Acción | Explicación |
| --- | --- | --- |
| `QrProfile/appsettings.json` | Modificado | Se vaciaron `ConnectionStrings:DefaultConnection`, `Authentication:Google:ClientId` y `Authentication:Google:ClientSecret` (`""`), se eliminó la sección `Email` (tras verificar con `git grep` que ningún archivo `.cs` la referencia), se agregó `Cloudinary` (`CloudName`, `ApiKey`, `ApiSecret` vacíos) y se mantuvo `Logging`. |
| `.gitignore` | Modificado | Se agregaron `appsettings.*.local.json`, `appsettings.Local.json`, `secrets.json`, `.env`, `.env.*` y `*.session.sql`, sin ignorar `appsettings.Development.json` ni `Historial/`. |
| `Taxi Ya Local.session.sql` | Eliminado (`git rm`) | Se quitó del repositorio el archivo vacío de sesión SQL local. |
| `Dockerfile` | Modificado | Se actualizaron las imágenes base de `sdk:8.0` y `aspnet:8.0` a `mcr.microsoft.com/dotnet/sdk:10.0` y `mcr.microsoft.com/dotnet/aspnet:10.0` para coincidir con `net10.0`. |
| `.dockerignore` | Creado | Se excluyeron del contexto de Docker `bin`, `obj`, `.git`, `.vs`, `appsettings.Development.json`, archivos locales de secretos/entorno y `Historial`. |
| `README.md` | Modificado | Se documentó la subida de fotos con Cloudinary, el endpoint `POST /api/taxistas`, los comandos `dotnet user-secrets set` con valores ficticios y la tabla de variables de entorno con ejemplos ficticios. |
| `Historial/2026-11-05_19-42_02-secretos-infraestructura.md` | Creado | Registro detallado de los cambios, verificaciones y acciones pendientes de esta tarea. |

Verificación del arranque sin configuración (punto 2):
- Sin cadena de conexión (`ASPNETCORE_ENVIRONMENT=Production` y claves vacías): el proceso termina inmediatamente con `System.InvalidOperationException: Falta ConnectionStrings:DefaultConnection. Configurala en appsettings, User Secrets o mediante ConnectionStrings__DefaultConnection.`
- Con una cadena de conexión sintácticamente válida de prueba (sin conectar a ninguna base) y credenciales de Google vacías: el proceso termina inmediatamente con `System.InvalidOperationException: Configure Authentication:Google:ClientId y Authentication:Google:ClientSecret mediante appsettings, User Secrets o las variables Authentication__Google__ClientId y Authentication__Google__ClientSecret antes de iniciar la API.`

Revisión de `QrProfile/QrProfile.csproj` (punto 5):
- No se modificó porque no presenta incoherencias (`TargetFramework` es `net10.0`, `Microsoft.AspNetCore.Authentication.Google` es `10.0.0`, `UserSecretsId` está presente y los paquetes compilan sin advertencias).

## Qué NO se hizo y por qué

- **No se modificó `QrProfile/QrProfile.csproj`:** es coherente con .NET 10 y sus dependencias (punto 5).
- **No se ejecutó `docker build`:** el comando `docker` no está instalado ni disponible en el `PATH` de este entorno (`Get-Command docker` no devolvió resultados).
- **No se ejecutó la aplicación contra ninguna base de datos ni con credenciales reales** (reglas 3 y 4, y punto 2).
- **No se copiaron los valores antiguos de `appsettings.json`** a ningún archivo, mensaje de commit ni a este registro (regla 4 y punto 1).
- **No se reescribió el historial de git ni se hizo `git push`, `merge` o deploy** (regla 2 y punto 7).

## Resultado de dotnet build y pruebas

- `dotnet build QrProfile/QrProfile.csproj -c Release`: **compilación correcta, 0 advertencias y 0 errores**.
- Pruebas de arranque en frío (fail-fast): verificadas exitosamente para cadena de conexión faltante y credenciales de Google faltantes.
- Pruebas automatizadas: el repositorio todavía no cuenta con un proyecto de tests.
- `docker build`: no ejecutado por no estar Docker disponible en la máquina.

## Acciones manuales pendientes para las personas

1. **Rotar el `ClientSecret` de Google OAuth** en Google Cloud Console, ya que el valor anterior quedó registrado en commits previos del historial de git.
2. **Rotar la contraseña de la base de datos** y **crear un usuario de base de datos específico para la aplicación con permisos limitados** (solo sobre `QrProfileDB`, por ejemplo `SELECT`, `INSERT`, `UPDATE`, `DELETE`, evitando el uso de `root`).
3. **Cargar las variables de entorno en Render** (y en User Secrets para desarrollo local):
   - `ConnectionStrings__DefaultConnection`
   - `Authentication__Google__ClientId`
   - `Authentication__Google__ClientSecret`
   - `ForwardedHeaders__TrustProxyHeaders`
   - `Cloudinary__CloudName`
   - `Cloudinary__ApiKey`
   - `Cloudinary__ApiSecret`
4. **Revisar los logs del último deploy en Render** para verificar que el servicio arranque correctamente con las variables configuradas y que no haya errores de conexión, OAuth o subida de imágenes.
5. **Confirmar qué repositorio y qué rama despliega Render** (por ejemplo, si despliega desde `main` o `PRUEBA`, y si usa el comando nativo de .NET o el `Dockerfile`) antes de integrar la rama `mejoras-taxiya`.
