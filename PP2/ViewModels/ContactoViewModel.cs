using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Models;
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

    public List<string> Medios { get; } = Enum
        .GetValues<MedioContacto>()
        .Select(m => m.Texto())
        .ToList();

    [ObservableProperty]
    private int medioIndice;

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

    private MedioContacto? MedioElegido()
    {
        MedioContacto[] medios = Enum.GetValues<MedioContacto>();

        return MedioIndice >= 0 && MedioIndice < medios.Length
            ? medios[MedioIndice]
            : null;
    }

    [RelayCommand]
    private async Task Guardar()
    {
        string? error = _contactos.Registrar(
            _legajo,
            Fecha,
            MedioElegido(),
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
