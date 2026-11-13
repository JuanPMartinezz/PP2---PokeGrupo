using RegistroAsistencia.Models;
using RegistroAsistencia.Repositorios;

namespace RegistroAsistencia.Services;

/// <summary>
/// Altas, cambios y bajas de alumnos (H1).
/// </summary>
public class NominaService
{
    private readonly IEstudianteRepositorio _estudiantes;

    private readonly IComisionRepositorio _comisiones;

    public NominaService(
        IEstudianteRepositorio estudiantes,
        IComisionRepositorio comisiones)
    {
        _estudiantes = estudiantes;
        _comisiones = comisiones;
    }

    public List<Estudiante> DeComision(string comision)
    {
        return _estudiantes.ObtenerDeComision(comision);
    }

    public Estudiante? Buscar(string legajo)
    {
        return _estudiantes.Buscar(legajo.Trim());
    }

    // Devuelve el motivo si no se pudo agregar al alumno
    public string? Agregar(
        string nombre,
        string apellido,
        string legajo,
        string? comision)
    {
        string? error = Validaciones.Alumno(nombre, apellido, legajo, comision)
                        ?? Validaciones.Legajo(legajo)
                        ?? ControlarComision(comision!)
                        ?? ControlarLegajoLibre(legajo);

        if (error is null)
        {
            _estudiantes.Agregar(new Estudiante
            {
                Nombre = nombre.Trim(),
                Apellido = apellido.Trim(),
                Legajo = legajo.Trim(),
                Comision = comision!
            });
        }

        return error;
    }

    // El legajo no se puede cambiar. Devuelve el motivo si no se pudo
    public string? Actualizar(
        string legajo,
        string nombre,
        string apellido,
        string? comision)
    {
        Estudiante? estudiante = Buscar(legajo);

        string? error = Validaciones.Alumno(nombre, apellido, legajo, comision)
                        ?? ControlarComision(comision!);

        if (error is null && estudiante is null)
        {
            error = "No se encontró al alumno";
        }

        if (error is null)
        {
            estudiante!.Nombre = nombre.Trim();
            estudiante.Apellido = apellido.Trim();
            estudiante.Comision = comision!;

            _estudiantes.Actualizar(estudiante);
        }

        return error;
    }

    // Elimina al alumno con sus asistencias, entregas y contactos
    public void Eliminar(string legajo)
    {
        _estudiantes.Eliminar(legajo.Trim());
    }

    private string? ControlarComision(string comision)
    {
        return _comisiones.ObtenerTodas().Contains(comision)
            ? null
            : "La comisión elegida no existe";
    }

    private string? ControlarLegajoLibre(string legajo)
    {
        Estudiante? existente = Buscar(legajo);

        return existente is null
            ? null
            : $"Ya existe el legajo {existente.Legajo}: " +
              $"{existente} ({existente.Comision})";
    }

    // NUEVO: Carga masiva unificada desde CSV
    // Formato: Nombre;Apellido;Legajo;Comision;Asistencias;Entregas
    // Ejemplo Asistencias: 2026-10-01:Presente|2026-10-05:Ausente
    // Ejemplo Entregas: TP 1:Entregado|TP 2:NoEntregado
    // NUEVO: Carga masiva unificada desde CSV
    // Formato: Nombre;Apellido;Legajo;Comision;Asistencias;Entregas
    public async Task<(int AlumnosNuevos, int RegistrosCargados, List<string> Errores)> ImportarDesdeCsvCompletoAsync(
        Stream streamArchivo,
        RegistroService registro,
        CalendarioService calendario)
    {
        var alumnosNuevos = new List<Estudiante>();
        var filasParaProcesar = new List<(Estudiante Alumno, string AsistenciasRaw, string EntregasRaw, int Linea)>();
        var errores = new List<string>();
        var legajosEnArchivo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(streamArchivo);
        int numeroLinea = 0;

        while (!reader.EndOfStream)
        {
            var linea = await reader.ReadLineAsync();
            numeroLinea++;

            if (string.IsNullOrWhiteSpace(linea)) continue;

            char separador = linea.Contains(';') ? ';' : ',';
            string[] columnas = linea.Split(separador);

            string primeraColumna = columnas.ElementAtOrDefault(0)?.Trim() ?? "";

            // Saltar encabezados
            if (numeroLinea == 1 &&
                (primeraColumna.Equals("Nombre", StringComparison.OrdinalIgnoreCase) ||
                 primeraColumna.Equals("Legajo", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (columnas.Length < 4)
            {
                errores.Add($"Línea {numeroLinea}: Faltan columnas obligatorias (Nombre, Apellido, Legajo, Comision).");
                continue;
            }

            string nombre = columnas.ElementAtOrDefault(0)?.Trim() ?? "";
            string apellido = columnas.ElementAtOrDefault(1)?.Trim() ?? "";
            string legajo = columnas.ElementAtOrDefault(2)?.Trim() ?? "";
            string comision = columnas.ElementAtOrDefault(3)?.Trim() ?? "";
            string asistenciasRaw = columnas.ElementAtOrDefault(4)?.Trim() ?? "";
            string entregasRaw = columnas.ElementAtOrDefault(5)?.Trim() ?? "";

            if (!legajosEnArchivo.Add(legajo))
            {
                errores.Add($"Línea {numeroLinea}: El legajo {legajo} está repetido en el archivo.");
                continue;
            }

            Estudiante? existente = _estudiantes.Buscar(legajo);

            if (existente is not null)
            {
                filasParaProcesar.Add((existente, asistenciasRaw, entregasRaw, numeroLinea));
                continue;
            }

            string? error = Validaciones.Alumno(nombre, apellido, legajo, comision)
                            ?? Validaciones.Legajo(legajo)
                            ?? ControlarComision(comision);

            if (error is not null)
            {
                errores.Add($"Línea {numeroLinea} ({apellido}, {nombre}): {error}");
                continue;
            }

            var nuevo = new Estudiante
            {
                Nombre = nombre,
                Apellido = apellido,
                Legajo = legajo,
                Comision = comision
            };

            alumnosNuevos.Add(nuevo);
            filasParaProcesar.Add((nuevo, asistenciasRaw, entregasRaw, numeroLinea));
        }

        if (alumnosNuevos.Count > 0)
        {
            _estudiantes.AgregarVarios(alumnosNuevos);
        }

        int registrosCargados = 0;

        foreach (var (alumno, asistenciasRaw, entregasRaw, linea) in filasParaProcesar)
        {
            if (!string.IsNullOrWhiteSpace(asistenciasRaw))
            {
                foreach (var item in asistenciasRaw.Split('|', StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] partes = item.Split(':');
                    string fechaTexto = partes.ElementAtOrDefault(0)?.Trim() ?? "";
                    string estTexto = partes.ElementAtOrDefault(1)?.Trim() ?? "";

                    if (partes.Length != 2 || !DateTime.TryParse(fechaTexto, out DateTime fecha))
                    {
                        errores.Add($"Línea {linea} (Legajo {alumno.Legajo}): Formato de asistencia inválido '{item}'.");
                        continue;
                    }

                    bool presente = estTexto.Equals("Presente", StringComparison.OrdinalIgnoreCase) ||
                                    estTexto.Equals("P", StringComparison.OrdinalIgnoreCase);
                    bool ausente = estTexto.Equals("Ausente", StringComparison.OrdinalIgnoreCase) ||
                                   estTexto.Equals("A", StringComparison.OrdinalIgnoreCase);

                    if (!presente && !ausente)
                    {
                        errores.Add($"Línea {linea} (Legajo {alumno.Legajo}): Estado de asistencia '{estTexto}' inválido.");
                        continue;
                    }

                    calendario.Agregar(alumno.Comision, TipoActividad.Clase, fecha.Date, "");
                    var res = registro.GuardarAsistencia(fecha.Date, new[] { new MarcaAsistencia(alumno, presente) });
                    registrosCargados += res.Nuevos + res.Modificados;
                }
            }

            if (!string.IsNullOrWhiteSpace(entregasRaw))
            {
                foreach (var item in entregasRaw.Split('|', StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] partes = item.Split(':');
                    string nombreTp = partes.ElementAtOrDefault(0)?.Trim() ?? "";
                    string estTexto = (partes.ElementAtOrDefault(1)?.Trim() ?? "").Replace(" ", "").ToLowerInvariant();

                    if (partes.Length != 2 || string.IsNullOrWhiteSpace(nombreTp))
                    {
                        errores.Add($"Línea {linea} (Legajo {alumno.Legajo}): Formato de entrega inválido '{item}'.");
                        continue;
                    }

                    EstadoEntrega? estado = estTexto switch
                    {
                        "entregado" => EstadoEntrega.Entregado,
                        "noentregado" => EstadoEntrega.NoEntregado,
                        "entregadofueradetermino" or "fueradetermino" => EstadoEntrega.EntregadoFueraDeTermino,
                        _ => null
                    };

                    if (estado is null)
                    {
                        errores.Add($"Línea {linea} (Legajo {alumno.Legajo}): Estado de entrega '{partes.ElementAtOrDefault(1)?.Trim()}' inválido.");
                        continue;
                    }

                    calendario.Agregar(alumno.Comision, TipoActividad.Trabajo, DateTime.Today, nombreTp);
                    Trabajo trabajo = calendario.ObtenerTrabajo(nombreTp);
                    var res = registro.GuardarEntregas(trabajo, new[] { new MarcaEntrega(alumno, estado.Value) });
                    registrosCargados += res.Nuevos + res.Modificados;
                }
            }
        }

        return (alumnosNuevos.Count, registrosCargados, errores);
    }
}
