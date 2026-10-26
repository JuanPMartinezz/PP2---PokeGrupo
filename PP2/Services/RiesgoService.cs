using RegistroAsistencia.Models;

namespace RegistroAsistencia.Services;

public class RiesgoService
{
    public const string NivelAlto = "Alto";
    public const string NivelMedio = "Medio";
    public const string NivelBajo = "Bajo";
    public const string NivelSinDatos = "Sin datos";

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

        // Valores configurables (H7)
        UmbralesRiesgo umbrales = DatosSistema.Umbrales;

        decimal riesgoTotal = 0;

        foreach (var entrega in entregas)
        {
            switch (entrega.Estado)
            {
                case EstadoEntrega.Entregado:
                    riesgoTotal += 0;
                    break;

                case EstadoEntrega.EntregadoFueraDeTermino:
                    riesgoTotal += umbrales.RiesgoFueraDeTermino;
                    break;

                case EstadoEntrega.NoEntregado:
                    riesgoTotal += umbrales.RiesgoNoEntregado;
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

        // Si tiene ambos tipos de registros se combinan según los pesos
        // configurados (H7). Por defecto: 50 % asistencia + 50 % entregas.
        if (tieneAsistencias && tieneEntregas)
        {
            UmbralesRiesgo umbrales = DatosSistema.Umbrales;

            return Math.Round(
                (riesgoAsistencia * umbrales.PesoAsistencia +
                 riesgoEntregas * umbrales.PesoEntregas) / 100,
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

    // Nivel de riesgo según los umbrales configurados (H7).
    // Un estudiante sin asistencias ni entregas registradas no tiene datos
    // suficientes: no se lo marca como "Bajo" solo porque su riesgo es 0.
    public string ObtenerNivel(
        decimal porcentajeRiesgo,
        int totalRegistros)
    {
        if (totalRegistros == 0)
        {
            return NivelSinDatos;
        }

        UmbralesRiesgo umbrales = DatosSistema.Umbrales;

        if (porcentajeRiesgo >= umbrales.RiesgoAlto)
        {
            return NivelAlto;
        }

        if (porcentajeRiesgo >= umbrales.RiesgoMedio)
        {
            return NivelMedio;
        }

        return NivelBajo;
    }
}
