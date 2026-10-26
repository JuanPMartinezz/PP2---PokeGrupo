using SQLite;
using RegistroAsistencia.Models;

namespace RegistroAsistencia.Datos;

/// <summary>
/// Conexión con la base SQLite. Recibe la ruta del archivo, así no
/// depende de dónde se ejecute la aplicación.
///
/// La comisión se guarda una sola vez, en la tabla Comisiones. Los
/// alumnos, el calendario y los umbrales la referencian por su
/// identificador; el nombre se busca acá y no se repite en cada tabla.
/// </summary>
public sealed class BaseDatos
{
    public SQLiteConnection Conexion { get; }

    public BaseDatos(string ruta)
    {
        Conexion = new SQLiteConnection(ruta);

        Conexion.CreateTable<ComisionDb>();
        Conexion.CreateTable<EstudianteDb>();
        Conexion.CreateTable<AsistenciaDb>();
        Conexion.CreateTable<TrabajoDb>();
        Conexion.CreateTable<EntregaDb>();
        Conexion.CreateTable<ActividadCalendarioDb>();
        Conexion.CreateTable<ContactoDb>();
        Conexion.CreateTable<UmbralesComisionDb>();
    }

    // Nulo si no existe una comisión con ese nombre
    public int? IdDeComision(string nombre)
    {
        return Conexion.Table<ComisionDb>()
            .FirstOrDefault(c => c.Nombre == nombre)
            ?.Id;
    }

    public int IdObligatorio(string nombre)
    {
        return IdDeComision(nombre) ??
               throw new InvalidOperationException(
                   $"No existe la comisión {nombre}");
    }

    public Dictionary<int, string> NombresPorId()
    {
        return Conexion.Table<ComisionDb>()
            .ToList()
            .ToDictionary(c => c.Id, c => c.Nombre);
    }

    public Estudiante AEstudiante(EstudianteDb fila)
    {
        string nombre = Conexion.Table<ComisionDb>()
            .First(c => c.Id == fila.ComisionId)
            .Nombre;

        return fila.AEstudiante(nombre);
    }
}
