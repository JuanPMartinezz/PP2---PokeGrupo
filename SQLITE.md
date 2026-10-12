# Persistencia con SQLite

## Qué se agregó

- Paquete `sqlite-net-pcl`.
- `Services/BaseDatosService.cs`: crea y consulta las tablas `Estudiantes` y `Asistencias`.
- `Services/DatosSistema.cs`: carga los registros guardados en las colecciones que usa la interfaz y centraliza las altas persistentes.
- `StartupPage.cs`: inicializa la base antes de abrir las pantallas.
- Los ViewModels guardan alumnos y asistencias en SQLite cuando se registran desde la app.

## Dónde se guarda la base

El archivo se llama `registro_asistencia.db3` y se crea dentro de `FileSystem.AppDataDirectory`, el directorio privado de datos de la aplicación. No hay que crear el archivo manualmente.

## Datos iniciales de ejemplo

En el primer inicio, si la tabla `Estudiantes` está vacía, se insertan cinco estudiantes ficticios y algunas asistencias de muestra para poder ver el panel de riesgo funcionando. La carga inicial se ejecuta una sola vez; no vuelve a insertar estos datos en cada apertura.

Para usar la nómina real del TP, reemplazá los registros del método `CrearDatosInicialesSiHaceFalta()` en `Services/BaseDatosService.cs` por los estudiantes y asistencias acordados por el grupo. Si ya se ejecutó la aplicación, la base existente no se sobrescribe automáticamente. Para probar el sembrado desde cero, borrá los datos de la aplicación o eliminá `registro_asistencia.db3` del almacenamiento de la app.

## Tablas

- `Estudiantes`: `Legajo` (clave primaria), `Nombre`, `Apellido`.
- `Asistencias`: `Id` (clave autoincremental), `LegajoEstudiante`, `Fecha` (`yyyy-MM-dd`) y `Estado` (valor numérico del enum `EstadoAsistencia`: Presente = 0, Ausente = 1).

El legajo conecta una asistencia con su estudiante. Al abrir la app, las asistencias se reconstruyen vinculándolas con los objetos de estudiante cargados en memoria. Se impide registrar más de una asistencia para el mismo alumno y fecha.

## Flujo resumido

1. Se abre la app y `StartupPage` inicializa SQLite.
2. Si es la primera ejecución, se insertan los datos de demostración.
3. Se cargan estudiantes y asistencias desde la base a `DatosSistema`.
4. Al agregar un alumno o registrar una asistencia, primero se guarda el registro en SQLite y luego se actualiza la colección observable de la interfaz.
5. Al reiniciar la app, se vuelven a leer los datos guardados.

> Nota: en este entorno no pude ejecutar `dotnet build` ni las pruebas porque el SDK de .NET no está instalado. Hay que restaurar paquetes y compilar el proyecto en Visual Studio con la carga de trabajo de .NET MAUI instalada.
