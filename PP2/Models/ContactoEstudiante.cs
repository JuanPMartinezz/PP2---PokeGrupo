namespace RegistroAsistencia.Models;

public class ContactoEstudiante
{
    // Textos del resultado de la llamada: se definen una sola vez
    public const string TextoAtendio = "Atendió";

    public const string TextoNoAtendio = "No atendió";

    public Estudiante Estudiante { get; set; } = new();

    public DateTime Fecha { get; set; }

    // Si el estudiante atendió o no atendió la llamada
    public bool Atendio { get; set; } = true;

    public string Resultado { get; set; } = string.Empty;

    public string EstadoTexto =>
        Atendio ? TextoAtendio : TextoNoAtendio;
}
