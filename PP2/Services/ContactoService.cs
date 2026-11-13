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
        MedioContacto? medio,
        bool atendio,
        string resultado)
    {
        Estudiante? estudiante = _estudiantes.Buscar(legajo);

        string? error = Validar(estudiante, fecha, medio, atendio, resultado);

        if (error is null)
        {
            _contactos.Agregar(new ContactoEstudiante
            {
                Estudiante = estudiante!,
                Fecha = fecha.Date,
                Medio = medio!.Value,
                Atendio = atendio,
                Resultado = resultado.Trim()
            });
        }

        return error;
    }

    private static string? Validar(
        Estudiante? estudiante,
        DateTime fecha,
        MedioContacto? medio,
        bool atendio,
        string resultado)
    {
        if (estudiante is null)
        {
            return "No se encontró al alumno";
        }

        if (medio is null)
        {
            return "Elegí el medio del contacto";
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
