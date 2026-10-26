using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RegistroAsistencia.Services;

namespace RegistroAsistencia.ViewModels;

/// <summary>
/// Pantalla para agregar otra comisión además de las que ya hay.
/// </summary>
public partial class NuevaComisionViewModel : ObservableObject
{
    private readonly INavegacion _navegacion;

    [ObservableProperty]
    private string nombre = "";

    [ObservableProperty]
    private string mensaje = "";

    public NuevaComisionViewModel(INavegacion navegacion)
    {
        _navegacion = navegacion;
    }

    [RelayCommand]
    private async Task Guardar()
    {
        string? error = Validaciones.NombreComision(Nombre);

        if (error is not null)
        {
            Mensaje = error;
            return;
        }

        DatosSistema.AgregarComision(Nombre);

        await _navegacion.VolverAsync();
    }
}
