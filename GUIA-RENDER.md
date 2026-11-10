# Guía para publicar TarjeTaxi en Render

Paso a paso para subir la aplicación con una cuenta propia. Ninguna clave va escrita
en el repositorio: todas se cargan como variables de entorno en Render.

## Qué necesitás

| Servicio | Para qué | Plan |
|---|---|---|
| GitHub | Código fuente (`Diego-2810/QR-PROYECTO`) | Gratis |
| Render | Ejecutar la aplicación (Docker) | Free |
| Aiven (u otro MySQL) | Base de datos. Render no ofrece MySQL | Free |
| Cloudinary | Fotos de perfil de los taxistas | Free |
| Google Cloud Console | Inicio de sesión con Google | Gratis |

---

## 1. Subir el código a GitHub

1. Subir la rama de trabajo: `git push origin mejoras-taxiya`.
2. En GitHub, abrir un Pull Request de `mejoras-taxiya` hacia `main` y unirlo.
3. Render publica siempre la rama `main` (está configurado en `render.yaml`).

## 2. Base de datos (Aiven)

1. Crear una cuenta en <https://aiven.io> y un servicio **MySQL** con el plan gratuito.
2. Cuando esté en estado *Running*, anotar: **Host**, **Port**, **User**, **Password** y **Database** (suele ser `defaultdb`).
3. Conectarse con la misma extensión de VS Code que usás en local (o HeidiSQL) con esos datos y SSL activado.
4. Ejecutar completo el archivo `Database/init-nube.sql`. Crea las tablas `Usuarios`, `Taxistas` y `DataProtectionKeys`.
5. Armar la cadena de conexión (Aiven exige SSL):

   ```
   Server=HOST;Port=PUERTO;Database=defaultdb;User ID=USUARIO;Password=CLAVE;SslMode=Required;
   ```

## 3. Cloudinary

1. Crear una cuenta en <https://cloudinary.com>.
2. En el *Dashboard*, copiar **Cloud name**, **API Key** y **API Secret**.

## 4. Crear el servicio en Render

1. Crear una cuenta en <https://render.com> e iniciar sesión con GitHub.
2. **New → Blueprint** y elegir el repositorio `QR-PROYECTO`. Render lee `render.yaml`.
3. Render pide los valores de las variables secretas:

   | Variable | Valor |
   |---|---|
   | `ConnectionStrings__DefaultConnection` | La cadena del paso 2 |
   | `Authentication__Google__ClientId` | ID del cliente OAuth |
   | `Authentication__Google__ClientSecret` | Secreto del cliente OAuth |
   | `Cloudinary__CloudName` | Del paso 3 |
   | `Cloudinary__ApiKey` | Del paso 3 |
   | `Cloudinary__ApiSecret` | Del paso 3 |

   Estas ya vienen cargadas en `render.yaml`: `ASPNETCORE_ENVIRONMENT=Production`,
   `PORT=8080` y `ForwardedHeaders__TrustProxyHeaders=true` (sin esta última falla
   el inicio de sesión con Google detrás del proxy de Render).
4. **Apply**. Esperar a que el despliegue termine en *Live*. La URL queda como
   `https://taxi-ya.onrender.com` (o parecida si el nombre está ocupado).

> Sin Blueprint también funciona: **New → Web Service**, *Runtime: Docker*, rama `main`,
> *Health Check Path* `/health`, y cargar a mano las 9 variables de arriba.

## 5. Conectar Google con la URL de Render

En Google Cloud Console → **APIs y servicios → Credenciales** → tu cliente OAuth:

- **Orígenes autorizados de JavaScript:** `https://TU-APP.onrender.com`
- **URIs de redireccionamiento autorizados:** `https://TU-APP.onrender.com/signin-google`

Dejar también las de `localhost` para seguir probando en la PC.
En **Pantalla de consentimiento**, publicar la app (o agregar los mails de prueba).

## 6. Probar

1. Abrir `https://TU-APP.onrender.com/health`: tiene que decir `Healthy`.
2. Abrir la página principal, iniciar sesión con Google y completar los datos con el
   teléfono en formato internacional (`+54 9 11 ...`).
3. Generar el QR, escanearlo **con el celular** y probar WhatsApp, Llamar, Guardar
   contacto, idiomas, modo oscuro y "Ver mi ubicación".

## Si algo falla

Mirar **Render → el servicio → Logs**. Mensajes frecuentes:

| Mensaje en el log | Causa | Solución |
|---|---|---|
| `Falta ConnectionStrings:DefaultConnection` | Variable vacía o mal escrita | Revisar el nombre (dos guiones bajos `__`) |
| `Unable to connect to any of the specified MySQL hosts` | Datos de Aiven incorrectos o falta SSL | Revisar host, puerto y `SslMode=Required` |
| `Falta la tabla DataProtectionKeys` / `Falta la columna PublicId` | No se ejecutó `init-nube.sql` | Ejecutarlo en la base de Aiven |
| `Table 'defaultdb.taxistas' doesn't exist` | Tablas creadas en minúscula | Volver a crearlas con `init-nube.sql` (usa `Taxistas`, `Usuarios`) |
| Error `redirect_uri_mismatch` al iniciar sesión | Falta la URI en Google | Paso 5 |
| `Cloudinary no está configurado` | Faltan variables de Cloudinary | Paso 3; la app funciona sin fotos |

## Limitaciones del plan gratuito

- El servicio se duerme tras unos 15 minutos sin visitas y tarda entre 30 y 60 segundos
  en despertar. Para uso real con pasajeros conviene el plan pago.
- Las sesiones de los taxistas sobreviven a reinicios porque las claves se guardan en la
  tabla `DataProtectionKeys`. No borrar esa tabla.
