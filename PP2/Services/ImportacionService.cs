using System.Globalization;
using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Services;

/// <summary>
/// Una fila del CSV ya separada en columnas. No se interpretó nada todavía.
/// </summary>
public record FilaImportada(
    int Linea,
    string Nombre,
    string Apellido,
    string Legajo,
    string Comision,
    string Asistencias,
    string Entregas);

public record ResultadoImportacion(
    int AlumnosNuevos,
    int Registros,
    List<string> Errores)
{
    private const int ErroresVisibles = 5;

    public string Texto()
    {
        string texto = $"Alumnos nuevos: {AlumnosNuevos}\n" +
                       $"Asistencias y entregas cargadas: {Registros}";

        if (Errores.Count == 0)
        {
            return texto;
        }

        string visibles = string.Join("\n", Errores.Take(ErroresVisibles));
        string resto = Errores.Count > ErroresVisibles
            ? $"\n... y {Errores.Count - ErroresVisibles} más."
            : "";

        return $"{texto}\n\nFilas con observaciones ({Errores.Count}):\n" +
               $"{visibles}{resto}";
    }
}

/// <summary>
/// Separa el texto del CSV en filas. Acepta ';' o ',' y saltea el
/// encabezado. Solo conoce el formato del archivo, no las reglas del negocio.
/// </summary>
public static class LectorCsv
{
    private const int ColumnasObligatorias = 4;

    public static List<FilaImportada> Leer(string contenido, List<string> errores)
    {
        var filas = new List<FilaImportada>();
        string[] lineas = contenido.Split('\n');

        for (int i = 0; i < lineas.Length; i++)
        {
            string linea = lineas[i].Trim('\r', ' ');

            if (linea.Length == 0 || EsEncabezado(i, linea))
            {
                continue;
            }

            FilaImportada? fila = LeerFila(i + 1, linea);

            if (fila is null)
            {
                errores.Add($"Línea {i + 1}: faltan columnas obligatorias " +
                            "(Nombre, Apellido, Legajo, Comisión).");
                continue;
            }

            filas.Add(fila);
        }

        return filas;
    }

    private static bool EsEncabezado(int indice, string linea)
    {
        string primera = linea.Split(';', ',')[0].Trim();

        return indice == 0 &&
               (primera.Equals("Nombre", StringComparison.OrdinalIgnoreCase) ||
                primera.Equals("Legajo", StringComparison.OrdinalIgnoreCase));
    }

    private static FilaImportada? LeerFila(int numero, string linea)
    {
        char separador = linea.Contains(';') ? ';' : ',';
        string[] c = linea.Split(separador).Select(x => x.Trim()).ToArray();

        if (c.Length < ColumnasObligatorias)
        {
            return null;
        }

        return new FilaImportada(
            numero, c[0], c[1], c[2], c[3],
            c.ElementAtOrDefault(4) ?? "",
            c.ElementAtOrDefault(5) ?? "");
    }
}

/// <summary>
/// Carga masiva de alumnos, asistencias y entregas desde un CSV.
/// Formato: Nombre;Apellido;Legajo;Comision;Asistencias;Entregas
/// Asistencias: 2026-10-01:Presente|2026-10-05:Ausente
/// Entregas: TP 1:Entregado|TP 2:NoEntregado
/// Usa los mismos servicios y validaciones que las pantallas, así que una
/// fila no puede cargar nada que la aplicación no deje cargar a mano.
/// </summary>
public class ImportacionService
{
    private readonly NominaService _nomina;

    private readonly RegistroService _registro;

    private readonly CalendarioService _calendario;

    private readonly IUnidadDeTrabajo _unidad;

    public ImportacionService(
        NominaService nomina,
        RegistroService registro,
        CalendarioService calendario,
        IUnidadDeTrabajo unidad)
    {
        _nomina = nomina;
        _registro = registro;
        _calendario = calendario;
        _unidad = unidad;
    }

    // Todo el archivo se guarda junto: si falla algo inesperado, no queda
    // cargado a medias. Las filas con datos inválidos no frenan la carga:
    // se informan y se sigue con las demás.
    public ResultadoImportacion Importar(string contenido)
    {
        return _unidad.EnTransaccion(() => ImportarFilas(contenido));
    }

    private ResultadoImportacion ImportarFilas(string contenido)
    {
        var errores = new List<string>();
        var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int nuevos = 0;
        int registros = 0;

        foreach (FilaImportada fila in LectorCsv.Leer(contenido, errores))
        {
            if (!vistos.Add(fila.Legajo))
            {
                errores.Add($"Línea {fila.Linea}: el legajo {fila.Legajo} " +
                            "está repetido en el archivo.");
                continue;
            }

            Estudiante? alumno = ObtenerOCrear(fila, errores, ref nuevos);

            if (alumno is not null)
            {
                registros += CargarAsistencias(alumno, fila, errores);
                registros += CargarEntregas(alumno, fila, errores);
            }
        }

        return new ResultadoImportacion(nuevos, registros, errores);
    }

    // Si el legajo ya existe se completan sus registros; si no, se da de alta
    private Estudiante? ObtenerOCrear(
        FilaImportada fila,
        List<string> errores,
        ref int nuevos)
    {
        Estudiante? existente = _nomina.Buscar(fila.Legajo);

        if (existente is not null)
        {
            return existente;
        }

        // Sin fecha de incorporación: cursa desde el comienzo
        string? error = _nomina.Agregar(
            fila.Nombre, fila.Apellido, fila.Legajo, fila.Comision, default);

        if (error is not null)
        {
            errores.Add($"Línea {fila.Linea} ({fila.Apellido}, {fila.Nombre}): {error}");
            return null;
        }

        nuevos++;
        return _nomina.Buscar(fila.Legajo);
    }

    private int CargarAsistencias(
        Estudiante alumno,
        FilaImportada fila,
        List<string> errores)
    {
        int cargados = 0;

        foreach (string item in Items(fila.Asistencias))
        {
            string? error = CargarAsistencia(alumno, item, out int guardados);

            cargados += guardados;

            if (error is not null)
            {
                errores.Add($"Línea {fila.Linea} (legajo {alumno.Legajo}): {error}");
            }
        }

        return cargados;
    }

    private string? CargarAsistencia(Estudiante alumno, string item, out int guardados)
    {
        guardados = 0;
        string[] partes = item.Split(':');

        if (partes.Length != 2 || !TryFecha(partes[0], out DateTime fecha))
        {
            return $"formato de asistencia inválido '{item}'.";
        }

        bool? presente = EsPresente(partes[1]);

        string? error = presente is null
            ? $"estado de asistencia '{partes[1].Trim()}' inválido."
            : Validaciones.FechaAsistencia(fecha);

        if (error is not null)
        {
            return error;
        }

        _calendario.Agregar(alumno.Comision, TipoActividad.Clase, fecha, "");
        ResultadoGuardado r = _registro.GuardarAsistencia(
            fecha, new[] { new MarcaAsistencia(alumno, presente!.Value) });

        guardados = r.Nuevos + r.Modificados;
        return null;
    }

    private int CargarEntregas(
        Estudiante alumno,
        FilaImportada fila,
        List<string> errores)
    {
        int cargados = 0;

        foreach (string item in Items(fila.Entregas))
        {
            string? error = CargarEntrega(alumno, item, out int guardados);

            cargados += guardados;

            if (error is not null)
            {
                errores.Add($"Línea {fila.Linea} (legajo {alumno.Legajo}): {error}");
            }
        }

        return cargados;
    }

    private string? CargarEntrega(Estudiante alumno, string item, out int guardados)
    {
        guardados = 0;
        string[] partes = item.Split(':');
        string nombre = partes[0].Trim();

        if (partes.Length != 2 || Validaciones.NombreTrabajo(nombre) is not null)
        {
            return $"formato de entrega inválido '{item}'.";
        }

        EstadoEntrega? estado = LeerEstado(partes[1]);

        if (estado is null)
        {
            return $"estado de entrega '{partes[1].Trim()}' inválido.";
        }

        // Si el trabajo ya existe, el calendario avisa y se usa el que está
        _calendario.Agregar(alumno.Comision, TipoActividad.Trabajo, DateTime.Today, nombre);
        ResultadoGuardado r = _registro.GuardarEntregas(
            _calendario.ObtenerTrabajo(nombre),
            new[] { new MarcaEntrega(alumno, estado) });

        guardados = r.Nuevos + r.Modificados;
        return null;
    }

    private static IEnumerable<string> Items(string texto)
    {
        return texto.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(i => i.Trim())
            .Where(i => i.Length > 0);
    }

    // El formato del archivo es siempre año-mes-día, sin importar el idioma
    private static bool TryFecha(string texto, out DateTime fecha)
    {
        return DateTime.TryParseExact(
            texto.Trim(),
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out fecha);
    }

    private static bool? EsPresente(string texto)
    {
        return texto.Trim().ToLowerInvariant() switch
        {
            "presente" or "p" => true,
            "ausente" or "a" => false,
            _ => null
        };
    }

    private static EstadoEntrega? LeerEstado(string texto)
    {
        return texto.Replace(" ", "").ToLowerInvariant() switch
        {
            "entregado" => EstadoEntrega.Entregado,
            "noentregado" => EstadoEntrega.NoEntregado,
            "entregadofueradetermino" or "entregadofueradetérmino" or
            "fueradetermino" or "fueradetérmino" =>
                EstadoEntrega.EntregadoFueraDeTermino,
            _ => null
        };
    }
}
