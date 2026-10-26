using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Datos;

public sealed class EntregaRepositorioSqlite : IEntregaRepositorio
{
    private readonly BaseDatos _db;

    public EntregaRepositorioSqlite(BaseDatos db)
    {
        _db = db;
    }

    public List<Entrega> ObtenerDeComision(string comision)
    {
        if (_db.IdDeComision(comision) is not int id)
        {
            return new();
        }

        var estudiantes = _db.Conexion.Table<EstudianteDb>()
            .Where(e => e.ComisionId == id)
            .ToList()
            .ToDictionary(e => e.Legajo, e => e.AEstudiante(comision));

        var trabajos = _db.Conexion.Table<TrabajoDb>()
            .ToList()
            .ToDictionary(t => t.Id, ATrabajo);

        return _db.Conexion.Query<EntregaDb>(
                "SELECT n.* FROM Entregas n " +
                "JOIN Estudiantes e ON e.Legajo = n.LegajoEstudiante " +
                "WHERE e.ComisionId = ?",
                id)
            .Where(fila => trabajos.ContainsKey(fila.TrabajoId))
            .Select(fila => ADominio(
                fila,
                estudiantes[fila.LegajoEstudiante],
                trabajos[fila.TrabajoId]))
            .ToList();
    }

    public Entrega? Buscar(string legajo, int trabajoId)
    {
        EntregaDb? fila = _db.Conexion.Table<EntregaDb>()
            .FirstOrDefault(e =>
                e.LegajoEstudiante == legajo && e.TrabajoId == trabajoId);

        EstudianteDb? estudiante = _db.Conexion.Table<EstudianteDb>()
            .FirstOrDefault(e => e.Legajo == legajo);

        TrabajoDb? trabajo = _db.Conexion.Table<TrabajoDb>()
            .FirstOrDefault(t => t.Id == trabajoId);

        return fila is null || estudiante is null || trabajo is null
            ? null
            : ADominio(fila, _db.AEstudiante(estudiante), ATrabajo(trabajo));
    }

    public void Agregar(Entrega entrega)
    {
        _db.Conexion.Insert(new EntregaDb
        {
            LegajoEstudiante = entrega.Estudiante.Legajo,
            TrabajoId = entrega.Trabajo.Id,
            Estado = (int)entrega.Estado
        });
    }

    public void ActualizarEstado(
        string legajo,
        int trabajoId,
        EstadoEntrega estado)
    {
        EntregaDb fila = _db.Conexion.Table<EntregaDb>()
            .First(e =>
                e.LegajoEstudiante == legajo && e.TrabajoId == trabajoId);

        fila.Estado = (int)estado;

        _db.Conexion.Update(fila);
    }

    public Trabajo ObtenerOCrearTrabajo(string nombre)
    {
        string limpio = nombre.Trim();

        TrabajoDb? existente = _db.Conexion.Table<TrabajoDb>()
            .ToList()
            .FirstOrDefault(t => t.Nombre.Equals(
                limpio,
                StringComparison.OrdinalIgnoreCase));

        if (existente is null)
        {
            existente = new TrabajoDb { Nombre = limpio };

            _db.Conexion.Insert(existente);
        }

        return ATrabajo(existente);
    }

    private static Trabajo ATrabajo(TrabajoDb fila)
    {
        return new Trabajo { Id = fila.Id, Nombre = fila.Nombre };
    }

    private static Entrega ADominio(
        EntregaDb fila,
        Estudiante estudiante,
        Trabajo trabajo)
    {
        return new Entrega
        {
            Estudiante = estudiante,
            Trabajo = trabajo,
            Estado = (EstadoEntrega)fila.Estado
        };
    }
}
