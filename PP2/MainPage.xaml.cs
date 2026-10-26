using RegistroAsistencia.ViewModels;
using RegistroAsistencia.Views;

namespace RegistroAsistencia;

/// <summary>
/// Pantalla de inicio. Es la primera de la aplicación: arma su ViewModel
/// con la navegación y no tiene más lógica que esa.
/// </summary>
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        BindingContext = new InicioViewModel(new NavegacionMaui());

        NavegacionMaui.ActualizarAlAparecer(this);
    }
}
