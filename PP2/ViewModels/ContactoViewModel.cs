using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Services;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Registro de un contacto con un alumno (H10): fecha, si atendió o no
/// y qué se habló.
/// </summary>
public partial class ContactoViewModel : PantallaBase
{
    private readonly string _legajo;

    private readonly INavegacion _navegacion;

    private readonly ContactoService _contactos;

    public string Alumno { get; }

    public DateTime FechaMaxima => DateTime.Today;

    [ObservableProperty]
    private DateTime fecha = DateTime.Today;

    [ObservableProperty]
    private bool atendio = true;

    [ObservableProperty]
    private string resultado = "";

    public ContactoViewModel(
        string legajo,
        INavegacion navegacion,
        NominaService nomina,
        ContactoService contactos)
    {
        _legajo = legajo;
        _navegacion = navegacion;
        _contactos = contactos;

        Alumno = nomina.Buscar(legajo)?.ToString() ?? "";
    }

    [RelayCommand]
    private async Task Guardar()
    {
        string? error = _contactos.Registrar(
            _legajo,
            Fecha,
            Atendio,
            Resultado);

        if (error is not null)
        {
            Rechazar(error);
            return;
        }

        await _navegacion.VolverAsync();
    }
}
