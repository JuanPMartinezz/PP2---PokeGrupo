# ADR-006: La carga masiva desde CSV pasa por los mismos servicios

**Estado:** aceptada · **Registrada al cierre del proyecto (12/11/2026)**

## Contexto

Para tener comisiones con muchos alumnos (y para la demo) hace falta cargar alumnos, asistencias y entregas de golpe. La primera versión de la importación vivía dentro de un método largo de `NominaService`, abría el selector de archivos desde el ViewModel y escribía directo en el repositorio, salteando validaciones.

## Decisión

- `ImportacionService` lee el CSV y, por cada fila, usa `NominaService`, `CalendarioService` y `RegistroService`: la importación no puede cargar nada que la app no deje cargar a mano (legajo de 3 a 8 dígitos, nombres válidos, comisión existente, sin fechas futuras).
- `LectorCsv` solo separa el texto en filas; no conoce reglas del negocio.
- Formato: `Nombre;Apellido;Legajo;Comision;Asistencias;Entregas`, con `;` o `,`. Fechas siempre `aaaa-mm-dd`. Una fila inválida se informa con su número de línea y se sigue con las demás.
- Un legajo que ya existe no se duplica: se le suman los registros del archivo.
- Elegir el archivo es responsabilidad de `IDialogos`.
- Todo el archivo se guarda dentro de una transacción (`IUnidadDeTrabajo`): es mucho más rápido con cientos de alumnos y, si falla algo inesperado, no queda cargado a medias. La importación corre en otro hilo para no congelar la pantalla.

## Consecuencias

- **Porque:** una sola regla por dato, y la importación se prueba sin pantallas.
- **A costa de:** una fila con datos inválidos no frena la carga: se informa y las demás se guardan (no es "todo o nada" por fila). Una interfaz más (`IUnidadDeTrabajo`) para que los servicios no conozcan SQLite.
- **Lo revisaríamos si:** el cliente quisiera que un solo error cancele todo el archivo.
