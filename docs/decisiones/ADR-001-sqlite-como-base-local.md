# ADR-001: SQLite como base de datos local

**Estado:** aceptada · **Registrada al cierre del proyecto (12/11/2026)**

## Contexto

La app la usa una sola persona, en su computadora, y tiene que funcionar sin instalar ni configurar nada. Los datos son pocos (comisiones, alumnos, asistencias, entregas, contactos).

## Decisión

Guardar todo en un archivo SQLite (`registro_asistencia_v2.db3`) dentro del directorio de datos privado de la app, con el paquete `sqlite-net-pcl`. Las tablas se crean solas al primer arranque (`BaseDatos`). La comisión se guarda una sola vez en `Comisiones`; el resto la referencia por `ComisionId`.

## Consecuencias

- **Porque:** no hay servidor ni cadena de conexión que configurar; cualquiera clona el repositorio, compila y corre.
- **A costa de:** un solo usuario por archivo, sin sincronización entre equipos ni copias de seguridad automáticas. El esquema v1 (con el nombre de la comisión repetido) se abandonó sin migrar sus datos.
- **Lo revisaríamos si:** varias personas necesitaran ver los mismos datos a la vez; ahí haría falta un servidor y cambiaría solo `Datos/`, porque los servicios usan interfaces.
