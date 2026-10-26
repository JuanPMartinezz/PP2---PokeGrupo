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
public partial class AlumnoFormViewModel : ObservableObject
{
    private readonly INavegacion _navegacion;

    private readonly Estudiante? _original;

    public ObservableCollection<string> Comisiones =>
        DatosSistema.Comisiones;

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
    private string mensaje = "";

    public AlumnoFormViewModel(
        INavegacion navegacion,
        string? legajoAEditar = null)
    {
        _navegacion = navegacion;

        if (legajoAEditar is not null)
        {
            _original = DatosSistema.BuscarEstudiante(legajoAEditar);
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
        }
    }

    private string? Validar()
    {
        string? error = Validaciones.Alumno(
            Nombre,
            Apellido,
            Legajo,
            ComisionSeleccionada);

        if (error is null && !EsEdicion)
        {
            error = Validaciones.LegajoNuevo(Legajo);
        }

        return error;
    }

    [RelayCommand]
    private async Task Guardar()
    {
        string? error = Validar();

        if (error is not null)
        {
            Mensaje = error;
            return;
        }

        if (!Aplicar())
        {
            return;
        }

        if (EsEdicion)
        {
            await _navegacion.VolverAsync();
        }
    }

    // Guarda el alumno. Devuelve falso si no se pudo y deja el motivo en Mensaje
    private bool Aplicar()
    {
        try
        {
            if (_original is null)
            {
                AgregarNuevo();
            }
            else
            {
                ActualizarExistente(_original);
            }

            return true;
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo guardar el alumno: {ex.Message}";

            return false;
        }
    }

    private void AgregarNuevo()
    {
        var nuevo = new Estudiante
        {
            Nombre = Nombre.Trim(),
            Apellido = Apellido.Trim(),
            Legajo = Legajo.Trim(),
            Comision = ComisionSeleccionada!
        };

        DatosSistema.AgregarEstudiante(nuevo);

        Mensaje = $"{nuevo} agregado a {nuevo.Comision}";

        Nombre = "";
        Apellido = "";
        Legajo = "";
    }

    private void ActualizarExistente(Estudiante estudiante)
    {
        estudiante.Nombre = Nombre.Trim();
        estudiante.Apellido = Apellido.Trim();
        estudiante.Comision = ComisionSeleccionada!;

        DatosSistema.ActualizarEstudiante(estudiante);
    }
}
