using RegistroAsistencia.Models;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.Services;

/// <summary>
/// Mantiene las colecciones observables utilizadas por la interfaz
/// y las sincroniza con SQLite.
/// </summary>
public static class DatosSistema
{
    private static BaseDatosService? _baseDatos;

    public static bool EstaInicializado =>
        _baseDatos is not null;

    public static ObservableCollection<Estudiante> Estudiantes { get; }
        = new();

    public static ObservableCollection<Asistencia> Asistencias { get; }
        = new();

    public static ObservableCollection<Trabajo> Trabajos { get; }
        = new();

    public static ObservableCollection<Entrega> Entregas { get; }
        = new();

    public static ObservableCollection<ActividadCalendario> Calendario { get; }
        = new();

    public static ObservableCollection<ContactoEstudiante> Contactos { get; }
        = new();

    public static UmbralesRiesgo Umbrales { get; private set; }
        = new();

    public static void Inicializar()
    {
        if (_baseDatos is not null)
        {
            return;
        }

        _baseDatos = new BaseDatosService();

        // Cargar estudiantes

        Estudiantes.Clear();

        foreach (Estudiante estudiante
                 in _baseDatos.ObtenerEstudiantes())
        {
            Estudiantes.Add(estudiante);
        }

        // Cargar asistencias

        Asistencias.Clear();

        foreach (Asistencia asistencia
                 in _baseDatos.ObtenerAsistencias(Estudiantes))
        {
            Asistencias.Add(asistencia);
        }

        // Cargar trabajos

        Trabajos.Clear();

        foreach (Trabajo trabajo
                 in _baseDatos.ObtenerTrabajos())
        {
            Trabajos.Add(trabajo);
        }

        // Cargar entregas

        Entregas.Clear();

        foreach (Entrega entrega
                 in _baseDatos.ObtenerEntregas(
                     Estudiantes,
                     Trabajos))
        {
            Entregas.Add(entrega);
        }

        // Cargar calendario

        Calendario.Clear();

        foreach (ActividadCalendario actividad
                 in _baseDatos.ObtenerActividadesCalendario())
        {
            Calendario.Add(actividad);
        }

        // Cargar contactos

        Contactos.Clear();

        foreach (ContactoEstudiante contacto
                 in _baseDatos.ObtenerContactos(
                     Estudiantes))
        {
            Contactos.Add(contacto);
        }

        // Cargar umbrales de riesgo

        Umbrales = _baseDatos.ObtenerUmbrales();
    }

    // Estudiantes

    public static bool ExisteEstudiante(string legajo)
    {
        string legajoLimpio = legajo.Trim();

        if (_baseDatos is not null)
        {
            return _baseDatos.ExisteEstudiante(
                legajoLimpio);
        }

        return Estudiantes.Any(e =>
            e.Legajo == legajoLimpio);
    }

    public static void AgregarEstudiante(
        Estudiante estudiante)
    {
        if (_baseDatos is not null)
        {
            _baseDatos.GuardarEstudiante(estudiante);
        }

        Estudiantes.Add(estudiante);
    }

    // Asistencias

    public static bool ExisteAsistencia(
        string legajo,
        DateTime fecha)
    {
        if (_baseDatos is not null)
        {
            return _baseDatos.ExisteAsistencia(
                legajo,
                fecha);
        }

        return Asistencias.Any(a =>
            a.Estudiante.Legajo == legajo &&
            a.Clase.Fecha.Date == fecha.Date);
    }

    public static void AgregarAsistencia(
        Asistencia asistencia)
    {
        if (_baseDatos is not null)
        {
            _baseDatos.GuardarAsistencia(asistencia);
        }

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

        Asistencias[indice] = asistenciaActualizada;

        return asistenciaActualizada;
    }

    // Trabajos

    public static void AgregarTrabajo(
        Trabajo trabajo)
    {
        if (string.IsNullOrWhiteSpace(trabajo.Nombre))
        {
            throw new InvalidOperationException(
                "El trabajo debe tener un nombre.");
        }

        if (_baseDatos is not null)
        {
            _baseDatos.GuardarTrabajo(trabajo);
        }
        else if (trabajo.Id == 0)
        {
            /*
             * Identificador temporal utilizado por las pruebas
             * cuando SQLite no está inicializado.
             */
            trabajo.Id = Trabajos.Count == 0
                ? 1
                : Trabajos.Max(t => t.Id) + 1;
        }

        Trabajos.Add(trabajo);
    }

    // Entregas

    public static bool ExisteEntrega(
        string legajo,
        int trabajoId)
    {
        if (_baseDatos is not null)
        {
            return _baseDatos.ExisteEntrega(
                legajo,
                trabajoId);
        }

        return Entregas.Any(e =>
            e.Estudiante.Legajo == legajo &&
            e.Trabajo.Id == trabajoId);
    }

    public static void AgregarEntrega(
        Entrega entrega)
    {
        if (ExisteEntrega(
                entrega.Estudiante.Legajo,
                entrega.Trabajo.Id))
        {
            throw new InvalidOperationException(
                "Ya existe una entrega registrada para ese estudiante y trabajo.");
        }

        if (_baseDatos is not null)
        {
            _baseDatos.GuardarEntrega(entrega);
        }

        Entregas.Add(entrega);
    }

    public static Entrega ActualizarEstadoEntrega(
        Entrega entrega,
        EstadoEntrega nuevoEstado)
    {
        int indice = Entregas.IndexOf(entrega);

        if (indice < 0)
        {
            throw new InvalidOperationException(
                "No se encontró la entrega seleccionada.");
        }

        if (_baseDatos is not null)
        {
            _baseDatos.ActualizarEntrega(
                entrega.Estudiante.Legajo,
                entrega.Trabajo.Id,
                nuevoEstado);
        }

        var entregaActualizada = new Entrega
        {
            Estudiante = entrega.Estudiante,
            Trabajo = entrega.Trabajo,
            Estado = nuevoEstado
        };

        Entregas[indice] = entregaActualizada;

        return entregaActualizada;
    }

    // Calendario (H2)

    public static bool ExisteActividadCalendario(
        string comision,
        string cuatrimestre,
        DateTime fecha,
        TipoActividad tipo,
        string descripcion)
    {
        return Calendario.Any(a =>
            a.Comision == comision &&
            a.Cuatrimestre == cuatrimestre &&
            a.Fecha.Date == fecha.Date &&
            a.Tipo == tipo &&
            a.Descripcion.Equals(
                descripcion.Trim(),
                StringComparison.OrdinalIgnoreCase));
    }

    public static void AgregarActividadCalendario(
        ActividadCalendario actividad)
    {
        if (string.IsNullOrWhiteSpace(actividad.Comision))
        {
            throw new InvalidOperationException(
                "La actividad debe pertenecer a una comisión.");
        }

        if (string.IsNullOrWhiteSpace(actividad.Cuatrimestre))
        {
            throw new InvalidOperationException(
                "La actividad debe pertenecer a un cuatrimestre.");
        }

        if (string.IsNullOrWhiteSpace(actividad.Descripcion))
        {
            throw new InvalidOperationException(
                "La actividad debe tener una descripción.");
        }

        actividad.Descripcion =
            actividad.Descripcion.Trim();

        if (ExisteActividadCalendario(
                actividad.Comision,
                actividad.Cuatrimestre,
                actividad.Fecha,
                actividad.Tipo,
                actividad.Descripcion))
        {
            throw new InvalidOperationException(
                "Ya existe esa actividad en el calendario para esa fecha.");
        }

        if (_baseDatos is not null)
        {
            _baseDatos.GuardarActividadCalendario(actividad);
        }

        Calendario.Add(actividad);
    }

    // Contactos (H10)

    public static void AgregarContacto(
        ContactoEstudiante contacto)
    {
        if (string.IsNullOrWhiteSpace(
                contacto.Estudiante.Legajo))
        {
            throw new InvalidOperationException(
                "El contacto debe estar asociado a un estudiante.");
        }

        if (string.IsNullOrWhiteSpace(contacto.Resultado))
        {
            throw new InvalidOperationException(
                "El contacto debe tener un resultado.");
        }

        contacto.Resultado =
            contacto.Resultado.Trim();

        if (_baseDatos is not null)
        {
            _baseDatos.GuardarContacto(contacto);
        }

        Contactos.Add(contacto);
    }

    // Umbrales de riesgo (H7)

    public static void GuardarUmbrales(
        UmbralesRiesgo umbrales)
    {
        string? error = umbrales.Validar();

        if (error is not null)
        {
            throw new InvalidOperationException(error);
        }

        /*
         * Se guarda una copia: si alguien modifica después el objeto
         * recibido, la configuración no cambia sin pasar por la validación.
         */
        var copia = new UmbralesRiesgo
        {
            PesoAsistencia = umbrales.PesoAsistencia,
            RiesgoFueraDeTermino = umbrales.RiesgoFueraDeTermino,
            RiesgoNoEntregado = umbrales.RiesgoNoEntregado,
            RiesgoMedio = umbrales.RiesgoMedio,
            RiesgoAlto = umbrales.RiesgoAlto
        };

        if (_baseDatos is not null)
        {
            _baseDatos.GuardarUmbrales(copia);
        }

        Umbrales = copia;
    }

    public static void RestablecerUmbrales()
    {
        GuardarUmbrales(new UmbralesRiesgo());
    }

    // Borrado general

    public static void BorrarTodosLosRegistros()
    {
        if (_baseDatos is not null)
        {
            _baseDatos.BorrarTodosLosRegistros();
        }

        Contactos.Clear();
        Calendario.Clear();
        Entregas.Clear();
        Asistencias.Clear();
        Trabajos.Clear();
        Estudiantes.Clear();
    }
}