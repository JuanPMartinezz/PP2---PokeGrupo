using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

public partial class CalendarioViewModel : ObservableObject
{
    public ObservableCollection<string> Comisiones { get; } = new()
    {
        "Comisión A",
        "Comisión B"
    };

    public ObservableCollection<string> Cuatrimestres { get; }
        = new(GenerarCuatrimestres());

    public ObservableCollection<TipoActividad> TiposActividad { get; }
        = new(
            Enum.GetValues<TipoActividad>());

    public ObservableCollection<ActividadCalendario> Actividades { get; }
        = new();

    private string comisionSeleccionada = "Comisión A";

    public string ComisionSeleccionada
    {
        get => comisionSeleccionada;

        set
        {
            if (SetProperty(
                ref comisionSeleccionada,
                value))
            {
                ConsultarCalendario();
            }
        }
    }

    private string cuatrimestreSeleccionado
        = ObtenerCuatrimestreActual();

    public string CuatrimestreSeleccionado
    {
        get => cuatrimestreSeleccionado;

        set
        {
            if (SetProperty(
                ref cuatrimestreSeleccionado,
                value))
            {
                ConsultarCalendario();
            }
        }
    }

    private TipoActividad tipoSeleccionado
        = TipoActividad.Clase;

    public TipoActividad TipoSeleccionado
    {
        get => tipoSeleccionado;

        set => SetProperty(
            ref tipoSeleccionado,
            value);
    }

    private DateTime fechaActividad = DateTime.Today;

    public DateTime FechaActividad
    {
        get => fechaActividad;

        set => SetProperty(
            ref fechaActividad,
            value);
    }

    private string descripcion = "";

    public string Descripcion
    {
        get => descripcion;

        set => SetProperty(
            ref descripcion,
            value);
    }

    private string mensaje = "";

    public string Mensaje
    {
        get => mensaje;

        set => SetProperty(
            ref mensaje,
            value);
    }

    public CalendarioViewModel()
    {
        ConsultarCalendario();
    }

    private static List<string> GenerarCuatrimestres()
    {
        int anioActual = DateTime.Today.Year;

        var cuatrimestres = new List<string>();

        for (int anio = anioActual - 1;
             anio <= anioActual + 1;
             anio++)
        {
            cuatrimestres.Add(
                $"{anio} - 1er cuatrimestre");

            cuatrimestres.Add(
                $"{anio} - 2do cuatrimestre");
        }

        return cuatrimestres;
    }

    private static string ObtenerCuatrimestreActual()
    {
        DateTime hoy = DateTime.Today;

        return hoy.Month <= 7
            ? $"{hoy.Year} - 1er cuatrimestre"
            : $"{hoy.Year} - 2do cuatrimestre";
    }

    [RelayCommand]
    private void RegistrarActividad()
    {
        string descripcionLimpia =
            Descripcion.Trim();

        if (TipoSeleccionado == TipoActividad.Trabajo &&
            string.IsNullOrWhiteSpace(descripcionLimpia))
        {
            Mensaje =
                "Ingrese el nombre del trabajo";

            return;
        }

        if (TipoSeleccionado == TipoActividad.Clase &&
            string.IsNullOrWhiteSpace(descripcionLimpia))
        {
            descripcionLimpia = "Clase";
        }

        if (DatosSistema.ExisteActividadCalendario(
                ComisionSeleccionada,
                CuatrimestreSeleccionado,
                FechaActividad,
                TipoSeleccionado,
                descripcionLimpia))
        {
            Mensaje =
                "Esa actividad ya fue registrada en el calendario para esa fecha";

            return;
        }

        var nuevaActividad = new ActividadCalendario
        {
            Comision =
                ComisionSeleccionada,

            Cuatrimestre =
                CuatrimestreSeleccionado,

            Fecha =
                FechaActividad.Date,

            Tipo =
                TipoSeleccionado,

            Descripcion =
                descripcionLimpia
        };

        try
        {
            DatosSistema.AgregarActividadCalendario(
                nuevaActividad);

            Descripcion = "";

            Mensaje =
                TipoSeleccionado == TipoActividad.Clase
                    ? "Clase registrada correctamente"
                    : "Trabajo registrado correctamente";

            ConsultarCalendario();
        }
        catch (Exception ex)
        {
            Mensaje =
                $"No se pudo guardar la actividad: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ConsultarCalendario()
    {
        Actividades.Clear();

        var actividadesFiltradas =
            DatosSistema.Calendario
                .Where(a =>
                    a.Comision ==
                    ComisionSeleccionada &&
                    a.Cuatrimestre ==
                    CuatrimestreSeleccionado)
                .OrderBy(a => a.Fecha)
                .ThenBy(a => a.Tipo)
                .ThenBy(a => a.Descripcion)
                .ToList();

        foreach (ActividadCalendario actividad
                 in actividadesFiltradas)
        {
            Actividades.Add(actividad);
        }
    }
}
