using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

// Una fila de la planilla: el alumno y su check de presente
public partial class FilaAsistencia : ObservableObject
{
    public FilaAsistencia(Estudiante estudiante, bool? guardado)
    {
        Estudiante = estudiante;
        Guardado = guardado;
        presente = guardado ?? false;
    }

    public Estudiante Estudiante { get; }

    public string Nombre => $"{Estudiante.Apellido}, {Estudiante.Nombre}";

    // Lo que hay guardado: nulo si todavía no se tomó asistencia
    public bool? Guardado { get; private set; }

    [ObservableProperty]
    private bool presente;

    // Ya había un registro y el check quedó distinto
    public bool Modifica => Guardado is bool g && g != Presente;

    public void Confirmar()
    {
        Guardado = Presente;

        OnPropertyChanged(nameof(Modifica));
    }

    partial void OnPresenteChanged(bool value)
    {
        OnPropertyChanged(nameof(Modifica));
    }
}

/// <summary>
/// Planilla de asistencia de una comisión para una fecha (H3 y H4).
/// La primera vez se guarda sin preguntar; si se corrige algo que ya
/// estaba guardado, se pide confirmación.
/// </summary>
public partial class AsistenciaViewModel : ObservableObject
{
    private readonly string _comision;

    private readonly INavegacion _navegacion;

    private readonly IDialogos _dialogos;

    private readonly RegistroService _registro;

    public string Comision => _comision;

    public ObservableCollection<FilaAsistencia> Filas { get; } = new();

    public DateTime FechaMaxima => DateTime.Today;

    [ObservableProperty]
    private DateTime fecha = DateTime.Today;

    [ObservableProperty]
    private string mensaje = "";

    [ObservableProperty]
    private string resumen = "";

    public AsistenciaViewModel(
        string comision,
        INavegacion navegacion,
        IDialogos dialogos,
        RegistroService registro)
    {
        _comision = comision;
        _navegacion = navegacion;
        _dialogos = dialogos;
        _registro = registro;

        CargarFilas();
    }

    partial void OnFechaChanged(DateTime value)
    {
        CargarFilas();
    }

    private void CargarFilas()
    {
        Filas.Clear();
        Mensaje = "";

        foreach (Estudiante e in DatosSistema.EstudiantesDe(_comision))
        {
            Filas.Add(CrearFila(e));
        }

        ActualizarResumen();
    }

    private FilaAsistencia CrearFila(Estudiante estudiante)
    {
        Asistencia? guardada = DatosSistema.BuscarAsistencia(
            estudiante.Legajo,
            Fecha);

        bool? presente = guardada is null
            ? null
            : guardada.Estado == EstadoAsistencia.Presente;

        var fila = new FilaAsistencia(estudiante, presente);

        fila.PropertyChanged += (_, _) => ActualizarResumen();

        return fila;
    }

    private void ActualizarResumen()
    {
        int presentes = Filas.Count(f => f.Presente);

        Resumen = $"Presentes: {presentes} de {Filas.Count}";
    }

    [RelayCommand]
    private void MarcarTodos()
    {
        foreach (FilaAsistencia fila in Filas)
        {
            fila.Presente = true;
        }
    }

    [RelayCommand]
    private async Task Guardar()
    {
        string? error = Validaciones.FechaAsistencia(Fecha);

        if (error is not null)
        {
            Mensaje = error;
            return;
        }

        if (!await ConfirmarCorrecciones())
        {
            return;
        }

        ResultadoGuardado resultado = GuardarMarcas();

        Mensaje = TextoResultado(resultado);
    }

    // Pregunta solo si se está cambiando algo que ya estaba guardado
    private async Task<bool> ConfirmarCorrecciones()
    {
        int cambios = Filas.Count(f => f.Modifica);

        if (cambios == 0)
        {
            return true;
        }

        return await _dialogos.ConfirmarAsync(
            "Modificar asistencia",
            $"Vas a modificar {cambios} registro(s) que ya estaban guardados. ¿Querés continuar?",
            "Modificar",
            "Cancelar");
    }

    private ResultadoGuardado GuardarMarcas()
    {
        var marcas = Filas
            .Select(f => new MarcaAsistencia(f.Estudiante, f.Presente))
            .ToList();

        ResultadoGuardado resultado = _registro.GuardarAsistencia(
            Fecha,
            marcas);

        foreach (FilaAsistencia fila in Filas)
        {
            fila.Confirmar();
        }

        return resultado;
    }

    private static string TextoResultado(ResultadoGuardado resultado)
    {
        if (resultado.Nuevos == 0 && resultado.Modificados == 0)
        {
            return "No había cambios para guardar";
        }

        return $"Asistencia guardada: {resultado.Nuevos} nuevos, " +
               $"{resultado.Modificados} corregidos";
    }
}
