using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Datos;

public sealed class CalendarioRepositorioSqlite : ICalendarioRepositorio
{
    private readonly BaseDatos _db;

    public CalendarioRepositorioSqlite(BaseDatos db)
    {
        _db = db;
    }

    public List<ActividadCalendario> ObtenerDeComision(string comision)
    {
        if (_db.IdDeComision(comision) is not int id)
        {
            return new List<ActividadCalendario>();
        }

        return _db.Conexion.Table<ActividadCalendarioDb>()
            .Where(a => a.ComisionId == id)
            .ToList()
            .Select(fila => fila.ADominio(comision))
            .ToList();
    }

    public void Agregar(ActividadCalendario actividad)
    {
        _db.Conexion.Insert(new ActividadCalendarioDb
        {
            ComisionId = _db.IdObligatorio(actividad.Comision),
            Cuatrimestre = actividad.Cuatrimestre,
            Fecha = Conversiones.GuardarFecha(actividad.Fecha),
            Tipo = (int)actividad.Tipo,
            Descripcion = actividad.Descripcion
        });
    }
}
