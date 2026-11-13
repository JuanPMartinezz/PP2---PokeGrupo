# ADR-007: Qué significa "para llamar"

**Estado:** aceptada · **Registrada al cierre del proyecto (12/11/2026)**

## Contexto

La pregunta del cliente es "a quién llamo esta semana". Hay que traducirla en un criterio que la app pueda calcular.

## Decisión

Un alumno es **"para llamar"** si tiene **riesgo alto** y **no tuvo contacto en los últimos 7 días** (`ResultadoRiesgo.DiasSinContactoReciente`). Cada contacto registra fecha, medio (llamada, mensaje, correo o en persona), si el alumno atendió y un resultado obligatorio. Un contacto en que el alumno no atendió cuenta como contacto hecho.

## Consecuencias

- **Porque:** evita insistir con quien acaba de ser contactado y pone al frente a quien más lo necesita.
- **A costa de:** 7 días es un valor fijo; y un intento sin respuesta frena el aviso por una semana.
- **Lo revisaríamos si:** el cliente quisiera que los intentos sin respuesta no cuenten, o un plazo distinto por comisión.
