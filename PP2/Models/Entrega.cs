namespace RegistroAsistencia.Models;

public class Entrega
{
    public Estudiante Estudiante { get; set; } = new();

    public Trabajo Trabajo { get; set; } = new();

    public EstadoEntrega Estado { get; set; }
}