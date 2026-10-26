using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

// Un trabajo del calendario para elegir en la lista
public sealed record ItemTrabajo(string Descripcion, DateTime Fecha)
{
    public bool Vencido => Fecha.Date < DateTime.Today;

    public string Texto =>
        $"{Descripcion} · {Formatos.FechaCorta(Fecha)}" +
        (Vencido ? " · Vencido" : " · Abierto");
}

// Una fila de la planilla: el alumno y lo que se marcó de su entrega
public partial class FilaEntrega : ObservableObject
{
    public const string SinRegistrar = "Sin registrar";

    public static IReadOnlyList<string> Opciones { get; } = new[]
    {
        SinRegistrar,
        EstadoEntrega.Entregado.Texto(),
        EstadoEntrega.EntregadoFueraDeTermino.Texto(),
        EstadoEntrega.NoEntregado.Texto()
    };

    public FilaEntrega(Estudiante estudiante, EstadoEntrega? guardado)
    {
        Estudiante = estudiante;
        Guardado = guardado;
        estadoTexto = TextoDe(guardado);
    }

    public Estudiante Estudiante { get; }

    public string Nombre => $"{Estudiante.Apellido}, {Estudiante.Nombre}";

    // Lo que hay guardado: nulo si todavía no se registró nada
    public EstadoEntrega? Guardado { get; private set; }

    [ObservableProperty]
    private string estadoTexto;

    // Lo que quedó marcado en la planilla
    public EstadoEntrega? Estado => Opciones.Contains(EstadoTexto)
        ? Convertir(EstadoTexto)
        : null;

    // Ya había un registro y se lo cambió por otro estado
    public bool Modifica => Guardado is not null &&
                            Estado is not null &&
                            Guardado != Estado;

    public void Confirmar()
    {
        Guardado = Estado ?? Guardado;

        OnPropertyChanged(nameof(Modifica));
    }

    partial void OnEstadoTextoChanged(string value)
    {
        OnPropertyChanged(nameof(Estado));
        OnPropertyChanged(nameof(Modifica));
    }

    private static string TextoDe(EstadoEntrega? estado)
    {
        return estado is EstadoEntrega e ? e.Texto() : SinRegistrar;
    }

    private static EstadoEntrega? Convertir(string texto)
    {
        foreach (EstadoEntrega e in Enum.GetValues<EstadoEntrega>())
        {
            if (e.Texto() == texto)
            {
                return e;
            }
        }

        return null;
    }
}

/// <summary>
/// Planilla de entregas de un trabajo de la comisión (H5).
/// Cada alumno puede quedar como entregado, fuera de término o no
/// entregado. Al cerrar la entrega, los que no tienen nada quedan como
/// "No entregado".
/// </summary>
public partial class EntregaViewModel : PantallaBase, IActualizable
{
    private readonly string _comision;

    private readonly INavegacion _navegacion;

    private readonly NominaService _nomina;

    private readonly CalendarioService _calendario;

    private readonly IDialogos _dialogos;

    private readonly RegistroService _registro;

    public string Comision => _comision;

    public IReadOnlyList<string> Opciones => FilaEntrega.Opciones;

    public ObservableCollection<ItemTrabajo> Trabajos { get; } = new();

    public ObservableCollection<FilaEntrega> Filas { get; } = new();

    [ObservableProperty]
    private ItemTrabajo? trabajoSeleccionado;

    [ObservableProperty]
    private string resumen = "";

    public bool HayTrabajos => Trabajos.Count > 0;

    public bool NoHayTrabajos => !HayTrabajos;

    public bool HayTrabajoElegido => TrabajoSeleccionado is not null;

    public EntregaViewModel(
        string comision,
        INavegacion navegacion,
        NominaService nomina,
        CalendarioService calendario,
        IDialogos dialogos,
        RegistroService registro)
    {
        _comision = comision;
        _navegacion = navegacion;
        _nomina = nomina;
        _calendario = calendario;
        _dialogos = dialogos;
        _registro = registro;

        Actualizar();
    }

    // Vuelve a leer los trabajos del calendario y mantiene el elegido
    public void Actualizar()
    {
        string? elegido = TrabajoSeleccionado?.Descripcion;

        Trabajos.Clear();

        foreach (ActividadCalendario a in _calendario.TrabajosDe(_comision))
        {
            Trabajos.Add(new ItemTrabajo(a.Descripcion, a.Fecha));
        }

        OnPropertyChanged(nameof(HayTrabajos));
        OnPropertyChanged(nameof(NoHayTrabajos));

        TrabajoSeleccionado =
            Trabajos.FirstOrDefault(t => t.Descripcion == elegido) ??
            Trabajos.FirstOrDefault();
    }

    partial void OnTrabajoSeleccionadoChanged(ItemTrabajo? value)
    {
        OnPropertyChanged(nameof(HayTrabajoElegido));

        CargarFilas();
    }

    private void CargarFilas()
    {
        Filas.Clear();
        LimpiarMensaje();

        if (TrabajoSeleccionado is null)
        {
            ActualizarResumen();
            return;
        }

        foreach (Estudiante e in _nomina.DeComision(_comision))
        {
            Filas.Add(CrearFila(e));
        }

        ActualizarResumen();
    }

    private FilaEntrega CrearFila(Estudiante estudiante)
    {
        Trabajo trabajo = TrabajoActual();

        EstadoEntrega? guardada = _registro.EntregaDe(
            estudiante.Legajo,
            trabajo.Id);

        var fila = new FilaEntrega(estudiante, guardada);

        fila.PropertyChanged += (_, _) => ActualizarResumen();

        return fila;
    }

    private Trabajo TrabajoActual()
    {
        return _calendario.ObtenerTrabajo(
            TrabajoSeleccionado!.Descripcion);
    }

    private void ActualizarResumen()
    {
        int registrados = Filas.Count(f => f.Estado is not null);

        Resumen = $"Registrados: {registrados} de {Filas.Count}";
    }

    [RelayCommand]
    private Task AgregarTrabajo()
    {
        return _navegacion.IrAAsync(Destino.Calendario, _comision);
    }

    [RelayCommand]
    private async Task Guardar()
    {
        if (!HayTrabajoElegido)
        {
            Rechazar("Elegí un trabajo o agregá uno nuevo");
            return;
        }

        if (!await ConfirmarCorrecciones())
        {
            return;
        }

        var marcas = Filas
            .Select(f => new MarcaEntrega(f.Estudiante, f.Estado))
            .ToList();

        ResultadoGuardado resultado = _registro.GuardarEntregas(
            TrabajoActual(),
            marcas);

        ConfirmarFilas();

        Informar($"Entregas guardadas: {resultado.Nuevos} nuevas, " +
                 $"{resultado.Modificados} corregidas");
    }

    [RelayCommand]
    private async Task CerrarEntrega()
    {
        if (!HayTrabajoElegido)
        {
            Rechazar("Elegí un trabajo o agregá uno nuevo");
            return;
        }

        bool acepta = await _dialogos.ConfirmarAsync(
            "Cerrar entrega",
            "Los alumnos que no tengan nada registrado van a quedar como \"No entregado\". ¿Querés continuar?",
            "Cerrar entrega",
            "Cancelar");

        if (!acepta)
        {
            return;
        }

        int cerrados = _registro.CerrarEntrega(
            TrabajoActual(),
            Filas.Select(f => f.Estudiante));

        CargarFilas();

        Informar($"Entrega cerrada: {cerrados} alumnos quedaron como " +
                 "\"No entregado\"");
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
            "Modificar entregas",
            $"Vas a modificar {cambios} registro(s) que ya estaban guardados. ¿Querés continuar?",
            "Modificar",
            "Cancelar");
    }

    private void ConfirmarFilas()
    {
        foreach (FilaEntrega fila in Filas)
        {
            fila.Confirmar();
        }
    }
}
