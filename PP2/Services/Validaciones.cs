using RegistroAsistencia.Models;

namespace RegistroAsistencia.Services;

/// <summary>
/// Reglas de validación de los formularios. Devuelven el motivo del
/// rechazo, escrito para quien usa la aplicación, o null si todo está bien.
/// </summary>
public static class Validaciones
{
    public static string? Alumno(
        string nombre,
        string apellido,
        string legajo,
        string? comision)
    {
        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(apellido) ||
            string.IsNullOrWhiteSpace(legajo))
        {
            return "Completá nombre, apellido y legajo";
        }

        if (string.IsNullOrWhiteSpace(comision))
        {
            return "Elegí la comisión del alumno";
        }

        return null;
    }

    public static string? LegajoNuevo(string legajo)
    {
        var existente = DatosSistema.BuscarEstudiante(legajo);

        if (existente is null)
        {
            return null;
        }

        return $"Ya existe el legajo {existente.Legajo}: " +
               $"{existente} ({existente.Comision})";
    }

    public static string? NombreComision(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return "Escribí el nombre de la comisión";
        }

        if (DatosSistema.ExisteComision(nombre))
        {
            return "Ya existe una comisión con ese nombre";
        }

        return null;
    }

    public static string? Trabajo(
        string nombre,
        string comision,
        DateTime fecha)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return "Escribí el nombre del trabajo";
        }

        bool repetido = DatosSistema.ExisteActividadCalendario(
            comision,
            Cuatrimestres.De(fecha),
            fecha,
            TipoActividad.Trabajo,
            nombre);

        return repetido
            ? "Ese trabajo ya está cargado con esa fecha"
            : null;
    }

    public static string? FechaContacto(DateTime fecha)
    {
        return fecha.Date > DateTime.Today
            ? "La fecha del contacto no puede ser futura"
            : null;
    }

    public static string? FechaAsistencia(DateTime fecha)
    {
        return fecha.Date > DateTime.Today
            ? "La fecha de la asistencia no puede ser futura"
            : null;
    }
}
