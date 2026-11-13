# ADR-004: Fecha de incorporación del alumno

**Estado:** aceptada · **Registrada al cierre del proyecto (12/11/2026)**

## Contexto

Un alumno que se suma a mitad del cuatrimestre aparecía con muchas ausencias de clases a las que todavía no cursaba. El modelo de dominio de la cátedra incluye la fecha de incorporación del estudiante.

## Decisión

`Estudiante` tiene `FechaIncorporacion`. Para el riesgo solo cuentan las clases dictadas **desde esa fecha** y sus asistencias desde esa fecha. Si no hay fecha (alumnos cargados antes de este dato o importados por CSV), cursa desde el comienzo. La fecha no puede ser futura.

## Consecuencias

- **Porque:** evita marcar en riesgo a quien no tuvo oportunidad de asistir.
- **A costa de:** un campo más al dar de alta y una columna nueva en la tabla (`CreateTable` la agrega sola a bases existentes).
- **Lo revisaríamos si:** hubiera pases entre comisiones; ahí haría falta un historial de incorporaciones y no una sola fecha.
