using RegistroAsistencia.Models;

namespace RegistroAsistencia.Services;

// Lo que se marcó para un alumno en la planilla de asistencia
public record MarcaAsistencia(Estudiante Estudiante, bool Presente);

// Lo que se marcó para un alumno en la planilla de entregas.
// Estado nulo: todavía no hay nada que registrar para ese alumno.
public record MarcaEntrega(Estudiante Estudiante, EstadoEntrega? Estado);

// Cuántos registros se crearon y cuántos se modificaron
public record ResultadoGuardado(int Nuevos, int Modificados);

/// <summary>
/// Guarda las planillas de asistencia (H3 y H4) y de entregas (H5).
/// Cada alumno tiene a lo sumo un registro por fecha o por trabajo:
/// si ya existe se corrige, no se duplica.
/// </summary>
public class RegistroService
{
    public ResultadoGuardado GuardarAsistencia(
        DateTime fecha,
        IEnumerable<MarcaAsistencia> marcas)
    {
        int nuevos = 0;
        int modificados = 0;

        foreach (MarcaAsistencia marca in marcas)
        {
            switch (GuardarMarcaAsistencia(fecha, marca))
            {
                case Cambio.Nuevo: nuevos++; break;
                case Cambio.Modificado: modificados++; break;
            }
        }

        return new ResultadoGuardado(nuevos, modificados);
    }

    public ResultadoGuardado GuardarEntregas(
        Trabajo trabajo,
        IEnumerable<MarcaEntrega> marcas)
    {
        int nuevos = 0;
        int modificados = 0;

        foreach (MarcaEntrega marca in marcas)
        {
            switch (GuardarMarcaEntrega(trabajo, marca))
            {
                case Cambio.Nuevo: nuevos++; break;
                case Cambio.Modificado: modificados++; break;
            }
        }

        return new ResultadoGuardado(nuevos, modificados);
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
            .Where(e => !DatosSistema.ExisteEntrega(e.Legajo, trabajo.Id))
            .ToList();

        foreach (Estudiante estudiante in pendientes)
        {
            DatosSistema.AgregarEntrega(
                CrearEntrega(trabajo, estudiante, EstadoEntrega.NoEntregado));
        }

        return pendientes.Count;
    }

    private enum Cambio
    {
        Ninguno,
        Nuevo,
        Modificado
    }

    private static Cambio GuardarMarcaAsistencia(
        DateTime fecha,
        MarcaAsistencia marca)
    {
        EstadoAsistencia estado = marca.Presente
            ? EstadoAsistencia.Presente
            : EstadoAsistencia.Ausente;

        Asistencia? existente = DatosSistema.BuscarAsistencia(
            marca.Estudiante.Legajo,
            fecha);

        if (existente is null)
        {
            DatosSistema.AgregarAsistencia(
                CrearAsistencia(marca.Estudiante, fecha, estado));

            return Cambio.Nuevo;
        }

        return ActualizarAsistencia(existente, estado);
    }

    private static Cambio ActualizarAsistencia(
        Asistencia existente,
        EstadoAsistencia estado)
    {
        if (existente.Estado == estado)
        {
            return Cambio.Ninguno;
        }

        DatosSistema.ActualizarEstadoAsistencia(existente, estado);

        return Cambio.Modificado;
    }

    private static Cambio GuardarMarcaEntrega(
        Trabajo trabajo,
        MarcaEntrega marca)
    {
        if (marca.Estado is not EstadoEntrega estado)
        {
            return Cambio.Ninguno;
        }

        Entrega? existente = DatosSistema.BuscarEntrega(
            marca.Estudiante.Legajo,
            trabajo.Id);

        if (existente is null)
        {
            DatosSistema.AgregarEntrega(
                CrearEntrega(trabajo, marca.Estudiante, estado));

            return Cambio.Nuevo;
        }

        return ActualizarEntrega(existente, estado);
    }

    private static Cambio ActualizarEntrega(
        Entrega existente,
        EstadoEntrega estado)
    {
        if (existente.Estado == estado)
        {
            return Cambio.Ninguno;
        }

        DatosSistema.ActualizarEstadoEntrega(existente, estado);

        return Cambio.Modificado;
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
