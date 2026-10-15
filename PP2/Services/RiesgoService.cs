using RegistroAsistencia.Models;

namespace RegistroAsistencia.Services;

public class RiesgoService
{
    // Riesgo según asistencias
    public decimal CalcularPorcentajeAusencias(
        Estudiante estudiante)
    {
        var asistencias = DatosSistema.Asistencias
            .Where(a =>
                a.Estudiante.Legajo == estudiante.Legajo)
            .ToList();

        if (asistencias.Count == 0)
        {
            return 0;
        }

        int ausencias = asistencias.Count(a =>
            a.Estado == EstadoAsistencia.Ausente);

        return Math.Round(
            (decimal)ausencias / asistencias.Count * 100,
            2);
    }

    // Riesgo según entregas
    public decimal CalcularPorcentajeEntregas(
        Estudiante estudiante)
    {
        var entregas = DatosSistema.Entregas
            .Where(e =>
                e.Estudiante.Legajo == estudiante.Legajo)
            .ToList();

        if (entregas.Count == 0)
        {
            return 0;
        }

        decimal riesgoTotal = 0;

        foreach (var entrega in entregas)
        {
            switch (entrega.Estado)
            {
                case EstadoEntrega.Entregado:
                    riesgoTotal += 0;
                    break;

                case EstadoEntrega.EntregadoFueraDeTermino:
                    riesgoTotal += 50;
                    break;

                case EstadoEntrega.NoEntregado:
                    riesgoTotal += 100;
                    break;
            }
        }

        return Math.Round(
            riesgoTotal / entregas.Count,
            2);
    }

    // Riesgo académico combinado
    public decimal CalcularRiesgo(
        Estudiante estudiante)
    {
        bool tieneAsistencias =
            DatosSistema.Asistencias.Any(a =>
                a.Estudiante.Legajo == estudiante.Legajo);

        bool tieneEntregas =
            DatosSistema.Entregas.Any(e =>
                e.Estudiante.Legajo == estudiante.Legajo);

        if (!tieneAsistencias && !tieneEntregas)
        {
            return 0;
        }

        decimal riesgoAsistencia =
            CalcularPorcentajeAusencias(estudiante);

        decimal riesgoEntregas =
            CalcularPorcentajeEntregas(estudiante);

        // Si tiene ambos tipos de registros:
        // 50 % asistencia + 50 % entregas.
        if (tieneAsistencias && tieneEntregas)
        {
            return Math.Round(
                (riesgoAsistencia + riesgoEntregas) / 2,
                2);
        }

        // Si solo tiene asistencias.
        if (tieneAsistencias)
        {
            return riesgoAsistencia;
        }

        // Si solo tiene entregas.
        return riesgoEntregas;
    }
}
