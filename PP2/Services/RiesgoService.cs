using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Services;

/// <summary>
/// Riesgo académico de los alumnos (H6, H8, H9 y H11). No conoce la base
/// de datos ni ninguna pantalla: recibe los repositorios por constructor.
/// </summary>
public class RiesgoService
{
    public const string NivelAlto = NivelesRiesgo.Alto;
    public const string NivelMedio = NivelesRiesgo.Medio;
    public const string NivelBajo = NivelesRiesgo.Bajo;
    public const string NivelSinDatos = NivelesRiesgo.SinDatos;

    private readonly IEstudianteRepositorio _estudiantes;

    private readonly IAsistenciaRepositorio _asistencias;

    private readonly IEntregaRepositorio _entregas;

    private readonly IContactoRepositorio _contactos;

    private readonly UmbralesService _umbrales;

    public RiesgoService(
        IEstudianteRepositorio estudiantes,
        IAsistenciaRepositorio asistencias,
        IEntregaRepositorio entregas,
        IContactoRepositorio contactos,
        UmbralesService umbrales)
    {
        _estudiantes = estudiantes;
        _asistencias = asistencias;
        _entregas = entregas;
        _contactos = contactos;
        _umbrales = umbrales;
    }

    // Todo lo registrado en una comisión, leído una sola vez
    private sealed record DatosComision(
        List<Asistencia> Asistencias,
        List<Entrega> Entregas,
        List<ContactoEstudiante> Contactos,
        int ClasesDictadas,
        UmbralesRiesgo Umbrales);

    // Resultado de todos los alumnos de la comisión
    public List<ResultadoRiesgo> ObtenerResultados(string comision)
    {
        DatosComision datos = Cargar(comision);

        return _estudiantes.ObtenerDeComision(comision)
            .Select(e => Armar(e, datos))
            .ToList();
    }

    // Resultado completo de un estudiante: riesgo, nivel, estadísticas
    // y contactos (H8, H9 y H11)
    public ResultadoRiesgo ObtenerResultado(Estudiante estudiante)
    {
        return Armar(estudiante, Cargar(estudiante.Comision));
    }

    // Riesgo académico combinado
    public decimal CalcularRiesgo(Estudiante estudiante)
    {
        return ObtenerResultado(estudiante).PorcentajeRiesgo;
    }

    // Porcentaje de las clases dictadas a las que faltó
    public decimal CalcularPorcentajeAusencias(Estudiante estudiante)
    {
        DatosComision datos = Cargar(estudiante.Comision);

        return PorcentajeAusencias(
            AsistenciasDe(estudiante, datos),
            datos.ClasesDictadas);
    }

    // Promedio del riesgo de cada entrega del estudiante
    public decimal CalcularPorcentajeEntregas(Estudiante estudiante)
    {
        DatosComision datos = Cargar(estudiante.Comision);

        return PorcentajeEntregas(
            EntregasDe(estudiante, datos),
            datos.Umbrales);
    }

    // Nivel de riesgo según los umbrales de la comisión (H7).
    // Un estudiante sin asistencias ni entregas registradas no tiene datos
    // suficientes: no se lo marca como "Bajo" solo porque su riesgo es 0.
    public string ObtenerNivel(
        string comision,
        decimal porcentajeRiesgo,
        int totalRegistros)
    {
        return Nivel(
            _umbrales.Obtener(comision),
            porcentajeRiesgo,
            totalRegistros);
    }

    private DatosComision Cargar(string comision)
    {
        List<Asistencia> asistencias =
            _asistencias.ObtenerDeComision(comision);

        // Una clase dictada es una fecha en la que se tomó asistencia
        int clases = asistencias
            .Select(a => a.Clase.Fecha.Date)
            .Distinct()
            .Count();

        return new DatosComision(
            asistencias,
            _entregas.ObtenerDeComision(comision),
            _contactos.ObtenerDeComision(comision),
            clases,
            _umbrales.Obtener(comision));
    }

    private static ResultadoRiesgo Armar(
        Estudiante estudiante,
        DatosComision datos)
    {
        var asistencias = AsistenciasDe(estudiante, datos);
        var entregas = EntregasDe(estudiante, datos);

        decimal riesgo = Combinar(
            asistencias.Count > 0,
            PorcentajeAusencias(asistencias, datos.ClasesDictadas),
            entregas.Count > 0,
            PorcentajeEntregas(entregas, datos.Umbrales),
            datos.Umbrales);

        ResultadoRiesgo resultado = Estadisticas(
            estudiante,
            asistencias,
            entregas,
            datos);

        resultado.PorcentajeRiesgo = riesgo;
        resultado.NivelRiesgo = Nivel(
            datos.Umbrales,
            riesgo,
            asistencias.Count + entregas.Count);

        return resultado;
    }

    private static ResultadoRiesgo Estadisticas(
        Estudiante estudiante,
        List<Asistencia> asistencias,
        List<Entrega> entregas,
        DatosComision datos)
    {
        return new ResultadoRiesgo
        {
            Estudiante = estudiante,
            TotalClases = datos.ClasesDictadas,
            Ausencias = Contar(asistencias, EstadoAsistencia.Ausente),
            Presentes = Contar(asistencias, EstadoAsistencia.Presente),
            TotalEntregas = entregas.Count,
            Entregados = Contar(entregas, EstadoEntrega.Entregado),
            NoEntregados = Contar(entregas, EstadoEntrega.NoEntregado),
            EntregadosFueraDeTermino = Contar(
                entregas,
                EstadoEntrega.EntregadoFueraDeTermino),
            Contactos = ContactosDe(estudiante, datos)
        };
    }

    private static List<Asistencia> AsistenciasDe(
        Estudiante estudiante,
        DatosComision datos)
    {
        return datos.Asistencias
            .Where(a => a.Estudiante.Legajo == estudiante.Legajo)
            .ToList();
    }

    private static List<Entrega> EntregasDe(
        Estudiante estudiante,
        DatosComision datos)
    {
        return datos.Entregas
            .Where(e => e.Estudiante.Legajo == estudiante.Legajo)
            .ToList();
    }

    private static List<ContactoEstudiante> ContactosDe(
        Estudiante estudiante,
        DatosComision datos)
    {
        return datos.Contactos
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

    /*
     * El porcentaje se calcula sobre las clases dictadas en la comisión,
     * no sobre los registros del alumno: una falta entre diez clases es
     * 10 %, no 100 %.
     */
    private static decimal PorcentajeAusencias(
        List<Asistencia> asistencias,
        int clasesDictadas)
    {
        if (clasesDictadas == 0)
        {
            return 0;
        }

        return Math.Round(
            (decimal)Contar(asistencias, EstadoAsistencia.Ausente) /
            clasesDictadas * 100,
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

    private static string Nivel(
        UmbralesRiesgo umbrales,
        decimal porcentajeRiesgo,
        int totalRegistros)
    {
        if (totalRegistros == 0)
        {
            return NivelSinDatos;
        }

        if (porcentajeRiesgo >= umbrales.RiesgoAlto)
        {
            return NivelAlto;
        }

        return porcentajeRiesgo >= umbrales.RiesgoMedio
            ? NivelMedio
            : NivelBajo;
    }
}
