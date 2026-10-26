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
        _conexion.CreateTable<ComisionDb>();
        _conexion.CreateTable<UmbralesComisionDb>();
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
             ? DatosSistema.ComisionesIniciales[0]
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
                ? DatosSistema.ComisionesIniciales[0]
                : estudiante.Comision.Trim()
        });
    }

    // Guarda los cambios de nombre, apellido y comisión
    public void ActualizarEstudiante(Estudiante estudiante)
    {
        _conexion.Update(new EstudianteDb
        {
            Legajo = estudiante.Legajo,
            Nombre = estudiante.Nombre.Trim(),
            Apellido = estudiante.Apellido.Trim(),
            Comision = estudiante.Comision.Trim()
        });
    }

    // Comisiones

    public List<string> ObtenerComisiones()
    {
        return _conexion.Table<ComisionDb>()
            .ToList()
            .Select(c => c.Nombre)
            .OrderBy(nombre => nombre)
            .ToList();
    }

    public void GuardarComision(string nombre)
    {
        _conexion.InsertOrReplace(new ComisionDb { Nombre = nombre });
    }

    // Cambia el nombre en la comisión y en todo lo que la usa
    public void RenombrarComision(string actual, string nuevo)
    {
        _conexion.RunInTransaction(() =>
        {
            _conexion.Execute(
                "UPDATE Estudiantes SET Comision = ? WHERE Comision = ?",
                nuevo, actual);

            _conexion.Execute(
                "UPDATE ActividadesCalendario SET Comision = ? WHERE Comision = ?",
                nuevo, actual);

            _conexion.Execute(
                "UPDATE UmbralesComision SET Comision = ? WHERE Comision = ?",
                nuevo, actual);

            _conexion.Execute(
                "UPDATE Comisiones SET Nombre = ? WHERE Nombre = ?",
                nuevo, actual);
        });
    }

    // Elimina la comisión, su calendario y sus umbrales
    public void EliminarComision(string nombre)
    {
        _conexion.RunInTransaction(() =>
        {
            _conexion.Table<ActividadCalendarioDb>()
                .Delete(a => a.Comision == nombre);

            _conexion.Table<UmbralesComisionDb>()
                .Delete(u => u.Comision == nombre);

            _conexion.Table<ComisionDb>()
                .Delete(c => c.Nombre == nombre);
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

                // Los contactos anteriores no guardaban este dato:
                // se los considera atendidos
                Atendio = fila.Atendio ?? true,

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
            Atendio = contacto.Atendio,
            Resultado = contacto.Resultado
        });
    }

    // Métodos para gestionar los umbrales de riesgo (H7), uno por comisión

    public Dictionary<string, UmbralesRiesgo> ObtenerUmbrales()
    {
        var resultado = new Dictionary<string, UmbralesRiesgo>();

        foreach (UmbralesComisionDb fila
                 in _conexion.Table<UmbralesComisionDb>().ToList())
        {
            resultado[fila.Comision] = new UmbralesRiesgo
            {
                PesoAsistencia = (decimal)fila.PesoAsistencia,
                RiesgoFueraDeTermino = (decimal)fila.RiesgoFueraDeTermino,
                RiesgoNoEntregado = (decimal)fila.RiesgoNoEntregado,
                RiesgoMedio = (decimal)fila.RiesgoMedio,
                RiesgoAlto = (decimal)fila.RiesgoAlto
            };
        }

        return resultado;
    }

    public void GuardarUmbrales(string comision, UmbralesRiesgo umbrales)
    {
        _conexion.InsertOrReplace(new UmbralesComisionDb
        {
            Comision = comision,
            PesoAsistencia = (double)umbrales.PesoAsistencia,
            RiesgoFueraDeTermino = (double)umbrales.RiesgoFueraDeTermino,
            RiesgoNoEntregado = (double)umbrales.RiesgoNoEntregado,
            RiesgoMedio = (double)umbrales.RiesgoMedio,
            RiesgoAlto = (double)umbrales.RiesgoAlto
        });
    }

    // Elimina un estudiante junto con sus asistencias, entregas y contactos
    public void EliminarEstudiante(string legajo)
    {
        _conexion.RunInTransaction(() =>
        {
            _conexion.Table<ContactoDb>()
                .Delete(c => c.LegajoEstudiante == legajo);

            _conexion.Table<EntregaDb>()
                .Delete(e => e.LegajoEstudiante == legajo);

            _conexion.Table<AsistenciaDb>()
                .Delete(a => a.LegajoEstudiante == legajo);

            _conexion.Table<EstudianteDb>()
                .Delete(e => e.Legajo == legajo);
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
        _conexion.DeleteAll<UmbralesComisionDb>();
        _conexion.DeleteAll<ComisionDb>();
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

        // Nulo en los contactos anteriores a este dato
        public bool? Atendio { get; set; }

        public string Resultado { get; set; }
            = string.Empty;
    }

    [Table("Comisiones")]
    public class ComisionDb
    {
        [PrimaryKey]
        public string Nombre { get; set; }
            = string.Empty;
    }

    [Table("UmbralesComision")]
    public class UmbralesComisionDb
    {
        [PrimaryKey]
        public string Comision { get; set; }
            = string.Empty;

        public double PesoAsistencia { get; set; }

        public double RiesgoFueraDeTermino { get; set; }

        public double RiesgoNoEntregado { get; set; }

        public double RiesgoMedio { get; set; }

        public double RiesgoAlto { get; set; }
    }

}
