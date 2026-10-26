using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Services;

// Lo que se marcó para un alumno en la planilla de asistencia
public record MarcaAsistencia(Estudiante Estudiante, bool Presente);

// Lo que se marcó para un alumno en la planilla de entregas.
// Estado nulo: todavía no hay nada que registrar para ese alumno.
public record MarcaEntrega(Estudiante Estudiante, EstadoEntrega? Estado);

// Cuántos registros se crearon y cuántos se modificaron
public record ResultadoGuardado(int Nuevos, int Modificados);

// Una clase dictada: el día en que se tomó asistencia y cómo fue
public record ResumenClase(DateTime Fecha, int Presentes, int Ausentes)
{
    public int Total => Presentes + Ausentes;
}

/// <summary>
/// Guarda las planillas de asistencia (H3 y H4) y de entregas (H5).
/// Cada alumno tiene a lo sumo un registro por fecha o por trabajo:
/// si ya existe se corrige, no se duplica.
/// </summary>
public class RegistroService
{
    private enum Cambio
    {
        Ninguno,
        Nuevo,
        Modificado
    }

    private readonly IAsistenciaRepositorio _asistencias;

    private readonly IEntregaRepositorio _entregas;

    public RegistroService(
        IAsistenciaRepositorio asistencias,
        IEntregaRepositorio entregas)
    {
        _asistencias = asistencias;
        _entregas = entregas;
    }

    // Las clases dictadas de la comisión son los días en que se tomó
    // asistencia. De la más reciente a la más vieja.
    public List<ResumenClase> ClasesDe(string comision)
    {
        return _asistencias.ObtenerDeComision(comision)
            .GroupBy(a => a.Clase.Fecha.Date)
            .Select(dia => new ResumenClase(
                dia.Key,
                dia.Count(a => a.Estado == EstadoAsistencia.Presente),
                dia.Count(a => a.Estado != EstadoAsistencia.Presente)))
            .OrderByDescending(c => c.Fecha)
            .ToList();
    }

    // Lo que hay guardado para el alumno en esa fecha; nulo si nada
    public EstadoAsistencia? AsistenciaDe(string legajo, DateTime fecha)
    {
        return _asistencias.Buscar(legajo, fecha)?.Estado;
    }

    // Lo que hay guardado para el alumno en ese trabajo; nulo si nada
    public EstadoEntrega? EntregaDe(string legajo, int trabajoId)
    {
        return _entregas.Buscar(legajo, trabajoId)?.Estado;
    }

    // Verdadero si ya se tomó asistencia a algún alumno de la comisión
    public bool HayAsistencia(string comision, DateTime fecha)
    {
        return _asistencias.ObtenerDeComision(comision)
            .Any(a => a.Clase.Fecha.Date == fecha.Date);
    }

    public ResultadoGuardado GuardarAsistencia(
        DateTime fecha,
        IEnumerable<MarcaAsistencia> marcas)
    {
        var cambios = marcas
            .Select(m => GuardarMarcaAsistencia(fecha, m))
            .ToList();

        return Resumir(cambios);
    }

    public ResultadoGuardado GuardarEntregas(
        Trabajo trabajo,
        IEnumerable<MarcaEntrega> marcas)
    {
        var cambios = marcas
            .Select(m => GuardarMarcaEntrega(trabajo, m))
            .ToList();

        return Resumir(cambios);
    }

    /*
     * Cierra la entrega de un trabajo: los alumnos que no tienen ningún
     * registro quedan como "No entregado". Devuelve cuántos son.
     */
    public int CerrarEntrega(
        Trabajo trabajo,
        IEnumerable<Estudiante> estudiantes)
    {
        var pendientes = estudiantes
            .Where(e => _entregas.Buscar(e.Legajo, trabajo.Id) is null)
            .ToList();

        foreach (Estudiante estudiante in pendientes)
        {
            _entregas.Agregar(
                CrearEntrega(trabajo, estudiante, EstadoEntrega.NoEntregado));
        }

        return pendientes.Count;
    }

    private static ResultadoGuardado Resumir(List<Cambio> cambios)
    {
        return new ResultadoGuardado(
            cambios.Count(c => c == Cambio.Nuevo),
            cambios.Count(c => c == Cambio.Modificado));
    }

    private Cambio GuardarMarcaAsistencia(
        DateTime fecha,
        MarcaAsistencia marca)
    {
        string legajo = marca.Estudiante.Legajo;

        EstadoAsistencia estado = marca.Presente
            ? EstadoAsistencia.Presente
            : EstadoAsistencia.Ausente;

        Cambio cambio = Decidir(AsistenciaDe(legajo, fecha), estado);

        if (cambio == Cambio.Nuevo)
        {
            _asistencias.Agregar(CrearAsistencia(marca.Estudiante, fecha, estado));
        }
        else if (cambio == Cambio.Modificado)
        {
            _asistencias.ActualizarEstado(legajo, fecha, estado);
        }

        return cambio;
    }

    private Cambio GuardarMarcaEntrega(
        Trabajo trabajo,
        MarcaEntrega marca)
    {
        if (marca.Estado is not EstadoEntrega estado)
        {
            return Cambio.Ninguno;
        }

        string legajo = marca.Estudiante.Legajo;

        Cambio cambio = Decidir(EntregaDe(legajo, trabajo.Id), estado);

        if (cambio == Cambio.Nuevo)
        {
            _entregas.Agregar(CrearEntrega(trabajo, marca.Estudiante, estado));
        }
        else if (cambio == Cambio.Modificado)
        {
            _entregas.ActualizarEstado(legajo, trabajo.Id, estado);
        }

        return cambio;
    }

    // Sin nada guardado es nuevo; con lo mismo guardado no hay cambio
    private static Cambio Decidir<T>(T? guardado, T estado)
        where T : struct
    {
        if (guardado is null)
        {
            return Cambio.Nuevo;
        }

        return guardado.Value.Equals(estado)
            ? Cambio.Ninguno
            : Cambio.Modificado;
    }

    private static Asistencia CrearAsistencia(
        Estudiante estudiante,
        DateTime fecha,
        EstadoAsistencia estado)
    {
        return new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase { Fecha = fecha.Date },
            Estado = estado
        };
    }

    private static Entrega CrearEntrega(
        Trabajo trabajo,
        Estudiante estudiante,
        EstadoEntrega estado)
    {
        return new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo,
            Estado = estado
        };
    }
}
