# ADR-005: Capas dentro de un proyecto y dirección de las dependencias

**Estado:** aceptada · **Registrada al cierre del proyecto (12/11/2026)**

## Contexto

Había que organizar el código para poder probar las reglas sin pantallas ni base de datos, y para que cada integrante trabajara sin pisar a los demás.

## Decisión

Un solo proyecto MAUI con carpetas por capa (`Models`, `Repositorios`, `Services`, `Datos`, `ViewModels`, `Views`) y un proyecto de pruebas aparte. Reglas:

- Los servicios dependen de **interfaces de repositorio**, nunca de SQLite.
- Los ViewModels no usan tipos de MAUI: navegan y confirman a través de `INavegacion` e `IDialogos`. Esto incluye elegir el archivo para la carga masiva (`IDialogos.ElegirArchivoDeTextoAsync`).
- `MauiProgram` es el único lugar que arma las dependencias.
- Las reglas de negocio no se repiten: por ejemplo, el reporte de riesgo reutiliza `RiesgoService` en vez de recalcular.

## Consecuencias

- **Porque:** las reglas se prueban con repositorios en memoria y, además, con SQLite en memoria en las pruebas de integración.
- **A costa de:** más interfaces y clases chicas que en una app de pantalla única; y las capas se respetan por convención, no porque el compilador lo impida (están en el mismo proyecto).
- **Lo revisaríamos si:** el equipo creciera o se agregara otra interfaz (web, por ejemplo): ahí valdría separar el dominio en su propio proyecto.
