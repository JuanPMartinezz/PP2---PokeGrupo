using RegistroAsistencia.ViewModels;

namespace RegistroAsistencia.Views;

public partial class ContactoPage : ContentPage
{
    private readonly ContactoViewModel _viewModel;

    public ContactoPage()
    {
        InitializeComponent();

        _viewModel = new ContactoViewModel();

        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.ConsultarContactosCommand.Execute(null);
    }
}
