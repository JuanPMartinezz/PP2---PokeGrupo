# PP2 PokeGrupo — Seguimiento de riesgo académico

Aplicación para quien sigue una o más comisiones de cursada: permite tomar asistencia, registrar entregas de trabajos y ver a qué estudiantes conviene contactar porque están en riesgo de abandonar o desaprobar.

Está hecha en **.NET MAUI** con el patrón **MVVM** (C#, XAML, CommunityToolkit.Mvvm), persistencia en **SQLite** (`sqlite-net-pcl`) y pruebas con **xUnit**.

## Cómo correrlo

Requisitos:

- Windows 10/11
- Visual Studio 2022 o 2026 con la carga de trabajo **Desarrollo de la interfaz de usuario multiplataforma de .NET (MAUI)**
- .NET 10 SDK

Pasos:

```bash
git clone https://github.com/JuanPMartinezz/PP2---PokeGrupo.git
cd PP2---PokeGrupo
dotnet build PP2/PP2.csproj -f net10.0-windows10.0.19041.0
dotnet run --project PP2/PP2.csproj -f net10.0-windows10.0.19041.0
```

También se puede abrir `PP2.slnx` en Visual Studio, elegir **Windows Machine** como destino y presionar F5.

La base SQLite (`registro_asistencia_v2.db3`) se crea sola en el primer arranque, dentro del directorio de datos privado de la aplicación. No hay que configurar nada más.

## Datos de ejemplo y base vacía

La primera vez se crean las comisiones "Comisión A" y "Comisión B", sin alumnos. Los alumnos, la asistencia y las entregas se cargan desde la aplicación, o todo junto con un archivo CSV.

**Cargar los datos de ejemplo:** en **Lista de alumnos** tocar **Importar alumnos desde CSV** y elegir `datos-ejemplo/alumnos-demo.csv`. Carga 200 alumnos (100 por comisión) con asistencias de cuatro clases y entregas de tres trabajos. En cada comisión deja 18 alumnos en riesgo alto, 14 en medio y 68 en bajo, así que el reporte y los filtros ("para llamar") tienen qué mostrar en la demostración. La carga se guarda como una sola operación, así que tarda unos segundos como máximo.

Formato del CSV (separador `;` o `,`; la primera línea puede ser el encabezado):

```text
Nombre;Apellido;Legajo;Comision;Asistencias;Entregas
Ana;Gómez;10234;Comisión A;2026-10-05:Presente|2026-10-12:Ausente;TP 1:Entregado|TP 2:NoEntregado
```

- Asistencias: `aaaa-mm-dd:Presente` o `:Ausente`, separadas por `|`.
- Entregas: `Trabajo:Entregado`, `:EntregadoFueraDeTermino` o `:NoEntregado`, separadas por `|`.
- Asistencias y entregas son opcionales. La importación usa las mismas validaciones que las pantallas: las filas con errores se informan con su número de línea y las demás se cargan.

Para volver a una base vacía: **Inicio → Administrar datos → Borrar todos los datos**, o eliminar el archivo `registro_asistencia_v2.db3` con la aplicación cerrada.

## Pruebas

```bash
dotnet test PP2.Tests/PP2.Tests.csproj
```

O desde Visual Studio: **Ver → Explorador de pruebas → Ejecutar todas**. Hay dos tipos de pruebas:

- `PruebasPP2.cs`: pruebas por historia de usuario (entre dos y cuatro por historia, incluida la H12, el reporte de riesgo) y de la importación del CSV, con repositorios en memoria. No necesitan base de datos ni pantallas. Las de aceptación están escritas como Dado/Cuando/Entonces (`Given_..._When_..._Then_...`).
- `PruebasIntegracionSqlite.cs`: dos pruebas con los servicios reales sobre SQLite: importar el CSV de ejemplo completo (200 alumnos) con sus reportes, y que lo guardado se lea igual al cerrar y volver a abrir la base.

## Qué hace

| Pantalla | Para qué sirve |
|---|---|
| Inicio | Un botón por comisión (con su cantidad de alumnos), alta de comisiones, alta de alumnos y administración de datos. |
| Comisión | Atajos de la comisión: tomar asistencia, registrar entrega, lista de alumnos, calendario y configuración del riesgo. |
| Tomar asistencia | Planilla de la comisión con un check de "presente". Se puede elegir otra fecha (nunca futura). Corregir una fecha ya guardada pide confirmación. |
| Registrar entrega | Se elige el trabajo del calendario y se marca a cada alumno como Entregado, Fuera de término o No entregado. "Cerrar entrega" deja como No entregado a quien no tiene registro (solo cuando el trabajo ya venció). |
| Calendario | Las clases salen solas de la asistencia tomada; los trabajos se cargan a mano, con nombre único por comisión. |
| Lista de alumnos | Orden por apellido o por mayor riesgo; filtros por contacto (sin contactar, sin contacto hace más de 7 días, para llamar) y por nivel de riesgo. Permite importar alumnos desde un CSV. |
| Reporte de riesgo | Cuántos alumnos de la comisión hay en cada nivel y qué porcentaje está en riesgo alto. Usa los mismos resultados que la lista de alumnos. |
| Ficha del alumno | Datos, fecha de incorporación, riesgo, asistencia, entregas y contactos. Desde acá se registra un contacto, se edita o se elimina al alumno. |
| Configuración del riesgo | Pesos y cortes propios de cada comisión. |

Se navega con el menú hamburguesa: comisión A o B, calendario, lista de alumnos, tomar asistencia, registrar entrega y reporte de riesgo.

## Reglas del negocio

**Riesgo de un alumno** (`Services/RiesgoService.cs`):

- **Ausencias** = ausencias del alumno ÷ clases dictadas × 100. Una clase dictada es una fecha en la que se tomó asistencia en la comisión; no se usan solo los registros del alumno.
- **Entregas** = suma del riesgo de cada entrega del alumno ÷ trabajos dictados × 100. Riesgo por estado: Entregado 0, Fuera de término 50, No entregado 100 (configurable). Un trabajo dictado es uno con entregas registradas en la comisión.
- **Riesgo total** = promedio ponderado de ambos con los pesos de la comisión (por defecto 50 % y 50 %).
- **Nivel**: Bajo, Medio (desde 30 %) o Alto (desde 60 %); los cortes también son de cada comisión.
- Un alumno sin ningún registro figura como **Sin datos**, no como riesgo bajo.
- **Alumno incorporado tarde**: solo cuentan las clases dictadas desde su fecha de incorporación.
- **Para llamar**: riesgo alto y sin contacto en los últimos 7 días.

**Validaciones** (`Services/Validaciones.cs`):

- Legajo numérico de 3 a 8 dígitos, único (si se repite se informa a quién pertenece).
- Nombre y apellido solo con letras, hasta 40 caracteres. Nombres de comisión y de trabajo hasta 60.
- Ninguna fecha de asistencia, contacto ni incorporación puede ser futura.
- No se puede cerrar la entrega de un trabajo que todavía no venció.
- Cada contacto lleva fecha, medio (llamada, mensaje, correo o en persona), si el alumno atendió y un resultado obligatorio.

Los mensajes de las pantallas son verdes si salió bien y rojos si es un error.

## Cómo está organizado

```text
PP2.slnx
├── PP2/                 aplicación MAUI
│   ├── Models/          dominio: Estudiante, Asistencia, Entrega, Contacto, niveles de riesgo
│   ├── Repositorios/    interfaces para guardar y leer datos (sin SQLite)
│   ├── Services/        reglas del negocio: riesgo, nómina, registro, calendario, contactos, importación, validaciones
│   ├── Datos/           SQLite: conexión, entidades de tabla y un repositorio por tema
│   ├── ViewModels/      una clase por pantalla, sin tipos de MAUI
│   ├── Views/           páginas XAML con código mínimo, navegación y diálogos
│   └── MauiProgram.cs   arma las dependencias
├── PP2.Tests/           pruebas xUnit: por historia (repositorios en memoria) y de integración (SQLite)
├── datos-ejemplo/       CSV de ejemplo para la demostración
├── docs/                arquitectura y decisiones (ADR)
└── SQLITE.md            detalle de la base: tablas y flujo
```

Cómo se conectan las capas:

```text
Views ──► ViewModels ──► Services ──► interfaces de Repositorios ◄── Datos (SQLite)
```

- Los servicios reciben las interfaces de los repositorios por constructor y no conocen SQLite ni MAUI.
- Los ViewModels reciben los servicios por constructor y piden ir a otra pantalla o confirmar algo a través de `INavegacion` e `IDialogos`.
- `MauiProgram` arma todo con el contenedor de dependencias. La base depende del dominio, no al revés.

Diagrama de arquitectura (contexto y componentes): [docs/arquitectura.md](docs/arquitectura.md) · Decisiones de diseño y reglas de negocio: [docs/decisiones/](docs/decisiones/)

## Cómo se trabaja

- La rama principal es `main`; el trabajo se integra a `main` y se sube desde Visual Studio.
- Cada integrante trabaja sus historias y deja sus propios commits. Cuando una tarea se hace en pareja, el commit lleva un `Co-authored-by` real de quien participó.
- Las tareas se siguen en el tablero del equipo.
- Integrantes: completar con los nombres del grupo.
