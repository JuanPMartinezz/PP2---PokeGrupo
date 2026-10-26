using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Services;

/// <summary>
/// Altas, cambios y bajas de comisiones. Las comisiones son datos: la
/// aplicación arranca con las iniciales y se pueden agregar otras.
/// </summary>
public class ComisionService
{
    // Comisiones con las que arranca la aplicación (y que quedan
    // después de borrar todos los datos)
    public static readonly string[] Iniciales =
    {
        "Comisión A",
        "Comisión B"
    };

    private readonly IComisionRepositorio _comisiones;

    private readonly IEstudianteRepositorio _estudiantes;

    private readonly IAdministracionRepositorio _administracion;

    public ComisionService(
        IComisionRepositorio comisiones,
        IEstudianteRepositorio estudiantes,
        IAdministracionRepositorio administracion)
    {
        _comisiones = comisiones;
        _estudiantes = estudiantes;
        _administracion = administracion;
    }

    public List<string> Todas()
    {
        return _comisiones.ObtenerTodas();
    }

    public bool Existe(string nombre)
    {
        return Todas().Any(c => MismoNombre(c, nombre));
    }

    /*
     * Deja las comisiones listas al abrir la aplicación: las bases
     * anteriores no las guardaban, así que se toman las que ya usan
     * los alumnos; si no hay ninguna, las iniciales.
     */
    public void Inicializar()
    {
        foreach (string nombre in _estudiantes.ObtenerTodos()
                     .Select(e => e.Comision)
                     .Distinct()
                     .Where(nombre => !Existe(nombre)))
        {
            _comisiones.Agregar(nombre);
        }

        if (Todas().Count == 0)
        {
            Sembrar();
        }
    }

    // Devuelve el motivo si no se pudo agregar
    public string? Agregar(string nombre)
    {
        string? error = Validar(nombre);

        if (error is null)
        {
            _comisiones.Agregar(nombre.Trim());
        }

        return error;
    }

    // Devuelve el motivo si no se pudo cambiar el nombre
    public string? Renombrar(string actual, string nuevo)
    {
        // Cambiar solo mayúsculas o minúsculas del mismo nombre se permite
        string? error = MismoNombre(actual, nuevo)
            ? Validaciones.NombreComision(nuevo)
            : Validar(nuevo);

        if (error is null)
        {
            _comisiones.Renombrar(actual, nuevo.Trim());
        }

        return error;
    }

    /*
     * Borra los alumnos de la comisión con sus asistencias, entregas
     * y contactos. La comisión y su calendario quedan.
     */
    public void Vaciar(string nombre)
    {
        foreach (string legajo in _estudiantes.ObtenerDeComision(nombre)
                     .Select(e => e.Legajo)
                     .ToList())
        {
            _estudiantes.Eliminar(legajo);
        }
    }

    // Borra todo lo de la comisión y la comisión misma
    public void Eliminar(string nombre)
    {
        Vaciar(nombre);

        _comisiones.Eliminar(nombre);
    }

    // Deja la aplicación como recién instalada
    public void BorrarTodo()
    {
        _administracion.BorrarTodo();

        Sembrar();
    }

    private void Sembrar()
    {
        foreach (string nombre in Iniciales)
        {
            _comisiones.Agregar(nombre);
        }
    }

    private string? Validar(string nombre)
    {
        string? error = Validaciones.NombreComision(nombre);

        if (error is null && Existe(nombre))
        {
            error = "Ya existe una comisión con ese nombre";
        }

        return error;
    }

    private static bool MismoNombre(string a, string b)
    {
        return a.Trim().Equals(
            b.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }
}
