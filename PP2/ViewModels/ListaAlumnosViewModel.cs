using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public sealed class ItemAlumno
{
    public ItemAlumno(ResultadoRiesgo resultado, IAsyncRelayCommand abrir)
    {
        Resultado = resultado;
        Abrir = abrir;
    }

    public ResultadoRiesgo Resultado { get; }

    public bool Llamar => Resultado.ParaLlamar(DateTime.Today);

    public string Nombre =>
        $"{Resultado.Estudiante.Apellido}, {Resultado.Estudiante.Nombre}";

    public string Riesgo => Resultado.RiesgoTexto;

    public string Nivel => Resultado.NivelRiesgo;

    public IAsyncRelayCommand Abrir { get; }
}

public partial class ListaAlumnosViewModel : ObservableObject, IActualizable
{
    private readonly INavegacion _navegacion;

    private readonly RiesgoService _riesgo;

    private readonly NominaService _nomina;

    private readonly IDialogos _dialogos;

    private readonly RegistroService _registro;

    private readonly CalendarioService _calendario;

    public ObservableCollection<string> Comisiones { get; }

    public IReadOnlyList<string> Ordenes { get; } = new[]
    {
        "Apellido A-Z",
        "Apellido Z-A",
        "Mayor riesgo"
    };

    public IReadOnlyList<string> FiltrosContacto { get; } = new[]
    {
        "Todos",
        "Sin contactar",
        $"Sin contacto hace más de {ResultadoRiesgo.DiasSinContactoReciente} días",
        "Para llamar (riesgo alto, sin contacto reciente)"
    };

    public IReadOnlyList<string> FiltrosNivel { get; } = new[]
    {
        "Todos los niveles",
        NivelesRiesgo.Alto,
        NivelesRiesgo.Medio,
        NivelesRiesgo.Bajo
    };

    public ObservableCollection<ItemAlumno> Alumnos { get; } = new();

    [ObservableProperty]
    private string? comisionSeleccionada;

    [ObservableProperty]
    private int ordenIndice;

    [ObservableProperty]
    private int contactoIndice;

    [ObservableProperty]
    private int nivelIndice;

    [ObservableProperty]
    private string resumen = "";

    public ListaAlumnosViewModel(
        string comision,
        INavegacion navegacion,
        RiesgoService riesgo,
        ComisionService comisiones,
        NominaService nomina,
        IDialogos dialogos,
        RegistroService registro,
        CalendarioService calendario)
    {
        _navegacion = navegacion;
        _riesgo = riesgo;
        _nomina = nomina;
        _dialogos = dialogos;
        _registro = registro;
        _calendario = calendario;

        Comisiones = new ObservableCollection<string>(comisiones.Todas());
        comisionSeleccionada = comision;

        Actualizar();
    }

    partial void OnComisionSeleccionadaChanged(string? value) => Actualizar();

    partial void OnOrdenIndiceChanged(int value) => Actualizar();

    partial void OnContactoIndiceChanged(int value) => Actualizar();

    partial void OnNivelIndiceChanged(int value) => Actualizar();

    public void Actualizar()
    {
        Alumnos.Clear();

        if (ComisionSeleccionada is null)
        {
            Resumen = "";
            return;
        }

        List<ResultadoRiesgo> todos =
            _riesgo.ObtenerResultados(ComisionSeleccionada);

        List<ResultadoRiesgo> mostrados = ListadoAlumnos.Aplicar(
            todos,
            (OrdenLista)Math.Max(OrdenIndice, 0),
            (FiltroContacto)Math.Max(ContactoIndice, 0),
            (FiltroNivel)Math.Max(NivelIndice, 0),
            DateTime.Today);

        foreach (ResultadoRiesgo r in mostrados)
        {
            Alumnos.Add(CrearItem(r));
        }

        Resumen = TextoResumen(mostrados.Count, todos.Count);
    }

    [RelayCommand]
    private async Task ImportarAlumnosAsync()
    {
        try
        {
            var opciones = new PickOptions
            {
                PickerTitle = "Seleccionar archivo CSV",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "text/comma-separated-values", "text/csv", "text/plain" } },
                    { DevicePlatform.WinUI, new[] { ".csv" } },
                    { DevicePlatform.iOS, new[] { "public.comma-separated-values-text" } }
                })
            };

            FileResult? archivo = await FilePicker.Default.PickAsync(opciones);
            if (archivo is null) return;

            using Stream stream = await archivo.OpenReadAsync();
            var (alumnosNuevos, registros, errores) = await _nomina.ImportarDesdeCsvCompletoAsync(
                stream,
                _registro,
                _calendario);

            Actualizar();

            string mensaje = $"• Alumnos nuevos: {alumnosNuevos}\n• Asistencias y entregas cargadas: {registros}";
            if (errores.Count > 0)
            {
                mensaje += $"\n\nFilas con observaciones ({errores.Count}):\n" +
                           string.Join("\n", errores.Take(5));
                if (errores.Count > 5)
                {
                    mensaje += $"\n... y {errores.Count - 5} errores más.";
                }
            }

            if (Application.Current?.Windows.FirstOrDefault()?.Page is Page pagina)
            {
                await pagina.DisplayAlertAsync("Resultado de importación", mensaje, "Aceptar");
            }
        }
        catch (Exception ex)
        {
            if (Application.Current?.Windows.FirstOrDefault()?.Page is Page pagina)
            {
                await pagina.DisplayAlertAsync("Error", $"No se pudo leer el archivo: {ex.Message}", "Aceptar");
            }
        }
    }

    private ItemAlumno CrearItem(ResultadoRiesgo resultado)
    {
        string legajo = resultado.Estudiante.Legajo;

        var abrir = new AsyncRelayCommand(() =>
            _navegacion.IrAAsync(Destino.Ficha, legajo));

        return new ItemAlumno(resultado, abrir);
    }

    private static string TextoResumen(int mostrados, int total)
    {
        if (total == 0)
        {
            return "Esta comisión todavía no tiene alumnos";
        }

        return mostrados == 0
            ? "Ningún alumno cumple con esos filtros"
            : $"Mostrando {mostrados} de {total} alumnos";
    }
}
