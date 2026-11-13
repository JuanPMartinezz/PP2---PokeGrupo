using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Services;

/// <summary>
/// Altas, cambios y bajas de alumnos (H1).
/// </summary>
public class NominaService
{
    private readonly IEstudianteRepositorio _estudiantes;

    private readonly IComisionRepositorio _comisiones;

    public NominaService(
        IEstudianteRepositorio estudiantes,
        IComisionRepositorio comisiones)
    {
        _estudiantes = estudiantes;
        _comisiones = comisiones;
    }

    public List<Estudiante> DeComision(string comision)
    {
        return _estudiantes.ObtenerDeComision(comision);
    }

    public Estudiante? Buscar(string legajo)
    {
        return _estudiantes.Buscar(legajo.Trim());
    }

    // Devuelve el motivo si no se pudo agregar al alumno
    public string? Agregar(
        string nombre,
        string apellido,
        string legajo,
        string? comision,
        DateTime fechaIncorporacion = default)
    {
        string? error = Validaciones.Alumno(nombre, apellido, legajo, comision)
                        ?? Validaciones.Legajo(legajo)
                        ?? Validaciones.FechaIncorporacion(fechaIncorporacion)
                        ?? ControlarComision(comision!)
                        ?? ControlarLegajoLibre(legajo);

        if (error is null)
        {
            _estudiantes.Agregar(new Estudiante
            {
                Nombre = nombre.Trim(),
                Apellido = apellido.Trim(),
                Legajo = legajo.Trim(),
                Comision = comision!,
                FechaIncorporacion = fechaIncorporacion.Date
            });
        }

        return error;
    }

    // El legajo no se puede cambiar. Devuelve el motivo si no se pudo
    public string? Actualizar(
        string legajo,
        string nombre,
        string apellido,
        string? comision,
        DateTime fechaIncorporacion)
    {
        Estudiante? estudiante = Buscar(legajo);

        string? error = Validaciones.Alumno(nombre, apellido, legajo, comision)
                        ?? Validaciones.FechaIncorporacion(fechaIncorporacion)
                        ?? ControlarComision(comision!);

        if (error is null && estudiante is null)
        {
            error = "No se encontró al alumno";
        }

        if (error is null)
        {
            estudiante!.Nombre = nombre.Trim();
            estudiante.Apellido = apellido.Trim();
            estudiante.Comision = comision!;
            estudiante.FechaIncorporacion = fechaIncorporacion.Date;

            _estudiantes.Actualizar(estudiante);
        }

        return error;
    }

    // Elimina al alumno con sus asistencias, entregas y contactos
    public void Eliminar(string legajo)
    {
        _estudiantes.Eliminar(legajo.Trim());
    }

    private string? ControlarComision(string comision)
    {
        return _comisiones.ObtenerTodas().Contains(comision)
            ? null
            : "La comisión elegida no existe";
    }

    private string? ControlarLegajoLibre(string legajo)
    {
        Estudiante? existente = Buscar(legajo);

        return existente is null
            ? null
            : $"Ya existe el legajo {existente.Legajo}: " +
              $"{existente} ({existente.Comision})";
    }
}
