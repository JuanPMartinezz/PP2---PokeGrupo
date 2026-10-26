using SQLite;
using System.Globalization;
using RegistroAsistencia.Models;

namespace RegistroAsistencia.Datos;

// Filas de las tablas de SQLite

[Table("Estudiantes")]
public class EstudianteDb
{
    [PrimaryKey]
    public string Legajo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    [Indexed]
    public int ComisionId { get; set; }
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

    public string Nombre { get; set; } = string.Empty;
}

[Table("Entregas")]
public class EntregaDb
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string LegajoEstudiante { get; set; } = string.Empty;

    public int TrabajoId { get; set; }

    public int Estado { get; set; }
}

[Table("ActividadesCalendario")]
public class ActividadCalendarioDb
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int ComisionId { get; set; }

    public string Cuatrimestre { get; set; } = string.Empty;

    public string Fecha { get; set; } = string.Empty;

    public int Tipo { get; set; }

    public string Descripcion { get; set; } = string.Empty;
}

[Table("Contactos")]
public class ContactoDb
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string LegajoEstudiante { get; set; } = string.Empty;

    public string Fecha { get; set; } = string.Empty;

    // Nulo en los contactos anteriores a este dato
    public bool? Atendio { get; set; }

    public string Resultado { get; set; } = string.Empty;
}

[Table("Comisiones")]
public class ComisionDb
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string Nombre { get; set; } = string.Empty;
}

[Table("UmbralesComision")]
public class UmbralesComisionDb
{
    [PrimaryKey]
    public int ComisionId { get; set; }

    public double PesoAsistencia { get; set; }

    public double RiesgoFueraDeTermino { get; set; }

    public double RiesgoNoEntregado { get; set; }

    public double RiesgoMedio { get; set; }

    public double RiesgoAlto { get; set; }
}

/// <summary>
/// Conversión entre las filas de la base y los objetos del dominio.
/// </summary>
public static class Conversiones
{
    private const string FormatoFecha = "yyyy-MM-dd";

    public static string GuardarFecha(DateTime fecha)
    {
        return fecha.Date.ToString(FormatoFecha, CultureInfo.InvariantCulture);
    }

    public static DateTime LeerFecha(string texto)
    {
        return DateTime.ParseExact(
            texto,
            FormatoFecha,
            CultureInfo.InvariantCulture);
    }

    public static Estudiante AEstudiante(
        this EstudianteDb fila,
        string comision)
    {
        return new Estudiante
        {
            Legajo = fila.Legajo,
            Nombre = fila.Nombre,
            Apellido = fila.Apellido,
            Comision = comision
        };
    }

    public static UmbralesRiesgo AUmbrales(this UmbralesComisionDb fila)
    {
        return new UmbralesRiesgo
        {
            PesoAsistencia = (decimal)fila.PesoAsistencia,
            RiesgoFueraDeTermino = (decimal)fila.RiesgoFueraDeTermino,
            RiesgoNoEntregado = (decimal)fila.RiesgoNoEntregado,
            RiesgoMedio = (decimal)fila.RiesgoMedio,
            RiesgoAlto = (decimal)fila.RiesgoAlto
        };
    }

    public static ActividadCalendario ADominio(
        this ActividadCalendarioDb fila,
        string comision)
    {
        return new ActividadCalendario
        {
            Comision = comision,
            Cuatrimestre = fila.Cuatrimestre,
            Fecha = LeerFecha(fila.Fecha),
            Tipo = (TipoActividad)fila.Tipo,
            Descripcion = fila.Descripcion
        };
    }
}
