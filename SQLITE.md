# Persistencia con SQLite

## Cómo está armado

- Paquete `sqlite-net-pcl`.
- `Datos/BaseDatos.cs`: abre la conexión y crea las tablas. Recibe la ruta del archivo por constructor.
- `Datos/*RepositorioSqlite.cs`: un repositorio por tema (comisiones, alumnos, asistencias, entregas, calendario, contactos y borrado general). Implementan las interfaces de `Repositorios/`.
- `Services/`: las reglas del negocio usan solo las interfaces, nunca SQLite. Así se pueden probar con repositorios en memoria (ver `PP2.Tests/RepositoriosEnMemoria.cs`).
- `MauiProgram.cs`: registra todo en el contenedor de dependencias y le pasa a `BaseDatos` la ruta de la base.
- `StartupPage.cs`: prepara las comisiones antes de abrir las pantallas.

Cada pantalla lee los datos de la base cuando aparece, así que siempre muestra lo último que se guardó.

## Dónde se guarda la base

El archivo se llama `registro_asistencia_v2.db3` (la versión anterior, `registro_asistencia.db3`, guardaba el nombre de la comisión repetido en cada tabla y ya no se usa; los datos que tuviera no se migran) y se crea dentro de `FileSystem.AppDataDirectory`, el directorio privado de datos de la aplicación. No hay que crear el archivo manualmente.

## Datos iniciales

La primera vez se crean las comisiones "Comisión A" y "Comisión B". Los alumnos, asistencias y demás datos se cargan desde la aplicación. Para empezar de cero: Inicio → Administrar datos → Borrar todos los datos, o eliminá `registro_asistencia_v2.db3` del almacenamiento de la app.

## Tablas

- `Comisiones`: `Id` (clave autoincremental) y `Nombre` (único). Es el único lugar donde se escribe el nombre de una comisión. Si la tabla está vacía se cargan "Comisión A" y "Comisión B".
- `Estudiantes`: `Legajo` (clave primaria), `Nombre`, `Apellido` y `ComisionId` (la comisión a la que pertenece).
- `Asistencias`: `Id` (clave autoincremental), `LegajoEstudiante`, `Fecha` (`yyyy-MM-dd`) y `Estado` (valor numérico del enum `EstadoAsistencia`: Presente = 0, Ausente = 1).

- `ActividadesCalendario` (H2): `Id` (clave autoincremental), `ComisionId`, `Cuatrimestre`, `Fecha` (`yyyy-MM-dd`), `Tipo` (valor numérico del enum `TipoActividad`: Clase = 0, Trabajo = 1) y `Descripcion`.
- `UmbralesComision` (H7): una fila por comisión (`ComisionId` es la clave primaria) con `PesoAsistencia`, `RiesgoFueraDeTermino`, `RiesgoNoEntregado`, `RiesgoMedio` y `RiesgoAlto`. Si una comisión no tiene fila se usan los valores por defecto (50, 50, 100, 30 y 60). La tabla anterior `UmbralesRiesgo` (una sola fila para todos) ya no se usa: los umbrales de cada comisión vuelven a los valores por defecto.
- `Contactos` (H10): `Id` (clave autoincremental), `LegajoEstudiante`, `Fecha` (`yyyy-MM-dd`), `Atendio` (verdadero si el alumno atendió; en contactos viejos, sin valor, se toma como verdadero) y `Resultado`.

El legajo conecta una asistencia (y un contacto) con su estudiante. Al abrir la app, las asistencias se reconstruyen vinculándolas con los objetos de estudiante cargados en memoria. Se impide registrar más de una asistencia para el mismo alumno y fecha.

Renombrar una comisión cambia una sola fila de `Comisiones`: alumnos, calendario y umbrales la referencian por `ComisionId` y siguen apuntando a ella.

## Flujo resumido

1. Se abre la app y `StartupPage` pide a `ComisionService` que prepare las comisiones; eso abre SQLite y crea las tablas que falten.
2. Cada pantalla pide los datos a un servicio; el servicio los pide al repositorio y el repositorio los lee de SQLite.
3. Al guardar, el servicio valida y recién después el repositorio escribe en SQLite.
4. Al reiniciar la app, todo sigue en la base.

> Nota: en este entorno no pude ejecutar `dotnet build` ni las pruebas porque el SDK de .NET no está instalado. Hay que restaurar paquetes y compilar el proyecto en Visual Studio con la carga de trabajo de .NET MAUI instalada.
