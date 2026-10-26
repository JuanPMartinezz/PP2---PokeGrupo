using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Calendario de la comisión (H2): las clases y los trabajos del
/// cuatrimestre, cada uno con su fecha.
/// </summary>
public partial class CalendarioViewModel : ObservableObject, IActualizable
{
    private const string DescripcionClase = "Clase";

    private readonly string _comision;

    public string Comision => _comision;

    public IReadOnlyList<string> Tipos { get; } = new[]
    {
        "Clase",
        "Trabajo"
    };

    public ObservableCollection<string> Actividades { get; } = new();

    // Para decidir si se pide el nombre: las clases no lo necesitan
    public bool EsTrabajo => TipoIndice == 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsTrabajo))]
    private int tipoIndice;

    [ObservableProperty]
    private DateTime fecha = DateTime.Today;

    [ObservableProperty]
    private string descripcion = "";

    [ObservableProperty]
    private string mensaje = "";

    [ObservableProperty]
    private string cuatrimestre = "";

    // Con trabajoInicial el tipo ya viene elegido como "Trabajo"
    public CalendarioViewModel(string comision, bool trabajoInicial = false)
    {
        _comision = comision;
        tipoIndice = trabajoInicial ? 1 : 0;

        Actualizar();
    }

    partial void OnFechaChanged(DateTime value)
    {
        Cuatrimestre = Cuatrimestres.De(value);
    }

    public void Actualizar()
    {
        Cuatrimestre = Cuatrimestres.De(Fecha);

        Actividades.Clear();

        foreach (ActividadCalendario a in DatosSistema.Calendario
                     .Where(a => a.Comision == _comision)
                     .OrderByDescending(a => a.Fecha))
        {
            Actividades.Add(TextoActividad(a));
        }
    }

    private static string TextoActividad(ActividadCalendario a)
    {
        string detalle = a.Tipo == TipoActividad.Trabajo
            ? $"Trabajo: {a.Descripcion}"
            : "Clase";

        return $"{Formatos.FechaCorta(a.Fecha)} · {detalle}";
    }

    private TipoActividad Tipo =>
        EsTrabajo ? TipoActividad.Trabajo : TipoActividad.Clase;

    private string? Validar()
    {
        if (EsTrabajo)
        {
            return Validaciones.Trabajo(Descripcion, _comision, Fecha);
        }

        bool repetida = DatosSistema.ExisteActividadCalendario(
            _comision,
            Cuatrimestres.De(Fecha),
            Fecha,
            TipoActividad.Clase,
            DescripcionClase);

        return repetida
            ? "Ya hay una clase cargada para esa fecha"
            : null;
    }

    [RelayCommand]
    private void Agregar()
    {
        string? error = Validar();

        if (error is not null)
        {
            Mensaje = error;
            return;
        }

        try
        {
            Guardar();
        }
        catch (InvalidOperationException ex)
        {
            Mensaje = ex.Message;
        }
    }

    private void Guardar()
    {
        string texto = EsTrabajo ? Descripcion.Trim() : DescripcionClase;

        if (EsTrabajo)
        {
            DatosSistema.ObtenerOCrearTrabajo(texto);
        }

        DatosSistema.AgregarActividadCalendario(new ActividadCalendario
        {
            Comision = _comision,
            Cuatrimestre = Cuatrimestres.De(Fecha),
            Fecha = Fecha.Date,
            Tipo = Tipo,
            Descripcion = texto
        });

        Descripcion = "";
        Mensaje = "Agregado al calendario";

        Actualizar();
    }
}
