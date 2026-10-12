using RegistroAsistencia.Models;

namespace RegistroAsistencia.Services;

public class RiesgoService
{
    public decimal CalcularPorcentajeAusencias(Estudiante estudiante)
    {
        var asistencias = DatosSistema.Asistencias.Where(a => a.Estudiante.Legajo == estudiante.Legajo).ToList();

        if (asistencias.Count == 0)
            return 0;

        int ausencias = asistencias.Count(a => a.Estado == EstadoAsistencia.Ausente);

        return Math.Round((decimal)ausencias / asistencias.Count * 100, 2); //Devuelve el riesgo solo teniendo en cuenta las asistencias.
    }
}
