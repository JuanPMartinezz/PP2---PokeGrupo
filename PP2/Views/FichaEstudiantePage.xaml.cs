using RegistroAsistencia.ViewModels;

namespace RegistroAsistencia.Views;

public partial class FichaEstudiantePage : ContentPage
{
    private readonly FichaEstudianteViewModel _viewModel;

    public FichaEstudiantePage()
    {
        InitializeComponent();

        _viewModel = new FichaEstudianteViewModel();

        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.CargarFicha();
    }
}