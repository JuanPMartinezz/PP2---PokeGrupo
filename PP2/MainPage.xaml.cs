using RegistroAsistencia.ViewModels;
using RegistroAsistencia.Views;

namespace RegistroAsistencia;

/// <summary>
/// Pantalla de inicio, la primera de la aplicación. Recibe su ViewModel
/// ya armado y no tiene más lógica que esa.
/// </summary>
public partial class MainPage : ContentPage
{
    public MainPage(InicioViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

        NavegacionMaui.ActualizarAlAparecer(this);
    }
}
