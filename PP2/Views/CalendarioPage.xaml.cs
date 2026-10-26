using RegistroAsistencia.ViewModels;

namespace RegistroAsistencia.Views;

public partial class CalendarioPage : ContentPage
{
    private readonly CalendarioViewModel _viewModel;

    public CalendarioPage()
    {
        InitializeComponent();

        _viewModel = new CalendarioViewModel();

        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.ConsultarCalendarioCommand.Execute(null);
    }
}
