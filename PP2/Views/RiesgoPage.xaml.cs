using RegistroAsistencia.ViewModels;

namespace RegistroAsistencia.Views;

public partial class RiesgoPage : ContentPage
{
    public RiesgoPage()
    {
        InitializeComponent();

        BindingContext = new RiesgoViewModel();
    }
}