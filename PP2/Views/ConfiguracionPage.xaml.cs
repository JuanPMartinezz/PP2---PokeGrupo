using RegistroAsistencia.ViewModels;

namespace RegistroAsistencia.Views;

public partial class ConfiguracionPage : ContentPage
{
    private readonly ConfiguracionViewModel _viewModel;

    public ConfiguracionPage()
    {
        InitializeComponent();

        _viewModel = new ConfiguracionViewModel();

        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.CargarUmbralesCommand.Execute(null);
    }
}
