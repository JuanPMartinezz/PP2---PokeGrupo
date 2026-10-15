using RegistroAsistencia.ViewModels;

namespace RegistroAsistencia.Views;

public partial class TrabajoPage : ContentPage
{
    private readonly TrabajoViewModel _viewModel;

    public TrabajoPage()
    {
        InitializeComponent();

        _viewModel = new TrabajoViewModel();

        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.ConsultarEntregasCommand.Execute(null);
    }
}