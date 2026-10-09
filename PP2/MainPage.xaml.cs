using RegistroAsistencia.ViewModels;
using RegistroAsistencia.Views;

namespace RegistroAsistencia;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        BindingContext = new ComisionViewModel();
    }

    private async void IrAH3_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(
            new AsistenciaPage());
    }
}