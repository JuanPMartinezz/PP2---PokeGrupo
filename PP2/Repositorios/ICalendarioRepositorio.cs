using RegistroAsistencia.Models;

namespace RegistroAsistencia.Repositorios;

public interface ICalendarioRepositorio
{
    List<ActividadCalendario> ObtenerDeComision(string comision);

    void Agregar(ActividadCalendario actividad);
}
