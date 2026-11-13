# ADR-008: Qué quedó fuera del alcance

**Estado:** aceptada · **Registrada al cierre del proyecto (12/11/2026)**

## Contexto

El proyecto se cierra en un cuatrimestre. Algunas cosas razonables no entraron.

## Decisión

No se construyeron:

- usuarios y permisos (la app es de una sola persona);
- sincronización o copias de seguridad entre equipos;
- envío automático de mensajes o correos a los alumnos;
- historial de la evolución del riesgo en el tiempo;
- exportación de reportes;
- migración de datos del esquema anterior de la base.

## Consecuencias

- **Porque:** el cliente pidió saber a quién llamar, y eso se resuelve con lo construido.
- **A costa de:** el tutor lleva a mano lo que no está (por ejemplo, ver cómo evolucionó un alumno).
- **Lo revisaríamos si:** el uso real mostrara que la tendencia importa más que la foto actual: sería el primer agregado.
