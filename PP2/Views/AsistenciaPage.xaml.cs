using RegistroAsistencia.ViewModels;

namespace RegistroAsistencia.Views;

public partial class AsistenciaPage : ContentPage
{
    public AsistenciaPage()
    {
        InitializeComponent();

        BindingContext = new AsistenciaViewModel();
    }
}