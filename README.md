🎓 PP2 - Sistema de Seguimiento de Riesgo Académico
Aplicación desarrollada en .NET MAUI utilizando el patrón MVVM para la gestión de estudiantes, registro de asistencias y detección temprana de estudiantes con riesgo académico.

🚀 Tecnologías utilizadas
.NET MAUI
C#
MVVM
CommunityToolkit.Mvvm
xUnit
Git / GitHub
✅ Funcionalidades implementadas
H1 - Nómina de estudiantes
Alta de estudiantes.
Validación de legajo duplicado.
Listado ordenado por apellido.
Conteo automático de alumnos registrados.
H3 - Registro de asistencia
Registro de presentes.
Registro de ausentes.
Selección de fecha.
Historial de asistencias.
Prevención de registros duplicados para el mismo alumno en la misma fecha.
H6 - Cálculo de riesgo académico
Cálculo automático del porcentaje de ausencias:

(Ausencias / TotalClases) * 100
Los resultados se muestran redondeados a 2 decimales.

H8 - Panel de riesgo
Ranking de estudiantes ordenado por nivel de riesgo.

Información mostrada:

Presentes.
Ausencias.
Total de clases.
Porcentaje de riesgo.
📁 Estructura del proyecto
PP2
│
├── Models
├── Services
├── ViewModels
├── Views
├── Helpers
│
└── PP2.Tests
Models
Estudiante
Comision
Clase
EstadoAsistencia
Asistencia
ResultadoRiesgo
Services
DatosSistema
RiesgoService
ViewModels
ComisionViewModel
AsistenciaViewModel
RiesgoViewModel
Views
MainPage
AsistenciaPage
RiesgoPage
🧪 Pruebas unitarias
Proyecto:

PP2.Tests
Framework:

xUnit
Pruebas implementadas:

H1
Agregar alumno válido.
Rechazar legajo duplicado.
H3
Registrar asistencia.
Rechazar asistencia duplicada.
H6
Riesgo 0%.
Riesgo 50%.
H8
Ordenamiento por riesgo.
Estadísticas correctas del estudiante.
Resultado
8 pruebas ejecutadas
8 pruebas aprobadas
0 errores
🔧 Estado actual
✅ H1 Implementada
✅ H3 Implementada
✅ H6 Implementada
✅ H8 Implementada

✅ 8 pruebas unitarias aprobadas
✅ Compilación sin errores
✅ Integración con GitHub realizada
📌 Mejoras futuras
Persistencia con SQLite.
Entity Framework.
Repositorios.
Reportes y estadísticas avanzadas.
Alertas automáticas para estudiantes en riesgo.
👥 Equipo
Trabajo práctico desarrollado para Programación II utilizando .NET MAUI, MVVM y pruebas automatizadas
