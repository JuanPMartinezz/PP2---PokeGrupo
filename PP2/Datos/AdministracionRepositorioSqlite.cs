using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Datos;

public sealed class AdministracionRepositorioSqlite : IAdministracionRepositorio
{
    private readonly BaseDatos _db;

    public AdministracionRepositorioSqlite(BaseDatos db)
    {
        _db = db;
    }

    public void BorrarTodo()
    {
        _db.Conexion.RunInTransaction(() =>
        {
            _db.Conexion.DeleteAll<ContactoDb>();
            _db.Conexion.DeleteAll<ActividadCalendarioDb>();
            _db.Conexion.DeleteAll<EntregaDb>();
            _db.Conexion.DeleteAll<AsistenciaDb>();
            _db.Conexion.DeleteAll<TrabajoDb>();
            _db.Conexion.DeleteAll<EstudianteDb>();
            _db.Conexion.DeleteAll<UmbralesComisionDb>();
            _db.Conexion.DeleteAll<ComisionDb>();
        });
    }
}
