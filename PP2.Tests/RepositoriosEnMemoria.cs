using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;
using RegistroAsistencia.Services;

namespace PP2.Tests;

/// <summary>
/// Datos de una prueba guardados en listas. Cada prueba arma el suyo,
/// así que no comparten nada entre sí ni necesitan una base de datos.
/// </summary>
public sealed class AlmacenEnMemoria
{
    public List<string> Comisiones { get; } = new();

    public Dictionary<string, UmbralesRiesgo> Umbrales { get; } = new();

    public List<Estudiante> Estudiantes { get; } = new();

    public List<Asistencia> Asistencias { get; } = new();

    public List<Trabajo> Trabajos { get; } = new();

    public List<Entrega> Entregas { get; } = new();

    public List<ActividadCalendario> Calendario { get; } = new();

    public List<ContactoEstudiante> Contactos { get; } = new();
}

public sealed class ComisionRepositorioEnMemoria : IComisionRepositorio
{
    private readonly AlmacenEnMemoria _datos;

    public ComisionRepositorioEnMemoria(AlmacenEnMemoria datos)
    {
        _datos = datos;
    }

    public List<string> ObtenerTodas() => _datos.Comisiones.OrderBy(c => c).ToList();

    public void Agregar(string nombre)
    {
        if (!_datos.Comisiones.Contains(nombre))
        {
            _datos.Comisiones.Add(nombre);
        }
    }

    public void Renombrar(string actual, string nuevo)
    {
        _datos.Comisiones[_datos.Comisiones.IndexOf(actual)] = nuevo;

        _datos.Estudiantes.Where(e => e.Comision == actual)
            .ToList().ForEach(e => e.Comision = nuevo);

        _datos.Calendario.Where(a => a.Comision == actual)
            .ToList().ForEach(a => a.Comision = nuevo);

        if (_datos.Umbrales.Remove(actual, out UmbralesRiesgo? umbrales))
        {
            _datos.Umbrales[nuevo] = umbrales;
        }
    }

    public void Eliminar(string nombre)
    {
        _datos.Comisiones.Remove(nombre);
        _datos.Umbrales.Remove(nombre);
        _datos.Calendario.RemoveAll(a => a.Comision == nombre);
    }

    public UmbralesRiesgo? ObtenerUmbrales(string comision)
    {
        return _datos.Umbrales.GetValueOrDefault(comision)?.Copiar();
    }

    public void GuardarUmbrales(string comision, UmbralesRiesgo umbrales)
    {
        _datos.Umbrales[comision] = umbrales;
    }
}

public sealed class EstudianteRepositorioEnMemoria : IEstudianteRepositorio
{
    private readonly AlmacenEnMemoria _datos;

    public EstudianteRepositorioEnMemoria(AlmacenEnMemoria datos)
    {
        _datos = datos;
    }

    public List<Estudiante> ObtenerTodos() => _datos.Estudiantes.ToList();

    public List<Estudiante> ObtenerDeComision(string comision)
    {
        return _datos.Estudiantes
            .Where(e => e.Comision == comision)
            .OrderBy(e => e.Apellido)
            .ThenBy(e => e.Nombre)
            .ToList();
    }

    public Estudiante? Buscar(string legajo)
    {
        return _datos.Estudiantes.FirstOrDefault(e => e.Legajo == legajo);
    }

    public void Agregar(Estudiante estudiante) => _datos.Estudiantes.Add(estudiante);

    public void Actualizar(Estudiante estudiante)
    {
        // Es el mismo objeto que está en la lista: ya quedó actualizado
    }

    public void Eliminar(string legajo)
    {
        _datos.Contactos.RemoveAll(c => c.Estudiante.Legajo == legajo);
        _datos.Entregas.RemoveAll(e => e.Estudiante.Legajo == legajo);
        _datos.Asistencias.RemoveAll(a => a.Estudiante.Legajo == legajo);
        _datos.Estudiantes.RemoveAll(e => e.Legajo == legajo);
    }
}

public sealed class AsistenciaRepositorioEnMemoria : IAsistenciaRepositorio
{
    private readonly AlmacenEnMemoria _datos;

    public AsistenciaRepositorioEnMemoria(AlmacenEnMemoria datos)
    {
        _datos = datos;
    }

    public List<Asistencia> ObtenerDeComision(string comision)
    {
        return _datos.Asistencias
            .Where(a => a.Estudiante.Comision == comision)
            .ToList();
    }

    public Asistencia? Buscar(string legajo, DateTime fecha)
    {
        return _datos.Asistencias.FirstOrDefault(a =>
            a.Estudiante.Legajo == legajo &&
            a.Clase.Fecha.Date == fecha.Date);
    }

    public void Agregar(Asistencia asistencia) => _datos.Asistencias.Add(asistencia);

    public void ActualizarEstado(
        string legajo,
        DateTime fecha,
        EstadoAsistencia estado)
    {
        Buscar(legajo, fecha)!.Estado = estado;
    }
}

public sealed class EntregaRepositorioEnMemoria : IEntregaRepositorio
{
    private readonly AlmacenEnMemoria _datos;

    public EntregaRepositorioEnMemoria(AlmacenEnMemoria datos)
    {
        _datos = datos;
    }

    public List<Entrega> ObtenerDeComision(string comision)
    {
        return _datos.Entregas
            .Where(e => e.Estudiante.Comision == comision)
            .ToList();
    }

    public Entrega? Buscar(string legajo, int trabajoId)
    {
        return _datos.Entregas.FirstOrDefault(e =>
            e.Estudiante.Legajo == legajo && e.Trabajo.Id == trabajoId);
    }

    public void Agregar(Entrega entrega) => _datos.Entregas.Add(entrega);

    public void ActualizarEstado(
        string legajo,
        int trabajoId,
        EstadoEntrega estado)
    {
        Buscar(legajo, trabajoId)!.Estado = estado;
    }

    public Trabajo ObtenerOCrearTrabajo(string nombre)
    {
        string limpio = nombre.Trim();

        Trabajo? existente = _datos.Trabajos.FirstOrDefault(t =>
            t.Nombre.Equals(limpio, StringComparison.OrdinalIgnoreCase));

        if (existente is null)
        {
            existente = new Trabajo
            {
                Id = _datos.Trabajos.Count + 1,
                Nombre = limpio
            };

            _datos.Trabajos.Add(existente);
        }

        return existente;
    }
}

public sealed class CalendarioRepositorioEnMemoria : ICalendarioRepositorio
{
    private readonly AlmacenEnMemoria _datos;

    public CalendarioRepositorioEnMemoria(AlmacenEnMemoria datos)
    {
        _datos = datos;
    }

    public List<ActividadCalendario> ObtenerDeComision(string comision)
    {
        return _datos.Calendario.Where(a => a.Comision == comision).ToList();
    }

    public void Agregar(ActividadCalendario actividad) => _datos.Calendario.Add(actividad);
}

public sealed class ContactoRepositorioEnMemoria : IContactoRepositorio
{
    private readonly AlmacenEnMemoria _datos;

    public ContactoRepositorioEnMemoria(AlmacenEnMemoria datos)
    {
        _datos = datos;
    }

    public List<ContactoEstudiante> ObtenerDeComision(string comision)
    {
        return _datos.Contactos
            .Where(c => c.Estudiante.Comision == comision)
            .ToList();
    }

    public void Agregar(ContactoEstudiante contacto) => _datos.Contactos.Add(contacto);
}

public sealed class AdministracionRepositorioEnMemoria : IAdministracionRepositorio
{
    private readonly AlmacenEnMemoria _datos;

    public AdministracionRepositorioEnMemoria(AlmacenEnMemoria datos)
    {
        _datos = datos;
    }

    public void BorrarTodo()
    {
        _datos.Contactos.Clear();
        _datos.Calendario.Clear();
        _datos.Entregas.Clear();
        _datos.Asistencias.Clear();
        _datos.Trabajos.Clear();
        _datos.Estudiantes.Clear();
        _datos.Umbrales.Clear();
        _datos.Comisiones.Clear();
    }
}

/// <summary>
/// Todos los servicios armados sobre repositorios en memoria, con las
/// comisiones iniciales ya cargadas. Es lo mismo que hace la aplicación,
/// pero con listas en lugar de SQLite.
/// </summary>
public sealed class Escenario
{
    public AlmacenEnMemoria Almacen { get; } = new();

    public IEstudianteRepositorio Estudiantes { get; }

    public IAsistenciaRepositorio Asistencias { get; }

    public IEntregaRepositorio Entregas { get; }

    public ICalendarioRepositorio Calendario { get; }

    public IContactoRepositorio Contactos { get; }

    public UmbralesService Umbrales { get; }

    public ComisionService Comisiones { get; }

    public NominaService Nomina { get; }

    public CalendarioService ServicioCalendario { get; }

    public ContactoService ServicioContactos { get; }

    public RegistroService Registro { get; }

    public RiesgoService Riesgo { get; }

    public Escenario()
    {
        var comisiones = new ComisionRepositorioEnMemoria(Almacen);

        Estudiantes = new EstudianteRepositorioEnMemoria(Almacen);
        Asistencias = new AsistenciaRepositorioEnMemoria(Almacen);
        Entregas = new EntregaRepositorioEnMemoria(Almacen);
        Calendario = new CalendarioRepositorioEnMemoria(Almacen);
        Contactos = new ContactoRepositorioEnMemoria(Almacen);

        Umbrales = new UmbralesService(comisiones);
        Comisiones = new ComisionService(
            comisiones,
            Estudiantes,
            new AdministracionRepositorioEnMemoria(Almacen));
        Nomina = new NominaService(Estudiantes, comisiones);
        ServicioCalendario = new CalendarioService(Calendario, Entregas);
        ServicioContactos = new ContactoService(Contactos, Estudiantes);
        Registro = new RegistroService(Asistencias, Entregas);
        Riesgo = new RiesgoService(
            Estudiantes,
            Asistencias,
            Entregas,
            Contactos,
            Umbrales);

        Comisiones.Inicializar();
    }
}
