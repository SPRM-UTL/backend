-----COMANDOS ÚTILES----- \n
Add-Migration <Nombre de la migración> -> Genera una nueva migración para la bd.
Update-database -> Corre las migraciones necesarias para mantener la base de datos actualizada.

-----CONEXIÓN A LA BD-----
Dependencias utilizadas:
Microsoft.EntityFrameworkCore.Tools 9.0.12.
Microsoft.VisualStudio.Web.CodeGeneration.Design 9.0.12.
Pomelo.EntityFrameworkCore.MySql 9.0.0.

¿Dónde buscar dependencias?
Herramientas->Administrador de paquetes Nugget->Administrar paquetes Nugget para la solución->Examinar.

¿Como se conectó?
-Se realizó una base de datos.
-Se descargaron las dependencias.
-Generar el contexto de la bd en la consola de del administrador de paquetes (Herramientas->Administrador de paquetes Nugget->Consola de del administrador de paquetes).
-Introducir el siguiente comando en la consola: 
  Scaffold-DbContext "server=<direccion_conexion>; post=<puerto_conexion>; database=<nombre_bd>; uid=<usuario_conexion>; password=<contraseña_usuario>;" Pomelo.EntityFrameworkCore.MySql -o <carpeta_guardar_dbcontext>
Ejemplo:
  Scaffold-DbContext "server=localhost; post=3306; database=prueba; uid=root; password=root;" Pomelo.EntityFrameworkCore.MySql -o Models
(Se generará el contexto de la base de datos, junto con los modelos de las tablas que tenga en ese momento).
-Se cambio de lugar la cadena de conexion siguiendo las recomenc¿daciones de microsoft (ahora se usa desde appsettings.json).

-----CREACION DE APIS AUTOMATICA-----
(Antes de esto se tuvo que haber conectado la base de datos, si ya esta conectada no habrá problemas).
-Click derecho en la carpeta Controller.
-Seleccionar Agregar->Nuevo elemento con Scaffold.
-Seleccionar Controlador de Api con acciones que usan Entity Framework.
-Se selecciona el modelo.
-Se selecciona el contexto de la bd.
-El proveedor se deja igual (SQL Server, igual jala ya lo calamos).
-Se pone nombre al controlador.
-Click en agregar.
-Esperar.
Al hacer esto te genera el controlador listo para usar con las acciones básicas de un CRUD para el modelo que seleccionaste:
-Metodo GET para traer uno o todas.
-Metodo POST para agregar.
-Metodo PUT para actualizar.
-Metodo DELETE para eliminar (eliminación física).
