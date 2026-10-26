using RegistroAsistencia.Models;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.Services;

/// <summary>
/// Mantiene las colecciones observables utilizadas por la interfaz
/// y las sincroniza con SQLite.
/// </summary>
public static class DatosSistema
{
    // Comisiones con las que arranca la aplicación (y que quedan
    // después de borrar todos los datos)
    public static readonly string[] ComisionesIniciales =
    {
        "Comisión A",
        "Comisión B"
    };

    private static BaseDatosService? _baseDatos;

    // Umbrales de riesgo propios de cada comisión (H7)
    private static readonly Dictionary<string, UmbralesRiesgo> _umbrales
        = new();

    public static bool EstaInicializado =>
        _baseDatos is not null;

    public static ObservableCollection<string> Comisiones { get; }
        = new();

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

    public static void Inicializar()
    {
        if (_baseDatos is not null)
        {
            return;
        }

        _baseDatos = new BaseDatosService();

        CargarEstudiantesYAsistencias();
        CargarTrabajosYEntregas();
        CargarCalendarioYContactos();
        CargarComisiones();
        CargarUmbrales();
    }

    private static void CargarEstudiantesYAsistencias()
    {
        Reemplazar(Estudiantes, _baseDatos!.ObtenerEstudiantes());

        Reemplazar(
            Asistencias,
            _baseDatos.ObtenerAsistencias(Estudiantes));
    }

    private static void CargarTrabajosYEntregas()
    {
        Reemplazar(Trabajos, _baseDatos!.ObtenerTrabajos());

        Reemplazar(
            Entregas,
            _baseDatos.ObtenerEntregas(Estudiantes, Trabajos));
    }

    private static void CargarCalendarioYContactos()
    {
        Reemplazar(
            Calendario,
            _baseDatos!.ObtenerActividadesCalendario());

        Reemplazar(
            Contactos,
            _baseDatos.ObtenerContactos(Estudiantes));
    }

    private static void Reemplazar<T>(
        ObservableCollection<T> coleccion,
        IEnumerable<T> nuevos)
    {
        coleccion.Clear();

        foreach (T item in nuevos)
        {
            coleccion.Add(item);
        }
    }

    // Comisiones

    private static void CargarComisiones()
    {
        Reemplazar(Comisiones, _baseDatos!.ObtenerComisiones());

        IncorporarComisionesConDatos();

        if (Comisiones.Count == 0)
        {
            SembrarComisionesIniciales();
        }
    }

    /*
     * Las bases anteriores no tenían tabla de comisiones: se toman
     * las que ya usan los alumnos y el calendario.
     */
    private static void IncorporarComisionesConDatos()
    {
        var conDatos = Estudiantes.Select(e => e.Comision)
            .Concat(Calendario.Select(a => a.Comision))
            .Distinct()
            .Where(nombre => !Comisiones.Contains(nombre))
            .ToList();

        foreach (string nombre in conDatos)
        {
            AgregarComisionSinValidar(nombre);
        }
    }

    private static void SembrarComisionesIniciales()
    {
        foreach (string nombre in ComisionesIniciales)
        {
            AgregarComisionSinValidar(nombre);
        }
    }

    private static void AgregarComisionSinValidar(string nombre)
    {
        _baseDatos?.GuardarComision(nombre);

        Comisiones.Add(nombre);
    }

    public static bool ExisteComision(string nombre)
    {
        string limpio = nombre.Trim();

        return Comisiones.Any(c =>
            c.Equals(limpio, StringComparison.OrdinalIgnoreCase));
    }

    public static void AgregarComision(string nombre)
    {
        string? error = Validaciones.NombreComision(nombre);

        if (error is not null)
        {
            throw new InvalidOperationException(error);
        }

        AgregarComisionSinValidar(nombre.Trim());
    }

    public static void RenombrarComision(
        string actual,
        string nuevo)
    {
        string limpio = nuevo.Trim();

        ValidarNuevoNombre(actual, limpio);

        _baseDatos?.RenombrarComision(actual, limpio);

        Comisiones[Comisiones.IndexOf(actual)] = limpio;

        foreach (Estudiante alumno in Estudiantes
                     .Where(x => x.Comision == actual))
        {
            alumno.Comision = limpio;
        }

        foreach (ActividadCalendario actividad in Calendario
                     .Where(x => x.Comision == actual))
        {
            actividad.Comision = limpio;
        }

        MoverUmbrales(actual, limpio);
    }

    private static void ValidarNuevoNombre(
        string actual,
        string nuevo)
    {
        bool soloCambiaMayusculas = actual.Equals(
            nuevo,
            StringComparison.OrdinalIgnoreCase);

        string? error = soloCambiaMayusculas
            ? null
            : Validaciones.NombreComision(nuevo);

        if (error is not null)
        {
            throw new InvalidOperationException(error);
        }
    }

    private static void MoverUmbrales(string actual, string nuevo)
    {
        if (_umbrales.Remove(actual, out UmbralesRiesgo? umbrales))
        {
            _umbrales[nuevo] = umbrales;
        }
    }

    /*
     * Borra los alumnos de la comisión con sus asistencias, entregas
     * y contactos. La comisión y su calendario quedan.
     */
    public static void VaciarComision(string nombre)
    {
        var legajos = Estudiantes
            .Where(e => e.Comision == nombre)
            .Select(e => e.Legajo)
            .ToList();

        foreach (string legajo in legajos)
        {
            EliminarEstudiante(legajo);
        }
    }

    // Borra todo lo de la comisión y la comisión misma
    public static void EliminarComision(string nombre)
    {
        VaciarComision(nombre);

        _baseDatos?.EliminarComision(nombre);

        foreach (ActividadCalendario actividad in Calendario
                     .Where(x => x.Comision == nombre)
                     .ToList())
        {
            Calendario.Remove(actividad);
        }

        _umbrales.Remove(nombre);

        Comisiones.Remove(nombre);
    }

    // Estudiantes

    // Alumnos de la comisión, ordenados por apellido y nombre
    public static List<Estudiante> EstudiantesDe(string comision)
    {
        return Estudiantes
            .Where(e => e.Comision == comision)
            .OrderBy(e => e.Apellido)
            .ThenBy(e => e.Nombre)
            .ToList();
    }

    public static Estudiante? BuscarEstudiante(string legajo)
    {
        string limpio = legajo.Trim();

        return Estudiantes.FirstOrDefault(e => e.Legajo == limpio);
    }

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

    // Guarda los cambios hechos a un estudiante que ya existe
    public static void ActualizarEstudiante(
        Estudiante estudiante)
    {
        _baseDatos?.ActualizarEstudiante(estudiante);
    }

    /*
     * Elimina un estudiante cargado por error. También se eliminan sus
     * asistencias, entregas y contactos, para no dejar datos huérfanos.
     */
    public static void EliminarEstudiante(string legajo)
    {
        string legajoLimpio = legajo.Trim();

        _baseDatos?.EliminarEstudiante(legajoLimpio);

        Quitar(Contactos, c => c.Estudiante.Legajo == legajoLimpio);
        Quitar(Entregas, e => e.Estudiante.Legajo == legajoLimpio);
        Quitar(Asistencias, a => a.Estudiante.Legajo == legajoLimpio);
        Quitar(Estudiantes, e => e.Legajo == legajoLimpio);
    }

    private static void Quitar<T>(
        ObservableCollection<T> coleccion,
        Func<T, bool> condicion)
    {
        foreach (T item in coleccion.Where(condicion).ToList())
        {
            coleccion.Remove(item);
        }
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

    public static Asistencia? BuscarAsistencia(
        string legajo,
        DateTime fecha)
    {
        return Asistencias.FirstOrDefault(a =>
            a.Estudiante.Legajo == legajo &&
            a.Clase.Fecha.Date == fecha.Date);
    }

    // Verdadero si ya se tomó asistencia a algún alumno de la comisión
    public static bool HayAsistencia(
        string comision,
        DateTime fecha)
    {
        return Asistencias.Any(a =>
            a.Estudiante.Comision == comision &&
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

        _baseDatos?.ActualizarAsistencia(
            asistencia.Estudiante.Legajo,
            asistencia.Clase.Fecha,
            nuevoEstado);

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

    // Devuelve el trabajo con ese nombre y, si no existe, lo crea
    public static Trabajo ObtenerOCrearTrabajo(string nombre)
    {
        string limpio = nombre.Trim();

        Trabajo? existente = Trabajos.FirstOrDefault(t =>
            t.Nombre.Equals(
                limpio,
                StringComparison.OrdinalIgnoreCase));

        if (existente is not null)
        {
            return existente;
        }

        var nuevo = new Trabajo { Nombre = limpio };

        AgregarTrabajo(nuevo);

        return nuevo;
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

    public static Entrega? BuscarEntrega(
        string legajo,
        int trabajoId)
    {
        return Entregas.FirstOrDefault(e =>
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

        _baseDatos?.GuardarEntrega(entrega);

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

        _baseDatos?.ActualizarEntrega(
            entrega.Estudiante.Legajo,
            entrega.Trabajo.Id,
            nuevoEstado);

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

    // Trabajos del calendario de la comisión, del más reciente al más viejo
    public static List<ActividadCalendario> TrabajosDelCalendario(
        string comision)
    {
        return Calendario
            .Where(a =>
                a.Comision == comision &&
                a.Tipo == TipoActividad.Trabajo)
            .OrderByDescending(a => a.Fecha)
            .ToList();
    }

    public static void AgregarActividadCalendario(
        ActividadCalendario actividad)
    {
        ValidarActividad(actividad);

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

        _baseDatos?.GuardarActividadCalendario(actividad);

        Calendario.Add(actividad);
    }

    private static void ValidarActividad(
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

        _baseDatos?.GuardarContacto(contacto);

        Contactos.Add(contacto);
    }

    // Umbrales de riesgo (H7): cada comisión tiene los suyos

    private static void CargarUmbrales()
    {
        _umbrales.Clear();

        foreach (var (comision, umbrales)
                 in _baseDatos!.ObtenerUmbrales())
        {
            _umbrales[comision] = umbrales;
        }
    }

    // Si la comisión no configuró nada, valen los valores por defecto
    public static UmbralesRiesgo ObtenerUmbrales(string comision)
    {
        return _umbrales.TryGetValue(comision, out UmbralesRiesgo? u)
            ? u
            : new UmbralesRiesgo();
    }

    public static void GuardarUmbrales(
        string comision,
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
        UmbralesRiesgo copia = umbrales.Copiar();

        _baseDatos?.GuardarUmbrales(comision, copia);

        _umbrales[comision] = copia;
    }

    public static void RestablecerUmbrales(string comision)
    {
        GuardarUmbrales(comision, new UmbralesRiesgo());
    }

    /*
     * Deja la aplicación como recién instalada: sin alumnos ni
     * registros, con las comisiones iniciales y sus umbrales por defecto.
     */
    public static void BorrarTodosLosRegistros()
    {
        _baseDatos?.BorrarTodosLosRegistros();

        Contactos.Clear();
        Calendario.Clear();
        Entregas.Clear();
        Asistencias.Clear();
        Trabajos.Clear();
        Estudiantes.Clear();
        Comisiones.Clear();
        _umbrales.Clear();

        SembrarComisionesIniciales();
    }
}
