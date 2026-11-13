using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;
using System.Collections.ObjectModel;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Formulario para cargar un alumno nuevo (H1) o para corregir los datos
/// de uno que ya existe. El legajo no se puede cambiar.
/// </summary>
public partial class AlumnoFormViewModel : PantallaBase
{
    private readonly INavegacion _navegacion;

    private readonly NominaService _nomina;

    private readonly Estudiante? _original;

    public ObservableCollection<string> Comisiones { get; }

    public bool EsEdicion => _original is not null;

    public bool LegajoEditable => !EsEdicion;

    public string Titulo =>
        EsEdicion ? "Editar alumno" : "Alumno nuevo";

    public string TextoBoton =>
        EsEdicion ? "Guardar cambios" : "Guardar alumno";

    [ObservableProperty]
    private string nombre = "";

    [ObservableProperty]
    private string apellido = "";

    [ObservableProperty]
    private string legajo = "";

    [ObservableProperty]
    private string? comisionSeleccionada;

    [ObservableProperty]
    private DateTime fechaIncorporacion = DateTime.Today;

    public DateTime FechaMaxima => DateTime.Today;

    public AlumnoFormViewModel(
        INavegacion navegacion,
        NominaService nomina,
        ComisionService comisiones,
        string? legajoAEditar = null)
    {
        _navegacion = navegacion;
        _nomina = nomina;
        Comisiones = new ObservableCollection<string>(comisiones.Todas());

        if (legajoAEditar is not null)
        {
            _original = nomina.Buscar(legajoAEditar);
        }

        CargarDatosIniciales();
    }

    private void CargarDatosIniciales()
    {
        ComisionSeleccionada = _original?.Comision
            ?? Comisiones.FirstOrDefault();

        if (_original is not null)
        {
            Nombre = _original.Nombre;
            Apellido = _original.Apellido;
            Legajo = _original.Legajo;

            if (_original.FechaIncorporacion != default)
            {
                FechaIncorporacion = _original.FechaIncorporacion;
            }
        }
    }

    [RelayCommand]
    private async Task Guardar()
    {
        string? error = GuardarEnNomina();

        if (error is not null)
        {
            Rechazar(error);
            return;
        }

        if (EsEdicion)
        {
            await _navegacion.VolverAsync();
            return;
        }

        AvisarAltaYLimpiar();
    }

    private string? GuardarEnNomina()
    {
        return EsEdicion
            ? _nomina.Actualizar(
                Legajo, Nombre, Apellido, ComisionSeleccionada,
                FechaIncorporacion)
            : _nomina.Agregar(
                Nombre, Apellido, Legajo, ComisionSeleccionada,
                FechaIncorporacion);
    }

    private void AvisarAltaYLimpiar()
    {
        Informar($"{Apellido.Trim()}, {Nombre.Trim()} agregado a " +
                 $"{ComisionSeleccionada}");

        Nombre = "";
        Apellido = "";
        Legajo = "";
        FechaIncorporacion = DateTime.Today;
    }
}
