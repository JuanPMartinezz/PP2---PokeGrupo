namespace RegistroAsistencia.Models;

/// <summary>
/// Cuántos estudiantes de una comisión hay en cada nivel de riesgo (H12).
/// Sale de los mismos resultados que muestra la lista de alumnos.
/// </summary>
public record ResumenComision(
    int Total,
    int Alto,
    int Medio,
    int Bajo,
    int SinDatos)
{
    public bool SinEstudiantes => Total == 0;

    // Hay estudiantes pero ninguno tiene asistencias ni entregas: no se
    // puede decir que haya 0 en riesgo
    public bool SinDatosSuficientes => Total > 0 && SinDatos == Total;

    public decimal PorcentajeEnRiesgo =>
        SinEstudiantes || SinDatosSuficientes
            ? 0
            : Math.Round((decimal)Alto / Total * 100, 1);
}
