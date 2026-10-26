using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Services;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Pantalla para agregar otra comisión además de las que ya hay.
/// </summary>
public partial class NuevaComisionViewModel : PantallaBase
{
    private readonly INavegacion _navegacion;

    private readonly ComisionService _comisiones;

    [ObservableProperty]
    private string nombre = "";

    public NuevaComisionViewModel(
        INavegacion navegacion,
        ComisionService comisiones)
    {
        _navegacion = navegacion;
        _comisiones = comisiones;
    }

    [RelayCommand]
    private async Task Guardar()
    {
        string? error = _comisiones.Agregar(Nombre);

        if (error is not null)
        {
            Rechazar(error);
            return;
        }

        await _navegacion.VolverAsync();
    }
}
