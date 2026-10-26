using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
using RegistroAsistencia.Services;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Registro de un contacto con un alumno (H10): fecha, si atendió o no
/// y qué se habló.
/// </summary>
public partial class ContactoViewModel : ObservableObject
{
    private readonly string _legajo;

    private readonly INavegacion _navegacion;

    public string Alumno { get; }

    public DateTime FechaMaxima => DateTime.Today;

    [ObservableProperty]
    private DateTime fecha = DateTime.Today;

    [ObservableProperty]
    private bool atendio = true;

    [ObservableProperty]
    private string resultado = "";

    [ObservableProperty]
    private string mensaje = "";

    public ContactoViewModel(string legajo, INavegacion navegacion)
    {
        _legajo = legajo;
        _navegacion = navegacion;

        Alumno = DatosSistema.BuscarEstudiante(legajo)?.ToString() ?? "";
    }

    private string? Validar()
    {
        string? error = Validaciones.FechaContacto(Fecha);

        if (error is null && string.IsNullOrWhiteSpace(Resultado))
        {
            error = Atendio
                ? "Escribí de qué se habló en el contacto"
                : "Escribí qué pasó (por ejemplo: sonó y no contestó)";
        }

        return error;
    }

    [RelayCommand]
    private async Task Guardar()
    {
        string? error = Validar();

        Estudiante? estudiante = DatosSistema.BuscarEstudiante(_legajo);

        if (error is null && estudiante is null)
        {
            error = "No se encontró al alumno";
        }

        if (error is not null)
        {
            Mensaje = error;
            return;
        }

        DatosSistema.AgregarContacto(new ContactoEstudiante
        {
            Estudiante = estudiante!,
            Fecha = Fecha.Date,
            Atendio = Atendio,
            Resultado = Resultado
        });

        await _navegacion.VolverAsync();
    }
}
