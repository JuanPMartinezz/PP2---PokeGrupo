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
H2 - Calendario de clases y trabajos del cuatrimestre
Selección de comisión y de cuatrimestre.
Registro de la fecha de una clase en el calendario.
Registro de un trabajo en la fecha indicada; su nombre (por ejemplo, TP1) es obligatorio.
Consulta del calendario con la fecha y el tipo de cada actividad.
Actividades ordenadas cronológicamente.
Calendarios independientes por comisión y por cuatrimestre.
Prevención de actividades duplicadas (misma comisión, cuatrimestre, fecha, tipo y descripción).
Persistencia de las actividades en SQLite.
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
H7 - Umbrales configurables
Pantalla para modificar los valores del cálculo de riesgo sin tocar el código:
Peso de la asistencia en el riesgo final (el de las entregas es el resto hasta 100 %).
Riesgo de una entrega fuera de término.
Riesgo de una entrega no entregada.
Cortes de nivel: riesgo medio desde y riesgo alto desde (por defecto 30 % y 60 %).
Valores por defecto: 50 % / 50 %, 50 % y 100 % (la fórmula original de H6).
Validación: cada valor debe ser un número entre 0 y 100, el riesgo fuera de término no puede superar al de no entregado y el corte medio no puede superar al alto.
Si un valor no es válido se informa el motivo y no se guarda ninguno.
Muestra los valores actualmente establecidos y permite restablecer los predeterminados.
RiesgoService lee los umbrales en cada cálculo, por lo que H6, H8 y H9 usan siempre los valores configurados.
Nivel de riesgo (Bajo, Medio, Alto o Sin datos) en el panel H8 y en la ficha H9, para no tener que interpretar porcentajes.
Persistencia de los umbrales en SQLite. No se borran con "Borrar todos los datos".

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
Historial de contactos con su resultado (H10).
H8 y H9 reutilizan el mismo RiesgoService, evitando duplicar la lógica de cálculo.
H10 - Contactos con estudiantes
Selección de comisión y de estudiante.
Registro de un contacto con fecha y resultado; el resultado es obligatorio y se escribe como texto libre.
La fecha del contacto no puede ser futura.
El resultado queda asociado únicamente al contacto y al estudiante seleccionados.
Un estudiante puede tener varios contactos.
Consulta de los contactos de un estudiante, del más reciente al más antiguo (pantalla de contactos y ficha del estudiante).
Persistencia de los contactos en SQLite.
H11 - Contactos en el panel
El panel de riesgo (H8) indica, para cada estudiante, si fue contactado o está sin contactar.
Muestra la fecha del último contacto y la cantidad de contactos.
Lista todos los contactos del estudiante (fecha y resultado), del más reciente al más antiguo.
Resumen de la comisión: "Contactados: X de Y", que cuenta siempre a toda la comisión.
Filtro "Mostrar": todos los alumnos, solo contactados o solo sin contactar.
Indica cuántos estudiantes se muestran con el filtro activo.
Muestra el nivel de riesgo (Bajo, Medio, Alto o Sin datos) de cada estudiante.
El orden del panel sigue siendo por riesgo, de mayor a menor, para priorizar a quién contactar.

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
TipoActividad
ActividadCalendario
ContactoEstudiante
UmbralesRiesgo
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
CalendarioViewModel
ContactoViewModel
ConfiguracionViewModel
Views
MainPage
AsistenciaPage
TrabajoPage
RiesgoPage
FichaEstudiantePage
CalendarioPage
ContactoPage
ConfiguracionPage
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

Las pruebas cubren las historias H1, H2, H3, H4, H5, H6, H7, H8, H9, H10 y H11.

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
Registro de una clase en el calendario (H2).
Orden cronológico de las actividades del calendario (H2).
Asociación del contacto y su resultado únicamente al estudiante correspondiente (H10).
Visualización del resultado del contacto en la ficha del estudiante (H10).
Rechazo de un contacto con fecha futura (H10).
Guardado de umbrales válidos y uso de los nuevos valores en el cálculo de riesgo (H7).
Visualización de los valores actuales y rechazo de valores no válidos, incluido un corte medio mayor que el alto (H7).
Nivel de riesgo según los cortes configurados (H7).
Indicación de quién fue contactado y cuándo en el panel de riesgo (H11).
Historial completo de contactos por estudiante en el panel (H11).
Filtro del panel por contactados y sin contactar (H11).
Resultado actual
✅ 18 pruebas previas (H1, H3, H4, H5, H6, H8, H9) aprobadas
🕒 8 pruebas nuevas (H2, H7, H10 y H11) pendientes de ejecutar en Visual Studio

🔧 Estado actual
✅ H1 implementada
✅ H2 implementada
✅ H3 implementada
✅ H4 implementada
✅ H5 implementada
✅ H6 implementada
✅ H7 implementada
✅ H8 implementada
✅ H9 implementada
✅ H10 implementada
✅ H11 implementada

✅ Persistencia con SQLite
✅ Riesgo combinado asistencia + entregas
✅ Filtrado por comisiones
✅ 18 pruebas unitarias previas aprobadas
✅ Integración con Git/GitHub

👥 Equipo
Trabajo práctico desarrollado para Programación II mediante .NET MAUI, C#, MVVM, SQLite y pruebas automatizadas.
