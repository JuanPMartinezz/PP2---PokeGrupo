namespace RegistroAsistencia.Models;

public class Trabajo
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public override string ToString()
    {
        return Nombre;
    }
}