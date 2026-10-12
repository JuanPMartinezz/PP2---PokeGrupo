using RegistroAsistencia.ViewModels;

namespace RegistroAsistencia.Views;

public partial class RiesgoPage : ContentPage
{
    public RiesgoPage()
    {
        InitializeComponent();
        BindingContext = new RiesgoViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RiesgoViewModel viewModel)
            viewModel.CargarRiesgos();
    }
}
