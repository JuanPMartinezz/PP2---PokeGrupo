using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Services;

/// <summary>
/// Calendario de clases y trabajos de cada comisión (H2).
/// </summary>
public class CalendarioService
{
    public const string DescripcionClase = "Clase";

    private readonly ICalendarioRepositorio _calendario;

    private readonly IEntregaRepositorio _entregas;

    public CalendarioService(
        ICalendarioRepositorio calendario,
        IEntregaRepositorio entregas)
    {
        _calendario = calendario;
        _entregas = entregas;
    }

    // Del más reciente al más viejo
    public List<ActividadCalendario> DeComision(string comision)
    {
        return _calendario.ObtenerDeComision(comision)
            .OrderByDescending(a => a.Fecha)
            .ToList();
    }

    public List<ActividadCalendario> TrabajosDe(string comision)
    {
        return DeComision(comision)
            .Where(a => a.Tipo == TipoActividad.Trabajo)
            .ToList();
    }

    // El trabajo con ese nombre; si todavía no existe, se crea
    public Trabajo ObtenerTrabajo(string nombre)
    {
        return _entregas.ObtenerOCrearTrabajo(nombre);
    }

    // Devuelve el motivo si no se pudo agregar la actividad.
    // Las clases no necesitan nombre; los trabajos sí.
    public string? Agregar(
        string comision,
        TipoActividad tipo,
        DateTime fecha,
        string descripcion)
    {
        string texto = tipo == TipoActividad.Clase
            ? DescripcionClase
            : descripcion.Trim();

        string? error = Validar(comision, tipo, fecha, texto);

        if (error is null)
        {
            Guardar(comision, tipo, fecha, texto);
        }

        return error;
    }

    private string? Validar(
        string comision,
        TipoActividad tipo,
        DateTime fecha,
        string texto)
    {
        string? invalido = tipo == TipoActividad.Trabajo
            ? Validaciones.NombreTrabajo(texto)
            : null;

        if (invalido is not null)
        {
            return invalido;
        }

        if (!EstaRepetida(comision, tipo, fecha, texto))
        {
            return null;
        }

        return tipo == TipoActividad.Clase
            ? "Ya hay una clase cargada para esa fecha"
            : "Ya hay un trabajo con ese nombre en esta comisión";
    }

    // Una clase se repite si es el mismo día; un trabajo, si tiene el
    // mismo nombre, sea cual sea la fecha
    private bool EstaRepetida(
        string comision,
        TipoActividad tipo,
        DateTime fecha,
        string texto)
    {
        return _calendario.ObtenerDeComision(comision).Any(a =>
            a.Tipo == tipo &&
            a.Descripcion.Equals(texto, StringComparison.OrdinalIgnoreCase) &&
            (tipo == TipoActividad.Trabajo || a.Fecha.Date == fecha.Date));
    }

    private void Guardar(
        string comision,
        TipoActividad tipo,
        DateTime fecha,
        string texto)
    {
        if (tipo == TipoActividad.Trabajo)
        {
            _entregas.ObtenerOCrearTrabajo(texto);
        }

        _calendario.Agregar(new ActividadCalendario
        {
            Comision = comision,
            Cuatrimestre = Cuatrimestres.De(fecha),
            Fecha = fecha.Date,
            Tipo = tipo,
            Descripcion = texto
        });
    }
}
