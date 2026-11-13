using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Datos;

public sealed class UnidadDeTrabajoSqlite : IUnidadDeTrabajo
{
    private readonly BaseDatos _db;

    public UnidadDeTrabajoSqlite(BaseDatos db)
    {
        _db = db;
    }

    public T EnTransaccion<T>(Func<T> operacion)
    {
        T resultado = default!;

        _db.Conexion.RunInTransaction(() => resultado = operacion());

        return resultado;
    }
}
