using RegistroAsistencia.Models;

namespace RegistroAsistencia.Services;

/// <summary>
/// Cálculo del riesgo académico (H6). Cada comisión usa sus propios
/// umbrales (H7).
/// </summary>
public class RiesgoService
{
    public const string NivelAlto = NivelesRiesgo.Alto;
    public const string NivelMedio = NivelesRiesgo.Medio;
    public const string NivelBajo = NivelesRiesgo.Bajo;
    public const string NivelSinDatos = NivelesRiesgo.SinDatos;

    // Riesgo según asistencias: porcentaje de ausencias
    public decimal CalcularPorcentajeAusencias(
        Estudiante estudiante)
    {
        return PorcentajeAusencias(AsistenciasDe(estudiante));
    }

    // Riesgo según entregas: promedio del riesgo de cada entrega
    public decimal CalcularPorcentajeEntregas(
        Estudiante estudiante)
    {
        return PorcentajeEntregas(
            EntregasDe(estudiante),
            DatosSistema.ObtenerUmbrales(estudiante.Comision));
    }

    // Riesgo académico combinado
    public decimal CalcularRiesgo(
        Estudiante estudiante)
    {
        var asistencias = AsistenciasDe(estudiante);
        var entregas = EntregasDe(estudiante);

        UmbralesRiesgo umbrales =
            DatosSistema.ObtenerUmbrales(estudiante.Comision);

        return Combinar(
            asistencias.Count > 0,
            PorcentajeAusencias(asistencias),
            entregas.Count > 0,
            PorcentajeEntregas(entregas, umbrales),
            umbrales);
    }

    /*
     * Con los dos tipos de registro se combinan según los pesos de la
     * comisión (por defecto 50 % y 50 %). Con uno solo, vale ese. Sin
     * registros no se penaliza a nadie: el riesgo es 0.
     */
    private static decimal Combinar(
        bool hayAsistencias,
        decimal riesgoAsistencia,
        bool hayEntregas,
        decimal riesgoEntregas,
        UmbralesRiesgo umbrales)
    {
        if (hayAsistencias && hayEntregas)
        {
            return Math.Round(
                (riesgoAsistencia * umbrales.PesoAsistencia +
                 riesgoEntregas * umbrales.PesoEntregas) / 100,
                2);
        }

        if (hayAsistencias)
        {
            return riesgoAsistencia;
        }

        return hayEntregas ? riesgoEntregas : 0;
    }

    // Nivel de riesgo según los umbrales de la comisión (H7).
    // Un estudiante sin asistencias ni entregas registradas no tiene datos
    // suficientes: no se lo marca como "Bajo" solo porque su riesgo es 0.
    public string ObtenerNivel(
        string comision,
        decimal porcentajeRiesgo,
        int totalRegistros)
    {
        if (totalRegistros == 0)
        {
            return NivelSinDatos;
        }

        UmbralesRiesgo umbrales =
            DatosSistema.ObtenerUmbrales(comision);

        if (porcentajeRiesgo >= umbrales.RiesgoAlto)
        {
            return NivelAlto;
        }

        return porcentajeRiesgo >= umbrales.RiesgoMedio
            ? NivelMedio
            : NivelBajo;
    }

    // Resultado completo de un estudiante: riesgo, nivel, estadísticas
    // y contactos (H8, H9 y H11)
    public ResultadoRiesgo ObtenerResultado(
        Estudiante estudiante)
    {
        var asistencias = AsistenciasDe(estudiante);
        var entregas = EntregasDe(estudiante);

        decimal riesgo = CalcularRiesgo(estudiante);

        return new ResultadoRiesgo
        {
            Estudiante = estudiante,
            TotalClases = asistencias.Count,
            Ausencias = Contar(asistencias, EstadoAsistencia.Ausente),
            Presentes = Contar(asistencias, EstadoAsistencia.Presente),
            TotalEntregas = entregas.Count,
            Entregados = Contar(entregas, EstadoEntrega.Entregado),
            NoEntregados = Contar(entregas, EstadoEntrega.NoEntregado),
            EntregadosFueraDeTermino = Contar(
                entregas,
                EstadoEntrega.EntregadoFueraDeTermino),
            PorcentajeRiesgo = riesgo,
            NivelRiesgo = ObtenerNivel(
                estudiante.Comision,
                riesgo,
                asistencias.Count + entregas.Count),
            Contactos = ContactosDe(estudiante)
        };
    }

    public List<ResultadoRiesgo> ObtenerResultados(string comision)
    {
        return DatosSistema.Estudiantes
            .Where(e => e.Comision == comision)
            .Select(ObtenerResultado)
            .ToList();
    }

    private static List<Asistencia> AsistenciasDe(Estudiante estudiante)
    {
        return DatosSistema.Asistencias
            .Where(a => a.Estudiante.Legajo == estudiante.Legajo)
            .ToList();
    }

    private static List<Entrega> EntregasDe(Estudiante estudiante)
    {
        return DatosSistema.Entregas
            .Where(e => e.Estudiante.Legajo == estudiante.Legajo)
            .ToList();
    }

    private static List<ContactoEstudiante> ContactosDe(
        Estudiante estudiante)
    {
        return DatosSistema.Contactos
            .Where(c => c.Estudiante.Legajo == estudiante.Legajo)
            .OrderByDescending(c => c.Fecha)
            .ToList();
    }

    private static int Contar(
        List<Asistencia> asistencias,
        EstadoAsistencia estado)
    {
        return asistencias.Count(a => a.Estado == estado);
    }

    private static int Contar(
        List<Entrega> entregas,
        EstadoEntrega estado)
    {
        return entregas.Count(e => e.Estado == estado);
    }

    private static decimal PorcentajeAusencias(
        List<Asistencia> asistencias)
    {
        if (asistencias.Count == 0)
        {
            return 0;
        }

        return Math.Round(
            (decimal)Contar(asistencias, EstadoAsistencia.Ausente) /
            asistencias.Count * 100,
            2);
    }

    private static decimal PorcentajeEntregas(
        List<Entrega> entregas,
        UmbralesRiesgo umbrales)
    {
        if (entregas.Count == 0)
        {
            return 0;
        }

        decimal total = entregas.Sum(e =>
            RiesgoDeEntrega(e.Estado, umbrales));

        return Math.Round(total / entregas.Count, 2);
    }

    private static decimal RiesgoDeEntrega(
        EstadoEntrega estado,
        UmbralesRiesgo umbrales)
    {
        return estado switch
        {
            EstadoEntrega.EntregadoFueraDeTermino =>
                umbrales.RiesgoFueraDeTermino,
            EstadoEntrega.NoEntregado =>
                umbrales.RiesgoNoEntregado,
            _ => 0
        };
    }
}
