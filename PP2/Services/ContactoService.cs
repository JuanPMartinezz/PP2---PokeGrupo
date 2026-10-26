using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Services;

/// <summary>
/// Registro de los contactos con los alumnos (H10).
/// </summary>
public class ContactoService
{
    private readonly IContactoRepositorio _contactos;

    private readonly IEstudianteRepositorio _estudiantes;

    public ContactoService(
        IContactoRepositorio contactos,
        IEstudianteRepositorio estudiantes)
    {
        _contactos = contactos;
        _estudiantes = estudiantes;
    }

    // Devuelve el motivo si no se pudo registrar el contacto
    public string? Registrar(
        string legajo,
        DateTime fecha,
        bool atendio,
        string resultado)
    {
        Estudiante? estudiante = _estudiantes.Buscar(legajo);

        string? error = Validar(estudiante, fecha, atendio, resultado);

        if (error is null)
        {
            _contactos.Agregar(new ContactoEstudiante
            {
                Estudiante = estudiante!,
                Fecha = fecha.Date,
                Atendio = atendio,
                Resultado = resultado.Trim()
            });
        }

        return error;
    }

    private static string? Validar(
        Estudiante? estudiante,
        DateTime fecha,
        bool atendio,
        string resultado)
    {
        if (estudiante is null)
        {
            return "No se encontró al alumno";
        }

        if (!string.IsNullOrWhiteSpace(resultado))
        {
            return Validaciones.FechaContacto(fecha);
        }

        return atendio
            ? "Escribí de qué se habló en el contacto"
            : "Escribí qué pasó (por ejemplo: sonó y no contestó)";
    }
}
