using SQLite;
using Microsoft.Maui.Storage;
using RegistroAsistencia.Models;

namespace RegistroAsistencia.Services;

/// Acceso a SQLite. La base se guarda en el directorio privado de la aplicación.
public sealed class BaseDatosService
{
    private readonly SQLiteConnection _conexion;
    private const string NombreArchivo = "registro_asistencia.db3";

    public BaseDatosService()
    {
        string ruta = Path.Combine(FileSystem.AppDataDirectory, NombreArchivo);
        _conexion = new SQLiteConnection(ruta);
        _conexion.CreateTable<EstudianteDb>();
        _conexion.CreateTable<AsistenciaDb>();
    }

    public List<Estudiante> ObtenerEstudiantes()
    {
        return _conexion.Table<EstudianteDb>().OrderBy(e => e.Apellido).ThenBy(e => e.Nombre).Select(e => new Estudiante
            {
                Legajo = e.Legajo,
                Nombre = e.Nombre,
                Apellido = e.Apellido
            })
            .ToList();
    }

    public List<Asistencia> ObtenerAsistencias(IReadOnlyCollection<Estudiante> estudiantes)
    {
        var porLegajo = estudiantes.ToDictionary(e => e.Legajo);
        var resultado = new List<Asistencia>();

        foreach (var fila in _conexion.Table<AsistenciaDb>().OrderBy(a => a.Fecha).ToList())
        {
            if (!porLegajo.TryGetValue(fila.LegajoEstudiante, out var estudiante))
                continue;

            resultado.Add(new Asistencia
            {
                Estudiante = estudiante,
                Clase = new Clase { Fecha = DateTime.Parse(fila.Fecha) },
                Estado = (EstadoAsistencia)fila.Estado
            });
        }

        return resultado;
    }

    public bool ExisteEstudiante(string legajo) =>
        _conexion.Table<EstudianteDb>().Any(e => e.Legajo == legajo);

    public void GuardarEstudiante(Estudiante estudiante)
    {
        _conexion.Insert(new EstudianteDb
        {
            Legajo = estudiante.Legajo.Trim(),
            Nombre = estudiante.Nombre.Trim(),
            Apellido = estudiante.Apellido.Trim()
        });
    }

    public bool ExisteAsistencia(string legajo, DateTime fecha) =>
        _conexion.Table<AsistenciaDb>().Any(a => a.LegajoEstudiante == legajo && a.Fecha == fecha.Date.ToString("yyyy-MM-dd"));

    public void GuardarAsistencia(Asistencia asistencia)
    {
        _conexion.Insert(new AsistenciaDb
        {
            LegajoEstudiante = asistencia.Estudiante.Legajo,
            Fecha = asistencia.Clase.Fecha.Date.ToString("yyyy-MM-dd"),
            Estado = (int)asistencia.Estado
        });
    }

    public void BorrarTodosLosRegistros()
    {
        _conexion.DeleteAll<AsistenciaDb>();
        _conexion.DeleteAll<EstudianteDb>();
    }

    private static AsistenciaDb CrearAsistencia(string legajo, DateTime fecha, EstadoAsistencia estado) => new()
        {
            LegajoEstudiante = legajo,
            Fecha = fecha.Date.ToString("yyyy-MM-dd"),
            Estado = (int)estado
        };

    [Table("Estudiantes")]
    public class EstudianteDb
    {
        [PrimaryKey]
        public string Legajo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
    }

    [Table("Asistencias")]
    public class AsistenciaDb
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string LegajoEstudiante { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public int Estado { get; set; }
    }
}
