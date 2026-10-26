using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Datos;

public sealed class EstudianteRepositorioSqlite : IEstudianteRepositorio
{
    private readonly BaseDatos _db;

    public EstudianteRepositorioSqlite(BaseDatos db)
    {
        _db = db;
    }

    public List<Estudiante> ObtenerTodos()
    {
        return Ordenar(_db.Conexion.Table<EstudianteDb>().ToList());
    }

    public List<Estudiante> ObtenerDeComision(string comision)
    {
        if (_db.IdDeComision(comision) is not int id)
        {
            return new List<Estudiante>();
        }

        return Ordenar(_db.Conexion.Table<EstudianteDb>()
            .Where(e => e.ComisionId == id)
            .ToList());
    }

    public Estudiante? Buscar(string legajo)
    {
        EstudianteDb? fila = _db.Conexion.Table<EstudianteDb>()
            .FirstOrDefault(e => e.Legajo == legajo);

        return fila is null ? null : _db.AEstudiante(fila);
    }

    public void Agregar(Estudiante estudiante)
    {
        _db.Conexion.Insert(Fila(estudiante));
    }

    public void Actualizar(Estudiante estudiante)
    {
        _db.Conexion.Update(Fila(estudiante));
    }

    public void Eliminar(string legajo)
    {
        _db.Conexion.RunInTransaction(() =>
        {
            _db.Conexion.Table<ContactoDb>()
                .Delete(c => c.LegajoEstudiante == legajo);

            _db.Conexion.Table<EntregaDb>()
                .Delete(e => e.LegajoEstudiante == legajo);

            _db.Conexion.Table<AsistenciaDb>()
                .Delete(a => a.LegajoEstudiante == legajo);

            _db.Conexion.Table<EstudianteDb>()
                .Delete(e => e.Legajo == legajo);
        });
    }

    // Por apellido y nombre, respetando los acentos
    private List<Estudiante> Ordenar(List<EstudianteDb> filas)
    {
        Dictionary<int, string> comisiones = _db.NombresPorId();

        return filas
            .OrderBy(e => e.Apellido)
            .ThenBy(e => e.Nombre)
            .Select(e => e.AEstudiante(comisiones[e.ComisionId]))
            .ToList();
    }

    private EstudianteDb Fila(Estudiante estudiante)
    {
        return new EstudianteDb
        {
            Legajo = estudiante.Legajo,
            Nombre = estudiante.Nombre,
            Apellido = estudiante.Apellido,
            ComisionId = _db.IdObligatorio(estudiante.Comision)
        };
    }
}
