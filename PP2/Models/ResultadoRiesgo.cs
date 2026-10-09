namespace RegistroAsistencia.Models;

public class ResultadoRiesgo
{
    public Estudiante Estudiante { get; set; }

    public int TotalClases { get; set; }

    public int Ausencias { get; set; }

    public int Presentes { get; set; }

    public decimal PorcentajeAusencias { get; set; }
}