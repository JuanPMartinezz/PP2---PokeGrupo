using RegistroAsistencia.Models;

namespace RegistroAsistencia.Repositorios;

public interface IAsistenciaRepositorio
{
    // Las asistencias de todos los alumnos de la comisión
    List<Asistencia> ObtenerDeComision(string comision);

    Asistencia? Buscar(string legajo, DateTime fecha);

    void Agregar(Asistencia asistencia);

    void ActualizarEstado(
        string legajo,
        DateTime fecha,
        EstadoAsistencia estado);
}
