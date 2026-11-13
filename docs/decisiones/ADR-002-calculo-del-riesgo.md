# ADR-002: Cómo se calcula el riesgo

**Estado:** aceptada · **Registrada al cierre del proyecto (12/11/2026)**

## Contexto

El cliente pidió saber "a quién llamar", pero no dijo cómo medir el riesgo de abandonar o desaprobar. Hubo que decidir qué señales usar, cómo combinarlas y dónde cortar los niveles.

## Decisión

- **Dos señales:** ausencias y entregas.
- **Ausencias** = ausencias del alumno ÷ clases dictadas × 100.
- **Entregas** = suma del riesgo de cada entrega ÷ trabajos dictados × 100, con riesgo por estado: Entregado 0, Fuera de término 50, No entregado 100.
- **Riesgo total** = promedio ponderado de ambas señales; por defecto 50 % y 50 %. Si el alumno solo tiene registros de una de las dos (por ejemplo, todavía no hay entregas cargadas), el riesgo es el de esa señal: no se promedia con un 0 que no existe.
- **Niveles:** Bajo, Medio desde 30 %, Alto desde 60 %. Un valor justo en el límite cuenta hacia arriba.
- **Pesos, riesgos por estado y cortes son de cada comisión** (`UmbralesComision`), no globales.
- **Un único lugar calcula el riesgo:** `RiesgoService`. La lista, la ficha, el filtro "para llamar" y el reporte de la comisión (H12) usan sus resultados; ninguna pantalla recalcula.

## Consecuencias

- **Porque:** es una regla simple, que el tutor puede explicar y ajustar según la materia (una comisión con muchos trabajos puede dar más peso a las entregas).
- **A costa de:** es una regla lineal; no distingue faltas seguidas de faltas salteadas ni mira la tendencia.
- **Lo revisaríamos si:** el cliente quisiera otra señal (por ejemplo, faltas consecutivas). Cambiaría solo `RiesgoService`.
