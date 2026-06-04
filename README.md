# Backend SPRM

Backend en ASP.NET Core para el sistema SPRM/Manordomo. Expone una API REST para usuarios, autenticacion, dispositivos, gestos e historial, y tambien un canal WebSocket para comunicacion con dispositivos ESP32.

## Que hace

El backend se encarga de:

- Registrar e iniciar sesion de usuarios.
- Generar tokens de sesion y validarlos en cada peticion protegida.
- Dar de baja tokens al cerrar sesion, sin eliminarlos fisicamente.
- Administrar dispositivos del usuario.
- Administrar gestos vinculados a dispositivos.
- Consultar historial de actividad.
- Registrar ESP32 conectados por WebSocket.
- Reenviar mensajes entre ESP32 conectados.

La API responde con un formato general:

```json
{
  "success": true,
  "status": 200,
  "data": {}
}
```

Este formato lo aplica `Middleware/RequestMiddleware.cs` para las rutas que empiezan con `/api`.

## Requisitos

Instala lo siguiente antes de correr el proyecto:

- .NET SDK 9
- MySQL 8.x
- Visual Studio 2022 o una terminal con `dotnet`
- Herramienta de Entity Framework, si vas a usar migraciones desde terminal

Para instalar `dotnet-ef`:

```powershell
dotnet tool install --global dotnet-ef
```

Si ya lo tienes instalado:

```powershell
dotnet tool update --global dotnet-ef
```

## Estructura

```text
backend/
  backend.sln
  Dockerfile
  README.md

  backend/
    Program.cs
    backend.csproj
    appsettings.json
    Controllers/
    DTOs/
    Middleware/
    Models/
    Migrations/
    Services/
    Properties/launchSettings.json
```

Carpetas principales:

- `Controllers/`: endpoints REST.
- `DTOs/`: objetos que entran y salen por la API.
- `Models/`: entidades de Entity Framework.
- `Middleware/`: validacion de token y formato de respuesta.
- `Migrations/`: cambios versionados de base de datos.
- `Services/`: logica reutilizable, como conexiones y ruteo de mensajes ESP32.

## Instalacion Inicial

Desde la raiz del backend:

```powershell
cd D:\DESARROLLO\UTL\SPRM\backend
dotnet restore backend.sln
dotnet build backend.sln
```

Si el build termina con `0 Errores`, el proyecto ya esta listo para configurarse.

## Configuracion De Base De Datos

El proyecto usa MySQL mediante Pomelo Entity Framework Core.

Importante: actualmente la conexion se lee desde una variable de entorno llamada `CONEXION`. `Program.cs` carga variables con `DotNetEnv`, por lo que puedes usar un archivo `.env`.

Crea un archivo `.env` en la carpeta desde donde vas a ejecutar `dotnet run`.

Si vas a usar los comandos de este README desde `D:\DESARROLLO\UTL\SPRM\backend`, crea:

```text
D:\DESARROLLO\UTL\SPRM\backend\.env
```

Si ejecutas desde Visual Studio y toma como directorio de trabajo el proyecto, tambien puedes colocarlo en:

```text
D:\DESARROLLO\UTL\SPRM\backend\backend\.env
```

Tambien puedes definir `CONEXION` como variable de entorno del sistema y omitir el archivo `.env`.

Contenido de ejemplo:

```env
CONEXION=server=localhost;port=3306;database=pruebaasp;uid=root;password=tu_password;
```

Si tu usuario root no tiene password:

```env
CONEXION=server=localhost;port=3306;database=pruebaasp;uid=root;
```

La base de datos debe existir antes de aplicar migraciones:

```sql
CREATE DATABASE pruebaasp CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

## Migraciones

Para aplicar las migraciones a la base de datos:

```powershell
dotnet ef database update --project backend\backend.csproj --startup-project backend\backend.csproj
```

Para crear una migracion nueva:

```powershell
dotnet ef migrations add NombreDeLaMigracion --project backend\backend.csproj --startup-project backend\backend.csproj
```

Desde Visual Studio tambien puedes usar la Consola del Administrador de Paquetes:

```powershell
Update-Database
Add-Migration NombreDeLaMigracion
```

Nota: si `dotnet ef` no existe, instala la herramienta global como se muestra en la seccion de requisitos.

## Ejecutar En Desarrollo

Desde:

```powershell
D:\DESARROLLO\UTL\SPRM\backend
```

Ejecuta:

```powershell
dotnet run --project backend\backend.csproj
```

Por `launchSettings.json`, el proyecto levanta en:

```text
http://localhost:5295
```

El frontend Angular esta permitido por CORS desde:

```text
http://localhost:4200
```

## Autenticacion

El login genera un token aleatorio y lo guarda en la tabla `token`.

Los endpoints protegidos requieren header:

```http
Authorization: Bearer <token>
```

Tambien se acepta el token directo sin `Bearer`, para mantener compatibilidad con clientes actuales.

El token expira en 30 minutos. Cada peticion valida renueva su expiracion otros 30 minutos.

Al cerrar sesion, el token no se elimina. Se marca como inactivo:

```text
activo = false
fecha_baja = fecha actual
```

## Rutas Principales

### Auth

Base:

```text
/api/Auth
```

Endpoints:

```http
POST /api/Auth/register
POST /api/Auth/login
POST /api/Auth/logout
```

Registro:

```json
{
  "nombre": "Juan",
  "correo": "juan@correo.com",
  "contrasenia": "Password123!"
}
```

Login:

```json
{
  "correo": "juan@correo.com",
  "contrasenia": "Password123!"
}
```

Respuesta de login dentro de `data`:

```json
{
  "id": 1,
  "nombre": "Juan",
  "token": "token_generado"
}
```

### Usuarios

Rutas compatibles:

```text
/api/UsuariosApi
/api/usuarios
```

Endpoints:

```http
GET /api/UsuariosApi/{id}
PUT /api/UsuariosApi/{id}
```

Android usa actualmente `api/UsuariosApi/{id}`, por eso esa ruta se conserva.

### Dispositivos

Rutas compatibles:

```text
/api/Dim_Aparatos
/api/aparatos
```

Endpoints:

```http
GET /api/Dim_Aparatos
GET /api/Dim_Aparatos/{id}
POST /api/Dim_Aparatos
PUT /api/Dim_Aparatos/{id}
DELETE /api/Dim_Aparatos/{id}
```

Ejemplo de dispositivo:

```json
{
  "sk_aparato_id": 0,
  "nombre_aparato": "Luz sala",
  "tipo_aparato": "Luces",
  "accion_nombre": "Encendido",
  "comando_bluetooth": "BT_ON",
  "icono": "lightbulb",
  "mac_bluetooth": "00:11:22:33:44:55",
  "nombre_bluetooth": "ESP32 Sala",
  "fecha_sincronizacion": "2026-06-04T12:00:00"
}
```

### Gestos

Rutas compatibles:

```text
/api/Dim_Gestos
/api/gestos
```

Endpoints:

```http
GET /api/Dim_Gestos
GET /api/Dim_Gestos/{id}
POST /api/Dim_Gestos
PUT /api/Dim_Gestos/{id}
DELETE /api/Dim_Gestos/{id}
```

Gestos permitidos actualmente:

```text
Manos Arriba
Una Mano Arriba
Agitar la Mano
Abrir Puño
Cerrar Puño
```

Ejemplo:

```json
{
  "sk_gesto_id": 0,
  "bk_gesto_id": 1,
  "nombre_gesto": "Manos Arriba",
  "identificador_ia": 1,
  "nivel_confianza_minimo": 0.80,
  "tipo_disparador_nombre": "Camara",
  "sk_aparato_id": 1
}
```

### Historial

Base:

```text
/api/Fact_Historico_Actividad
```

Endpoint principal:

```http
GET /api/Fact_Historico_Actividad
```

Devuelve actividades con datos listos para mostrar en UI: hora, accion, dispositivo, icono, color, estado y metodo.

### ESP32 REST

Base:

```text
/api/Devices
/api/Messages
```

Uso:

```http
GET /api/Devices
POST /api/Devices
GET /api/Messages
```

`DevicesController` administra dispositivos ESP32 registrados por `deviceKey`.

`MessagesController` devuelve los ultimos mensajes procesados.

## WebSocket ESP32

El WebSocket escucha en:

```text
/ws
```

Conexion basica:

```text
ws://localhost:5295/ws?deviceKey=esp32-sala
```

Conexion con destino:

```text
ws://localhost:5295/ws?deviceKey=esp32-control&targetDeviceKey=esp32-sala
```

Funcionamiento:

1. El ESP32 se conecta con `deviceKey`.
2. El backend registra o actualiza ese dispositivo.
3. Si envia mensajes y existe `targetDeviceKey`, el backend intenta reenviarlos al ESP32 destino.
4. Cada mensaje queda registrado en base de datos.

Implementacion actual:

- `Controllers/Esp32WebSocketController.cs`: recibe la conexion `/ws`.
- `Services/Esp32ConnectionManager.cs`: mantiene los sockets activos por `deviceKey`.
- `Services/Esp32MessageRouter.cs`: lee mensajes, busca el destino, reenvia y guarda el log.
- `Services/Esp32MessageEventHub.cs`: publica eventos internos para futuras pantallas en tiempo real o monitores.

Esta separacion evita que el pipeline global dependa del WebSocket y facilita agregar despues autenticacion por dispositivo, tipos de mensaje, reconexion controlada o streaming de eventos.

## Nombres De Tablas

El contexto define explicitamente estos nombres fisicos:

```text
usuario
aparato
gesto
tiempo
historial_actividad
token
esp32_device
esp32_message
```

Las rutas antiguas conservan nombres como `Dim_Aparatos` solo como alias temporal para no romper Android, pero el dominio y las tablas ya usan nombres simples.

## Docker

El proyecto incluye `Dockerfile`.

Construir imagen:

```powershell
docker build -t sprm-backend -f Dockerfile .
```

Ejecutar contenedor:

```powershell
docker run --rm -p 8080:8080 `
  -e CONEXION="server=host.docker.internal;port=3306;database=pruebaasp;uid=root;password=tu_password;" `
  sprm-backend
```

URL en Docker:

```text
http://localhost:8080
```

## Flujo Recomendado Desde Cero

1. Instala .NET SDK 9 y MySQL.
2. Crea la base de datos `pruebaasp`.
3. Crea `.env` con la variable `CONEXION` en el directorio desde donde ejecutes el backend.
4. Restaura y compila:

```powershell
dotnet restore backend.sln
dotnet build backend.sln
```

5. Aplica migraciones:

```powershell
dotnet ef database update --project backend\backend.csproj --startup-project backend\backend.csproj
```

6. Ejecuta:

```powershell
dotnet run --project backend\backend.csproj
```

7. Registra un usuario con `POST /api/Auth/register`.
8. Inicia sesion con `POST /api/Auth/login`.
9. Usa el token recibido en el header `Authorization`.
10. Crea dispositivos y gestos desde las rutas protegidas.

## Verificacion Rapida

Compilar:

```powershell
dotnet build backend.sln
```

Probar que levanta:

```powershell
dotnet run --project backend\backend.csproj
```

Login exitoso esperado:

```json
{
  "success": true,
  "status": 200,
  "data": {
    "id": 1,
    "nombre": "Juan",
    "token": "..."
  }
}
```

Si recibes `401`, revisa:

- Que estes mandando el header `Authorization`.
- Que el token exista en la tabla `token`.
- Que `activo` sea `true`.
- Que `fecha_expiracion` no haya vencido.

## Notas De Desarrollo

- No regreses entidades completas si puedes usar DTOs.
- Mantener rutas antiguas mientras Android las siga usando.
- Si se cambia el contrato JSON, revisar primero los `SerializedName` de Android.
- No eliminar tokens fisicamente: usar baja logica.
- Las migraciones deben revisarse antes de aplicarse si renombran tablas con datos existentes.
