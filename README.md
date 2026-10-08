# Taxi Ya

Taxi Ya permite a taxistas independientes de Buenos Aires crear una tarjeta digital informativa y compartirla mediante un código QR personal. El pasajero escanea el código para consultar datos de contacto, vehículo, servicios y medios de pago; la plataforma no asigna viajes ni recibe pedidos.

## Requisitos

- .NET SDK 10.0, requerido por `QrProfile/QrProfile.csproj` (`net10.0`).
- MariaDB compatible con `Database/init.sql` o un servidor MySQL compatible, accesible desde la aplicación.
- Proyecto OAuth 2.0 de tipo aplicación web en Google Cloud, con pantalla de consentimiento configurada.
- Git y una terminal para la configuración local.

No hay un proyecto de pruebas automatizadas en el repositorio. Antes de publicar cambios, ejecutar al menos `dotnet build QrProfile/QrProfile.csproj -c Release` y probar OAuth y lectura/escritura contra una base de staging.

## Arquitectura

- **Servidor:** C# y ASP.NET Core sobre .NET 10.
- **Persistencia:** Dapper y `MySql.Data` (ADO.NET) contra MariaDB/MySQL.
- **Esquema:** script SQL reejecutable en `Database/init.sql`; no se usa Entity Framework Core, Pomelo ni `dotnet ef`.
- **Autenticación:** Google OAuth 2.0 y cookie de autenticación ASP.NET Core.
- **Interfaz:** HTML, CSS y JavaScript en `QrProfile/Views` y `QrProfile/wwwroot`.
- **QR:** generado en el navegador mediante QRCode.js; el contenido dirige a `/perfil/{id}`.

Los perfiles, usuarios y tarjetas se consultan mediante repositorios Dapper. La aplicación no crea la base ni aplica migraciones al arrancar: un operador debe aplicar `Database/init.sql` antes del primer inicio de sesión.

## Módulos y rutas

| Ruta | Acceso | Función |
| --- | --- | --- |
| `/` | Público | Presentación de Taxi Ya y acceso/registro de taxistas. |
| `/admin` | Google OAuth | Panel para editar la tarjeta y generar, ampliar, descargar o imprimir el QR. |
| `/panel` | Google OAuth | Alias del panel. |
| `/perfil/{taxistaId}` | Público | Tarjeta del taxista para consulta desde celular o navegador. |
| `GET /api/taxistas/{taxistaId}` | Público | Datos públicos de una tarjeta. |
| `GET/PUT /api/administracion/perfil` | Autenticado | Consultar y actualizar el perfil propio. |
| `GET /api/auth/login-google` | Público | Iniciar autenticación Google. |
| `POST /api/auth/logout` | Cookie de autenticación | Cerrar sesión. |

En el primer acceso Google se registra o recupera el usuario y se crea su perfil de taxista. El panel guarda nombre, teléfono/WhatsApp, URL de foto, vehículo, patente, servicios y datos o enlace de pago. La foto se proporciona como URL pública; la aplicación no ofrece carga de archivos.

## Configuración local

### 1. Obtener el código

```bash
git clone <URL-del-repositorio>
cd QR-PROYECTO
```

### 2. Preparar MariaDB

Crear un usuario de base de datos de desarrollo y ejecutar el esquema. El usuario necesita permisos para crear la base/tablas si todavía no existen.

```bash
mariadb --host 127.0.0.1 --user taxiya --password < Database/init.sql
```

El comando solicita la contraseña en forma interactiva; reemplazar `taxiya` por el usuario de base de datos que se haya creado.

Guardar la cadena completa en User Secrets para no versionar contraseñas:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;Port=3306;Database=QrProfileDB;User ID=<usuario>;Password=<contraseña>;" --project QrProfile/QrProfile.csproj
```

La misma clave puede proporcionarse mediante `ConnectionStrings__DefaultConnection`. El arranque comprueba que la cadena exista y tenga formato MariaDB/MySQL, pero no crea la base ni modifica el esquema.

### 3. Configurar Google OAuth

Crear credenciales OAuth de tipo aplicación web. En **Authorized redirect URIs**, registrar las URL exactas correspondientes al perfil local elegido:

```text
http://localhost:5264/signin-google
https://localhost:7215/signin-google
```

Configurar las credenciales localmente. El proyecto incluye `UserSecretsId`, y `WebApplication.CreateBuilder` carga User Secrets automáticamente en Development.

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "<client-id>" --project QrProfile/QrProfile.csproj
dotnet user-secrets set "Authentication:Google:ClientSecret" "<client-secret>" --project QrProfile/QrProfile.csproj
```

Si la pantalla de consentimiento está en modo de prueba, incluir las cuentas que iniciarán sesión como usuarios de prueba. No reutilizar secretos OAuth que se hayan compartido o comprometido: revocarlos y crear otros nuevos.

### 4. Ejecutar

```bash
dotnet run --project QrProfile/QrProfile.csproj --launch-profile https
```

Abrir `https://localhost:7215/`. También está disponible `http://localhost:5264/` con el perfil `http`. El certificado local puede requerir confianza explícita en el sistema operativo. Si se cambia host o puerto, registrar la URI de callback exacta en Google Cloud.

## Despliegue en Render

### Base de datos

Usar una instancia MariaDB/MySQL gestionada o un VPS accesible desde Render. Habilitar TLS si el proveedor lo ofrece, limitar conexiones entrantes y ejecutar `Database/init.sql` una vez desde una conexión administrativa. Planificar copias de seguridad y aplicar cambios futuros del esquema de forma controlada; la aplicación no ejecuta migraciones al arrancar.

### Servicio web

Crear un servicio web con raíz del repositorio y configurar:

- **Build Command:** `dotnet publish QrProfile/QrProfile.csproj -c Release -o publish`
- **Start Command:** `dotnet publish/QrProfile.dll --urls "http://0.0.0.0:${PORT:-10000}"`
- **Environment:** `ASPNETCORE_ENVIRONMENT=Production`
- **Environment:** `ConnectionStrings__DefaultConnection` con la cadena entregada por el proveedor MariaDB/MySQL.
- **Environment:** `Authentication__Google__ClientId` con el Client ID OAuth.
- **Environment:** `Authentication__Google__ClientSecret` con el Client Secret OAuth.
- **Environment:** `ForwardedHeaders__TrustProxyHeaders=true` cuando la aplicación esté detrás del proxy TLS de Render.

La opción de forwarded headers permite a ASP.NET Core usar el protocolo externo indicado por el proxy para construir el callback OAuth. Activarla solo cuando Kestrel esté detrás de un proxy inverso confiable; la aplicación acepta los encabezados reenviados en ese modo.

En Google Cloud Console, registrar como **Authorized redirect URI**:

```text
https://<tu-app>.onrender.com/signin-google
```

La URI debe coincidir exactamente con el dominio final. Si se usa un dominio personalizado, registrar también `https://<dominio-personalizado>/signin-google`. Probar inicio/cierre de sesión y edición de tarjeta contra una base de staging antes de habilitar tráfico real.

No guardar secretos en `appsettings.json`, `appsettings.Development.json`, `Claves.txt` ni en el repositorio. Si un secreto llegó a un commit previo, eliminar el archivo no lo revoca: rotarlo y, si corresponde, limpiar el historial publicado.

## Uso

1. Abrir `/` y elegir **Acceso / Registro de Taxistas**.
2. Autenticarse con Google. En el primer acceso se crea el perfil y el navegador abre `/admin`.
3. Completar nombre, teléfono, foto mediante URL, vehículo, patente, servicios y datos de pago; guardar los cambios.
4. En **Generar mi QR**, ampliar el código para mostrarlo en pantalla, descargarlo como PNG o imprimirlo.
5. El pasajero escanea el código y abre `/perfil/{taxistaId}` sin iniciar sesión. Puede consultar los datos, abrir WhatsApp, llamar, guardar el contacto o copiar la información de pago.

## Configuración de producción

La configuración utiliza los proveedores estándar de .NET. Las variables de entorno representan niveles con `__`:

| Clave .NET | Variable de entorno |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |
| `Authentication:Google:ClientId` | `Authentication__Google__ClientId` |
| `Authentication:Google:ClientSecret` | `Authentication__Google__ClientSecret` |
| `ForwardedHeaders:TrustProxyHeaders` | `ForwardedHeaders__TrustProxyHeaders` |

Las credenciales OAuth son obligatorias; si faltan, la aplicación detiene el arranque con instrucciones en Development. La cookie de autenticación se marca `Secure` fuera de Development. El procesamiento de forwarded headers no se activa por defecto.