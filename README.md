🎓 PP2 - Sistema de Seguimiento de Riesgo Académico
Aplicación desarrollada en .NET MAUI utilizando el patrón MVVM para gestionar estudiantes por comisión, registrar asistencias y entregas de trabajos, y detectar estudiantes con posible riesgo académico.

🚀 Tecnologías utilizadas
.NET MAUI
C#
XAML
MVVM
CommunityToolkit.Mvvm
SQLite
xUnit
Git
GitHub
✅ Funcionalidades implementadas
H1 - Nómina de estudiantes
Alta de estudiantes.
Asociación de estudiantes a una comisión.
Separación de nóminas por Comisión A y Comisión B.
Listado ordenado por apellido.
Conteo automático de alumnos por comisión.
Validación de legajos duplicados.
Persistencia de estudiantes en SQLite.
H3 - Registro de asistencia
Registro de estudiantes presentes.
Registro de estudiantes ausentes.
Selección de fecha.
Selección de comisión.
Consulta de asistencias por fecha y comisión.
Prevención de registros duplicados para el mismo estudiante y fecha.
Persistencia de asistencias en SQLite.
H4 - Corrección de asistencia
Selección de una asistencia previamente registrada.
Modificación del estado Presente/Ausente.
Actualización del registro existente.
Conservación de los registros de los demás estudiantes.
Persistencia de la corrección en SQLite.
H5 - Registro de entregas de trabajos
Creación de trabajos.
Registro de entregas por estudiante.
Asociación entre estudiante y trabajo.
Estados disponibles:
Entregado.
No entregado.
Entregado fuera de término.
Prevención de entregas duplicadas para un mismo estudiante y trabajo.
Consulta de entregas por comisión y trabajo.
Modificación del estado de una entrega existente.
Persistencia de trabajos y entregas en SQLite.
H6 - Cálculo de riesgo académico
El riesgo académico utiliza dos componentes:

Riesgo por asistencia
RiesgoAsistencia = (Ausencias / TotalAsistencias) × 100

Riesgo por entregas
Entregado = 0 % de riesgo.
Entregado fuera de término = 50 % de riesgo.
No entregado = 100 % de riesgo.
Cuando existen varias entregas se calcula el promedio de sus valores de riesgo.

Riesgo combinado
Cuando existen registros de asistencia y entregas:

RiesgoFinal = (RiesgoAsistencia + RiesgoEntregas) / 2

Por lo tanto:

Asistencia: 50 %.
Entregas: 50 %.
Casos especiales
Si solo existen asistencias, se utiliza únicamente el riesgo por asistencia.
Si solo existen entregas, se utiliza únicamente el riesgo por entregas.
Si no existen registros, el riesgo es 0 %.
No se penaliza al estudiante por información que todavía no fue registrada.
La lógica se encuentra centralizada en RiesgoService.

H8 - Panel de riesgo
Selección de comisión.
Ranking independiente por comisión.
Cálculo del riesgo académico mediante RiesgoService.
Ordenamiento de estudiantes de mayor a menor riesgo.
Visualización del porcentaje de riesgo académico.
Detalle de asistencia:
Presentes.
Ausencias.
Total de clases.
Detalle de entregas:
Entregados.
No entregados.
Entregados fuera de término.
Total de entregas.
H9 - Ficha del estudiante
Selección de comisión.
Selección de estudiante dentro de la comisión.
Visualización de nombre, apellido, legajo y comisión.
Riesgo académico combinado.
Cantidad de presentes.
Cantidad de ausencias.
Total de clases.
Historial de asistencias.
Cantidad de trabajos entregados.
Cantidad de trabajos no entregados.
Cantidad de trabajos entregados fuera de término.
Total de entregas.
Historial de entregas.
H8 y H9 reutilizan el mismo RiesgoService, evitando duplicar la lógica de cálculo.

📁 Estructura principal del proyecto
PP2 │ ├── Models ├── Services ├── ViewModels ├── Views └── PP2.Tests

Models
Estudiante
Comision
Clase
EstadoAsistencia
Asistencia
Trabajo
Entrega
EstadoEntrega
ResultadoRiesgo
Services
DatosSistema
BaseDatosService
RiesgoService
ViewModels
ComisionViewModel
AsistenciaViewModel
TrabajoViewModel
RiesgoViewModel
FichaEstudianteViewModel
Views
MainPage
AsistenciaPage
TrabajoPage
RiesgoPage
FichaEstudiantePage
💾 Persistencia
La aplicación utiliza SQLite para mantener la información aunque la aplicación se cierre.

El flujo principal es:

View → ViewModel → DatosSistema → BaseDatosService → SQLite

DatosSistema mantiene las colecciones utilizadas durante la ejecución y BaseDatosService centraliza el acceso a SQLite.

🧪 Pruebas unitarias
Proyecto de pruebas:

PP2.Tests

Framework:

xUnit

Las pruebas cubren las historias H1, H3, H4, H5, H6, H8 y H9.

Se verifican, entre otros casos:

Alta válida de estudiantes.
Legajos duplicados.
Asociación y separación por comisión.
Registro de asistencias.
Asistencias duplicadas.
Consulta por fecha y comisión.
Corrección de asistencias.
Registro de entregas.
Entregas duplicadas.
Modificación de entregas.
Riesgo académico combinado.
Ordenamiento del ranking.
Estadísticas de riesgo.
Historial individual de estudiantes.
Estadísticas de la ficha individual.
Resultado actual
✅ 18 pruebas ejecutadas
✅ 18 pruebas aprobadas
✅ 0 pruebas fallidas

🔧 Estado actual
✅ H1 implementada
✅ H3 implementada
✅ H4 implementada
✅ H5 implementada
✅ H6 implementada
✅ H8 implementada
✅ H9 implementada

✅ Persistencia con SQLite
✅ Riesgo combinado asistencia + entregas
✅ Filtrado por comisiones
✅ 18 pruebas unitarias aprobadas
✅ Compilación sin errores
✅ Integración con Git/GitHub

📌 Mejoras futuras
Mejoras en la interfaz de usuario.
Nuevas validaciones.
Reportes y estadísticas avanzadas.
Alertas automáticas para estudiantes con alto riesgo.
Evolución de la fórmula de riesgo según nuevas reglas de negocio.
Ampliación de historias de usuario en futuros sprints.
👥 Equipo
Trabajo práctico desarrollado para Programación II mediante .NET MAUI, C#, MVVM, SQLite y pruebas automatizadas.
