using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Datos;

public sealed class ComisionRepositorioSqlite : IComisionRepositorio
{
    private readonly BaseDatos _db;

    public ComisionRepositorioSqlite(BaseDatos db)
    {
        _db = db;
    }

    public List<string> ObtenerTodas()
    {
        return _db.Conexion.Table<ComisionDb>()
            .ToList()
            .Select(c => c.Nombre)
            .OrderBy(nombre => nombre)
            .ToList();
    }

    public void Agregar(string nombre)
    {
        if (_db.IdDeComision(nombre) is null)
        {
            _db.Conexion.Insert(new ComisionDb { Nombre = nombre });
        }
    }

    // Alumnos, calendario y umbrales apuntan a la comisión por su
    // identificador: alcanza con cambiar el nombre en un solo lugar
    public void Renombrar(string actual, string nuevo)
    {
        _db.Conexion.Execute(
            "UPDATE Comisiones SET Nombre = ? WHERE Nombre = ?",
            nuevo,
            actual);
    }

    public void Eliminar(string nombre)
    {
        if (_db.IdDeComision(nombre) is not int id)
        {
            return;
        }

        _db.Conexion.RunInTransaction(() =>
        {
            _db.Conexion.Table<ActividadCalendarioDb>()
                .Delete(a => a.ComisionId == id);

            _db.Conexion.Table<UmbralesComisionDb>()
                .Delete(u => u.ComisionId == id);

            _db.Conexion.Table<ComisionDb>()
                .Delete(c => c.Id == id);
        });
    }

    public UmbralesRiesgo? ObtenerUmbrales(string comision)
    {
        if (_db.IdDeComision(comision) is not int id)
        {
            return null;
        }

        return _db.Conexion.Table<UmbralesComisionDb>()
            .FirstOrDefault(u => u.ComisionId == id)
            ?.AUmbrales();
    }

    public void GuardarUmbrales(string comision, UmbralesRiesgo umbrales)
    {
        _db.Conexion.InsertOrReplace(new UmbralesComisionDb
        {
            ComisionId = _db.IdObligatorio(comision),
            PesoAsistencia = (double)umbrales.PesoAsistencia,
            RiesgoFueraDeTermino = (double)umbrales.RiesgoFueraDeTermino,
            RiesgoNoEntregado = (double)umbrales.RiesgoNoEntregado,
            RiesgoMedio = (double)umbrales.RiesgoMedio,
            RiesgoAlto = (double)umbrales.RiesgoAlto
        });
    }
}
