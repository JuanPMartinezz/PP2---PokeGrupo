using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Datos;

public sealed class AsistenciaRepositorioSqlite : IAsistenciaRepositorio
{
    private readonly BaseDatos _db;

    public AsistenciaRepositorioSqlite(BaseDatos db)
    {
        _db = db;
    }

    public List<Asistencia> ObtenerDeComision(string comision)
    {
        if (_db.IdDeComision(comision) is not int id)
        {
            return new();
        }

        var estudiantes = _db.Conexion.Table<EstudianteDb>()
            .Where(e => e.ComisionId == id)
            .ToList()
            .ToDictionary(e => e.Legajo, e => e.AEstudiante(comision));

        return _db.Conexion.Query<AsistenciaDb>(
                "SELECT a.* FROM Asistencias a " +
                "JOIN Estudiantes e ON e.Legajo = a.LegajoEstudiante " +
                "WHERE e.ComisionId = ?",
                id)
            .Select(fila => ADominio(fila, estudiantes[fila.LegajoEstudiante]))
            .ToList();
    }

    public Asistencia? Buscar(string legajo, DateTime fecha)
    {
        string dia = Conversiones.GuardarFecha(fecha);

        AsistenciaDb? fila = _db.Conexion.Table<AsistenciaDb>()
            .FirstOrDefault(a =>
                a.LegajoEstudiante == legajo && a.Fecha == dia);

        EstudianteDb? estudiante = _db.Conexion.Table<EstudianteDb>()
            .FirstOrDefault(e => e.Legajo == legajo);

        return fila is null || estudiante is null
            ? null
            : ADominio(fila, _db.AEstudiante(estudiante));
    }

    public void Agregar(Asistencia asistencia)
    {
        _db.Conexion.Insert(new AsistenciaDb
        {
            LegajoEstudiante = asistencia.Estudiante.Legajo,
            Fecha = Conversiones.GuardarFecha(asistencia.Clase.Fecha),
            Estado = (int)asistencia.Estado
        });
    }

    public void ActualizarEstado(
        string legajo,
        DateTime fecha,
        EstadoAsistencia estado)
    {
        string dia = Conversiones.GuardarFecha(fecha);

        AsistenciaDb fila = _db.Conexion.Table<AsistenciaDb>()
            .First(a => a.LegajoEstudiante == legajo && a.Fecha == dia);

        fila.Estado = (int)estado;

        _db.Conexion.Update(fila);
    }

    private static Asistencia ADominio(
        AsistenciaDb fila,
        Estudiante estudiante)
    {
        return new Asistencia
        {
            Estudiante = estudiante,
            Clase = new Clase { Fecha = Conversiones.LeerFecha(fila.Fecha) },
            Estado = (EstadoAsistencia)fila.Estado
        };
    }
}
