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
public partial class CalendarioViewModel : ObservableObject, IActualizable
{
    private readonly string _comision;

    private readonly RegistroService _registro;

    public string Comision => _comision;

    public ObservableCollection<string> Actividades { get; } = new();

    public ObservableCollection<string> Clases { get; } = new();

    [ObservableProperty]
    private DateTime fecha = DateTime.Today;

    [ObservableProperty]
    private string descripcion = "";

    [ObservableProperty]
    private string mensaje = "";

    [ObservableProperty]
    private string cuatrimestre = "";

    public CalendarioViewModel(string comision, RegistroService registro)
    {
        _comision = comision;
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

        foreach (ActividadCalendario a in DatosSistema
                     .TrabajosDelCalendario(_comision)
                     .OrderByDescending(a => a.Fecha))
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
        string? error = Validaciones.Trabajo(Descripcion, _comision, Fecha);

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
        string nombre = Descripcion.Trim();

        DatosSistema.ObtenerOCrearTrabajo(nombre);

        DatosSistema.AgregarActividadCalendario(new ActividadCalendario
        {
            Comision = _comision,
            Cuatrimestre = Cuatrimestres.De(Fecha),
            Fecha = Fecha.Date,
            Tipo = TipoActividad.Trabajo,
            Descripcion = nombre
        });

        Descripcion = "";
        Mensaje = "Trabajo agregado al calendario";

        Actualizar();
    }
}
