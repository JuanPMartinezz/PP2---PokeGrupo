using RegistroAsistencia.Models;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.Services;

/// <summary>
/// Mantiene las colecciones observables que usa la interfaz y las sincroniza con SQLite.
/// </summary>
public static class DatosSistema
{
    private static BaseDatosService? _baseDatos;
    public static bool EstaInicializado => _baseDatos is not null;

    public static ObservableCollection<Estudiante> Estudiantes { get; } = new();
    public static ObservableCollection<Asistencia> Asistencias { get; } = new();

    public static void Inicializar()
    {
        if (_baseDatos is not null)
            return;

        _baseDatos = new BaseDatosService();
        Estudiantes.Clear();
        foreach (var estudiante in _baseDatos.ObtenerEstudiantes())
            Estudiantes.Add(estudiante);

        Asistencias.Clear();
        foreach (var asistencia in _baseDatos.ObtenerAsistencias(Estudiantes))
            Asistencias.Add(asistencia);
    }

    public static bool ExisteEstudiante(string legajo)
    {
        if (_baseDatos is not null)
        return _baseDatos.ExisteEstudiante(legajo.Trim());
        return Estudiantes.Any(e => e.Legajo == legajo.Trim());
    }

    public static void AgregarEstudiante(Estudiante estudiante)
    {
        if (_baseDatos is not null)
        _baseDatos.GuardarEstudiante(estudiante);
        Estudiantes.Add(estudiante);
    }

    public static bool ExisteAsistencia(string legajo, DateTime fecha)
    {
        if (_baseDatos is not null)
        return _baseDatos.ExisteAsistencia(legajo, fecha);
        return Asistencias.Any(a => a.Estudiante.Legajo == legajo && a.Clase.Fecha.Date == fecha.Date);
    }

    public static void AgregarAsistencia(Asistencia asistencia)
    {
        if (_baseDatos is not null) _baseDatos.GuardarAsistencia(asistencia);
        Asistencias.Add(asistencia);
    }

    public static Asistencia ActualizarEstadoAsistencia(
    Asistencia asistencia,
    EstadoAsistencia nuevoEstado)
    {
        int indice = Asistencias.IndexOf(asistencia);

        if (indice < 0)
        {
            throw new InvalidOperationException(
                "No se encontró la asistencia seleccionada.");
        }

        if (_baseDatos is not null)
        {
            _baseDatos.ActualizarAsistencia(
                asistencia.Estudiante.Legajo,
                asistencia.Clase.Fecha,
                nuevoEstado);
        }

        var asistenciaActualizada = new Asistencia
        {
            Estudiante = asistencia.Estudiante,

            Clase = asistencia.Clase,

            Estado = nuevoEstado
        };

        /*
         * Reemplazamos el registro en la misma posición.
         * Esto actualiza la interfaz sin agregar otra asistencia.
         */
        Asistencias[indice] = asistenciaActualizada;

        return asistenciaActualizada;
    }
    public static void BorrarTodosLosRegistros()
    {
        _baseDatos!.BorrarTodosLosRegistros();
        Asistencias.Clear();
        Estudiantes.Clear();
    }
}
