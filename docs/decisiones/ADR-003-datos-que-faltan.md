# ADR-003: Qué pasa cuando faltan datos

**Estado:** aceptada · **Registrada al cierre del proyecto (12/11/2026)**

## Contexto

Hay casos donde calcular un porcentaje no tiene sentido: una comisión sin clases, sin trabajos, un alumno sin registros, o una comisión sin estudiantes. El cliente no dijo qué mostrar en esos casos.

## Decisión

- **Clase dictada:** una fecha en la que se tomó asistencia en la comisión. No se usan solo los registros del alumno: si faltó a una clase en la que no figura, igual cuenta.
- **Trabajo dictado:** uno con entregas registradas en la comisión.
- **Alumno sin ningún registro:** nivel **"Sin datos"**, no "Bajo". Que no tenga datos no significa que no esté en riesgo.
- **Sin clases o sin trabajos dictados:** esa señal vale 0 y no se divide por cero.
- **Comisión sin estudiantes:** el reporte lo dice ("todavía no tiene estudiantes") y el porcentaje en riesgo es 0.
- **Comisión con estudiantes pero ninguno con datos:** el reporte dice que todavía no hay datos suficientes en vez de afirmar que nadie está en riesgo.
- **No se puede cerrar la entrega de un trabajo que todavía no venció.**

## Consecuencias

- **Porque:** es más honesto decir "no sé" que mostrar un verde falso; el tutor sabe a quién todavía no pudo evaluar.
- **A costa de:** un alumno recién cargado figura como "Sin datos" y no entra en el conteo de riesgo hasta que haya algún registro.
- **Lo revisaríamos si:** el cliente prefiriera que un alumno sin registros se trate como riesgo alto después de cierto tiempo.
