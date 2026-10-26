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
        _conexion.CreateTable<TrabajoDb>();
        _conexion.CreateTable<EntregaDb>();
        _conexion.CreateTable<ActividadCalendarioDb>();
        _conexion.CreateTable<ContactoDb>();
        _conexion.CreateTable<UmbralesRiesgoDb>();
    }

    public List<Estudiante> ObtenerEstudiantes()
    {
        return _conexion.Table<EstudianteDb>()
     .OrderBy(e => e.Apellido)
     .ThenBy(e => e.Nombre)
     .Select(e => new Estudiante
     {
         Legajo = e.Legajo,
         Nombre = e.Nombre,
         Apellido = e.Apellido,

         Comision = string.IsNullOrWhiteSpace(e.Comision)
             ? "Comisión A"
             : e.Comision
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
            Apellido = estudiante.Apellido.Trim(),

            Comision = string.IsNullOrWhiteSpace(estudiante.Comision)
                ? "Comisión A"
                : estudiante.Comision.Trim()
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
    public void ActualizarAsistencia(
    string legajo,
    DateTime fecha,
    EstadoAsistencia nuevoEstado)
    {
        string fechaGuardada =
            fecha.Date.ToString("yyyy-MM-dd");

        AsistenciaDb? registro =
            _conexion.Table<AsistenciaDb>()
                .FirstOrDefault(a =>
                    a.LegajoEstudiante == legajo &&
                    a.Fecha == fechaGuardada);

        if (registro is null)
        {
            throw new InvalidOperationException(
                "No se encontró la asistencia en la base de datos.");
        }

        registro.Estado = (int)nuevoEstado;

        _conexion.Update(registro);
    }
    // Métodos para gestionar trabajos

    public List<Trabajo> ObtenerTrabajos()
    {
        return _conexion.Table<TrabajoDb>()
            .OrderBy(t => t.Nombre)
            .Select(t => new Trabajo
            {
                Id = t.Id,
                Nombre = t.Nombre
            })
            .ToList();
    }

    public void GuardarTrabajo(Trabajo trabajo)
    {
        var trabajoDb = new TrabajoDb
        {
            Nombre = trabajo.Nombre.Trim()
        };

        _conexion.Insert(trabajoDb);

        trabajo.Id = trabajoDb.Id;
    }
    public List<Entrega> ObtenerEntregas(
    IReadOnlyCollection<Estudiante> estudiantes,
    IReadOnlyCollection<Trabajo> trabajos)
    {
        var estudiantesPorLegajo =
            estudiantes.ToDictionary(e => e.Legajo);

        var trabajosPorId =
            trabajos.ToDictionary(t => t.Id);

        var resultado = new List<Entrega>();

        var filas = _conexion
            .Table<EntregaDb>()
            .ToList();

        foreach (var fila in filas)
        {
            if (!estudiantesPorLegajo.TryGetValue(
                    fila.LegajoEstudiante,
                    out var estudiante))
            {
                continue;
            }

            if (!trabajosPorId.TryGetValue(
                    fila.TrabajoId,
                    out var trabajo))
            {
                continue;
            }

            resultado.Add(new Entrega
            {
                Estudiante = estudiante,
                Trabajo = trabajo,
                Estado = (EstadoEntrega)fila.Estado
            });
        }

        return resultado;
    }

    public bool ExisteEntrega(
        string legajo,
        int trabajoId)
    {
        return _conexion
            .Table<EntregaDb>()
            .Any(e =>
                e.LegajoEstudiante == legajo &&
                e.TrabajoId == trabajoId);
    }

    public void GuardarEntrega(Entrega entrega)
    {
        _conexion.Insert(new EntregaDb
        {
            LegajoEstudiante =
                entrega.Estudiante.Legajo,

            TrabajoId =
                entrega.Trabajo.Id,

            Estado =
                (int)entrega.Estado
        });
    }

    public void ActualizarEntrega(
        string legajo,
        int trabajoId,
        EstadoEntrega nuevoEstado)
    {
        EntregaDb? registro =
            _conexion.Table<EntregaDb>()
                .FirstOrDefault(e =>
                    e.LegajoEstudiante == legajo &&
                    e.TrabajoId == trabajoId);

        if (registro is null)
        {
            throw new InvalidOperationException(
                "No se encontró la entrega en la base de datos.");
        }

        registro.Estado = (int)nuevoEstado;

        _conexion.Update(registro);
    }

    // Métodos para gestionar el calendario (H2)

    public List<ActividadCalendario> ObtenerActividadesCalendario()
    {
        var resultado = new List<ActividadCalendario>();

        foreach (var fila in _conexion
                     .Table<ActividadCalendarioDb>()
                     .OrderBy(a => a.Fecha)
                     .ToList())
        {
            resultado.Add(new ActividadCalendario
            {
                Comision = fila.Comision,
                Cuatrimestre = fila.Cuatrimestre,
                Fecha = DateTime.Parse(fila.Fecha),
                Tipo = (TipoActividad)fila.Tipo,
                Descripcion = fila.Descripcion
            });
        }

        return resultado;
    }

    public void GuardarActividadCalendario(
        ActividadCalendario actividad)
    {
        _conexion.Insert(new ActividadCalendarioDb
        {
            Comision = actividad.Comision,
            Cuatrimestre = actividad.Cuatrimestre,
            Fecha = actividad.Fecha.Date.ToString("yyyy-MM-dd"),
            Tipo = (int)actividad.Tipo,
            Descripcion = actividad.Descripcion
        });
    }

    // Métodos para gestionar contactos con estudiantes (H10)

    public List<ContactoEstudiante> ObtenerContactos(
        IReadOnlyCollection<Estudiante> estudiantes)
    {
        var porLegajo = estudiantes.ToDictionary(e => e.Legajo);
        var resultado = new List<ContactoEstudiante>();

        foreach (var fila in _conexion
                     .Table<ContactoDb>()
                     .OrderBy(c => c.Fecha)
                     .ToList())
        {
            if (!porLegajo.TryGetValue(
                    fila.LegajoEstudiante,
                    out var estudiante))
            {
                continue;
            }

            resultado.Add(new ContactoEstudiante
            {
                Estudiante = estudiante,
                Fecha = DateTime.Parse(fila.Fecha),
                Resultado = fila.Resultado
            });
        }

        return resultado;
    }

    public void GuardarContacto(ContactoEstudiante contacto)
    {
        _conexion.Insert(new ContactoDb
        {
            LegajoEstudiante = contacto.Estudiante.Legajo,
            Fecha = contacto.Fecha.Date.ToString("yyyy-MM-dd"),
            Resultado = contacto.Resultado
        });
    }

    // Métodos para gestionar los umbrales de riesgo (H7)

    public UmbralesRiesgo ObtenerUmbrales()
    {
        UmbralesRiesgoDb? fila =
            _conexion.Table<UmbralesRiesgoDb>()
                .FirstOrDefault(u => u.Id == 1);

        if (fila is null)
        {
            // Todavía no se configuró nada: valores por defecto.
            return new UmbralesRiesgo();
        }

        var umbrales = new UmbralesRiesgo
        {
            PesoAsistencia = (decimal)fila.PesoAsistencia,
            RiesgoFueraDeTermino = (decimal)fila.RiesgoFueraDeTermino,
            RiesgoNoEntregado = (decimal)fila.RiesgoNoEntregado
        };

        // Las columnas de nivel se agregaron después: si la fila es
        // anterior y no las tiene, se mantienen los valores por defecto.
        if (fila.RiesgoMedio.HasValue)
        {
            umbrales.RiesgoMedio = (decimal)fila.RiesgoMedio.Value;
        }

        if (fila.RiesgoAlto.HasValue)
        {
            umbrales.RiesgoAlto = (decimal)fila.RiesgoAlto.Value;
        }

        return umbrales;
    }

    public void GuardarUmbrales(UmbralesRiesgo umbrales)
    {
        // Siempre se usa el Id 1: la tabla guarda una sola configuración.
        _conexion.InsertOrReplace(new UmbralesRiesgoDb
        {
            Id = 1,
            PesoAsistencia = (double)umbrales.PesoAsistencia,
            RiesgoFueraDeTermino = (double)umbrales.RiesgoFueraDeTermino,
            RiesgoNoEntregado = (double)umbrales.RiesgoNoEntregado,
            RiesgoMedio = (double)umbrales.RiesgoMedio,
            RiesgoAlto = (double)umbrales.RiesgoAlto
        });
    }

    public void BorrarTodosLosRegistros()
    {
        _conexion.DeleteAll<ContactoDb>();
        _conexion.DeleteAll<ActividadCalendarioDb>();
        _conexion.DeleteAll<EntregaDb>();
        _conexion.DeleteAll<AsistenciaDb>();
        _conexion.DeleteAll<TrabajoDb>();
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

        public string Comision { get; set; } = "Comisión A";
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
    [Table("Trabajos")]
    public class TrabajoDb
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nombre { get; set; }
            = string.Empty;
    }

    [Table("Entregas")]
    public class EntregaDb
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string LegajoEstudiante { get; set; }
            = string.Empty;

        public int TrabajoId { get; set; }

        public int Estado { get; set; }
    }

    [Table("ActividadesCalendario")]
    public class ActividadCalendarioDb
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Comision { get; set; }
            = string.Empty;

        public string Cuatrimestre { get; set; }
            = string.Empty;

        public string Fecha { get; set; }
            = string.Empty;

        public int Tipo { get; set; }

        public string Descripcion { get; set; }
            = string.Empty;
    }

    [Table("Contactos")]
    public class ContactoDb
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string LegajoEstudiante { get; set; }
            = string.Empty;

        public string Fecha { get; set; }
            = string.Empty;

        public string Resultado { get; set; }
            = string.Empty;
    }

    [Table("UmbralesRiesgo")]
    public class UmbralesRiesgoDb
    {
        [PrimaryKey]
        public int Id { get; set; }

        public double PesoAsistencia { get; set; }

        public double RiesgoFueraDeTermino { get; set; }

        public double RiesgoNoEntregado { get; set; }

        // Aceptan nulos para poder leer filas guardadas antes de agregarlas.
        public double? RiesgoMedio { get; set; }

        public double? RiesgoAlto { get; set; }
    }

}
