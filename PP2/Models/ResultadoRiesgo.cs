namespace RegistroAsistencia.Models;

public class ResultadoRiesgo
{
    public Estudiante Estudiante { get; set; } = new();

    // Asistencias
    public int TotalClases { get; set; }

    public int Ausencias { get; set; }

    public int Presentes { get; set; }

    // Entregas
    public int TotalEntregas { get; set; }

    public int Entregados { get; set; }

    public int NoEntregados { get; set; }

    public int EntregadosFueraDeTermino { get; set; }

    // Riesgo
    public decimal PorcentajeRiesgo { get; set; }
}