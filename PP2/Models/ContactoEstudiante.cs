namespace RegistroAsistencia.Models;

public class ContactoEstudiante
{
    public Estudiante Estudiante { get; set; } = new();

    public DateTime Fecha { get; set; }

    public string Resultado { get; set; } = string.Empty;
}
