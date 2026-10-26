using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Calendario de la comisión (H2): los trabajos del cuatrimestre con su
/// fecha y las clases dictadas. Las clases no se cargan: salen de los
/// días en que se tomó asistencia y muestran cómo fue cada una.
/// </summary>
public partial class CalendarioViewModel : PantallaBase, IActualizable
{
    private readonly string _comision;

    private readonly CalendarioService _calendario;

    private readonly RegistroService _registro;

    public string Comision => _comision;

    public ObservableCollection<string> Actividades { get; } = new();

    public ObservableCollection<string> Clases { get; } = new();

    [ObservableProperty]
    private DateTime fecha = DateTime.Today;

    [ObservableProperty]
    private string descripcion = "";

    [ObservableProperty]
    private string cuatrimestre = "";

    public CalendarioViewModel(
        string comision,
        CalendarioService calendario,
        RegistroService registro)
    {
        _comision = comision;
        _calendario = calendario;
        _registro = registro;

        Actualizar();
    }

    partial void OnFechaChanged(DateTime value)
    {
        Cuatrimestre = Cuatrimestres.De(value);
    }

    public void Actualizar()
    {
        Cuatrimestre = Cuatrimestres.De(Fecha);

        CargarTrabajos();
        CargarClases();
    }

    private void CargarTrabajos()
    {
        Actividades.Clear();

        foreach (ActividadCalendario a in _calendario.TrabajosDe(_comision))
        {
            Actividades.Add($"{Formatos.FechaCorta(a.Fecha)} · {a.Descripcion}");
        }
    }

    private void CargarClases()
    {
        Clases.Clear();

        foreach (ResumenClase clase in _registro.ClasesDe(_comision))
        {
            Clases.Add(TextoClase(clase));
        }
    }

    private static string TextoClase(ResumenClase clase)
    {
        return $"{Formatos.FechaCorta(clase.Fecha)} · " +
               $"Presentes: {clase.Presentes} · " +
               $"Ausentes: {clase.Ausentes} (de {clase.Total})";
    }

    [RelayCommand]
    private void Agregar()
    {
        string? error = _calendario.Agregar(
            _comision,
            TipoActividad.Trabajo,
            Fecha,
            Descripcion);

        if (error is not null)
        {
            Rechazar(error);
            return;
        }

        Descripcion = "";
        Informar("Trabajo agregado al calendario");

        Actualizar();
    }
}
