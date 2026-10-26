🎓 PP2 - Sistema de Seguimiento de Riesgo Académico
Aplicación desarrollada en .NET MAUI con el patrón MVVM para gestionar estudiantes por comisión, tomar asistencia, registrar entregas de trabajos y detectar estudiantes con riesgo académico.

🚀 Tecnologías utilizadas
.NET MAUI, C#, XAML, MVVM con CommunityToolkit.Mvvm, SQLite (sqlite-net-pcl), xUnit, Git y GitHub.

🧭 Cómo se usa
Inicio
Un botón por comisión (con la cantidad de alumnos), "Agregar otra comisión", "Agregar alumno nuevo" y, al final, "Administrar datos".
Las comisiones son datos: arrancan "Comisión A" y "Comisión B" y se pueden agregar, renombrar, vaciar o eliminar.

Pantalla de una comisión
Muestra la fecha de hoy y ofrece:
- Tomar asistencia de hoy.
- Registrar entrega.
- Ver la lista de alumnos.
- Calendario de clases y trabajos.
- Configurar el cálculo de riesgo de esa comisión.

Tomar asistencia
Lista de alumnos de la comisión con un check de "presente". La primera vez que se guarda una fecha no se pregunta nada; si se corrige algo que ya estaba guardado, se pide confirmación. Se puede elegir otra fecha (nunca futura).

Registrar entrega
Se elige el trabajo del calendario (figura como Abierto o Vencido según su fecha) y se marca a cada alumno como Entregado, Fuera de término o No entregado. "Cerrar entrega" deja como No entregado a quienes no tienen ningún registro.

Lista de alumnos
Cambio de comisión, orden (apellido A-Z, Z-A o mayor riesgo), filtro por contacto (todos, sin contactar, sin contacto hace más de 7 días) y filtro por nivel de riesgo. Cada fila muestra "Apellido, Nombre" y el riesgo; al tocarla se abre la ficha completa.

Ficha del alumno
Datos, riesgo, asistencia, entregas y contactos. Desde ahí se registra un contacto (fecha, si atendió o no y qué pasó), se editan los datos o se elimina al alumno.

Administrar datos
Renombrar, vaciar o eliminar una comisión y borrar todos los datos. Todo pide confirmación.

✅ Historias de usuario
H1 - Nómina: alta de alumnos con su comisión, legajo único (si se repite se informa a quién pertenece), edición y baja con sus registros.
H2 - Calendario: clases y trabajos por comisión y cuatrimestre, sin duplicados.
H3 - Asistencia: una asistencia por alumno y fecha.
H4 - Corrección: se corrige sobre el mismo registro, con confirmación.
H5 - Entregas: un registro por alumno y trabajo; estados Entregado, Fuera de término y No entregado.
H6 - Riesgo: ausencias sobre el total de clases y promedio del riesgo de las entregas, combinados según los pesos de la comisión (por defecto 50 % y 50 %). Sin registros no hay datos y el riesgo es 0 %. La lógica está en RiesgoService.
H7 - Configuración: cada comisión tiene sus propios pesos, riesgos por estado de entrega y cortes de nivel (por defecto 30 % medio y 60 % alto). Se valida todo y se explica el motivo si algo no sirve.
H8 - Lista de alumnos con orden y niveles de riesgo.
H9 - Ficha del alumno.
H10 - Contactos: fecha, "Atendió" o "No atendió" y el texto de lo que pasó (obligatorio, fecha no futura).
H11 - Contactos en la lista: filtros "Sin contactar" y "Sin contacto hace más de 7 días", que se combinan con el filtro de nivel.

📁 Estructura
PP2
├── Models      datos del dominio y textos de los estados
├── Services    reglas del negocio (riesgo, listado, registro, validaciones), DatosSistema y SQLite
├── ViewModels  una clase por pantalla, sin tipos de MAUI
├── Views       páginas XAML con código mínimo; NavegacionMaui y DialogosMaui
└── PP2.Tests   pruebas xUnit por historia (PruebasPP2)

Los ViewModels piden ir a otra pantalla o confirmar algo a través de INavegacion e IDialogos, por eso se pueden probar sin abrir ninguna página.
