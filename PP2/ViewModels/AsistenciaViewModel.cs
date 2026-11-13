
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

// Una fila de la planilla: el alumno y su check de presente

public class FilaAsistencia : ObservableObject
{
    private bool presente;

    public FilaAsistencia(
        Estudiante estudiante,
        bool? guardado,
        string nivelRiesgo)
    {
        Estudiante = estudiante;
        Guardado = guardado;
        NivelRiesgo = nivelRiesgo;
        presente = guardado ?? false;
    }

    public Estudiante Estudiante { get; }

    public string Nombre => $"{Estudiante.Apellido}, {Estudiante.Nombre}";

    public bool? Guardado { get; private set; }

    public string NivelRiesgo { get; }

    public string IconoRiesgo => NivelRiesgo switch
    {
        NivelesRiesgo.Alto => "🔴",
        NivelesRiesgo.Medio => "🟡",
        NivelesRiesgo.Bajo => "🟢",
        NivelesRiesgo.SinDatos => "⚪",
        _ => "❔"
    };

    public bool Presente
    {
        get => presente;
        set
        {
            if (SetProperty(ref presente, value))
                OnPropertyChanged(nameof(Modifica));
        }
    }

    public bool Modifica => Guardado is bool g && g != Presente;

    public void Confirmar()
    {
        Guardado = Presente;
        OnPropertyChanged(nameof(Modifica));
    }
}

/// <summary>
/// Planilla de asistencia de una comisión para una fecha (H3 y H4).
/// </summary>
public class AsistenciaViewModel : PantallaBase
{
    private readonly string _comision;
    private readonly NominaService _nomina;
    private readonly IDialogos _dialogos;
    private readonly RegistroService _registro;
    private readonly RiesgoService _riesgo;

    private DateTime fecha = DateTime.Today;
    private string resumen = "";

    public string Comision => _comision;

    public ObservableCollection<FilaAsistencia> Filas { get; } = new();

    public DateTime FechaMaxima => DateTime.Today;

    public DateTime Fecha
    {
        get => fecha;
        set
        {
            if (SetProperty(ref fecha, value))
            {
                CargarFilas();
            }
        }
    }

    public string Resumen
    {
        get => resumen;
        set => SetProperty(ref resumen, value);
    }

    public IRelayCommand MarcarTodosCommand { get; }

    public IAsyncRelayCommand GuardarCommand { get; }

    public AsistenciaViewModel(
        string comision,
        NominaService nomina,
        IDialogos dialogos,
        RegistroService registro,
        RiesgoService riesgo)
    {
        _comision = comision;
        _nomina = nomina;
        _dialogos = dialogos;
        _registro = registro;
        _riesgo = riesgo;

        MarcarTodosCommand = new RelayCommand(MarcarTodos);
        GuardarCommand = new AsyncRelayCommand(Guardar);

        CargarFilas();
    }


    private void CargarFilas()
    {
        Filas.Clear();
        LimpiarMensaje();

        List<ResultadoRiesgo> resultados =
            _riesgo.ObtenerResultados(_comision);

        foreach (Estudiante e in _nomina.DeComision(_comision))
        {
            ResultadoRiesgo? resultado = resultados.FirstOrDefault(
                r => r.Estudiante.Legajo == e.Legajo);

            string nivel = resultado?.NivelRiesgo
                ?? NivelesRiesgo.SinDatos;

            Filas.Add(CrearFila(e, nivel));
        }

        ActualizarResumen();
    }



    private FilaAsistencia CrearFila(
        Estudiante estudiante,
        string nivel)
    {
        EstadoAsistencia? guardada = _registro.AsistenciaDe(
            estudiante.Legajo,
            Fecha);

        bool? presente = guardada is null
            ? null
            : guardada == EstadoAsistencia.Presente;

        var fila = new FilaAsistencia(
            estudiante,
            presente,
            nivel);

        fila.PropertyChanged += (_, _) => ActualizarResumen();

        return fila;
    }

    private void ActualizarResumen()
    {
        int presentes = Filas.Count(f => f.Presente);

        Resumen = $"Presentes: {presentes} de {Filas.Count}";
    }

    private void MarcarTodos()
    {
        foreach (FilaAsistencia fila in Filas)
        {
            fila.Presente = true;
        }
    }

    private async Task Guardar()
    {
        string? error = Validaciones.FechaAsistencia(Fecha);

        if (error is not null)
        {
            Rechazar(error);
            return;
        }

        if (!await ConfirmarCorrecciones())
        {
            return;
        }

        ResultadoGuardado resultado = GuardarMarcas();

        Informar(TextoResultado(resultado));
    }

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
